using System.Collections.ObjectModel;
using System.Text.Json.Serialization;

namespace PowerPwerPoint.Media.Modele;

/// <summary>Diapositive telle que vue par le module média / diaporama.</summary>
public sealed class DiapositiveMedia : ModeleObservable
{
    private string _titre = "Diapositive";
    private bool _masquee;
    private string _notes = string.Empty;
    private TransitionDiapositive _transition = new();
    private string? _narration;

    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Titre
    {
        get => _titre;
        set
        {
            if (Set(ref _titre, value))
            {
                Notifier(nameof(LibelleListe));
            }
        }
    }

    /// <summary>Diapositive masquée : ignorée pendant le diaporama.</summary>
    public bool Masquee
    {
        get => _masquee;
        set
        {
            if (Set(ref _masquee, value))
            {
                Notifier(nameof(LibelleListe));
                Notifier(nameof(AAnimations));
            }
        }
    }

    public string Notes
    {
        get => _notes;
        set => Set(ref _notes, value);
    }

    public TransitionDiapositive Transition
    {
        get => _transition;
        set => Set(ref _transition, value);
    }

    /// <summary>Ordre de la collection = ordre d'exécution des animations.</summary>
    public ObservableCollection<AnimationEffet> Animations { get; set; } = new();

    public ObservableCollection<ElementDiapositive> Elements { get; set; } = new();

    public ObservableCollection<ObjetMedia> Objets { get; set; } = new();

    /// <summary>Narration enregistrée pendant la répétition / l'enregistrement du diaporama.</summary>
    public string? Narration
    {
        get => _narration;
        set => Set(ref _narration, value);
    }

    [JsonIgnore]
    public string LibelleListe => Masquee ? $"{Titre} (masquée)" : Titre;

    [JsonIgnore]
    public bool AAnimations => Animations.Count > 0;

    public ElementDiapositive? TrouverElement(string id)
        => string.IsNullOrEmpty(id) ? null : Elements.FirstOrDefault(e => e.Id == id);

    public ObjetMedia? TrouverObjet(string id)
        => string.IsNullOrEmpty(id) ? null : Objets.FirstOrDefault(o => o.Id == id);

    public IEnumerable<AnimationEffet> AnimationsDe(string elementId)
        => Animations.Where(a => a.CibleId == elementId);

    /// <summary>Nom lisible d'une cible d'animation (élément ou objet média).</summary>
    public string NomCible(string id)
    {
        var element = TrouverElement(id);
        if (element is not null)
        {
            return element.Nom;
        }

        var objet = TrouverObjet(id);
        return objet?.Libelle ?? "(cible supprimée)";
    }
}
