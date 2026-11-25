namespace PowerPwerPoint.Media.Modele;

public enum CategorieAnimation
{
    Entree,
    Emphase,
    Sortie,
    Trajectoire
}

public enum EffetAnimation
{
    // Entrée
    Apparition,
    FonduEntree,
    VolerEntree,
    ZoomEntree,
    BalayageEntree,
    RotationEntree,
    // Emphase
    Pulsation,
    Grossir,
    Retrecir,
    Pivot,
    Transparence,
    // Sortie
    Disparition,
    FonduSortie,
    VolerSortie,
    ZoomSortie,
    Reduire,
    // Trajectoires
    TrajectoireLigne,
    TrajectoireArc,
    TrajectoireCercle,
    TrajectoireDiagonale
}

public enum DeclencheurAnimation
{
    AuClic,
    AvecLaPrecedente,
    ApresLaPrecedente
}

/// <summary>Un effet d'animation appliqué à un élément d'une diapositive.</summary>
public sealed class AnimationEffet : ModeleObservable
{
    private string _cibleId = string.Empty;
    private EffetAnimation _effet = EffetAnimation.FonduEntree;
    private DeclencheurAnimation _declencheur = DeclencheurAnimation.AuClic;
    private double _duree = 0.5;
    private double _delai;
    private int _repetition;
    private SensTransition _sens = SensTransition.Gauche;
    private double _ampleur = 1.0;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string CibleId
    {
        get => _cibleId;
        set => Set(ref _cibleId, value);
    }

    public EffetAnimation Effet
    {
        get => _effet;
        set
        {
            if (Set(ref _effet, value))
            {
                Notifier(nameof(Nom));
                Notifier(nameof(Categorie));
            }
        }
    }

    public DeclencheurAnimation Declencheur
    {
        get => _declencheur;
        set => Set(ref _declencheur, value);
    }

    public double Duree
    {
        get => _duree;
        set => Set(ref _duree, Math.Clamp(value, 0.05, 30));
    }

    public double Delai
    {
        get => _delai;
        set => Set(ref _delai, Math.Clamp(value, 0, 30));
    }

    /// <summary>Nombre de répétitions ; 0 = une seule fois, -1 = jusqu'à la fin de la diapositive.</summary>
    public int Repetition
    {
        get => _repetition;
        set => Set(ref _repetition, Math.Clamp(value, -1, 20));
    }

    public SensTransition Sens
    {
        get => _sens;
        set => Set(ref _sens, value);
    }

    public double Ampleur
    {
        get => _ampleur;
        set => Set(ref _ampleur, Math.Clamp(value, 0.1, 5));
    }

    public CategorieAnimation Categorie => AnimationCatalogue.CategorieDe(Effet);

    public string Nom => AnimationCatalogue.NomDe(Effet);

    public AnimationEffet Copier()
        => new()
        {
            CibleId = CibleId,
            Effet = Effet,
            Declencheur = Declencheur,
            Duree = Duree,
            Delai = Delai,
            Repetition = Repetition,
            Sens = Sens,
            Ampleur = Ampleur
        };
}

public sealed record EntreeAnimation(EffetAnimation Effet, string Nom, string Glyphe, CategorieAnimation Categorie);

public static class AnimationCatalogue
{
    public static IReadOnlyList<EntreeAnimation> Toutes { get; } = new List<EntreeAnimation>
    {
        new(EffetAnimation.Apparition, "Apparition", "\uE8A7", CategorieAnimation.Entree),
        new(EffetAnimation.FonduEntree, "Fondu", "\uE8A7", CategorieAnimation.Entree),
        new(EffetAnimation.VolerEntree, "Voler", "\uE8AB", CategorieAnimation.Entree),
        new(EffetAnimation.ZoomEntree, "Zoom", "\uE8A6", CategorieAnimation.Entree),
        new(EffetAnimation.BalayageEntree, "Balayage", "\uE8AC", CategorieAnimation.Entree),
        new(EffetAnimation.RotationEntree, "Rotation", "\uE895", CategorieAnimation.Entree),

        new(EffetAnimation.Pulsation, "Pulsation", "\uE93E", CategorieAnimation.Emphase),
        new(EffetAnimation.Grossir, "Agrandir", "\uE8A9", CategorieAnimation.Emphase),
        new(EffetAnimation.Retrecir, "Réduire", "\uE8AA", CategorieAnimation.Emphase),
        new(EffetAnimation.Pivot, "Pivoter", "\uE895", CategorieAnimation.Emphase),
        new(EffetAnimation.Transparence, "Transparence", "\uE8D0", CategorieAnimation.Emphase),

        new(EffetAnimation.Disparition, "Disparaître", "\uE894", CategorieAnimation.Sortie),
        new(EffetAnimation.FonduSortie, "Fondu", "\uE8A7", CategorieAnimation.Sortie),
        new(EffetAnimation.VolerSortie, "Voler", "\uE8AB", CategorieAnimation.Sortie),
        new(EffetAnimation.ZoomSortie, "Zoom", "\uE8A6", CategorieAnimation.Sortie),
        new(EffetAnimation.Reduire, "Réduire", "\uE8AA", CategorieAnimation.Sortie),

        new(EffetAnimation.TrajectoireLigne, "Ligne", "\uE809", CategorieAnimation.Trajectoire),
        new(EffetAnimation.TrajectoireArc, "Arc", "\uE8B3", CategorieAnimation.Trajectoire),
        new(EffetAnimation.TrajectoireCercle, "Cercle", "\uEA3A", CategorieAnimation.Trajectoire),
        new(EffetAnimation.TrajectoireDiagonale, "Diagonale", "\uE8AB", CategorieAnimation.Trajectoire)
    };

    public static string NomDe(EffetAnimation effet)
        => Toutes.FirstOrDefault(e => e.Effet == effet)?.Nom ?? effet.ToString();

    public static CategorieAnimation CategorieDe(EffetAnimation effet)
        => Toutes.FirstOrDefault(e => e.Effet == effet)?.Categorie ?? CategorieAnimation.Entree;

    public static string LibelleCategorie(CategorieAnimation categorie) => categorie switch
    {
        CategorieAnimation.Entree => "Entrée",
        CategorieAnimation.Emphase => "Emphase",
        CategorieAnimation.Sortie => "Sortie",
        _ => "Trajectoire"
    };

    public static IEnumerable<EntreeAnimation> ParCategorie(CategorieAnimation categorie)
        => Toutes.Where(e => e.Categorie == categorie);

    public static string NomDeclencheur(DeclencheurAnimation declencheur) => declencheur switch
    {
        DeclencheurAnimation.AuClic => "Au clic",
        DeclencheurAnimation.AvecLaPrecedente => "Avec la précédente",
        _ => "Après la précédente"
    };
}
