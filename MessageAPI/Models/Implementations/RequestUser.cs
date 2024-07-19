using InstagramApiSharp.Classes;

namespace MessageAPI.Models.Implementations
{
    public class RequestUser
    {
        public InstaData Instagram { get; set; } = null!;

        public MailData Mail { get; set; } = null!;



        public class InstaData
        {
            public required string Username { get; set; }
            public required string Password { get; set; }
            public StateData? State { get; set; }
        }


        public class MailData
        {
            public required string Login { get; set; }
            public required string Password { get; set; }
        }
    }
}
