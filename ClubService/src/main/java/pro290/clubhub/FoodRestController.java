package pro290.clubhub;
import java.time.LocalDate;
import java.util.UUID;
import java.util.List;
import java.util.NoSuchElementException;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.cglib.core.Local;
import org.springframework.http.HttpStatus;
import org.springframework.web.bind.annotation.*;

@RestController
@RequestMapping("/api/foods")
public class FoodRestController {
    @Autowired
    private FoodRepository foodsRepo;

    @GetMapping(path="test")
    @ResponseStatus(code = HttpStatus.OK)
    public String test() {
        return "hello";
    }

    @GetMapping(path="")
    @ResponseStatus(code = HttpStatus.OK)
    public List<Clu> findAllFoods() {
        return foodsRepo.findAll();
    }

    @GetMapping(path = "/{foodUUID}")
    @ResponseStatus(code = HttpStatus.OK)
    public Food getFood(@PathVariable UUID foodUUID) {
        return foodsRepo.findById(foodUUID).orElseThrow(() -> new NoSuchElementException());
    }

    public String getMethodName(@RequestParam String param) {
        return new String();
    }

    @PostMapping(path="")
    @ResponseStatus(code = HttpStatus.CREATED)
    public void createFood(@RequestBody Food food ) {
        food.setFoodUUID(UUID.randomUUID());
        food.setCreatedDate(LocalDate.now());
        foodsRepo.save(food);
    }

    @PostMapping(path = "/addfoods")  
    public void createMultipleFoods(@RequestBody List<Food> foods) {

        for (Food food : foods) {
            food.setFoodUUID(UUID.randomUUID());
            food.setCreatedDate(LocalDate.now());
            foodsRepo.save(food);
        }
    }

    @GetMapping(path = "/searchByMenuItem/{searchText}")
    @ResponseStatus(code = HttpStatus.OK)
    public List<Food> searchItems(@PathVariable(required = true) String searchText) {
        
        return foodsRepo.findByFoodNameContainingIgnoreCase(searchText);
    }

    @PutMapping(path = "/{foodUUID}")
    @ResponseStatus(HttpStatus.NO_CONTENT)
    public void updateFood(@PathVariable(required = true) UUID foodUUID, @RequestBody Food food) {

        if (!food.getFoodUUID().equals(foodUUID)) {
            throw new RuntimeException(
                    String.format("Path itemId %s did not match body itemId %s", foodUUID, food.getFoodUUID()));
        }

        foodsRepo.save(food);
    }

    @DeleteMapping(path = "/{foodUUID}")
    @ResponseStatus(HttpStatus.OK)
    public void DeleteItem(@PathVariable(required = true) UUID foodUUID) {
        foodsRepo.deleteById(foodUUID);
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
