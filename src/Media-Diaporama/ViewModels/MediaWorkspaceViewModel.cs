using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>
/// View model racine du module : détient la présentation, la diapositive courante,
/// le statut, les commandes de fichier du module et les quatre sous-view models
/// (Insertion, Transitions, Animations, Diaporama).
/// </summary>
public sealed class MediaWorkspaceViewModel : ViewModelBase
{
    private readonly IServiceDialogues _dialogues;
    private PresentationMedia _presentation;
    private DiapositiveMedia? _diapositiveCourante;
    private ElementDiapositive? _elementSelectionne;
    private string _statut = "Prêt";
    private bool _modifie;

    public MediaWorkspaceViewModel(IServiceDialogues dialogues)
    {
        _dialogues = dialogues ?? throw new ArgumentNullException(nameof(dialogues));
        _presentation = PresentationExemple.Creer();
        _diapositiveCourante = _presentation.Diapositives.FirstOrDefault();

        Insertion = new InsertionViewModel(this);
        Transitions = new TransitionsViewModel(this);
        Animations = new AnimationsViewModel(this);
        Diaporama = new DiaporamaViewModel(this);

        NouvellePresentationCommand = new RelayCommand(NouvellePresentation);
        OuvrirCommand = new RelayCommand(Ouvrir);
        EnregistrerCommand = new RelayCommand(() => Enregistrer(choisirEmplacement: false));
        EnregistrerSousCommand = new RelayCommand(() => Enregistrer(choisirEmplacement: true));

        RafraichirPanneaux();
    }

    public InsertionViewModel Insertion { get; }

    public TransitionsViewModel Transitions { get; }

    public AnimationsViewModel Animations { get; }

    public DiaporamaViewModel Diaporama { get; }

    public PresentationMedia Presentation => _presentation;

    internal IServiceDialogues ServiceDialogues => _dialogues;

    public ObservableCollection<DiapositiveMedia> Diapositives => _presentation.Diapositives;

    public DiapositiveMedia? DiapositiveCourante
    {
        get => _diapositiveCourante;
        set
        {
            if (!SetProperty(ref _diapositiveCourante, value))
            {
                return;
            }

            ElementSelectionne = null;
            Notifier(nameof(NumeroDiapositive));
            Notifier(nameof(TitreDiapositive));
            RafraichirPanneaux();
        }
    }

    /// <summary>Élément sélectionné dans l'aperçu (cible par défaut des animations).</summary>
    public ElementDiapositive? ElementSelectionne
    {
        get => _elementSelectionne;
        set
        {
            if (SetProperty(ref _elementSelectionne, value))
            {
                Animations.CibleParDefaut = value;
            }
        }
    }

    public string Statut
    {
        get => _statut;
        private set => SetProperty(ref _statut, value);
    }

    public bool Modifie
    {
        get => _modifie;
        private set
        {
            if (SetProperty(ref _modifie, value))
            {
                Notifier(nameof(TitreFenetre));
            }
        }
    }

    public string TitreFenetre
        => $"{(Modifie ? "* " : string.Empty)}{Presentation.Titre} — Média, Transitions, Animations & Diaporama";

    public string CheminAffiche => Presentation.CheminFichier is { Length: > 0 } chemin
        ? chemin
        : "Présentation non enregistrée (Ctrl+S pour enregistrer)";

    public int NumeroDiapositive => DiapositiveCourante is null
        ? 0
        : Presentation.IndexDe(DiapositiveCourante) + 1;

    public int TotalDiapositives => Presentation.Diapositives.Count;

    public string TitreDiapositive
        => DiapositiveCourante is null
            ? "Aucune diapositive"
            : $"Diapositive {NumeroDiapositive} / {TotalDiapositives} — {DiapositiveCourante.Titre}";

    public ICommand NouvellePresentationCommand { get; }

    public ICommand OuvrirCommand { get; }

    public ICommand EnregistrerCommand { get; }

    public ICommand EnregistrerSousCommand { get; }

    /// <summary>Émis lorsqu'un diaporama doit être lancé (traité par la fenêtre hôte).</summary>
    public event EventHandler<DemandeDiaporama>? DiaporamaDemande;

    /// <summary>Émis pour prévisualiser la transition de la diapositive courante.</summary>
    public event EventHandler? ApercuTransitionDemande;

    /// <summary>Émis pour prévisualiser les animations de la diapositive courante.</summary>
    public event EventHandler<DemandeApercuAnimations>? ApercuAnimationsDemande;

    internal void SignalerStatut(string message)
    {
        Statut = message;
    }

    internal void MarquerModifie(string message)
    {
        Modifie = true;
        Statut = message;
    }

    /// <summary>Rafraîchit les listes dépendant du contenu de la diapositive courante, sans perdre la sélection.</summary>
    internal void ActualiserContenuDiapositive()
    {
        Insertion.NotifierListe();
        Animations.Rafraichir();
        Diaporama.Rafraichir();
        Notifier(nameof(TitreDiapositive));
    }

    internal void DemanderApercuTransition()
    {
        if (DiapositiveCourante is null)
        {
            return;
        }

        ApercuTransitionDemande?.Invoke(this, EventArgs.Empty);
    }

    internal void DemanderApercuAnimations(AnimationEffet? depuis)
    {
        if (DiapositiveCourante is null)
        {
            return;
        }

        ApercuAnimationsDemande?.Invoke(this, new DemandeApercuAnimations(DiapositiveCourante, depuis));
    }

    /// <summary>
    /// Construit et émet une demande de diaporama.
    /// </summary>
    internal void LancerDiaporama(
        bool depuisDiapositiveCourante,
        bool modePresentateur,
        bool enregistrer = false,
        bool repetition = false,
        DiaporamaPersonnalise? personnalise = null)
    {
        var options = Presentation.Options.Copier();
        options.ModePresentateur = modePresentateur;

        // Un diaporama personnalisé configuré (ou demandé) remplace la plage de diapositives.
        personnalise ??= Presentation.DiaporamasPersonnalises
            .FirstOrDefault(d => string.Equals(d.Nom, options.DiaporamaPersonnalise, StringComparison.Ordinal));

        IReadOnlyList<DiapositiveMedia> sequence = personnalise is not null
            ? PlageDiaporama.ResoudrePersonnalise(Presentation.Diapositives, personnalise)
            : PlageDiaporama.Resoudre(Presentation.Diapositives, options);

        if (sequence.Count == 0)
        {
            _dialogues.AfficherInformation("Aucune diapositive visible à présenter. Vérifiez les diapositives masquées et la plage configurée.");
            return;
        }

        int index = 0;
        if (depuisDiapositiveCourante && DiapositiveCourante is not null)
        {
            int position = sequence.ToList().FindIndex(d => d.Id == DiapositiveCourante.Id);
            index = position >= 0 ? position : 0;
        }

        DiaporamaDemande?.Invoke(this, new DemandeDiaporama(
            sequence,
            index,
            options,
            modePresentateur,
            enregistrer,
            repetition));
    }

    private void RafraichirPanneaux()
    {
        Insertion.ActualiserListe();
        Transitions.Rafraichir();
        Animations.Rafraichir();
        Diaporama.Rafraichir();
    }

    private void NouvellePresentation()
    {
        if (Modifie && !_dialogues.Confirmer("La présentation en cours contient des modifications non enregistrées. Créer une nouvelle présentation ?", "Nouvelle présentation"))
        {
            return;
        }

        var presentation = new PresentationMedia { Titre = "Présentation sans titre" };
        var diapositive = new DiapositiveMedia { Titre = "Diapositive 1" };
        diapositive.Elements.Add(new ElementDiapositive
        {
            Nom = "Titre",
            Texte = "Titre de la diapositive",
            X = 80,
            Y = 200,
            Largeur = 800,
            Hauteur = 120,
            CouleurFond = "#00000000",
            CouleurTexte = "#FF1F2430",
            TaillePolice = 40,
            EstZoneTexte = true
        });
        presentation.Diapositives.Add(diapositive);

        ChargerPresentation(presentation);
        MarquerModifie("Nouvelle présentation créée.");
        Modifie = false;
    }

    private void Ouvrir()
    {
        string? chemin = _dialogues.OuvrirPresentation();
        if (string.IsNullOrEmpty(chemin))
        {
            return;
        }

        try
        {
            var presentation = SerialisationMedia.Charger(chemin);
            ChargerPresentation(presentation);
            Modifie = false;
            Statut = $"Présentation ouverte : {Path.GetFileName(chemin)}";
        }
        catch (Exception ex)
        {
            _dialogues.AfficherErreur($"Impossible d'ouvrir le fichier :\n{ex.Message}");
        }
    }

    private void Enregistrer(bool choisirEmplacement)
    {
        string? chemin = _dialogues.EnregistrerPresentation(Presentation.CheminFichier, Presentation.Titre, choisirEmplacement);
        if (string.IsNullOrEmpty(chemin))
        {
            return;
        }

        try
        {
            SerialisationMedia.Enregistrer(Presentation, chemin);
            Presentation.CheminFichier = chemin;
            Modifie = false;
            Notifier(nameof(CheminAffiche));
            Statut = $"Enregistré : {Path.GetFileName(chemin)}";
        }
        catch (Exception ex)
        {
            _dialogues.AfficherErreur($"Impossible d'enregistrer :\n{ex.Message}");
        }
    }

    private void ChargerPresentation(PresentationMedia presentation)
    {
        _presentation = presentation;
        _diapositiveCourante = presentation.Diapositives.FirstOrDefault();
        _elementSelectionne = null;

        Notifier(nameof(Presentation));
        Notifier(nameof(Diapositives));
        Notifier(nameof(DiapositiveCourante));
        Notifier(nameof(CheminAffiche));
        Notifier(nameof(TitreFenetre));
        Notifier(nameof(NumeroDiapositive));
        Notifier(nameof(TotalDiapositives));
        Notifier(nameof(TitreDiapositive));

        RafraichirPanneaux();
    }
}
