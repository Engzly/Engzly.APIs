"""
Engzly.CategoryAI — task AI service.

Exposes:
  POST /classify — zero-shot category classification for a task.
  POST /estimate — AI cost + duration estimation using k-NN regression
                   over sentence embeddings of historical completed tasks.

Model: sentence-transformers/all-MiniLM-L6-v2 (English, ~90MB, fast on CPU).
The same model powers both endpoints: we embed the task + reference items,
rank by cosine similarity, and either return the top categories (classify)
or take a similarity-weighted regression over historical budgets/durations
(estimate).
"""

from __future__ import annotations

import hashlib
import logging
import math
from contextlib import asynccontextmanager
from typing import Dict, List, Optional

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


class Exemplar(BaseModel):
    title: str
    description: str = ""
    budget: float = Field(..., ge=0)
    duration_minutes: int = Field(..., ge=0)


class EstimateRequest(BaseModel):
    title: str = Field(..., min_length=1)
    description: str = Field("", max_length=4000)
    number_of_taskers_needed: int = Field(1, ge=1, le=50)
    top_k: int = Field(5, ge=1, le=50)
    exemplars: List[Exemplar] = Field(default_factory=list)
    category_id: Optional[str] = None
    category_name: Optional[str] = None
    currency: str = "EGP"


class BudgetEstimate(BaseModel):
    min: float
    expected: float
    max: float
    currency: str


class DurationEstimate(BaseModel):
    min_minutes: int
    expected_minutes: int
    max_minutes: int


class EstimateResponse(BaseModel):
    category_id: Optional[str]
    category_name: Optional[str]
    budget: BudgetEstimate
    duration: DurationEstimate
    method: str
    sample_size: int
    confidence: float
    neighbors_used: int


FALLBACK_BUDGET = 250.0
FALLBACK_DURATION_MINUTES = 120


def _round_budget(value: float) -> float:
    if value <= 0:
        return 0.0
    return round(value / 5.0) * 5.0


def _complexity_factor(description: str) -> float:
    words = len(description.split()) if description else 0
    if words < 20:
        return 1.0
    if words < 60:
        return 1.15
    if words < 120:
        return 1.30
    return 1.50


def _fallback_estimate(req: EstimateRequest) -> EstimateResponse:
    factor = _complexity_factor(req.description)
    taskers = max(1, req.number_of_taskers_needed)
    budget_mult = factor * taskers

    return EstimateResponse(
        category_id=req.category_id,
        category_name=req.category_name,
        budget=BudgetEstimate(
            min=_round_budget(FALLBACK_BUDGET * 0.6 * budget_mult),
            expected=_round_budget(FALLBACK_BUDGET * budget_mult),
            max=_round_budget(FALLBACK_BUDGET * 1.8 * budget_mult),
            currency=req.currency,
        ),
        duration=DurationEstimate(
            min_minutes=int(round(FALLBACK_DURATION_MINUTES * 0.5 * factor)),
            expected_minutes=int(round(FALLBACK_DURATION_MINUTES * factor)),
            max_minutes=int(round(FALLBACK_DURATION_MINUTES * 2.0 * factor)),
        ),
        method="fallback",
        sample_size=0,
        confidence=0.25,
        neighbors_used=0,
    )


def _exemplar_text(e: Exemplar) -> str:
    return f"{e.title}. {e.description}" if e.description else e.title


@app.post("/estimate", response_model=EstimateResponse)
async def estimate(req: EstimateRequest) -> EstimateResponse:
    """
    AI cost + duration estimation.

    Approach:
      1. Embed the incoming task and each exemplar with the same
         sentence-transformers model used for classification.
      2. Compute cosine similarity between the task and each exemplar.
      3. Select the top-k most similar exemplars.
      4. Similarity-weighted regression on their budget and duration to
         produce an expected value; min/max come from the weighted spread.
      5. Confidence is the mean similarity of the neighbors used, clamped.
      6. Falls back to a complexity-factor heuristic when no exemplars
         are provided (cold start for a category).
    """
    if _model is None:
        raise HTTPException(status_code=503, detail="model not loaded")

    if not req.exemplars:
        return _fallback_estimate(req)

    task_text = req.title if not req.description else f"{req.title}. {req.description}"
    task_vec = _embed(task_text)

    scored: List[tuple[float, Exemplar]] = []
    for ex in req.exemplars:
        ex_vec = _embed(_exemplar_text(ex))
        sim = float(np.dot(task_vec, ex_vec))
        scored.append((sim, ex))

    scored.sort(key=lambda t: t[0], reverse=True)
    top = scored[: req.top_k]

    # Shift similarities into a non-negative weight space so a borderline
    # neighbor still contributes proportionally rather than being zeroed.
    weights = np.array([max(0.0, (s + 1.0) / 2.0) for s, _ in top], dtype=np.float64)
    weight_sum = float(weights.sum())
    if weight_sum <= 0:
        return _fallback_estimate(req)

    budgets = np.array([ex.budget for _, ex in top], dtype=np.float64)
    durations = np.array([ex.duration_minutes for _, ex in top], dtype=np.float64)

    expected_budget = float(np.dot(weights, budgets) / weight_sum)
    expected_duration = float(np.dot(weights, durations) / weight_sum)

    budget_var = float(np.dot(weights, (budgets - expected_budget) ** 2) / weight_sum)
    duration_var = float(np.dot(weights, (durations - expected_duration) ** 2) / weight_sum)
    budget_std = math.sqrt(max(0.0, budget_var))
    duration_std = math.sqrt(max(0.0, duration_var))

    min_budget = max(0.0, expected_budget - budget_std)
    max_budget = expected_budget + budget_std
    min_duration = max(0.0, expected_duration - duration_std)
    max_duration = expected_duration + duration_std

    complexity = _complexity_factor(req.description)
    taskers = max(1, req.number_of_taskers_needed)
    budget_mult = complexity * taskers

    mean_similarity = float(np.mean([s for s, _ in top]))
    confidence = max(0.0, min(1.0, (mean_similarity + 1.0) / 2.0)) * min(
        1.0, len(top) / 10.0
    )

    return EstimateResponse(
        category_id=req.category_id,
        category_name=req.category_name,
        budget=BudgetEstimate(
            min=_round_budget(min_budget * budget_mult),
            expected=_round_budget(expected_budget * budget_mult),
            max=_round_budget(max_budget * budget_mult),
            currency=req.currency,
        ),
        duration=DurationEstimate(
            min_minutes=int(round(min_duration * complexity)),
            expected_minutes=int(round(expected_duration * complexity)),
            max_minutes=int(round(max_duration * complexity)),
        ),
        method="knn-embedding",
        sample_size=len(req.exemplars),
        confidence=round(confidence, 3),
        neighbors_used=len(top),
    )
