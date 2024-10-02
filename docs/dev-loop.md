# Dev Loop Process

## Data Seeds

### Influx DB

Open [Influx Db web UI](http://localhost:8086)

#### Configure

1. Create user name and extract token.
2. Update ```src/Rag.Db.Api/appsettings.Development.json``` settings file with Influx Db token above.

#### Data

Load air quality sample data from the following annotated csv file, using [Influx Db web UI](http://localhost:8086).

<https://raw.githubusercontent.com/influxdata/influxdb2-sample-data/master/air-sensor-data/air-sensor-data-annotated.csv>

#### Queries

Add the sample Influx DB query (escape double quotes before inserting via Post in Swagger UI):

```influxdb
from(bucket: "air_quality")
  |> range(start: -4, stop: -2)
  |> filter(fn: (r) => r["_measurement"] == "airSensors")
  |> filter(fn: (r) => r["_field"] == "humidity")
  |> filter(fn: (r) => r["sensor_id"] == "TLM0100")
```

## Common Errors

1. "Wrong input: Not existing vector name error" occurs if a collection is created already with an incorrect vector dimension, this is not signifying that the collection not present in vector db.

## Local Dev UIs

1. [Qdrant UI Dashboard](http://localhost:6333/dashboard)
2. [Influx Db web UI](http://localhost:8086)
3. Ollama UI [https://github.com/open-webui/open-webui]
