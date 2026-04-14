"""
Engzly.CategoryAI — task category classification service.

Exposes a /classify endpoint that takes a task (title + description) and a
list of candidate categories, returns the top-K categories ranked by cosine
similarity of sentence embeddings.

Model: sentence-transformers/all-MiniLM-L6-v2 (English, ~90MB, fast on CPU).
"""

from __future__ import annotations

import hashlib
import logging
from contextlib import asynccontextmanager
from typing import Dict, List

import numpy as np
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel, Field
from sentence_transformers import SentenceTransformer

logging.basicConfig(level=logging.INFO, format="%(asctime)s %(levelname)s %(message)s")
log = logging.getLogger("category-ai")

MODEL_NAME = "sentence-transformers/all-MiniLM-L6-v2"

_model: SentenceTransformer | None = None
_category_cache: Dict[str, np.ndarray] = {}


@asynccontextmanager
async def lifespan(app: FastAPI):
    global _model
    log.info("loading model %s", MODEL_NAME)
    _model = SentenceTransformer(MODEL_NAME)
    log.info("model loaded, embedding dim=%d", _model.get_sentence_embedding_dimension())
    yield
    _category_cache.clear()


app = FastAPI(title="Engzly.CategoryAI", version="1.0.0", lifespan=lifespan)


class Candidate(BaseModel):
    id: str
    name: str
    description: str = ""


class ClassifyRequest(BaseModel):
    title: str = Field(..., min_length=1)
    description: str = Field("", max_length=4000)
    top_k: int = Field(3, ge=1, le=20)
    candidates: List[Candidate] = Field(..., min_length=1)


class Suggestion(BaseModel):
    categoryId: str
    score: float


class ClassifyResponse(BaseModel):
    results: List[Suggestion]


def _cache_key(c: Candidate) -> str:
    raw = f"{c.id}|{c.name}|{c.description}".encode("utf-8")
    return hashlib.sha1(raw).hexdigest()


def _embed(text: str) -> np.ndarray:
    assert _model is not None
    vec = _model.encode(text, normalize_embeddings=True, convert_to_numpy=True)
    return vec.astype(np.float32)


def _candidate_vector(c: Candidate) -> np.ndarray:
    key = _cache_key(c)
    cached = _category_cache.get(key)
    if cached is not None:
        return cached
    text = f"{c.name}. {c.description}" if c.description else c.name
    vec = _embed(text)
    _category_cache[key] = vec
    return vec


@app.get("/health")
async def health() -> dict:
    return {"status": "ok", "model": MODEL_NAME, "cache_size": len(_category_cache)}


@app.post("/classify", response_model=ClassifyResponse)
async def classify(req: ClassifyRequest) -> ClassifyResponse:
    if _model is None:
        raise HTTPException(status_code=503, detail="model not loaded")

    task_text = req.title if not req.description else f"{req.title}. {req.description}"
    task_vec = _embed(task_text)

    scored: List[Suggestion] = []
    for candidate in req.candidates:
        cand_vec = _candidate_vector(candidate)
        score = float(np.dot(task_vec, cand_vec))
        scored.append(Suggestion(categoryId=candidate.id, score=score))

    scored.sort(key=lambda s: s.score, reverse=True)
    return ClassifyResponse(results=scored[: req.top_k])
