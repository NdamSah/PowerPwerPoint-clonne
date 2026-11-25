using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace PowerPwerPoint.Media.Modele;

public enum TypeDiaporama
{
    Intervenant,
    ParcoursIndividuel,
    Kiosque
}

/// <summary>Paramètres du diaporama (« Configurer le diaporama »).</summary>
public sealed class DiaporamaOptions : ModeleObservable
{
    private TypeDiaporama _type = TypeDiaporama.Intervenant;
    private bool _boucleContinue;
    private bool _sansNarration;
    private bool _sansAnimation;
    private bool _modePresentateur;
    private int _debut;
    private int _fin;
    private string? _diaporamaPersonnalise;
    private string _couleurPointeur = "#FFFF2D2D";

    public TypeDiaporama Type
    {
        get => _type;
        set => Set(ref _type, value);
    }

    public bool BoucleContinue
    {
        get => _boucleContinue;
        set => Set(ref _boucleContinue, value);
    }

    public bool SansNarration
    {
        get => _sansNarration;
        set => Set(ref _sansNarration, value);
    }

    public bool SansAnimation
    {
        get => _sansAnimation;
        set => Set(ref _sansAnimation, value);
    }

    /// <summary>Mode présentateur (notes, chronomètre, diapositive suivante).</summary>
    public bool ModePresentateur
    {
        get => _modePresentateur;
        set => Set(ref _modePresentateur, value);
    }

    /// <summary>Index 1-based de la première diapositive ; 0 = depuis le début.</summary>
    public int Debut
    {
        get => _debut;
        set => Set(ref _debut, Math.Max(0, value));
    }

    /// <summary>Index 1-based de la dernière diapositive ; 0 = jusqu'à la fin.</summary>
    public int Fin
    {
        get => _fin;
        set => Set(ref _fin, Math.Max(0, value));
    }

    /// <summary>Nom du diaporama personnalisé utilisé comme plage, si défini.</summary>
    public string? DiaporamaPersonnalise
    {
        get => _diaporamaPersonnalise;
        set => Set(ref _diaporamaPersonnalise, string.IsNullOrWhiteSpace(value) ? null : value);
    }

    public string CouleurPointeur
    {
        get => _couleurPointeur;
        set => Set(ref _couleurPointeur, value);
    }

    public DiaporamaOptions Copier()
        => new()
        {
            Type = Type,
            BoucleContinue = BoucleContinue,
            SansNarration = SansNarration,
            SansAnimation = SansAnimation,
            ModePresentateur = ModePresentateur,
            Debut = Debut,
            Fin = Fin,
            DiaporamaPersonnalise = DiaporamaPersonnalise,
            CouleurPointeur = CouleurPointeur
        };
}

/// <summary>Diaporama personnalisé : une sélection ordonnée de diapositives.</summary>
public sealed class DiaporamaPersonnalise
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Nom { get; set; } = "Diaporama personnalisé";

    public List<string> Diapositives { get; set; } = new();

    public override string ToString() => Nom;
}

/// <summary>Minutage enregistré pour une diapositive (répétition ou enregistrement).</summary>
public sealed class MinutageDiapositive
{
    public string DiapositiveId { get; set; } = string.Empty;

    public double Secondes { get; set; }

    public override string ToString() => $"{Secondes:0.0} s";
}

/// <summary>Présentation manipulée par le module média.</summary>
public sealed class PresentationMedia : ModeleObservable
{
    private string _titre = "Présentation sans titre";
    private DiaporamaOptions _options = new();

    public string Titre
    {
        get => _titre;
        set => Set(ref _titre, value);
    }

    public ObservableCollection<DiapositiveMedia> Diapositives { get; set; } = new();

    public DiaporamaOptions Options
    {
        get => _options;
        set => Set(ref _options, value);
    }

    public ObservableCollection<DiaporamaPersonnalise> DiaporamasPersonnalises { get; set; } = new();

    public ObservableCollection<MinutageDiapositive> Minutages { get; set; } = new();

    /// <summary>Chemin du fichier de travail (non sérialisé).</summary>
    [JsonIgnore]
    public string? CheminFichier { get; set; }

    [JsonIgnore]
    public string? DossierMedia => string.IsNullOrEmpty(CheminFichier)
        ? null
        : Path.Combine(Path.GetDirectoryName(CheminFichier) ?? ".", Path.GetFileNameWithoutExtension(CheminFichier) + "_media");

    public DiapositiveMedia? TrouverDiapositive(string id)
        => Diapositives.FirstOrDefault(d => d.Id == id);

    public int IndexDe(DiapositiveMedia diapositive)
        => Diapositives.IndexOf(diapositive);

    /// <summary>Diapositives visibles (non masquées), dans l'ordre.</summary>
    [JsonIgnore]
    public IReadOnlyList<DiapositiveMedia> DiapositivesVisibles
        => Diapositives.Where(d => !d.Masquee).ToList();

    public void AppliquerTransitionPartout(TransitionDiapositive modele)
    {
        foreach (var diapositive in Diapositives)
        {
            diapositive.Transition = modele.Copier();
        }
    }

    public void EnregistrerMinutage(string diapositiveId, double secondes)
    {
        var existant = Minutages.FirstOrDefault(m => m.DiapositiveId == diapositiveId);
        if (existant is null)
        {
            Minutages.Add(new MinutageDiapositive { DiapositiveId = diapositiveId, Secondes = secondes });
        }
        else
        {
            existant.Secondes = secondes;
        }
    }

    public double DureeTotaleMinutage()
        => Minutages.Sum(m => m.Secondes);
}
