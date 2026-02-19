# EShopDistributed

EShopDistributed is a sample/portfolio ecommerce application that demonstrates a distributed .NET architecture with an AI-enabled product suggestion feature. It is intended for demos, learning, and showcasing integration patterns between services and a simple recommender.

## Purpose

Provide a lightweight example of how to build a modular ecommerce system (catalog, cart, orders) and integrate a recommendation engine to surface personalized product suggestions.

## Features

- Product catalog browsing with sample data
- Cart and checkout flows (demo) -> API Only
- Order history (sample) -> API Only
- AI-enabled product suggestions (content-based / collaborative demo) -> Blazor UI
- Clear separation of services suitable for experimenting with microservices and containers
- Orchestration demonstration using Aspire

## High-level architecture

- Backend: .NET APIs (products, cart/orders, recommendations)
- Recommender: pluggable module (local algorithm or external ML service)
- Frontend: simple UI to browse and view suggested items
- Data: sample datasets; easy to replace with a real DB or event store
- Optional: containerization (Docker) for each service

## AI product-suggestion flow

1. Capture signals: page views, purchases, categories, and user/session context.  
2. Build features/embeddings for products and interactions.  
3. Query the recommender (local model or remote service) to get ranked candidates.  
4. Apply business filters (stock, promotions) and return top-N suggestions to the UI.

Implementation notes:

- The included recommender favors simplicity and explainability for demos.
- Replace or extend with vector search, collaborative filtering, or hybrid models for production.
- The UI is not completed its for the sake of using the search (traditional and AI-enabled)

## Tech stack

- Aspire for orchestration
- PostreSQL
- Resis
- RabbitMQ
- Keycloak
- Ollama (llama3.2 and all-minilm)
- .NET 10, C# for backend services  
- Blazor default template for demos  

## Quick start

1. Clone the repo:
   - git clone <https://github.com/><your-username>/<this-repo>.git
2. Docker/Docker Desktop must be ready
3. Build and run `AppHost` project
4. The `Aspire` dashboard should be open now, otherwise you can access it via `https://localhost:17125/`
5. `WebApp` url access: `https://localhost:7080/`
6. For testing Catalog endpoint use [Catalog.http](/Catalog/Catalog.http) file.
7. For testing Basket endpoint use [Basket.http](/Basket/Basket.http) file.
   1. Make sure refresh the token using the token endpoint from keycloak (placed in the same http file)

## Screenshots

### Aspire Dashboard

![Aspire Dashboard](/docs/aspire_dashboard.png)

### Docker Containers

![Docker Containers](/docs/docker_containers.png)

### Frontend

![Frontend](/docs/product_list.png)

### Semantic Search

![Semantic Search](/docs/semantic_search.png)

## License

Use and adapt this sample for demos and learning.
