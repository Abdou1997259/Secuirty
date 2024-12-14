namespace Secuirty.Dtos
{
    public class RegisterWithThirdPartyModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string AccessToken { get; set; }
        public string UserId { get; set; }
        public string Provider { get; set; }

    }
}
