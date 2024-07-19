namespace MessageAPI.Models.Implementations
{
    public class ResponseData
    {
        public bool IsSuccess { get; set; } = true;

        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ResponseData<T> : ResponseData
    {
        public T? Data { get; set; }
    }
}
