using System.ComponentModel.DataAnnotations;

public class UserDTO
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public Role UsersRole { get; set; }
}
