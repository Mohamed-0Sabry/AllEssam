using AlIssam.API.Common;

namespace AlIssam.API.Services.InterFaces
{
    public interface IEmailService
    {
        public Task SendEmail(Message email);
    }
}
