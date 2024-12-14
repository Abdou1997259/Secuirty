using System;

namespace Secuirty.Dtos
{
    public class AuthModel
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryDate { get; set; }
    }
}
