using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ACRM.mobile.Domain.Configuration.UserInterface
{
    public class ListUIItem : INotifyPropertyChanged
    {
        public string Configuration { get; set; }
        public string DisplayValue { get; set; }
        public string ColorString { get; set; }
        public int Id { get; set; } = -1;
        private bool _selected = false;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged();
            }
        }
        public string ExtKey { get; set; }

        public string ImageName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
