# Build phase
FROM maven:3.9-sapmachine-21 AS build
COPY src /home/app/src
COPY pom.xml /home/app
RUN mvn -f /home/app/pom.xml clean package

# Runtime phase
FROM tomcat:11.0.0-jdk21

# Copy WAR into Tomcat
COPY --from=build /home/app/target/ClubService.war /usr/local/tomcat/webapps/ROOT.war

# Environment variables for Spring Boot config
ENV SERVER_PORT=8081
ENV DB_HOST=clubhub
ENV DB_PORT=3306
ENV DB_NAME=clubhub
ENV DB_USERNAME=root
ENV DB_PASSWORD=abc12345!

# You do NOT put Spring config here. It should be in application.properties
# OR you inject these ENV vars into Spring Boot at runtime
