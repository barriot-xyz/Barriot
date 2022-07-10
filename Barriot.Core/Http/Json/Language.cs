using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Barriot.Http.Json
{
    public class Language
    {
        [JsonProperty("code")]
        public string Code { get; set; } = "";

        [JsonProperty("name")]
        public string Name { get; set; } = "";
    }
}
