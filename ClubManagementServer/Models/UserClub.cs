using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessageService.Models
{
    public class UserClub
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid userClubID { get; set; }

        [Required]
        public required Guid userID { get; set; }

        [Required]
        public required Guid clubID { get; set; }
    }
}