# Documents and Data Search using Vector DB (Cloud Agnostic)

The repo demonstrates how to create a deterministic solution which leverages Gen AI technologies for assisting domain specific users to achieve operational improvements. It also highlights some of the ground realities of the challenges and common assumptions e.g. **Large Language Models are not necessarily required for RAG solutions** if they are only used to convert a response to a more natural sounding language.

## Primary Features

1. Search the pre-indexed documents using vector DB and respond in natural language using S/LLM models.
2. Search the databases (e.g. Influx, SQL) using predefined queries Vector DB or synthesized by S/LLM model, and respond in natural language using S/LLM models.

## Persona Alignment (Database Search)

On database search side of things, this solution addresses the challenge faced by non IT users who'd benefit from data exploration apart from well thought-out and predefined queries written by IT upfront.

Users want to search data from data stores in both well defined queries way or exploration way. This sits in-between od data analyst who are aware of the query syntaxes and business users who are limited by the IT provided user queries.

## Design Overview and Use Cases

The solution makes use of Rust programming language for the core development but it does not require use of Rust on the client side i.e. consumers of this solution rely on open communication standards like GRPC or RESTFul APIs.

There two primary use cases handled by this solution, they are described below along with their respective flows.

### Document Search

User would like to search existing documents, these documents can be machine manuals in industrial domain, regulatory/compliance policies in financial domain.

![Document Search Process](docs/images/rag-doc-process.png?raw=true "Document Search Process")

### Database Search

![Database Search Process](docs/images/rag-db-process.png?raw=true "Database Search Process")

## Deployment

This section describes the steps which you can use to deploy this solution in your environment to try out.

![K8s Setup](docs/images/rag-k8s-setup.png?raw=true "K8s Setup")

### Codespace Deployment

[Add steps to deploy the solution in Codespace/Dev Container]
[Add Codespace Button]

### Local Deployment/Development

1. clone repo ```git clone git@github.com:suneetnangia/rag-doc-data-search.git && cd rag-doc-data-search```
2. Install dependent services ```make setup```
3. Run document search service ```cd src/Doc.Api && dotnet run```
4. Open Swagger link ```http://localhost:5152/swagger/index.html``` to try the APIs.

## Configuration and Extensibility

This repo makes use of [Ollama](https://github.com/ollama/ollama) to host both embeddings models and S/LLM models. Ollama provides various options regarding hosting and management of models, we surface some of those options in this solution, they are listed here:

1. Embeddings Model

   Allows you to change embeddings model which is responsible for creating vector DB embeddings from the documents.
2. S/LLM Model

   Allows you to change S/LLM model used to generate natural language responses for your database or doc queries.
