using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public enum Role
    {
        guest,
        student,
        president,
        advisor,
        admin
    }

    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid userID { get; set; }

        [Required]
        [StringLength(20)]
        public string firstName { get; set; }

        [Required]
        [StringLength(40)]
        public string lastName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public string email { get; set; }

        [Required]
        [StringLength(15)]
        public string phoneNumber { get; set; }

        [Required]
        public byte[] password { get; set; }

        [Required]
        [Column("role")]
        public Role role { get; set; }

        public override string ToString()
        {
            return userID.ToString();
        }
    }
}
