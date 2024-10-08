# Documents and Data Search using Vector DB (Cloud Agnostic)

The repo demonstrates the deterministic solution for searching documents and data, it leverages the Gen AI technologies for assisting end users with their domain specific searches. It also highlights some of the ground realities and common assumptions in this space e.g. **Large Language Models (LLMs) are absolutely required for RAG solutions**. In RAG solutions, S/LLMs are only used to convert the response to a more natural sounding language whereas the actual search is provided by vector database. Use of S/LLMs often result in hallucinations and inaccurate results when used in a straight through processing, it isn't suitable for matters of consequence.

## Primary Features

1. Search the pre-indexed documents using vector DB and respond in natural language using S/LLM models, optionally.
2. Search the databases (e.g. Influx, SQL) using predefined queries in vector DB or synthesized by S/LLM model, and respond in natural language using S/LLM models, optionally.

## Design Overview and Use Cases

There two primary and basic use cases handled by this solution, they are described below along with their respective flows.

### Document Search

User would like to search existing documents, these documents can be machine manuals in industrial domain or regulatory/compliance policies in financial domain.

![Document Search Process](docs/images/rag-doc-process.png?raw=true "Document Search Process")

### Database Search

On database search side of things, this solution addresses the challenge faced by non IT users who'd benefit from data exploration apart from well thought-out and predefined queries written by IT upfront.

![Database Search Process](docs/images/rag-db-process.png?raw=true "Database Search Process")

## Deployment

This section describes the steps to deploy this solution in your environment.

### Codespace Deployment

1. Open this codespace in your browser or in your local Visual Studio Code.

    [![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/suneetnangia/rag-doc-data-search/)

2. Install dependent services ```make setup```
3. Run Document API:
   1. Run document search service ```make run_doc```
   2. Open Swagger link ```http://localhost:5152/swagger/index.html``` if you are on VS Code.
   3. Open Swagger link by appending ```/swagger/index.html``` to the hostname from the Ports tab if you are on Codespaces in a browser.
4. Run Data API:
   1. Configure Influx DB as described [here](docs/dev-loop.md#influx-db).
   2. Run document search service ```make run_db```
   3. Open Swagger link ```http://localhost:5155/swagger/index.html``` if you are on VS Code.
   4. Open Swagger link by appending ```/swagger/index.html``` to the hostname from the Ports tab if you are on Codespaces in a browser.

### Local Deployment/Development on WSL/Linux

1. Clone repo ```git clone git@github.com:suneetnangia/rag-doc-data-search.git && cd rag-doc-data-search```
2. Optionally, open the repo in a pre-configured Dev Container.
3. Install dependent services ```make setup```
4. Run Document API:
   1. Run document search service ```make run_doc```
   2. Open Swagger link ```http://localhost:5152/swagger/index.html``` to try the APIs.
5. Run Data API:
   1. Configure Influx DB as described [here](docs/dev-loop.md#influx-db).
   2. Run document search service ```make run_db```
   3. Open Swagger link ```http://localhost:5155/swagger/index.html``` to try the APIs.

## Configuration and Extensibility

This repo makes use of [Ollama](https://github.com/ollama/ollama) to host both embeddings models and S/LLM models. Ollama provides various options regarding hosting and management of models, we surface some of those options along with vector db options in this solution, they can be configured via [appsettings](src/Doc.Api/appsettings.Development.json).

## Potential Extensions

These potential extensions can provide layers on top of this solution, to provide an on-ramp for various use cases.

1. CLI Repo: Provides access to the solution via CLI interface for scripting and automating.
2. Bootstrapping Repo: Loads sample data in the solution.
3. K8s Repo: Deploys the solution in K8s setting using sidecar pattern.
