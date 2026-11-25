using System.ComponentModel;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>Ligne du volet Animation (numéro, effet, cible, déclencheur).</summary>
public sealed class AnimationItemViewModel : ViewModelBase, IDisposable
{
    private int _numero;
    private string _nomCible = string.Empty;

    public AnimationItemViewModel(AnimationEffet effet, int numero, string nomCible)
    {
        Effet = effet;
        _numero = numero;
        _nomCible = nomCible;
        Effet.PropertyChanged += SurEffetModifie;
    }

    public AnimationEffet Effet { get; }

    public int Numero
    {
        get => _numero;
        private set => SetProperty(ref _numero, value);
    }

    public string NomCible
    {
        get => _nomCible;
        private set => SetProperty(ref _nomCible, value);
    }

    public string Resume => $"{Effet.Nom} — {NomCible}";

    public string Detail
        => $"{LibellesEnum.LibelleCategorieAnimation(Effet.Categorie)} · {AnimationCatalogue.NomDeclencheur(Effet.Declencheur)} · {Effet.Duree:0.#} s";

    public void MettreAJour(int numero, string nomCible)
    {
        Numero = numero;
        NomCible = nomCible;
        Notifier(nameof(Resume));
        Notifier(nameof(Detail));
    }

    private void SurEffetModifie(object? sender, PropertyChangedEventArgs e)
    {
        Notifier(nameof(Resume));
        Notifier(nameof(Detail));
    }

    public void Dispose() => Effet.PropertyChanged -= SurEffetModifie;
}
