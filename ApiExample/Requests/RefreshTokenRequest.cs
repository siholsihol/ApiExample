using System.ComponentModel.DataAnnotations;

namespace ApiExample.Requests
{
    public class RefreshTokenRequest
    {
        [Required]
        public string Token { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }
}
