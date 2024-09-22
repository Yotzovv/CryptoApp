using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace CryptoApp.Data.Models
{

    [Table("AspNetUsers")]
    public class AspNetUser : IdentityUser<Guid>
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public bool IsActive { get; set; } = true;
        
        public Portfolio Portfolio { get; set; }
    }
}