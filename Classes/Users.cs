 public class Users
 {
     public int Id { get; set; }
     public string UserName { get; set; }
     public string Email { get; set; }
     public string Phone { get; set; }
     public string Password { get; set; }
     public DateTime Birthday { get; set; }
     public string? AvatarUrl { get; set; }
     public string? PlaylistId { get; set; }

     public override string ToString()
     {
         return $"Id: {Id}, UserName: {UserName}, Email: {Email}, Phone: {Phone}, Password: {Password},  Birthday: {Birthday}, AvatarUrl: {AvatarUrl}, PlaylistId: {PlaylistId}";
     }
 }