# Documents and Data Search using S/LLMs (Cloud Agnostic)

The repo demonstrates how to create a solution which leverages Gen AI technologies for assisting domain specific users to achieve operational improvements.

## Primary Features

1. Search the pre-indexed documents using vector DB and respond in natural language using S/LLM models.
2. Search the databases (e.g. Influx, SQL) using predefined queries Vector DB or synthesized by S/LLM model, and respond in natural language using S/LLM models.

## Persona Alignment (Database Search)

On database search side of things, this solution addresses the challenge faced by non IT users who'd benefit from data exploration apart from well thought-out and predefined queries written by IT upfront.

Users want to search data from data stores in both well defined queries way or exploration way. This sits in-between od data analyst who are aware of the query syntaxes and business users who are limited by the IT provided user queries.

## Design Overview and Use Cases

There two primary use cases handled by this solution, they are described below along with their respective flows.

### Document Search

User would like to search existing documents, these documents can be machine manuals in industrial domain, regulatory/compliance policies in financial domain.

![Document Search Process](docs/images/rag-doc-process.png?raw=true "Document Search Process")

### Database Search

![Database Search Process](docs/images/rag-db-process.png?raw=true "Database Search Process")

## Deployment

This section describes the steps which you can use to deploy this solution in your environment to try out.

### Codespace Deployment

[Add steps to deploy the solution in Codespace]

## Configuration and Extensibility

This repo makes use of [Ollama] (https://github.com/ollama/ollama) to host both embeddings models and S/LLM models. Ollama provides various options regarding hosting and management of models, we surface some of those options in this solution, they are listed here:

1. Embeddings Model

   Allows you to change embeddings model which is responsible for creating vector DB embeddings from the documents.
2. S/LLM Model

   Allows you to change S/LLM model used to generate natural language responses for your database or doc queries.
