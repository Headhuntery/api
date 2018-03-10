using Newtonsoft.Json;

namespace HeadHuntery.Api.Models
{
    public static class CommonResponse
    {
        public static string FromError(string error)
            => JsonConvert.SerializeObject(new ErrorResponseModel(error));
    }
}