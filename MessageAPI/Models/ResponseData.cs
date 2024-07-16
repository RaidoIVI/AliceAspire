namespace MessageAPI.Models
{
    public class ResponseData
    {
        public bool IsSuccess { get; set; } = true;

        public string ErrorMessage { get; set; } = string.Empty;
    }
}
