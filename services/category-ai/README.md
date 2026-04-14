# Engzly.CategoryAI

Embedding-based category classifier for Engzly tasks.

## Run locally

```bash
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8000
```

First run downloads `all-MiniLM-L6-v2` (~90MB).

## Docker

```bash
docker build -t engzly-category-ai .
docker run -p 8000:8000 engzly-category-ai
```

## API

### `POST /classify`

Request:
```json
{
  "title": "I need my fridge fixed, the freezer is leaking",
  "description": "Appliance repair in my apartment",
  "top_k": 3,
  "candidates": [
    { "id": "cat-fix",      "name": "Fix Things", "description": "Repairs and maintenance..." },
    { "id": "cat-delivery", "name": "Delivery",   "description": "Picking up and delivering..." }
  ]
}
```

Response:
```json
{
  "results": [
    { "categoryId": "cat-fix",      "score": 0.82 },
    { "categoryId": "cat-delivery", "score": 0.21 }
  ]
}
```

Scores are cosine similarity in `[-1, 1]`, typically `[0, 1]` for related text.

### `GET /health`

Returns model info and cache size.
