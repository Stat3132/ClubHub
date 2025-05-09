using System.ComponentModel.DataAnnotations;

namespace UserService.Models
{
    public class UserDTO
    {
        public UUID userID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public Role UsersRole { get; set; }
    }
}