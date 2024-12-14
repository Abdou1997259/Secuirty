namespace Secuirty.Dtos
{
    public class LoginWithThirdPartyModel
    {
        public string AccessToken { get; set; }
        public string Provider { get; set; }
        public string UserId { get; set; }

    }
}
