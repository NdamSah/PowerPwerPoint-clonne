using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerPwerPoint.Media.Modele;

/// <summary>
/// Base des modèles du module : implémente <see cref="INotifyPropertyChanged"/> afin que les
/// panneaux WPF et le rendu du diaporama se mettent à jour sans code de synchronisation manuel.
/// </summary>
public abstract class ModeleObservable : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected bool Set<T>(ref T champ, T valeur, [CallerMemberName] string? nom = null)
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
