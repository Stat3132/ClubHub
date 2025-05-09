using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid userID { get; set; }

        [Required]
        [StringLength(20)]
        public required string firstName { get; set; }

        [Required]
        [StringLength(40)]
        public required string lastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public required string email { get; set; }

        [Required]
        [StringLength(15)]
        public required string phoneNumber { get; set; }

        [Required]
        public required byte[] password { get; set; }

        [Required]
        [Column("role")]
        public required Role role { get; set; }

        public override string ToString()
        {
            return userID.ToString();
        }
    }
}
