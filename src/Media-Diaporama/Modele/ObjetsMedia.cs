using System.Text.Json.Serialization;

namespace PowerPwerPoint.Media.Modele;

public enum TypeObjetMedia
{
    Video,
    Audio,
    Zoom,
    Lien,
    Action
}

/// <summary>Moment de déclenchement de la lecture d'un média.</summary>
public enum DemarrageMedia
{
    AuClic,
    Automatiquement,
    ApresPrecedent
}

public enum TypeZoom
{
    Resume,
    Section,
    Diapositive
}

public enum TypeLien
{
    PageWeb,
    Fichier,
    Diapositive,
    Email
}

public enum TypeAction
{
    Aucune,
    AllerDiapositive,
    DiapositiveSuivante,
    DiapositivePrecedente,
    PremiereDiapositive,
    DerniereDiapositive,
    FinDiaporama,
    LancerProgramme,
    LireSon
}

public enum DeclencheurAction
{
    Clic,
    Survol
}

/// <summary>
/// Objet multimédia ou interactif posé sur une diapositive.
/// Sérialisé en JSON polymorphe (discriminateur <c>type</c>) en attendant la sérialisation
/// OpenXML du module noyau.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(ObjetVideo), "video")]
[JsonDerivedType(typeof(ObjetAudio), "audio")]
[JsonDerivedType(typeof(ObjetZoom), "zoom")]
[JsonDerivedType(typeof(ObjetLien), "lien")]
[JsonDerivedType(typeof(ObjetAction), "action")]
public abstract class ObjetMedia : ModeleObservable
{
    private string _nom = string.Empty;
    private double _x = 60;
    private double _y = 60;
    private double _largeur = 360;
    private double _hauteur = 220;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Nom
    {
        get => _nom;
        set
        {
            if (Set(ref _nom, value))
            {
                Notifier(nameof(Libelle));
            }
        }
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

    [JsonIgnore]
    public abstract TypeObjetMedia Type { get; }

    [JsonIgnore]
    public abstract string Libelle { get; }

    [JsonIgnore]
    public string TypeLibelle => Type switch
    {
        TypeObjetMedia.Video => "Vidéo",
        TypeObjetMedia.Audio => "Audio",
        TypeObjetMedia.Zoom => "Zoom",
        TypeObjetMedia.Lien => "Lien",
        TypeObjetMedia.Action => "Action",
        _ => "Objet"
    };
}

public sealed class ObjetVideo : ObjetMedia
{
    private string _chemin = string.Empty;
    private TimeSpan _debutRognage = TimeSpan.Zero;
    private TimeSpan? _finRognage;
    private DemarrageMedia _demarrage = DemarrageMedia.AuClic;
    private int _volume = 80;
    private bool _muet;
    private bool _lireEnBoucle;
    private bool _rembobinerApresLecture = true;
    private bool _afficherCommandes = true;
    private bool _lireEnPleinEcran;
    private bool _masquerHorsLecture;
    private string? _pisteAudio;

    public override TypeObjetMedia Type => TypeObjetMedia.Video;

    public override string Libelle => string.IsNullOrWhiteSpace(Nom) ? System.IO.Path.GetFileName(Chemin) : Nom;

    public string Chemin
    {
        get => _chemin;
        set
        {
            if (Set(ref _chemin, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    /// <summary>Point d'entrée du rognage (rognage non destructif).</summary>
    public TimeSpan DebutRognage
    {
        get => _debutRognage;
        set
        {
            if (Set(ref _debutRognage, value))
            {
                Notifier(nameof(EstRogne));
                Notifier(nameof(ResumeRognage));
            }
        }
    }

    /// <summary>Point de sortie du rognage ; <c>null</c> = fin du fichier.</summary>
    public TimeSpan? FinRognage
    {
        get => _finRognage;
        set
        {
            if (Set(ref _finRognage, value))
            {
                Notifier(nameof(EstRogne));
                Notifier(nameof(ResumeRognage));
            }
        }
    }

    public DemarrageMedia Demarrage
    {
        get => _demarrage;
        set => Set(ref _demarrage, value);
    }

    public int Volume
    {
        get => _volume;
        set => Set(ref _volume, Math.Clamp(value, 0, 100));
    }

    public bool Muet
    {
        get => _muet;
        set => Set(ref _muet, value);
    }

    public bool LireEnBoucle
    {
        get => _lireEnBoucle;
        set => Set(ref _lireEnBoucle, value);
    }

    public bool RembobinerApresLecture
    {
        get => _rembobinerApresLecture;
        set => Set(ref _rembobinerApresLecture, value);
    }

    public bool AfficherCommandes
    {
        get => _afficherCommandes;
        set => Set(ref _afficherCommandes, value);
    }

    public bool LireEnPleinEcran
    {
        get => _lireEnPleinEcran;
        set => Set(ref _lireEnPleinEcran, value);
    }

    public bool MasquerHorsLecture
    {
        get => _masquerHorsLecture;
        set => Set(ref _masquerHorsLecture, value);
    }

    /// <summary>Piste audio enregistrée en même temps qu'une capture d'écran (WAV).</summary>
    public string? PisteAudio
    {
        get => _pisteAudio;
        set => Set(ref _pisteAudio, value);
    }

    public TimeSpan? Duree { get; set; }

    [JsonIgnore]
    public bool EstRogne => DebutRognage > TimeSpan.Zero || FinRognage.HasValue;

    [JsonIgnore]
    public string ResumeRognage => EstRogne
        ? $"Rognée de {DebutRognage:mm\\:ss} à {FinRognage?.ToString("mm\\:ss") ?? "fin"}"
        : "Vidéo complète";
}

public sealed class ObjetAudio : ObjetMedia
{
    private string _chemin = string.Empty;
    private int _volume = 80;
    private bool _muet;
    private bool _lireEnBoucle;
    private bool _lireEnArrierePlan;
    private DemarrageMedia _demarrage = DemarrageMedia.AuClic;
    private bool _enregistreDansApplication;

    public override TypeObjetMedia Type => TypeObjetMedia.Audio;

    public override string Libelle => string.IsNullOrWhiteSpace(Nom) ? System.IO.Path.GetFileName(Chemin) : Nom;

    public string Chemin
    {
        get => _chemin;
        set
        {
            if (Set(ref _chemin, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    public int Volume
    {
        get => _volume;
        set => Set(ref _volume, Math.Clamp(value, 0, 100));
    }

    public bool Muet
    {
        get => _muet;
        set => Set(ref _muet, value);
    }

    public bool LireEnBoucle
    {
        get => _lireEnBoucle;
        set => Set(ref _lireEnBoucle, value);
    }

    /// <summary>Continue la lecture lors du passage aux diapositives suivantes.</summary>
    public bool LireEnArrierePlan
    {
        get => _lireEnArrierePlan;
        set => Set(ref _lireEnArrierePlan, value);
    }

    public DemarrageMedia Demarrage
    {
        get => _demarrage;
        set => Set(ref _demarrage, value);
    }

    /// <summary>Vrai si le fichier a été créé depuis l'application (enregistrement d'un son).</summary>
    public bool EnregistreDansApplication
    {
        get => _enregistreDansApplication;
        set => Set(ref _enregistreDansApplication, value);
    }

    public TimeSpan? Duree { get; set; }
}

/// <summary>Zoom de présentation : Résumé, Section ou Diapositive.</summary>
public sealed class ObjetZoom : ObjetMedia
{
    private TypeZoom _typeZoom = TypeZoom.Diapositive;
    private string _titre = string.Empty;
    private List<string> _cibles = new();
    private string _couleurFond = "#FF1F2430";

    public override TypeObjetMedia Type => TypeObjetMedia.Zoom;

    public override string Libelle => string.IsNullOrWhiteSpace(Titre)
        ? TypeZoom switch
        {
            TypeZoom.Resume => "Zoom Résumé",
            TypeZoom.Section => "Zoom Section",
            _ => "Zoom Diapositive"
        }
        : Titre;

    public TypeZoom TypeZoom
    {
        get => _typeZoom;
        set
        {
            if (Set(ref _typeZoom, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    public string Titre
    {
        get => _titre;
        set
        {
            if (Set(ref _titre, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    /// <summary>Identifiants des diapositives ciblées.</summary>
    public List<string> Cibles
    {
        get => _cibles;
        set
        {
            if (Set(ref _cibles, value))
            {
                Notifier(nameof(ResumeCibles));
            }
        }
    }

    public string CouleurFond
    {
        get => _couleurFond;
        set => Set(ref _couleurFond, value);
    }

    [JsonIgnore]
    public string ResumeCibles => Cibles.Count == 0
        ? "Aucune diapositive ciblée"
        : $"{Cibles.Count} diapositive(s) ciblée(s)";
}

/// <summary>Lien hypertexte posé sur un élément de la diapositive.</summary>
public sealed class ObjetLien : ObjetMedia
{
    private string _elementId = string.Empty;
    private TypeLien _typeLien = TypeLien.PageWeb;
    private string _cible = string.Empty;
    private string _infoBulle = string.Empty;

    public override TypeObjetMedia Type => TypeObjetMedia.Lien;

    public override string Libelle => string.IsNullOrWhiteSpace(InfoBulle) ? Cible : InfoBulle;

    /// <summary>Élément de la diapositive porteur du lien ; vide = toute la diapositive.</summary>
    public string ElementId
    {
        get => _elementId;
        set => Set(ref _elementId, value);
    }

    public TypeLien TypeLien
    {
        get => _typeLien;
        set => Set(ref _typeLien, value);
    }

    public string Cible
    {
        get => _cible;
        set
        {
            if (Set(ref _cible, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    public string InfoBulle
    {
        get => _infoBulle;
        set
        {
            if (Set(ref _infoBulle, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }
}

/// <summary>Action interactive posée sur un élément de la diapositive.</summary>
public sealed class ObjetAction : ObjetMedia
{
    private string _elementId = string.Empty;
    private TypeAction _typeAction = TypeAction.Aucune;
    private string _parametre = string.Empty;
    private string _arguments = string.Empty;
    private DeclencheurAction _declencheur = DeclencheurAction.Clic;

    public override TypeObjetMedia Type => TypeObjetMedia.Action;

    public override string Libelle => TypeAction switch
    {
        TypeAction.LancerProgramme => $"Lancer : {System.IO.Path.GetFileName(Parametre)}",
        TypeAction.LireSon => $"Lire : {System.IO.Path.GetFileName(Parametre)}",
        TypeAction.AllerDiapositive => "Aller à la diapositive",
        TypeAction.FinDiaporama => "Fin du diaporama",
        _ => TypeAction.ToString()
    };

    /// <summary>Élément de la diapositive porteur de l'action ; vide = toute la diapositive.</summary>
    public string ElementId
    {
        get => _elementId;
        set => Set(ref _elementId, value);
    }

    public TypeAction TypeAction
    {
        get => _typeAction;
        set
        {
            if (Set(ref _typeAction, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    /// <summary>Identifiant de diapositive, chemin de programme ou son selon <see cref="TypeAction"/>.</summary>
    public string Parametre
    {
        get => _parametre;
        set
        {
            if (Set(ref _parametre, value))
            {
                Notifier(nameof(Libelle));
            }
        }
    }

    public string Arguments
    {
        get => _arguments;
        set => Set(ref _arguments, value);
    }

    public DeclencheurAction Declencheur
    {
        get => _declencheur;
        set => Set(ref _declencheur, value);
    }
}
