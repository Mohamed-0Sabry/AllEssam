namespace AlIssam.API.Common
{
    public class Message(string to, string email, string subject, string content)
    {
        public string To { get; set; } = to;
        public string Email { get; } = email;
        public string Subject { get; set; } = subject;
        public string Content { get; set; } = content;
    }
}
