﻿using EduPlatform.Functions.Entities;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace EduPlatform.Functions.Email
{
    // this class is responsible for sending email notifications related to video requests
    public class EmailNotification : IEmailNotification
    {
        private readonly IConfiguration _configuration;

        public EmailNotification(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendVideoRequestConfirmation(VideoRequest videoRequest, string userFullName, string userEmailId)
        {
            var apiKey = _configuration["SENDGRID_API_KEY"];
            var from = new EmailAddress(_configuration["From"]);
            
            //var userFullName = $"{videoRequest.User.LastName},{videoRequest.User.FirstName}";
            //var to = new EmailAddress(videoRequest.User.Email, userFullName);
            
            var to = new EmailAddress(userEmailId, userFullName);
            
            //var cc = new EmailAddress(_configuration["From"], "EduPlatfrom App");

            /*
              <option value="Requested">Requested</option>
              <option value="Reviewed">Reviewed</option>
              <option value="Pending Clarification">Pending Clarification</option>
              <option value="InProcess">In Process</option>
              <option value="Completed">Completed</option>
              <option value="Published">Published</option>
             */

            var isRequestNew = videoRequest.RequestStatus.Equals("Requested");

            var sendGridMessage = new SendGridMessage()
            {
                From = from,
                Subject = isRequestNew
                    ? "New Video Request Received"
                : "Video Request Status Update"
            };

            sendGridMessage.AddContent(MimeType.Html, GetEmailBody(videoRequest, userFullName));
            sendGridMessage.AddTo(to);

            // Only add "CC" if it's not the same as the "To" address,
            // because SendGrid does not allow sending to the same address in "To" and "CC".
            //if (!string.Equals(userEmailId, fromEmail, StringComparison.OrdinalIgnoreCase))
            //{
            //    sendGridMessage.AddCc(cc);
            //}

            Console.WriteLine($"Sending email with payload: \n{sendGridMessage.Serialize()}");

            var response = await new SendGridClient(apiKey).SendEmailAsync(sendGridMessage).ConfigureAwait(false);

            Console.WriteLine($"Response: {response.StatusCode}");
            
            Console.WriteLine(response.Headers);
        }

        private string GetEmailBody(VideoRequest videoRequest, string userFullName)
        {
            // Example HTML email body with detailed information
            var statusDescription = videoRequest.RequestStatus switch
            {
                "Requested" => "Your video request has been received and is under review.",
                "Reviewed" => "Your video request has been reviewed.",
                "Pending Clarification" => "We need more information about your video request.",
                "InProcess" => "Your video request is being processed.",
                "Completed" => "Your video request has been completed.",
                "Published" => "Your video request has been published.",
                _ => "Your video request status is unknown."
            };

            var videoUrls = !string.IsNullOrEmpty(videoRequest.VideoUrls)
                ? $"<p><strong>Video URLs:</strong> {videoRequest.VideoUrls}</p>"
                :  "<p><strong>Video URLs:</strong> Not available</p>";

            var htmlContent = $@"
                    <html>
                    <body>
                        <h2>Hello {userFullName},</h2>
                        <p>{statusDescription}</p>
                        <p>Here are the details of your video request:</p>
                        <ul>
                            <li><strong>Topic:</strong> {videoRequest.Topic}</li>
                            <li><strong>Sub-Topic:</strong> {videoRequest.SubTopic}</li>
                            <li><strong>Short Title:</strong> {videoRequest.ShortTitle}</li>
                            <li><strong>Description:</strong> {videoRequest.RequestDescription}</li>
                            <li><strong>Response:</strong> {(string.IsNullOrEmpty(videoRequest.Response) ? "No response yet" : videoRequest.Response)}</li>
                            {videoUrls}
                        </ul>
                        <p>Thank you for your request!</p>
                    </body>
                    </html>";

            return htmlContent;
        }
    }
}
