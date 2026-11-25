using System.ComponentModel;
using System.IO;
using System.Windows.Input;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>
/// Panneau Transitions : galerie d'effets, options d'effet, son, durée, « Appliquer partout »,
/// avancement (au clic / après un délai) et aperçu.
/// </summary>
public sealed class TransitionsViewModel : ViewModelBase
{
    private readonly MediaWorkspaceViewModel _espace;
    private TransitionDiapositive? _transitionObservee;
    private List<OptionSon> _sons;

    public TransitionsViewModel(MediaWorkspaceViewModel espace)
    {
        _espace = espace;
        SensPossibles = OptionsEnum.Creer<SensTransition>(LibellesEnum.Sens);
        _sons = ConstruireSons(null);

        AppliquerPartoutCommand = new RelayCommand(AppliquerPartout, () => Transition is not null);
        ApercuCommand = new RelayCommand(() => _espace.DemanderApercuTransition(), () => Transition is not null);
        ChoisirSonCommand = new RelayCommand(ChoisirSon, () => Transition is not null);
        RetirerSonCommand = new RelayCommand(RetirerSon, () => Transition?.Son is not null);
        TesterSonCommand = new RelayCommand(TesterSon, () => Transition?.Son is not null);

        Attacher();
    }

    public TransitionDiapositive? Transition => _espace.DiapositiveCourante?.Transition;

    public IReadOnlyList<EntreeTransition> Galerie => TransitionCatalogue.Toutes;

    public IReadOnlyList<OptionEnum<SensTransition>> SensPossibles { get; }

    public IReadOnlyList<OptionSon> Sons => _sons;

    public bool AccepteSens => Transition is not null && TransitionCatalogue.AccepteSens(Transition.Effet);

    public bool AAnefetsOptionnels => Transition is not null;

    public TransitionEffet EffetSelectionne
    {
        get => Transition?.Effet ?? TransitionEffet.Aucune;
        set
        {
            if (Transition is null || Transition.Effet == value)
            {
                return;
            }

            Transition.Effet = value;
            Notifier(nameof(AccepteSens));
            Notifier(nameof(DescriptionEffet));
            _espace.MarquerModifie($"Transition « {TransitionCatalogue.NomDe(value)} » appliquée à la diapositive {_espace.NumeroDiapositive}.");
        }
    }

    public string? SonSelectionne
    {
        get => Transition?.Son;
        set
        {
            if (Transition is null)
            {
                return;
            }

            Transition.Son = value;
            RafraichirSons();
            Notifier(nameof(SonSelectionne));
            (RetirerSonCommand as RelayCommand)?.SignalerChangement();
            (TesterSonCommand as RelayCommand)?.SignalerChangement();
            _espace.MarquerModifie(value is null ? "Son de transition retiré." : $"Son de transition : {Path.GetFileName(value)}");
        }
    }

    public string DescriptionEffet
    {
        get
        {
            var entree = TransitionCatalogue.Obtenir(EffetSelectionne);
            return $"{entree.Nom} — {entree.Description}";
        }
    }

    public string ResumeAvancement
    {
        get
        {
            if (Transition is null)
            {
                return "Aucune diapositive";
            }

            return Transition.AvanceAutomatique
                ? $"Avance automatique après {Transition.DelaiAvance:0.#} s"
                : "Avance au clic";
        }
    }

    public ICommand AppliquerPartoutCommand { get; }

    public ICommand ApercuCommand { get; }

    public ICommand ChoisirSonCommand { get; }

    public ICommand RetirerSonCommand { get; }

    public ICommand TesterSonCommand { get; }

    /// <summary>Réattache l'observation et rafraîchit toutes les propriétés affichées.</summary>
    public void Rafraichir()
    {
        Attacher();
        RafraichirSons();

        Notifier(nameof(Transition));
        Notifier(nameof(EffetSelectionne));
        Notifier(nameof(AccepteSens));
        Notifier(nameof(AAnefetsOptionnels));
        Notifier(nameof(SonSelectionne));
        Notifier(nameof(DescriptionEffet));
        Notifier(nameof(ResumeAvancement));
        Notifier(nameof(Sons));

        (AppliquerPartoutCommand as RelayCommand)?.SignalerChangement();
        (ApercuCommand as RelayCommand)?.SignalerChangement();
        (ChoisirSonCommand as RelayCommand)?.SignalerChangement();
        (RetirerSonCommand as RelayCommand)?.SignalerChangement();
        (TesterSonCommand as RelayCommand)?.SignalerChangement();
    }

    private void Attacher()
    {
        if (ReferenceEquals(_transitionObservee, Transition))
        {
            return;
        }

        if (_transitionObservee is not null)
        {
            _transitionObservee.PropertyChanged -= SurTransitionModifiee;
        }

        _transitionObservee = Transition;
        if (_transitionObservee is not null)
        {
            _transitionObservee.PropertyChanged += SurTransitionModifiee;
        }
    }

    private void SurTransitionModifiee(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(TransitionDiapositive.Effet):
                Notifier(nameof(AccepteSens));
                Notifier(nameof(DescriptionEffet));
                break;
            case nameof(TransitionDiapositive.AvanceAutomatique):
            case nameof(TransitionDiapositive.DelaiAvance):
                Notifier(nameof(ResumeAvancement));
                break;
            case nameof(TransitionDiapositive.Son):
                RafraichirSons();
                Notifier(nameof(SonSelectionne));
                Notifier(nameof(Sons));
                (RetirerSonCommand as RelayCommand)?.SignalerChangement();
                (TesterSonCommand as RelayCommand)?.SignalerChangement();
                break;
        }
    }

    private void AppliquerPartout()
    {
        if (Transition is null)
        {
            return;
        }

        _espace.Presentation.AppliquerTransitionPartout(Transition);
        _espace.MarquerModifie($"Transition « {TransitionCatalogue.NomDe(Transition.Effet)} » appliquée à toutes les diapositives.");
    }

    private void ChoisirSon()
    {
        string? chemin = _espace.ServiceDialogues.ChoisirFichier("Choisir un son de transition", StockageMedia.FiltreSon);
        if (string.IsNullOrEmpty(chemin) || Transition is null)
        {
            return;
        }

        string copie = StockageMedia.Importer(chemin, _espace.Presentation);
        SonSelectionne = copie;
    }

    private void RetirerSon() => SonSelectionne = null;

    private void TesterSon()
    {
        if (Transition?.Son is null)
        {
            return;
        }

        MoteurTransitions.JouerSon(Transition);
        _espace.SignalerStatut("Lecture du son de transition.");
    }

    private void RafraichirSons()
    {
        var nouveaux = ConstruireSons(Transition?.Son);
        if (nouveaux.Count == _sons.Count && nouveaux.Select(s => s.Chemin).SequenceEqual(_sons.Select(s => s.Chemin)))
        {
            return;
        }

        _sons = nouveaux;
        Notifier(nameof(Sons));
    }

    private static List<OptionSon> ConstruireSons(string? sonCourant)
    {
        var liste = new List<OptionSon> { new("Aucun son", null) };

        string dossier = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Media");
        if (Directory.Exists(dossier))
        {
            foreach (var fichier in Directory.EnumerateFiles(dossier, "*.wav").OrderBy(f => f).Take(12))
            {
                liste.Add(new OptionSon(Path.GetFileNameWithoutExtension(fichier), fichier));
            }
        }

        if (!string.IsNullOrEmpty(sonCourant) && liste.All(s => !string.Equals(s.Chemin, sonCourant, StringComparison.OrdinalIgnoreCase)))
        {
            liste.Add(new OptionSon($"Personnalisé : {Path.GetFileName(sonCourant)}", sonCourant));
        }

        return liste;
    }
}
