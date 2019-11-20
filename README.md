# Object recognition as Web API

This project wraps object recognition neural networks as web API Docker image.

## Prerequisites

* [Docker](https://www.docker.com/), for Ubuntu 18.04 installation see e.g. [here](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-18-04)

## Building the image

```
docker build -t object-recognition-service:v1 . 
```

## Running the image

To redirect to port 8080 run following command:

```
docker run -p 8080:80 object-recognition-service:v1
```

Navigating to `http://localhost:8080` should open Swagger UI for API testing.

## ToDo

- [ ] Integrate with Cuda
- [ ] Add TensorFlow implementation