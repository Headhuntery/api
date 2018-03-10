using Newtonsoft.Json;

namespace HeadHuntery.Api.Models
{
    public class ErrorResponseModel
    {
        [JsonProperty("error")]
        public string ErrorReason { get; }

        public ErrorResponseModel(string reason)
            => this.ErrorReason = reason;
    }
}