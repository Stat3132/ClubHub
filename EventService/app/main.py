from fastapi import FastAPI, HTTPException
from bson import ObjectId
from app.database import events_collection
from app.models import Event
from fastapi.responses import JSONResponse
from py_eureka_client import eureka_client
import os

app = FastAPI()

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
        eureka_server=os.getenv("EUREKA_URL", "http://PRO290EurekaRegistry:8761/eureka"),
        app_name="PRO290EventServiceAPI",
        instance_port=8000,
        instance_host="PRO290EventServiceAPI",
        health_check_url="http://PRO290EventServiceAPI:8000/health",
        home_page_url="http://PRO290EventServiceAPI:8000",
        data_center_name="MyOwn"
    )

@app.get("/health")
def health():
    return {"status": "UP"}


@app.post("/createevent", status_code=201)
def create_event(event: Event):
    result = events_collection.insert_one(event.dict())
    return {"id": str(result.inserted_id)}

@app.get("/getevent/{id}", status_code=201)
def get_event(id: str):
    event = events_collection.find_one({"_id": ObjectId(id)})
    if not event:
        raise HTTPException(status_code=404, detail="Event not found")
    return serialize_event(event)

@app.get("/getevent", status_code=201)
def get_all_events():
    events = events_collection.find()
    return [serialize_event(event) for event in events]

@app.put("/updateevent/{id}", status_code=201)
def update_event(id: str, updated_event: Event):
    result = events_collection.update_one(
        {"_id": ObjectId(id)},
        {"$set": updated_event.dict()}
    )
    if result.matched_count == 0:
        raise HTTPException(status_code=404, detail="Event not found")
    return {"message": "Event updated successfully"}

@app.delete("/deleteevent/{id}", status_code=201)
def delete_event(id: str):
    result = events_collection.delete_one({"_id": ObjectId(id)})
    if result.deleted_count == 0:
        raise HTTPException(status_code=404, detail="Event not found")
    return {"message": "Event deleted successfully"}
