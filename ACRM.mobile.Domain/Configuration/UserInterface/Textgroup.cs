using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<Textgroup>))]
    public class Textgroup : BaseConfigUnit
    {
        [JsonArrayIndex(1)]
        [JsonConverter(typeof(JsonArrayToStringConverter<string>))]
        public string Texts { get; set; }

        private string[] _textsArray;
        
        public Textgroup()
        {
        }

        public string GetText(int textIndex)
        {
            string result = null;

            if (_textsArray == null)
            {
                //_textsArray = Texts.Split(",", StringSplitOptions.None);
                try
                {
                    _textsArray = JsonConvert.DeserializeObject<string[]>(Texts);
                }
                catch { }
            }

            if (_textsArray != null && textIndex < _textsArray.Length)
            {
                try
                {
                    var i = 0;
                    result = Regex.Replace(_textsArray[textIndex], "%@", $"{{{i++}}}");

                } catch
                {
                    result = _textsArray[textIndex];
                }
            }

            return result;
        }
    }
}
