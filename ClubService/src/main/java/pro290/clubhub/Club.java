package pro290.clubhub;

import java.util.UUID;

import jakarta.persistence.Column;
import jakarta.persistence.Entity;
import jakarta.persistence.Id;


@Entity
public class Club {

    @Id
    private UUID clubID;
    private String clubName;
    @Column(name = "clubDeclaration")
    private String clubDeclaration;
    private String presidentName;
    private UUID presidentID;
    private UUID advisorID;
    
    
    public String getClubName() {
        return clubName;
    }
    public void setClubName(String clubName) {
        this.clubName = clubName;
    }
    public String getClubDeclaration() {
        return clubDeclaration;
    }
    public void setClubDeclaration(String clubDeclaration) {
        this.clubDeclaration = clubDeclaration;
    }
    public String getClubPresidentName() {
        return presidentName;
    }
    public void setClubPresidentName(String clubPresidentName) {
        this.presidentName = clubPresidentName;
    }
    public UUID getClubPresidentID() {
        return presidentID;
    }
    public void setClubPresidentID(UUID presidentID) {
        this.presidentID = presidentID;
    }
    public UUID getAdvisorID() {
        return advisorID;
    }
    public void setAdvisorID(UUID advisorID) {
        this.advisorID = advisorID;
    }
    public UUID getClubID() {
        return clubID;
    }
    public void setClubID(UUID clubID) {
        this.clubID = clubID;
    }
    
}
