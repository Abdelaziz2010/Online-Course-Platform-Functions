using EduPlatform.Functions.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using EduPlatform.Functions.DTOs;
using System.Text.Json;

namespace EduPlatform.Functions;

// The function is triggered by an HTTP POST request and returns the updated user profile with roles.
// What this function does:
// 1. It updates the user profile in the database.
// 2. If the user profile does not exist, it creates a new one.
// 3. It retrieves the user's roles and returns them in the response.
public class UserProfileFunction
{
    private readonly ILogger<UserProfileFunction> _logger;
    private readonly IConfiguration _configuration;

    public UserProfileFunction(ILogger<UserProfileFunction> logger,IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    [Function("UpdateUserProfile")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UpdateUserProfile")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed UpdateUserProfile request.");

        var userProfileResponse = new ProfileDTO();

        try
        {
            //Read connection string value from our local settings from project.
            string connectionString = _configuration.GetConnectionString("PlatformDB")!;

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("The connection string is null or empty.");
                throw new InvalidOperationException("The connection string has not been initialized.");
            }

            //_logger.LogInformation($"DbContext: {connectionString}");

            var optionsBuilder = new DbContextOptionsBuilder<EduPlatformDbContext>();
            
            optionsBuilder.UseSqlServer(connectionString);
             
            var _context = new EduPlatformDbContext(optionsBuilder.Options);   

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            if (string.IsNullOrEmpty(requestBody))
            {
                return new BadRequestObjectResult("Invalid request body. Please provide a valid Profile. Body cannot be empty");
            }

            //we will parse our request body to this model
            ProfileDTO? profile = JsonSerializer.Deserialize<ProfileDTO>(requestBody);

            if (profile == null)
            {
                return new BadRequestObjectResult("Invalid request body. Please provide a valid Profile.");
            }

            string adObjId = profile.AdObjId;

            if (string.IsNullOrEmpty(adObjId))
            {
                return new BadRequestObjectResult("Please provide AdObjId in the request body.");
            }

            // Check if UserProfile with given AdObjId exists
            var userProfile = await _context.UserProfiles.Include(d => d.UserRoles).FirstOrDefaultAsync(u => u.AdObjId == adObjId);
           
            var role = await _context.Roles.FirstOrDefaultAsync(f => f.RoleName == "Student");

            if (userProfile == null)
            {
                // If not exists, create a new UserProfile
                userProfile = new UserProfile
                {
                    AdObjId = adObjId,
                    DisplayName = profile.DisplayName,
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    Email = profile.Email,
                    UserRoles = new List<UserRole>() 
                    {
                        new UserRole() { SmartAppId = 1, RoleId = role.RoleId}
                    }
                };

                _context.UserProfiles.Add(userProfile);
            }
            else
            {
                // If exists, update the existing UserProfile
                // You can update other properties here if needed
                userProfile.DisplayName = profile.DisplayName;
                userProfile.FirstName = profile.FirstName;
                userProfile.LastName = profile.LastName;
                userProfile.Email = profile.Email;
            }

            await _context.SaveChangesAsync();

            //get user's roles here
            var userRoles = await _context.UserRoles.Include(i => i.Role)
                .Where(u => u.UserId == userProfile.UserId).Select(s => s.Role.RoleName).ToListAsync();

            userProfileResponse = new ProfileDTO()
            {
                UserId = userProfile.UserId,
                AdObjId = userProfile.AdObjId,
                DisplayName = userProfile.DisplayName,
                Email = userProfile.Email,
                FirstName = userProfile.FirstName,
                LastName = userProfile.LastName,
                Roles = userRoles == null ? new List<string>() : userRoles
            };

        }
        catch (Exception ex)
        {
            _logger.LogError(ex.ToString());
        }

        return new OkObjectResult(userProfileResponse);
    }
}