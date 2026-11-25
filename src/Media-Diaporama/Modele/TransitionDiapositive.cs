namespace PowerPwerPoint.Media.Modele;

/// <summary>Effets de transition disponibles dans la galerie.</summary>
public enum TransitionEffet
{
    Aucune,
    Fondu,
    FonduEnchaine,
    Poussee,
    Balayage,
    Volet,
    Diapositive,
    Fractionnement,
    Ouverture,
    Fermeture,
    Tourbillon,
    Zoom,
    Cercle
}

/// <summary>Sens / option d'effet de la transition (d'où arrive la diapositive).</summary>
public enum SensTransition
{
    Gauche,
    Droite,
    Haut,
    Bas,
    Centre
}

/// <summary>Transition appliquée au passage vers une diapositive.</summary>
public sealed class TransitionDiapositive : ModeleObservable
{
    private TransitionEffet _effet = TransitionEffet.Aucune;
    private SensTransition _sens = SensTransition.Droite;
    private double _duree = 1.0;
    private bool _avanceAutomatique;
    private double _delaiAvance = 5.0;
    private string? _son;
    private bool _lireEnBoucleSon;

    public TransitionEffet Effet
    {
        get => _effet;
        set => Set(ref _effet, value);
    }

    public SensTransition Sens
    {
        get => _sens;
        set => Set(ref _sens, value);
    }

    /// <summary>Durée en secondes.</summary>
    public double Duree
    {
        get => _duree;
        set => Set(ref _duree, Math.Clamp(value, 0.1, 10));
    }

    public bool AvanceAutomatique
    {
        get => _avanceAutomatique;
        set => Set(ref _avanceAutomatique, value);
    }

    public double DelaiAvance
    {
        get => _delaiAvance;
        set => Set(ref _delaiAvance, Math.Clamp(value, 0, 3600));
    }

    /// <summary>Chemin du son de transition (WAV).</summary>
    public string? Son
    {
        get => _son;
        set => Set(ref _son, string.IsNullOrWhiteSpace(value) ? null : value);
    }

    public bool LireEnBoucleSon
    {
        get => _lireEnBoucleSon;
        set => Set(ref _lireEnBoucleSon, value);
    }

    public TransitionDiapositive Copier()
        => new()
        {
            Effet = Effet,
            Sens = Sens,
            Duree = Duree,
            AvanceAutomatique = AvanceAutomatique,
            DelaiAvance = DelaiAvance,
            Son = Son,
            LireEnBoucleSon = LireEnBoucleSon
        };

    public override string ToString() => TransitionCatalogue.NomDe(Effet);
}

/// <summary>Descriptif d'une entrée de la galerie de transitions.</summary>
public sealed record EntreeTransition(TransitionEffet Effet, string Nom, string Glyphe, string Description);

public static class TransitionCatalogue
{
    public static IReadOnlyList<EntreeTransition> Toutes { get; } = new List<EntreeTransition>
    {
        new(TransitionEffet.Aucune, "Aucune", "\uE894", "Aucun effet, passage instantané."),
        new(TransitionEffet.Fondu, "Fondu", "\uE8A7", "La diapositive apparaît en fondu."),
        new(TransitionEffet.FonduEnchaine, "Fondu enchaîné", "\uE8A7", "Fondu croisé entre la diapositive précédente et la nouvelle."),
        new(TransitionEffet.Poussee, "Poussée", "\uE8AB", "La nouvelle diapositive pousse la précédente."),
        new(TransitionEffet.Balayage, "Balayage", "\uE8AC", "La diapositive se dévoile progressivement dans le sens choisi."),
        new(TransitionEffet.Volet, "Volet", "\uE8A0", "La diapositive se dévoile depuis le centre."),
        new(TransitionEffet.Diapositive, "Diapositive", "\uE8B0", "Glissement de type diapositive, la précédente recule."),
        new(TransitionEffet.Fractionnement, "Fractionnement", "\uE950", "La diapositive se dévoile depuis le milieu, haut et bas."),
        new(TransitionEffet.Ouverture, "Ouverture", "\uE8A3", "La diapositive s'ouvre depuis le centre."),
        new(TransitionEffet.Fermeture, "Fermeture", "\uE8A4", "La diapositive se referme vers son centre."),
        new(TransitionEffet.Tourbillon, "Tourbillon", "\uE895", "Rotation et zoom d'entrée."),
        new(TransitionEffet.Zoom, "Zoom", "\uE8A6", "La diapositive arrive en zoom, la précédente s'éloigne."),
        new(TransitionEffet.Cercle, "Cercle", "\uEA3A", "Révélation circulaire depuis le centre.")
    };

    public static string NomDe(TransitionEffet effet)
        => Toutes.FirstOrDefault(e => e.Effet == effet)?.Nom ?? effet.ToString();

    public static EntreeTransition Obtenir(TransitionEffet effet)
        => Toutes.First(e => e.Effet == effet);

    /// <summary>Vrai si l'effet accepte une option de sens.</summary>
    public static bool AccepteSens(TransitionEffet effet)
        => effet is TransitionEffet.Poussee or TransitionEffet.Balayage or TransitionEffet.Diapositive;
}
