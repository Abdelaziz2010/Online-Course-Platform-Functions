
using EduPlatform.Functions.Entities;

namespace EduPlatform.Functions.Email
{
    public interface IEmailNotification
    {
        Task SendVideoRequestConfirmation(VideoRequest videoRequest, string userFullName, string userEmailId);
    }
}
