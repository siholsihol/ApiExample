using System.ComponentModel.DataAnnotations;

namespace ApiExample.Requests
{
    public class LoginRequest
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
