using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Facepunch8.ViewModel
{
    class SettingViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private ObservableCollection<string> _imageModeChoices = new ObservableCollection<string>();
        private ObservableCollection<string> _themeChoices = new ObservableCollection<string>();
        private ObservableCollection<string> _headerThemeChoices = new ObservableCollection<string>();

        public ObservableCollection<string> ImageModeChoices { get { return _imageModeChoices; } }
        public ObservableCollection<string> ThemeChoices { get { return _themeChoices; } }
        public ObservableCollection<string> HeaderThemeChoices { get { return _headerThemeChoices; } }
        public object ImageModeSelection { get; set; }

        public SettingViewModel()
        {
            _imageModeChoices.Add("Do Not Display Images");
            _imageModeChoices.Add("Display Images");
            _themeChoices.Add("System");
            _themeChoices.Add("Dark");
            _themeChoices.Add("Light");

            _headerThemeChoices.Add("System");
            _headerThemeChoices.Add("Dark");
            _headerThemeChoices.Add("Light");
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
