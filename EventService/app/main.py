from fastapi import FastAPI, HTTPException
from bson import ObjectId
from app.database import events_collection
from app.models import Event
from fastapi.responses import JSONResponse
from py_eureka_client import eureka_client
from fastapi import Depends, HTTPException, status
from fastapi.security import OAuth2PasswordBearer
from jose import jwt, JWTError
import os

app = FastAPI()
oauth2_scheme = OAuth2PasswordBearer(tokenUrl="token")
SECRET_KEY = "SuperSecretPasswordWithASuperSecretSecretThatNoOneWillEverFind"


def get_current_user_role(token: str = Depends(oauth2_scheme)):
    print(f"Received token: {token}", flush=True)
    try:
        payload = jwt.decode(
            token, SECRET_KEY, algorithms=["HS256"], options={"verify_aud": False}
        )
        print(payload)
        role = payload.get("role") or payload.get(
            "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
        )
        print("Role: ", role)
        if role is None:
            raise HTTPException(status_code=403, detail="Role not found in token")
        return role
    except JWTError as e:
        print(f"JWTError: {e}", flush=True)
        raise HTTPException(status_code=401, detail="Invalid token")


def advisor_or_admin(role: str = Depends(get_current_user_role)):
    if role.upper() not in ["ADVISOR", "ADMIN"]:
        print(role)
        raise HTTPException(status_code=403, detail="Not enough permissions")


def serialize_event(event) -> dict:
    return {
        "id": str(event["_id"]),
        "EventName": event["EventName"],
        "eventDescription": event["eventDescription"],
        "eventBudget": event["eventBudget"],
        "eventCoordinator": event["eventCoordinator"],
        "eventCoordinatorsNumber": event["eventCoordinatorsNumber"],
    }


@app.on_event("startup")
async def register_to_eureka():
    await eureka_client.init_async(
        eureka_server=os.getenv(
            "EUREKA_URL", "http://PRO290EurekaRegistry:8761"
        ),
        app_name="PRO290EventServiceAPI",
        instance_port=8000,
        instance_host="PRO290EventServiceAPI",
        health_check_url="http://PRO290EventServiceAPI:8000/health",
        home_page_url="http://PRO290EventServiceAPI:8000",
        data_center_name="MyOwn",
    )


@app.get("/health")
def health():
    return {"status": "UP"}

@app.post("/createevent", status_code=201, dependencies=[Depends(advisor_or_admin)])
def create_event(event: Event):
    result = events_collection.insert_one(event.dict())
    return {"id": str(result.inserted_id)}


@app.get("/getevent/{id}", status_code=201)
def get_event(id: str):
    event = events_collection.find_one({"_id": ObjectId(id)})
    if not event:
        raise HTTPException(status_code=404, detail="Event not found")
    return serialize_event(event)


@app.get("/api/events")
def get_all_events():
    events = events_collection.find()
    return {"events": [serialize_event(e) for e in events]}



@app.put("/updateevent/{id}", status_code=201, dependencies=[Depends(advisor_or_admin)])
def update_event(id: str, updated_event: Event):
    result = events_collection.update_one(
        {"_id": ObjectId(id)}, {"$set": updated_event.dict()}
    )
    if result.matched_count == 0:
        raise HTTPException(status_code=404, detail="Event not found")
    return {"message": "Event updated successfully"}


@app.delete("/deleteevent/{id}", status_code=201, dependencies=[Depends(advisor_or_admin)])
def delete_event(id: str):
    result = events_collection.delete_one({"_id": ObjectId(id)})
    if result.deleted_count == 0:
        raise HTTPException(status_code=404, detail="Event not found")
    return {"message": "Event deleted successfully"}
