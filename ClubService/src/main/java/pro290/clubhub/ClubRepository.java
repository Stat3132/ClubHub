package pro290.clubhub;

import java.util.List;
import java.util.UUID;

import org.springframework.data.jpa.repository.JpaRepository;

public interface ClubRepository extends JpaRepository<Club, UUID> { // second param used to be long when Id was long,
                                                                      // see Food class

    //public List<Food> findByTitleContainingOrDescriptionContaining(String txt, String txt2);

    //List<Food> findByTitleContainingIgnoreCase(String title);

    //Custom query to find foods in foodItem
    List<Club> findByClubNameContainingIgnoreCase(String clubName);
}