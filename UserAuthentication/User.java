package UserAuthentication;

public class User {
    String firstName;
    String lastName;
    String email;
    String phoneNumber;
    Role usersRole;
    public String getFirstName() {
        return firstName;
    }
    public void setFirstName(String firstName) {
        this.firstName = firstName;
    }
    public String getLastName() {
        return lastName;
    }
    public void setLastName(String lastName) {
        this.lastName = lastName;
    }
    public String getEmail() {
        return email;
    }
    public void setEmail(String email) {
        this.email = email;
    }
    public String getPhoneNumber() {
        return phoneNumber;
    }
    public void setPhoneNumber(String phoneNumber) {
        this.phoneNumber = phoneNumber;
    }
    public Role getUsersRole() {
        return usersRole;
    }
    public void setUsersRole(Role usersRole) {
        this.usersRole = usersRole;
    }
    
}
