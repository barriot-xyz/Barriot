using Barriot.Http.Json;
using Newtonsoft.Json;

namespace Barriot.Application.API
{
    public class TranslateClient : ITranslateClient
    {
        private readonly HttpClient _httpClient;

        public TranslateClient(HttpClient client)
            => _httpClient = client;

        /// <inheritdoc/>
        public async Task<List<Language>> GetSupportedLanguagesAsync()
            => JsonConvert.DeserializeObject<List<Language>>(await _httpClient.GetStringAsync("/languages"))
            ?? new();

        /// <inheritdoc/>
        public async Task<string> TranslateAsync(Action<TranslationRequest> action)
        {
            var post = new TranslationRequest();
            action(post);

            var content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                { "q", post.Text },
                { "source", post.Source.ToString() },
                { "target", post.Target.ToString() },
                { "api_key", post.ApiKey }
            });
            var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/translate")
            {
                Content = content
            });
            if (response.IsSuccessStatusCode)
            {
                var result = JsonConvert.DeserializeObject<Translation>(await response.Content.ReadAsStringAsync());
                return result?.TranslatedText
                    ?? string.Empty;
            }
            return string.Empty;
        }
    }
}
