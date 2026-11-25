using System.Collections.ObjectModel;
using System.Windows.Input;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>
/// Panneau Diaporama : lancement, mode présentateur, configuration, masquage,
/// répétition des minutages, enregistrement, diaporamas personnalisés et aide des raccourcis.
/// </summary>
public sealed class DiaporamaViewModel : ViewModelBase
{
    private readonly MediaWorkspaceViewModel _espace;
    private DiaporamaPersonnalise? _selectionPersonnalise;

    public DiaporamaViewModel(MediaWorkspaceViewModel espace)
    {
        _espace = espace;

        DepuisDebutCommand = new RelayCommand(() => _espace.LancerDiaporama(false, false));
        DepuisDiapositiveCommand = new RelayCommand(
            () => _espace.LancerDiaporama(true, false),
            () => _espace.DiapositiveCourante is not null);
        ModePresentateurCommand = new RelayCommand(() => _espace.LancerDiaporama(false, true));
        ConfigurerCommand = new RelayCommand(Configurer);
        GererDiaporamasCommand = new RelayCommand(GererDiaporamas);
        LireDiaporamaPersonnaliseCommand = new RelayCommand(
            p => LireDiaporamaPersonnalise(p as DiaporamaPersonnalise ?? SelectionPersonnalise),
            p => (p as DiaporamaPersonnalise ?? SelectionPersonnalise) is not null);
        VerifierMinutageCommand = new RelayCommand(() => _espace.LancerDiaporama(false, false, repetition: true));
        EnregistrerDiaporamaCommand = new RelayCommand(() => _espace.LancerDiaporama(false, false, enregistrer: true));
        AideRaccourcisCommand = new RelayCommand(() => _espace.ServiceDialogues.AfficherAideRaccourcis());
    }

    public DiapositiveMedia? Diapositive => _espace.DiapositiveCourante;

    public bool EstMasquee
    {
        get => Diapositive?.Masquee ?? false;
        set
        {
            if (Diapositive is null || Diapositive.Masquee == value)
            {
                return;
            }

            Diapositive.Masquee = value;
            Notifier(nameof(EstMasquee));
            Notifier(nameof(ResumeMasquage));
            _espace.MarquerModifie(value ? "Diapositive masquée : elle sera ignorée au diaporama." : "Diapositive de nouveau affichée.");
            _espace.ActualiserContenuDiapositive();
        }
    }

    public string ResumeMasquage => EstMasquee
        ? "Cette diapositive est masquée."
        : "Cette diapositive est visible au diaporama.";

    public ObservableCollection<DiaporamaPersonnalise> DiaporamasPersonnalises => _espace.Presentation.DiaporamasPersonnalises;

    public DiaporamaPersonnalise? SelectionPersonnalise
    {
        get => _selectionPersonnalise;
        set
        {
            if (SetProperty(ref _selectionPersonnalise, value))
            {
                (LireDiaporamaPersonnaliseCommand as RelayCommand)?.SignalerChangement();
            }
        }
    }

    public string ResumeConfiguration
    {
        get
        {
            var options = _espace.Presentation.Options;
            var parties = new List<string>
            {
                LibellesEnum.LibelleTypeDiaporama(options.Type)
            };

            if (options.Debut > 0 || options.Fin > 0)
            {
                parties.Add($"plage {(options.Debut == 0 ? 1 : options.Debut)} → {(options.Fin == 0 ? "fin" : options.Fin.ToString())}");
            }

            if (options.BoucleContinue)
            {
                parties.Add("boucle continue");
            }

            if (options.SansAnimation)
            {
                parties.Add("sans animation");
            }

            if (options.SansNarration)
            {
                parties.Add("sans narration");
            }

            return string.Join(" · ", parties);
        }
    }

    public string ResumeMinutage
    {
        get
        {
            var minutages = _espace.Presentation.Minutages;
            if (minutages.Count == 0)
            {
                return "Aucun minutage enregistré.";
            }

            var total = TimeSpan.FromSeconds(_espace.Presentation.DureeTotaleMinutage());
            return $"{minutages.Count} diapositive(s) minutée(s) · total {ChronometreDiaporama.Formater(total)}";
        }
    }

    public ICommand DepuisDebutCommand { get; }

    public ICommand DepuisDiapositiveCommand { get; }

    public ICommand ModePresentateurCommand { get; }

    public ICommand ConfigurerCommand { get; }

    public ICommand GererDiaporamasCommand { get; }

    public ICommand LireDiaporamaPersonnaliseCommand { get; }

    public ICommand VerifierMinutageCommand { get; }

    public ICommand EnregistrerDiaporamaCommand { get; }

    public ICommand AideRaccourcisCommand { get; }

    public void Rafraichir()
    {
        Notifier(nameof(Diapositive));
        Notifier(nameof(EstMasquee));
        Notifier(nameof(ResumeMasquage));
        Notifier(nameof(ResumeConfiguration));
        Notifier(nameof(ResumeMinutage));
        Notifier(nameof(DiaporamasPersonnalises));
        (DepuisDiapositiveCommand as RelayCommand)?.SignalerChangement();
    }

    private void Configurer()
    {
        var resultat = _espace.ServiceDialogues.ConfigurerDiaporama(_espace.Presentation.Options, _espace.Presentation);
        if (resultat is null)
        {
            return;
        }

        _espace.Presentation.Options = resultat;
        _espace.MarquerModifie("Configuration du diaporama mise à jour.");
        Rafraichir();
    }

    private void GererDiaporamas()
    {
        if (_espace.ServiceDialogues.EditerDiaporamasPersonnalises(_espace.Presentation))
        {
            _espace.MarquerModifie("Diaporamas personnalisés mis à jour.");
            Notifier(nameof(DiaporamasPersonnalises));
            (LireDiaporamaPersonnaliseCommand as RelayCommand)?.SignalerChangement();
        }
    }

    private void LireDiaporamaPersonnalise(DiaporamaPersonnalise? personnalise)
    {
        if (personnalise is null)
        {
            return;
        }

        _espace.LancerDiaporama(false, false, personnalise: personnalise);
    }
}
