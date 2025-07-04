﻿
namespace EduPlatform.Functions.DTOs
{
    public class ProfileDTO
    {
        public int UserId { get; set; }

        public string DisplayName { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string AdObjId { get; set; } = null!;

        public List<string> Roles { get; set; } = null!;
    }
}
