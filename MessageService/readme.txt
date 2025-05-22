docker build -t sen300messageserviceapi:1 .
docker run -d -p 8084:8080 --name SEN300MessageServiceAPI --net netSEN300 sen300messageserviceeapi:1