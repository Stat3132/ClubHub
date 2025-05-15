using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserDTO
    {
        public required Guid userID { get; set; }
        public required string firstName { get; set; }
        public required string lastName { get; set; }
        public required string email { get; set; }
        public required string phoneNumber { get; set; }
        public required string password { get; set; }
        public required Role role { get; set; }
    }
}