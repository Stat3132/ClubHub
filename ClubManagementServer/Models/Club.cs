using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClubManagementServer.Models
{
    public class Club
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid clubID { get; set; }

        [Required]
        [StringLength(50)]
        public required string clubName { get; set; }

        [Required]
        public required string clubDeclaration { get; set; }

        [Required]
        [StringLength(60)]
        public required string presidentName { get; set; }

        [Required]
        public required Guid presidentID { get; set; }

        [Required]
        public required Guid advisorID { get; set; }

        public override string ToString()
        {
            return $"{clubName} (President: {presidentName})";
        }
    }
}

