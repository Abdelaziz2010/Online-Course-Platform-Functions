using EduPlatform.Functions.DTOs;
using EduPlatform.Functions.Email;
using EduPlatform.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Sql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace EduPlatform.Functions;

// The VideoRequestTrigger function is triggered by changes in the VideoRequests table in the PlatformDB database.
// It processes the changes, sends email notifications to users about their video requests, and provides an HTTP endpoint to send acknowledgment emails.
// What this function does:
// 1. It listens for changes in the VideoRequests table and processes each change.
// 2. For each change, it retrieves the user information and sends a confirmation email about the video request.
// 3. It provides an HTTP endpoint to send acknowledgment emails to users based on a video request ID.

public class VideoRequestTrigger
{
    private readonly ILogger _logger;
    private readonly IEmailNotification _emailNotification;
    private readonly IConfiguration _configuration;

    public VideoRequestTrigger(ILoggerFactory loggerFactory, IEmailNotification emailNotification,
        IConfiguration configuration)
    {
        _logger = loggerFactory.CreateLogger<VideoRequestTrigger>();
        _emailNotification = emailNotification;
        _configuration = configuration;
    }


    // Visit https://aka.ms/sqltrigger to learn how to use this trigger binding
    [Function("VideoRequestTrigger")]
    public async Task RunAsync(
        [SqlTrigger("[dbo].[VideoRequests]", "PlatformDB")] IReadOnlyList<SqlChange<VideoRequest>> videoRequests,
        FunctionContext context)
    {
        _logger.LogInformation("SQL Changes: " + JsonSerializer.Serialize(videoRequests));

        var logger = context.GetLogger("VideoRequestTrigger");
        
        _logger.LogInformation("C# HTTP trigger with SQL Output Binding function processed a request.");

        var platformDbContext = GetDbContext();

        foreach (SqlChange<VideoRequest> change in videoRequests)
        {
            VideoRequest videoRequest = change.Item;

            var userInfo = await platformDbContext.UserProfiles.FirstOrDefaultAsync(f => f.UserId == videoRequest.UserId);
            
            var userFullName = $"{userInfo.LastName},{userInfo.FirstName}";

            await _emailNotification.SendVideoRequestConfirmation(videoRequest, userFullName, userInfo.Email);
            
            logger.LogInformation($"Change operation: {change.Operation}");
        }
    }


    [Function("SendVideoRequestAckEmailToUser")]
    public async Task<IActionResult> SendVideoRequestAckEmailToUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "SendVideoRequestAckEmailToUser")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed SendVideoRequestAckEmailToUser request.");
       
        VideoRequestDTO model = new VideoRequestDTO();

        try
        {
            var platformDbContext = GetDbContext();
            
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Invalid request body. Please provide a valid Profile. Body cannot be empty");
            }

            //we will parse our request body to this model
            model = JsonSerializer.Deserialize<VideoRequestDTO>(requestBody);

            if (model == null || model.VideoRequestId < 1)
            {
                return new BadRequestObjectResult("Invalid request body. Please provide a valid videoRequest.");
            }

            var videoRequest = await platformDbContext.VideoRequests.Include(i => i.User).FirstOrDefaultAsync(f => f.VideoRequestId == model.VideoRequestId);
            
            var userFullName = $"{videoRequest.User.LastName},{videoRequest.User.FirstName}";

            if (videoRequest != null)
            {
                await _emailNotification.SendVideoRequestConfirmation(videoRequest, userFullName, videoRequest.User.Email);
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }
        return new OkObjectResult(model);
    }


    private EduPlatformDbContext GetDbContext()
    {
        //Read connection string value from our local settings from project.
        string connectionString = _configuration.GetConnectionString("PlatformDB");

        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogError("The connection string is null or empty.");
            throw new InvalidOperationException("The connection string has not been initialized.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<EduPlatformDbContext>();
        
        optionsBuilder.UseSqlServer(connectionString);

        var context = new EduPlatformDbContext(optionsBuilder.Options);
        
        return context;
    }
}