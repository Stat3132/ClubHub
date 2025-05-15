package pro290.clubhub;

import java.util.UUID;

import jakarta.persistence.Entity;
import jakarta.persistence.Id;

@Entity
public class Club {
    
    @Id
    UUID clubID;
    String clubName;
    String clubDecleration;
    String clubPresidentName;
    int clubPresidentID;
    int advisorID;
    
    public String getClubName() {
        return clubName;
    }
    public void setClubName(String clubName) {
        this.clubName = clubName;
    }
    public String getClubDecleration() {
        return clubDecleration;
    }
    public void setClubDecleration(String clubDecleration) {
        this.clubDecleration = clubDecleration;
    }
    public String getClubPresidentName() {
        return clubPresidentName;
    }
    public void setClubPresidentName(String clubPresidentName) {
        this.clubPresidentName = clubPresidentName;
    }
    public int getClubPresidentID() {
        return clubPresidentID;
    }
    public void setClubPresidentID(int clubPresidentID) {
        this.clubPresidentID = clubPresidentID;
    }
    public int getAdvisorID() {
        return advisorID;
    }
    public void setAdvisorID(int advisorID) {
        this.advisorID = advisorID;
    }
    public UUID getClubID() {
        return clubID;
    }
    public void setClubID(UUID clubID) {
        this.clubID = clubID;
    }
    
}
