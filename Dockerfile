FROM mcr.microsoft.com/dotnet/core/sdk:3.0.101-alpine3.9 AS build-env

ARG OPENCV_VERSION="4.0.1"

# Update image to latest packages
RUN apk update && \
    apk upgrade

# Install compilation dependencies 
# TODO: install and compile OpenCV
RUN apk add \
    git \
    build-base

# Download YOLO weights
WORKDIR /home
RUN wget https://pjreddie.com/media/files/yolov3.weights

# Build darknet
WORKDIR /home
RUN git clone https://github.com/pjreddie/darknet

WORKDIR /home/darknet

# Fix compilation issues
RUN sed -i '1 i\#include <sys/select.h>' examples/go.c
RUN make -j3

# Run sample
RUN ./darknet detect cfg/yolov3.cfg ../yolov3.weights data/dog.jpg
RUN ls -la

# Build glue library
WORKDIR /home/glue
COPY /src/glue/*.c .
RUN gcc -c -fPIC glue.c -o glue.o -I ../darknet/include
RUN gcc -shared glue.o -o libdarknet_glue.so -ldarknet -L../darknet

# Build ASP.NET Core wrapper and web API
WORKDIR /home/webapi
COPY /src/webapi/*.csproj .
RUN dotnet restore

COPY /src/webapi ./
RUN dotnet publish -c Release -o out

# build runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0

WORKDIR /home

COPY --from=build-env /home/webapi/out .
COPY --from=build-env /home/darknet/libdarknet.a /usr/lib
COPY --from=build-env /home/darknet/libdarknet.so /usr/lib
COPY --from=build-env /home/glue/libdarknet_glue.so /usr/lib
COPY --from=build-env /lib/libc.musl-x86_64.so.1 /lib

COPY --from=build-env /home/darknet/cfg/coco.data ./cfg/
COPY --from=build-env /home/darknet/cfg/yolov3.cfg ./cfg/
COPY --from=build-env /home/darknet/data/coco.names ./data/
COPY --from=build-env /home/darknet/data/labels/*.png ./data/labels/
COPY --from=build-env /home/darknet/data/dog.jpg ./
COPY --from=build-env /home/yolov3.weights ./

ENTRYPOINT ["dotnet", "/home/webapi.dll", "--environment=Development"]