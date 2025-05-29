Completed Microservices
- User Service (Tommy)
    - Only allow admins/advisors to get, put, and delete users (allow user to fetch themselves) 
- Ocelot API Gateway (Tommy)
- Eureka Service Discovery (Tommy)
- Club Service (Diego)
    - only allow advisor+ to access post, put, and delete
- RabbitMQ (Message Queue) (Tommy)
- MongoDB for Events (Easy) (Tommy)
- Event Service (Medium) (Tommy)
    - only allow advisor+ to put/delete, allow president+ to post
- Email Sender Service (Easy) (Tommy)
    - only allow admin+ to get/post (inclusive to only microservice cross communication)

Incomplete Microservices
- Club Management Service (Hard)
    - only allow advisor+ to get/delete
- Customer UI (React) (Diego) (Medium)
- User Authentication (Medium) (Can be added to existing USER microservice)
    - For User Auth, add JWT Token, and allow user to fetch it
    - Also add checks in existing microservices dependent on role