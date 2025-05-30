using Newtonsoft.Json;

namespace EduPlatform.Functions.DTOs
{
    public class ResponseContentDTO
    {
        public const string ApiVersion = "1.0.0";

        public ResponseContentDTO()
        {
            this.version = ResponseContentDTO.ApiVersion;
            this.action = "Continue";
        }

        public ResponseContentDTO(string action, string userMessage)
        {
            this.version = ApiVersion;
            this.action = action;
            this.userMessage = userMessage;
            if (action == "ValidationError")
            {
                this.status = "400";
            }
        }

        public string version { get; }

        public string action { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string userMessage { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string status { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string jobTitle { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string extension_userRoles { get; set; }


        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string extension_userId { get; set; }
    }
}
