using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AlIssam.API.Dtos.Response
{
    public class ErrorResponse
    {
        public Errors Errors { get; set; }

    }
    public class Errors
    {
        public List<string> Ar { get; set; }
        public List<string> En { get; set; }
    }
}
