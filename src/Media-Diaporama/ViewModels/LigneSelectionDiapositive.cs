namespace PowerPwerPoint.Media.ViewModels;

/// <summary>Ligne cochable représentant une diapositive dans les boîtes de dialogue de sélection.</summary>
public sealed class LigneSelectionDiapositive : ViewModelBase
{
    private bool _selectionnee;

    public LigneSelectionDiapositive(string id, string libelle, bool selectionnee)
    {
        Id = id;
        Libelle = libelle;
        _selectionnee = selectionnee;
    }

    public string Id { get; }

    public string Libelle { get; }

    public bool Selectionnee
    {
        get => _selectionnee;
        set => SetProperty(ref _selectionnee, value);
    }
}
