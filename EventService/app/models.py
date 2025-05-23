from pydantic import BaseModel

class Event(BaseModel):
    EventName: str
    eventDescription: str
    eventBudget: float
    eventCoordinator: str
    eventCoordinatorsNumber: str
