namespace APIEccomerce.Models
{
    public class Response<T>
    {
        public T? Value { get; set; }
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "";
    }
}
