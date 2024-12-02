using Microsoft.AspNetCore.Identity;
using System;

namespace Secuirty.Models
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }
        public bool IsActive => RefreshTokenExpiryDate >= DateTime.UtcNow && RefreshToken != null;
        public bool IsRevoked => RefreshToken == null || !IsActive;

    }
}
