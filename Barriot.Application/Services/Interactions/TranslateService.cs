using Barriot.Application.API;
using Barriot.Http.Json;

namespace Barriot.Application.Services
{
    public class TranslateService : IService
    {
        private readonly ITranslateClient _translator;

        private List<IEnumerable<Language>>? _languages;

        private DateTime _lastCheck;

        private static readonly char[] _alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

        public TranslateService(ITranslateClient client)
        {
            _lastCheck = DateTime.UtcNow;
            _translator = client;
        }

        /// <summary>
        ///     Get all supported languages.
        /// </summary>
        /// <returns></returns>
        public async Task<List<IEnumerable<Language>>> GetSupportedLanguagesAsync()
        {
            if (_languages is null || _lastCheck > DateTime.UtcNow.AddDays(1))
            {
                _lastCheck = DateTime.UtcNow;

                var languages = await _translator.GetSupportedLanguagesAsync();

                List<IEnumerable<Language>> data = new();

                var cursor = _alphabet.Length / 2;
                for (int i = 0; i < _alphabet.Length; i += cursor)
                    data.Add(languages.OrderBy(x => x.Name[0]).ToList().GetRange(i, cursor));

                _languages = data;

                return data;
            }
            else
                return _languages;
        }

        /// <summary>
        ///     Translates text to the target language.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public async Task<string> TranslateAsync(string target, string text)
            => await _translator.TranslateAsync(x =>
            {
                x.ApiKey = "";
                x.Source = "auto";
                x.Target = target;
                x.Text = text;
            });
    }
}
