using System.ComponentModel.DataAnnotations;

namespace QuizApp.Models
{
    public class User
    {
       
            [Key]
            public int Id { get; set; }
            [Required]

            public string Name { get; set; }
            [Required]

            public string Email { get; set; }
            [Required]
            public string InstituteName { get; set; }
            public string Password { get; set; }
            public User() { }
        
    }
}
