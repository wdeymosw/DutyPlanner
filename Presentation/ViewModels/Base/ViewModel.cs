using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Markup;

namespace DutyPlanner.Presentation.ViewModels
{
    public abstract class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;



        protected virtual void OnPropertyChanged([CallerMemberName] string? PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected virtual bool Set<T>(ref T field, T value, [CallerMemberName] string? PropertyName = null)
        {
            if (Equals(field, value)) return false;

            field = value;
            OnPropertyChanged(PropertyName);
            return true;

        }

        public XmlLanguage CalendarLanguage => XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag);

        protected void OnLanguageChanged()
        {
            OnPropertyChanged(nameof(CalendarLanguage));
        }

       
    }
}
