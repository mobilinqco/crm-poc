using System;
using System.ComponentModel;
using ACRM.mobile.Domain.JsonUtils;
using Newtonsoft.Json;

namespace ACRM.mobile.Domain.Configuration.DataModel
{
    [JsonConverter(typeof(JsonArrayToObjectConverter<ServerLanguage>))]
    public class ServerLanguage: INotifyPropertyChanged
    {
        [JsonArrayIndex(0)]
        public string Code { get; set; }
        [JsonArrayIndex(1)]
        public string Name { get; set; }
        [JsonArrayIndex(2)]
        public int OrderLangId { get; set; }
        [JsonArrayIndex(3)]
        public int LangId { get; set; }
        [JsonArrayIndex(4)]
        public string ImageName { get; set; }

        [JsonIgnore]
        private bool _isSelected;
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged("IsSelected");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}
