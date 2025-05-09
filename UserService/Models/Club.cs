using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    public class Club
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid clubID { get; set; }

        [Required]
        [StringLength(50)]
        public string clubName { get; set; }

        [Required]
        public string clubDeclaration { get; set; }

        [Required]
        [StringLength(60)]
        public string presidentName { get; set; }

        [Required]
        public Guid presidentID { get; set; }

        [Required]
        public Guid advisorID { get; set; }

        public override string ToString()
        {
            return $"{clubName} (President: {presidentName})";
        }
    }
}

