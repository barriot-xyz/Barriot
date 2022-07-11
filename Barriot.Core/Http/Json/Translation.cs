using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barriot.Http.Json
{
    public class Translation
    {
        [JsonProperty("translatedText")]
        public string TranslatedText { get; set; } = "";
    }
}
