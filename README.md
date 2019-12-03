# Object recognition as Web API

This project wraps object recognition neural networks as web API Docker image.

## Prerequisites

* [Docker](https://www.docker.com/), for Ubuntu 18.04 installation see e.g. [here](https://www.digitalocean.com/community/tutorials/how-to-install-and-use-docker-on-ubuntu-18-04)

## Building the image

For CPU:

```
docker build -f Dockerfile.cpu -t object-recognition-service:cpu . 
```

For GPU (CUDA + CUDNN):

```
docker build -f Dockerfile.cuda -t object-recognition-service:cuda .
```

## Running the image

To redirect to port 8080 run following command:

CPU:

```
docker run -p 8080:80 object-recognition-service:cpu
```

GPU (host system must have NVidia drivers installed):

```
docker run -p 8080:80 --runtime=nvidia object-recognition-service:cuda
```

Navigating to `http://localhost:8080` should open Swagger UI for API testing.
Navigating to `http://localhost:8080/dashboard` should open Hangfire UI for job monitoring.

## Enqueue training job

```
backgroundJobs.Enqueue<DarknetTrainJob>(job =>
        job.Start(Guid.NewGuid().ToString(),
        "/darknet_host/data/coco/trainvalno5k.txt",
        "/darknet_host/cfg/yolov3.cfg",
        "/darknet_host/darknet53.conv.74",
        new int[] { 0 },
        false));
```


## ToDo

- [x] Provide CPU option
- [x] Integrate with Cuda
- [ ] Add TensorFlow implementation
