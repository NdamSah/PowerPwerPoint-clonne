namespace PowerPwerPoint.Media.Modele;

/// <summary>
/// Élément visuel minimal d'une diapositive (zone de texte ou forme simple).
/// Le rendu réel appartient au module éditeur / insertion : ce type sert de cible stable
/// (identifiant) pour les animations et de contenu de démonstration.
/// </summary>
public sealed class ElementDiapositive : ModeleObservable
{
    private string _nom = "Élément";
    private string _texte = string.Empty;
    private double _x = 80;
    private double _y = 80;
    private double _largeur = 320;
    private double _hauteur = 120;
    private string _couleurFond = "#FF2F6FED";
    private string _couleurTexte = "#FFFFFFFF";
    private double _taillePolice = 20;
    private bool _estZoneTexte;
    private string _alignementTexte = "Center";

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Nom
    {
        get => _nom;
        set => Set(ref _nom, value);
    }

    public string Texte
    {
        get => _texte;
        set => Set(ref _texte, value);
    }

    public double X
    {
        get => _x;
        set => Set(ref _x, value);
    }

    public double Y
    {
        get => _y;
        set => Set(ref _y, value);
    }

    public double Largeur
    {
        get => _largeur;
        set => Set(ref _largeur, value);
    }

    public double Hauteur
    {
        get => _hauteur;
        set => Set(ref _hauteur, value);
    }

    public string CouleurFond
    {
        get => _couleurFond;
        set => Set(ref _couleurFond, value);
    }

    public string CouleurTexte
    {
        get => _couleurTexte;
        set => Set(ref _couleurTexte, value);
    }

    public double TaillePolice
    {
        get => _taillePolice;
        set => Set(ref _taillePolice, value);
    }

    public bool EstZoneTexte
    {
        get => _estZoneTexte;
        set => Set(ref _estZoneTexte, value);
    }

    public string AlignementTexte
    {
        get => _alignementTexte;
        set => Set(ref _alignementTexte, value);
    }
}
