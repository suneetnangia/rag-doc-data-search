# Dev Loop Process

This is a work in progress document.

## Data Seeds

### Influx DB

#### Data

Load air quality sample data from the following annotated csv file, using [Influx Db web UI](http://localhost:8086).

https://raw.githubusercontent.com/influxdata/influxdb2-sample-data/master/air-sensor-data/air-sensor-data-annotated.csv

#### Queries

Sample Influx DB queries (escape double quotes before inserting via Swagger):

```influxdb
from(bucket: "air_quality")
  |> range(start: -4, stop: -2)
  |> filter(fn: (r) => r["_measurement"] == "airSensors")
  |> filter(fn: (r) => r["_field"] == "humidity")
  |> filter(fn: (r) => r["sensor_id"] == "TLM0100")
```

## Outstanding

1. Avoid returning internal errors to client.
2. Enable streaming endpoint on API for immediate response from S/LLMs.
3. Enable async processing using workers and http 202 accept for long running requests e.g. loading models.
4. Unit and integration Tests.
5. Add IDisposable pattern where applicable.

## Common Errors

1. "Wrong input: Not existing vector name error" occurs if a collection is created already with an incorrect vector dimension, this is not signifying that the collection not present in vector db.

## Local Dev UIs

1. [Qdrant UI Dashboard](http://localhost:6333/dashboard)
2. [Influx Db web UI](http://localhost:8086)
3. Ollama UI [https://github.com/open-webui/open-webui]

