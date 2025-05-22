
using System.ComponentModel.DataAnnotations;

public class Book
{
    [Key]
    public Guid BookUUID { get; set; }

    [Required]
    public Guid OrderGuid { get; set; }

    [Required]
    public Order Order { get; set; }

    [Required]
    public String Title { get; set; }

    [Required]
    public String Author { get; set; }

    [Required]
    public String Description { get; set; }

    [Required]
    public float Price { get; set; }

    [Required]
    public DateTime PublishedDate { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; }
}