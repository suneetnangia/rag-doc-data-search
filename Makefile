#! /bin/bash

# Default target
setup:
	docker run -d --rm --name chromadb -v ./chroma:/chroma/chroma -p 8080:8000 -e IS_PERSISTENT=TRUE -e ANONYMIZED_TELEMETRY=FALSE chromadb/chroma:latest
	docker run -d --name ollama --gpus=all -v ollama:/root/.ollama -p 11434:11434 ollama/ollama

# TODO: Stop and remove containers cause some delay so you want to run it twice.
clean:
	if docker ps -a | grep -q chromadb; then \
		docker stop chromadb && docker rm chromadb; \
	fi
	if docker ps -a | grep -q ollama; then \
		docker stop ollama && docker rm ollama; \
	fi