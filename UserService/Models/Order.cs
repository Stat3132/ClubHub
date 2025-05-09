using System.ComponentModel.DataAnnotations;

public class Order
{
    [Key]
    public Guid OrderGuid { get; set; }

    [Required]
    public Guid UserGuid { get; set; }

    [Required]
    public User User { get; set; }

    [Required]
    public Guid BasketGuid { get; set; }
    
    [Required]
    public DateTime CreatedDate { get; set; }

    [Required]
    public List<Food>? Foods { get; set; }

}