namespace MessageAPI.Models.Implementations
{
    public class RegistrationResponse
    {
        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public InstaData? Data { get; set; }

        public class InstaData
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

    }
}
