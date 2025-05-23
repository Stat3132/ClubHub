from fastapi import FastAPI, HTTPException
from bson import ObjectId
from app.database import events_collection
from app.models import Event
from fastapi.responses import JSONResponse

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
