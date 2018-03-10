using Newtonsoft.Json;

namespace HeadHuntery.Api.Models
{
    public class TokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; }

        public TokenResponse(string token, int expiresIn)
        {
            this.AccessToken = token;
            this.ExpiresIn = expiresIn;
        }
    }
}