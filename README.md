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

### Database Search [WIP]

On database search side of things, this solution addresses the challenge faced by non IT users who'd benefit from data exploration apart from well thought-out and predefined queries written by IT upfront.

![Database Search Process](docs/images/rag-db-process.png?raw=true "Database Search Process")

## Deployment

This section describes the steps to deploy this solution in your environment.

### Codespace Deployment

1. Open this codespace in your browser or in your local Visual Studio Code.

    [![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/suneetnangia/rag-doc-data-search/)

2. Install dependent services ```make setup```
3. Run document search service ```cd src/Doc.Api && dotnet run```
4. Open Swagger link to try the APIs. If you are running the codespace Visual Studio Code Desktop, you can use ```http://localhost:5152/swagger/index.html```. In case you are running the codespace in the browser, you can get the hostname from the Ports tab. Copy the URL and append ```/swagger/index.html``` to it, open in a new browser window.

> Note: optionally you can also open the Dev Container locally by first cloning this repo, opening it in Visual Studio Code and choosing **Ctrl/Cmd + Shift + P > Dev Containers: Reopen in container**.

### Local Deployment/Development on WSL/Linux

1. Clone repo ```git clone git@github.com:suneetnangia/rag-doc-data-search.git && cd rag-doc-data-search```
2. Install dependent services ```make setup```
3. Run document search service ```cd src/Doc.Api && dotnet run```
4. Open Swagger link ```http://localhost:5152/swagger/index.html``` to try the APIs.

## Configuration and Extensibility

This repo makes use of [Ollama](https://github.com/ollama/ollama) to host both embeddings models and S/LLM models. Ollama provides various options regarding hosting and management of models, we surface some of those options along with vector db options in this solution, they can be configured via [appsettings](src/Doc.Api/appsettings.Development.json).

## Extension Repos [WIP]

These repos provide layers on top of this solution, to provide an on-ramp for various use cases.

1. CLI Repo: Provides access to the solution via CLI interface for scripting and automating.
2. Bootstrapping Repo: Loads sample data in the solution.
3. K8s Repo: Deploys the solution in K8s setting using sidecar pattern.
