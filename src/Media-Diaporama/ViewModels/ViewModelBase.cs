using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerPwerPoint.Media.ViewModels;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool SetProperty<T>(ref T champ, T valeur, [CallerMemberName] string? nom = null)
    {
        if (EqualityComparer<T>.Default.Equals(champ, valeur))
        {
            return false;
        }

        champ = valeur;
        Notifier(nom);
        return true;
    }

    protected void Notifier([CallerMemberName] string? nom = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nom));
}
