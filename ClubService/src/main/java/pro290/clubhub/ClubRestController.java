package pro290.clubhub;
import java.util.List;
import java.util.NoSuchElementException;
import java.util.UUID;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.http.HttpStatus;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.ResponseStatus;
import org.springframework.web.bind.annotation.RestController;
//import org.springframework.security.access.prepost.PreAuthorize;


@RestController
@RequestMapping("/api/clubs")
public class ClubRestController {
    @Autowired
    private ClubRepository clubsRepo;
    
    @GetMapping(path="test")
    @ResponseStatus(code = HttpStatus.OK)
    public String test() {
        return "hello";
    }

    @GetMapping(path = "")
    @PreAuthorize("hasAnyRole('STUDENT', 'ADVISOR', 'ADMIN')")
    @ResponseStatus(code = HttpStatus.OK)
    public List<Club> findAllClubs() {
    return clubsRepo.findAll();
    }

    @GetMapping(path = "/{clubUUID}")
    @ResponseStatus(code = HttpStatus.OK)
    public Club getClub(@PathVariable UUID clubUUID) {
        return clubsRepo.findById(clubUUID).orElseThrow(() -> new NoSuchElementException());
    }

    public String getMethodName(@RequestParam String param) {
        return new String();
    }

    //TODO: Fix this part to get authorization working (spring-boot-starter-oauth2-jose)

    @PostMapping(path="")
    @ResponseStatus(code = HttpStatus.CREATED)
    @PreAuthorize("hasAnyRole('ADVISOR', 'ADMIN')")
    public void createFood(@RequestBody Club club ) {
        club.setClubID(UUID.randomUUID());
        //club.setCreatedDate(LocalDate.now()); TODO: MAKE IT SO THAT CLUBS HAVE CREATED DATES
        clubsRepo.save(club);
    }

    @PostMapping(path = "/addclubs")  
    public void createMultipleFoods(@RequestBody List<Club> clubs) {

        for (Club club : clubs) {
            club.setClubID(UUID.randomUUID());
            //club.setCreatedDate(LocalDate.now()); TODO: MAKE IT SO THAT CLUBS HAVE CREATED DATES
            clubsRepo.save(club);
        }
    }

    @GetMapping(path = "/searchByMenuItem/{searchText}")
    @ResponseStatus(code = HttpStatus.OK)
    public List<Club> searchItems(@PathVariable(required = true) String searchText) {
        
        return clubsRepo.findByClubNameContainingIgnoreCase(searchText);
    }

    @PutMapping(path = "/{clubUUID}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void updateFood(@PathVariable(required = true) UUID clubUUID, @RequestBody Club club) {

        if (!club.getClubID().equals(clubUUID)) {
            throw new RuntimeException(
                    String.format("Path itemId %s did not match body itemId %s", clubUUID, club.getClubID()));
        }
        clubsRepo.save(club);
    }

    @DeleteMapping(path = "/{clubUUID}")
    @ResponseStatus(HttpStatus.OK)
    public void DeleteItem(@PathVariable(required = true) UUID clubUUID) {
        clubsRepo.deleteById(clubUUID);
    }

    //UNUSED:

    // private static int createRandomIntBetween(int start, int end) {
    //     return start + (int) Math.round(Math.random() * (end - start));
    // }

    // private static LocalDate createRandomDate(int startYear, int endYear) {
    //     int day = createRandomIntBetween(1, 28);
    //     int month = createRandomIntBetween(1, 12);
    //     int year = createRandomIntBetween(startYear, endYear);
    //     return LocalDate.of(year, month, day);
    
}
