
using System.ComponentModel.DataAnnotations;

public class Food
{
    [Key]
    public Guid FoodID { get; set; }

    [Required]
    public Guid OrderGuid { get; set; }

    [Required]
    public Order Order { get; set; }

    [Required]
    public String Title { get; set; }

    [Required]
    public String Description { get; set; }

    [Required]
    public double Price { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }
}