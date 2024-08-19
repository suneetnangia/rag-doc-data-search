#! /bin/bash

# Default target
setup:
	docker run -d --name qdrantdb -p 6333:6333 -p 6334:6334 -v $(pwd)/qdrant_storage:/qdrant/storage:z qdrant/qdrant
	docker run -d --name ollama --gpus=all -v ollama:/root/.ollama -p 11434:11434 ollama/ollama

# TODO: Stop and remove containers cause some delay so you want to run it twice.
clean:
	if docker ps -a | grep -q qdrantdb; then \
		docker stop qdrantdb && docker rm qdrantdb; \
	fi
	if docker ps -a | grep -q ollama; then \
		docker stop ollama && docker rm ollama; \
	fi