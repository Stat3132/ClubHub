FROM maven:3.9-sapmachine-21 AS build
COPY src /home/app/src
COPY pom.xml /home/app
RUN mvn -f /home/app/pom.xml clean package

#FROM tomcat:8-jdk8-openjdk
FROM tomcat:11.0.0-jdk21
COPY --from=build /home/app/target/ClubService.war /usr/local/tomcat/webapps/ROOT.war

ENV SERVER_PORT=1433
ENV MONGO_HOST=SEN300UserClubServiceDBSqlServer
ENV SQL_PORT=1433
ENV SQL_DB=clubservice
#ENV EUREKA_HOST=SEN300EurekaRegistry WE WILL DO EUREKA LATER

# ADD ./target/BookServiceAPI.war /usr/local/tomcat/webapps/ROOT.war

#docker build -t sen300bookserviceapi:1 .
#docker run -d -p 8081:8080 --name SEN300BookServiceAPI --net netSEN300 sen300bookserviceapi:1