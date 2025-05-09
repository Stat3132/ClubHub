docker build -t sen300userserviceapi:1 .
docker run -d -p 8084:8080 --name SEN300UserServiceAPI --net netSEN300 sen300serviceeapi:1