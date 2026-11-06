namespace DataWebService.Models
{
    public class APIError
    {
        public string Error { get; set; } = "";
        public string Message { get; set; } = "";
        public Dictionary<string, string>? Details { get; set; }
    }
}