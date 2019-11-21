# Build Darknet library and executable

FROM nvidia/cuda:10.1-cudnn7-devel AS darknet-env

## Install build tools

RUN apt-get update && apt-get upgrade -y
RUN apt-get install -y --no-install-recommends build-essential
RUN apt-get install -y --no-install-recommends wget git

## Download YOLO weights

WORKDIR /home
RUN wget https://pjreddie.com/media/files/yolov3.weights

## Download darknet repository

WORKDIR /home
RUN git clone https://github.com/pjreddie/darknet

WORKDIR /home/darknet

## Fix compilation issues

RUN sed -i '1 i\#include <sys/select.h>' examples/go.c

## Enable CUDA and CUDNN

RUN sed -i'' 's/GPU=0/GPU=1/g' Makefile
RUN sed -i'' 's/CUDNN=0/CUDNN=1/g' Makefile

## Build Darknet library

RUN make -j3

## Build glue library

WORKDIR /home/glue
COPY /src/glue/*.c .
RUN gcc -c -fPIC glue.c -o glue.o -I ../darknet/include
RUN gcc -shared glue.o -o libdarknet_glue.so -ldarknet -L../darknet

# Build Web API

FROM mcr.microsoft.com/dotnet/core/sdk AS netcoresdk-env




# Build runtime image

FROM nvidia/cuda:10.1-cudnn7-runtime
WORKDIR /home

## Copy darknet library and glue

COPY --from=darknet-env /home/darknet/libdarknet.so /usr/lib
COPY --from=darknet-env /home/glue/libdarknet_glue.so /usr/lib

## Copy data

COPY --from=darknet-env /home/darknet/cfg/coco.data ./cfg/
COPY --from=darknet-env /home/darknet/cfg/yolov3.cfg ./cfg/
COPY --from=darknet-env /home/darknet/data/coco.names ./data/
COPY --from=darknet-env /home/darknet/data/labels/*.png ./data/labels/
COPY --from=darknet-env /home/darknet/data/dog.jpg ./data/
COPY --from=darknet-env /home/yolov3.weights ./

## Copy executable

COPY --from=darknet-env /home/darknet/darknet ./

ENTRYPOINT ["./darknet", "detect", "cfg/yolov3.cfg", "yolov3.weights", "data/dog.jpg"]