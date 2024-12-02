namespace Secuirty.Helper
{

    public class Jwt
    {
        public string Key { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public double DurationInMin { get; set; }
    }

}
