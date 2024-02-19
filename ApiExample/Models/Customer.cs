using System.ComponentModel.DataAnnotations;

namespace ApiExample.Models
{
    public class Customer
    {
        [StringLength(5, MinimumLength = 5)]
        public string CustomerID { get; set; }
        public string CompanyName { get; set; }
    }
}
