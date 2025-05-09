namespace UserService.Models
{
    public class UserClub
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid userClubID { get; set; }

        [Required]
        public Guid userID { get; set; }

        [Required]
        public Guid clubID { get; set; }
    }
}