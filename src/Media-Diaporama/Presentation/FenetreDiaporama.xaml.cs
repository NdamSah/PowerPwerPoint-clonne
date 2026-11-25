using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;
using PowerPwerPoint.Media.ViewModels;
using PowerPwerPoint.Media.Vues;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>
/// Fenêtre de diaporama plein écran : transitions, animations, médias, navigation clavier,
/// pointeur laser, écran noir/blanc, minutage, enregistrement et mode présentateur.
/// </summary>
public partial class FenetreDiaporama : Window
{
    private readonly PresentationMedia _presentation;
    private readonly DemandeDiaporama _demande;
    private readonly IReadOnlyList<DiapositiveMedia> _sequence;
    private readonly DiaporamaOptions _options;
    private readonly ChronometreDiaporama _chrono = new();
    private readonly Stopwatch _chronoTexte = new();
    private readonly DispatcherTimer _minuterieAvance = new() { Interval = TimeSpan.FromSeconds(5) };
    private readonly DispatcherTimer _minuterieChrono = new() { Interval = TimeSpan.FromMilliseconds(200) };

    private FenetrePresentateur? _presentateur;
    private EnregistreurAudio? _narration;
    private RenduDiapositive _entrante;
    private RenduDiapositive _sortante;
    private List<PasAnimation> _plan = new();
    private int _index;
    private int _pasCourant;
    private string _saisie = string.Empty;
    private bool _occupe;
    private bool _termine;
    private bool _laser;
    private bool _ecranNoir;
    private bool _ecranBlanc;
    private bool _muet;
    private DiapositiveMedia? _diapositiveChronometree;

    public FenetreDiaporama(PresentationMedia presentation, DemandeDiaporama demande)
    {
        InitializeComponent();

        _presentation = presentation;
        _demande = demande;
        _sequence = demande.Sequence;
        _options = demande.Options;
        _entrante = CoucheA;
        _sortante = CoucheB;

        CoucheA.Presentation = presentation;
        CoucheB.Presentation = presentation;
        CoucheA.InteractionDemandee += SurInteraction;
        CoucheB.InteractionDemandee += SurInteraction;
        CoucheA.ZoomActive += SurZoom;
        CoucheB.ZoomActive += SurZoom;
        CoucheA.Statut += (_, message) => AfficherStatut(message);
        CoucheB.Statut += (_, message) => AfficherStatut(message);

        _minuterieAvance.Tick += SurAvanceAutomatique;
        _minuterieChrono.Tick += (_, _) => ChronoActualise?.Invoke(this, EventArgs.Empty);
        _minuterieChrono.Start();

        Loaded += SurChargement;
        Closed += SurFermeture;
    }

    public PresentationMedia Presentation => _presentation;

    public DiapositiveMedia? DiapositiveCourante
        => _index >= 0 && _index < _sequence.Count ? _sequence[_index] : null;

    public DiapositiveMedia? DiapositiveSuivante
        => _index + 1 >= 0 && _index + 1 < _sequence.Count ? _sequence[_index + 1] : null;

    public int Index => _index;

    public int Total => _sequence.Count;

    public ChronometreDiaporama Chronometre => _chrono;

    public bool LaserActif => _laser;

    public event EventHandler? DiapositiveChangee;

    public event EventHandler? ChronoActualise;

    public void Avancer() => Executer(() => AvancerInterneAsync());

    public void Reculer() => Executer(() => ReculerInterneAsync());

    public void AllerA(int index) => Executer(() => AllerAAsync(index));

    public void BasculerLaser() => DefinirLaser(!_laser);

    public void BasculerPause() => _chrono.BasculerPause();

    public void TerminerDiaporama() => Terminer();

    private async void SurChargement(object sender, RoutedEventArgs e)
    {
        _chrono.Demarrer();
        if (_demande.ModePresentateur)
        {
            OuvrirPresentateur();
        }

        await AllerAAsync(_demande.IndexDepart);
    }

    private async Task AllerAAsync(int index)
    {
        if (_occupe)
        {
            return;
        }

        _occupe = true;
        try
        {
            if (_sequence.Count == 0)
            {
                Terminer();
                return;
            }

            if (index >= _sequence.Count)
            {
                if (_options.BoucleContinue)
                {
                    index = 0;
                }
                else
                {
                    Terminer();
                    return;
                }
            }

            if (index < 0)
            {
                index = 0;
            }

            EnregistrerMinutageCourant();
            ArreterNarration();

            _index = index;
            var diapositive = _sequence[index];

            // Échange des couches : l'entrante passe au premier plan.
            (_entrante, _sortante) = (_sortante, _entrante);
            Panel.SetZIndex(_entrante, 1);
            Panel.SetZIndex(_sortante, 0);

            _entrante.Charger(diapositive, _presentation, ModeRendu.Diaporama);
            _entrante.Visibility = Visibility.Visible;
            _entrante.MettreEnEvidence(null);

            if (!_options.SansAnimation)
            {
                _entrante.PreparerAnimations();
            }

            ChangerTextes(diapositive);
            DiapositiveChangee?.Invoke(this, EventArgs.Empty);

            await MoteurTransitions.ExecuterAsync(diapositive.Transition, _sortante, _entrante);

            _sortante.Visibility = Visibility.Collapsed;
            _sortante.ArreterLectures();

            _entrante.DemarrerMediasAutomatiques();
            DemarrerNarration(diapositive);

            _plan = _options.SansAnimation
                ? new List<PasAnimation>()
                : MoteurAnimations.ConstruirePlan(diapositive.Animations);
            _pasCourant = 0;

            await ExecuterPasAutomatiquesAsync();
            ArmerAvance(diapositive);
        }
        finally
        {
            _occupe = false;
        }
    }

    private async Task AvancerInterneAsync()
    {
        if (_occupe || _termine)
        {
            return;
        }

        if (_pasCourant < _plan.Count)
        {
            await ExecuterPasCourantAsync();
            return;
        }

        await AllerAAsync(_index + 1);
    }

    private async Task ReculerInterneAsync()
    {
        if (_occupe || _termine)
        {
            return;
        }

        if (_index <= 0 && !_options.BoucleContinue)
        {
            return;
        }

        await AllerAAsync(_index - 1);
    }

    private async Task ExecuterPasCourantAsync()
    {
        if (_occupe || _pasCourant >= _plan.Count)
        {
            return;
        }

        _occupe = true;
        try
        {
            _minuterieAvance.Stop();
            var pas = _plan[_pasCourant];
            _pasCourant++;
            await MoteurAnimations.ExecuterPasAsync(pas, _entrante.Resoudre);
        }
        finally
        {
            _occupe = false;
        }

        ArmerAvance(DiapositiveCourante);
    }

    private async Task ExecuterPasAutomatiquesAsync()
    {
        while (_pasCourant < _plan.Count && _plan[_pasCourant].DemarrageAutomatique)
        {
            _occupe = true;
            try
            {
                var pas = _plan[_pasCourant];
                _pasCourant++;
                await MoteurAnimations.ExecuterPasAsync(pas, _entrante.Resoudre);
            }
            finally
            {
                _occupe = false;
            }
        }
    }

    private void ArmerAvance(DiapositiveMedia? diapositive)
    {
        _minuterieAvance.Stop();
        if (diapositive is null || diapositive.Masquee)
        {
            return;
        }

        double? delai = null;
        if (diapositive.Transition.AvanceAutomatique)
        {
            delai = diapositive.Transition.DelaiAvance;
        }
        else if (_options.Type == TypeDiaporama.Kiosque)
        {
            var minutage = _presentation.Minutages.FirstOrDefault(m => m.DiapositiveId == diapositive.Id);
            delai = minutage?.Secondes;
        }

        if (delai is > 0)
        {
            _minuterieAvance.Interval = TimeSpan.FromSeconds(delai.Value);
            _minuterieAvance.Start();
        }
    }

    private void SurAvanceAutomatique(object? sender, EventArgs e)
    {
        _minuterieAvance.Stop();
        if (_pasCourant < _plan.Count)
        {
            Executer(() => ExecuterPasCourantAsync());
        }
        else
        {
            Executer(() => AllerAAsync(_index + 1));
        }
    }

    private void SurClicGauche(object sender, MouseButtonEventArgs e)
    {
        if (_occupe || _termine)
        {
            return;
        }

        var source = e.OriginalSource as DependencyObject;
        if (TrouverAncetre<Button>(source) is not null)
        {
            return;
        }

        Avancer();
    }

    private static T? TrouverAncetre<T>(DependencyObject? element) where T : DependencyObject
    {
        while (element is not null)
        {
            if (element is T recherche)
            {
                return recherche;
            }

            element = VisualTreeHelper.GetParent(element);
        }

        return null;
    }

    private void SurTouche(object sender, KeyEventArgs e)
    {
        string touche = e.Key.ToString();

        string? chiffre = NavigateurDiaporama.ChiffreDepuisTouche(touche);
        if (chiffre is not null)
        {
            _saisie += chiffre;
            TexteSaisie.Text = $"Aller à la diapositive {_saisie} — Entrée pour valider";
            BandeauSaisie.Visibility = Visibility.Visible;
            e.Handled = true;
            return;
        }

        if (_saisie.Length > 0 && touche is "Enter" or "Return")
        {
            int? cible = NavigateurDiaporama.ResoudreSaisie(_saisie, _sequence.Count);
            _saisie = string.Empty;
            BandeauSaisie.Visibility = Visibility.Collapsed;

            if (cible is int position)
            {
                AllerA(position);
            }

            e.Handled = true;
            return;
        }

        _saisie = string.Empty;
        BandeauSaisie.Visibility = Visibility.Collapsed;

        if (touche is "F1")
        {
            AfficherAide();
            e.Handled = true;
            return;
        }

        if (touche is "P" && Keyboard.Modifiers == ModifierKeys.Control)
        {
            BasculerPause();
            e.Handled = true;
            return;
        }

        if (touche is "M")
        {
            _muet = !_muet;
            _entrante.BasculerMuet(_muet);
            _sortante.BasculerMuet(_muet);
            e.Handled = true;
            return;
        }

        var resultat = NavigateurDiaporama.Interpreter(touche, _index, _sequence.Count);
        switch (resultat.Action)
        {
            case ActionNavigation.Suivante:
                Avancer();
                break;
            case ActionNavigation.Precedente:
                Reculer();
                break;
            case ActionNavigation.Premiere:
            case ActionNavigation.Derniere:
                if (resultat.Index >= 0)
                {
                    AllerA(resultat.Index);
                }

                break;
            case ActionNavigation.Terminer:
                Terminer();
                break;
            case ActionNavigation.EcranNoir:
                BasculerEcran(noir: true);
                break;
            case ActionNavigation.EcranBlanc:
                BasculerEcran(noir: false);
                break;
            case ActionNavigation.PointeurLaser:
                BasculerLaser();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void BasculerEcran(bool noir)
    {
        if (noir)
        {
            _ecranNoir = !_ecranNoir;
            _ecranBlanc = false;
        }
        else
        {
            _ecranBlanc = !_ecranBlanc;
            _ecranNoir = false;
        }

        if (!_ecranNoir && !_ecranBlanc)
        {
            VoileEcran.Visibility = Visibility.Collapsed;
            return;
        }

        VoileEcran.Background = _ecranNoir ? Brushes.Black : Brushes.White;
        VoileEcran.Visibility = Visibility.Visible;
    }

    private void DefinirLaser(bool actif)
    {
        _laser = actif;
        Pointeur.Visibility = actif ? Visibility.Visible : Visibility.Collapsed;
        Cursor = actif ? Cursors.None : Cursors.Arrow;
        if (actif)
        {
            DeplacerPointeur(Mouse.GetPosition(Racine));
        }
    }

    private void DeplacerPointeur(Point position)
    {
        Canvas.SetLeft(Pointeur, position.X - (Pointeur.Width / 2));
        Canvas.SetTop(Pointeur, position.Y - (Pointeur.Height / 2));
    }

    private void SurMouvementSouris(object sender, MouseEventArgs e)
    {
        if (_laser)
        {
            DeplacerPointeur(e.GetPosition(Racine));
        }
    }

    private void SurInteraction(object? sender, InteractionMedia interaction)
    {
        string message = string.Empty;

        if (interaction.Lien is not null)
        {
            ExecuteurInteraction.ExecuterLien(interaction.Lien, _presentation, AllerA, out message);
        }
        else if (interaction.Action is not null)
        {
            ExecuteurInteraction.ExecuterAction(
                interaction.Action,
                _presentation,
                _index,
                index => AllerA(index),
                Terminer,
                out message);
        }

        if (!string.IsNullOrEmpty(message))
        {
            AfficherStatut(message);
        }
    }

    private void SurZoom(object? sender, ObjetZoom zoom)
    {
        if (zoom.Cibles.Count == 0)
        {
            AfficherStatut("Ce zoom ne cible aucune diapositive.");
            return;
        }

        int cible = _sequence.ToList().FindIndex(d => d.Id == zoom.Cibles[0]);
        if (cible < 0)
        {
            AfficherStatut("La diapositive ciblée par le zoom est introuvable (elle est peut-être masquée).");
            return;
        }

        AllerA(cible);
    }

    private void AfficherStatut(string message)
    {
        IndicateurMasquage.Text = message;
        IndicateurMasquage.Visibility = Visibility.Visible;

        var minuterie = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        minuterie.Tick += (_, _) =>
        {
            minuterie.Stop();
            IndicateurMasquage.Visibility = Visibility.Collapsed;
        };
        minuterie.Start();
    }

    private void ChangerTextes(DiapositiveMedia diapositive)
    {
        _chronoTexte.Restart();
        _diapositiveChronometree = diapositive;
        IndicateurMasquage.Text = diapositive.Masquee ? "Diapositive masquée" : string.Empty;
        IndicateurMasquage.Visibility = diapositive.Masquee ? Visibility.Visible : Visibility.Collapsed;
        _ = _chronoTexte;
    }

    private void EnregistrerMinutageCourant()
    {
        if (_demande.RepetitionMinutage || _demande.EnregistrerDiaporama)
        {
            if (_diapositiveChronometree is not null)
            {
                _presentation.EnregistrerMinutage(_diapositiveChronometree.Id, _chrono.DiapositiveCourante.TotalSeconds);
            }
        }

        _chrono.DemarrerDiapositive();
    }

    private void DemarrerNarration(DiapositiveMedia diapositive)
    {
        if (!_demande.EnregistrerDiaporama || _options.SansNarration)
        {
            return;
        }

        try
        {
            string dossier = StockageMedia.DossierPour(_presentation);
            string chemin = Path.Combine(dossier, $"narration_{diapositive.Id}.wav");
            _narration = new EnregistreurAudio();
            _narration.Demarrer(chemin);
            diapositive.Narration = chemin;
        }
        catch
        {
            // Pas de microphone : l'enregistrement du minutage continue.
            _narration = null;
        }
    }

    private void ArreterNarration()
    {
        if (_narration is null)
        {
            return;
        }

        try
        {
            _narration.Arreter();
            _narration.Dispose();
        }
        catch
        {
            // Périphérique déjà libéré.
        }

        _narration = null;
    }

    private void OuvrirPresentateur()
    {
        if (_presentateur is not null)
        {
            _presentateur.Activate();
            return;
        }

        _presentateur = new FenetrePresentateur(this);
        _presentateur.Closed += (_, _) => _presentateur = null;
        _presentateur.Show();

        var ecrans = System.Windows.Forms.Screen.AllScreens;
        if (ecrans.Length > 1)
        {
            var secondaire = ecrans.FirstOrDefault(e => !e.Primary) ?? ecrans[0];
            _presentateur.Left = secondaire.WorkingArea.Left;
            _presentateur.Top = secondaire.WorkingArea.Top;
            _presentateur.Width = Math.Max(720, secondaire.WorkingArea.Width / 2);
            _presentateur.Height = Math.Max(520, secondaire.WorkingArea.Height / 2);
        }
    }

    private void AfficherAide()
    {
        AfficherStatut("Flèches/Espace : naviguer · Échap : terminer · numéro + Entrée · B/W · L : laser · M : muet · Ctrl+P : pause");
    }

    private void Terminer()
    {
        if (_termine)
        {
            return;
        }

        _termine = true;
        _minuterieAvance.Stop();
        _minuterieChrono.Stop();
        ArreterNarration();
        CoucheA.ArreterLectures();
        CoucheB.ArreterLectures();

        if (_presentateur is not null)
        {
            var fenetre = _presentateur;
            _presentateur = null;
            fenetre.Close();
        }

        Close();
    }

    private void SurFermeture(object? sender, EventArgs e)
    {
        _minuterieAvance.Stop();
        _minuterieChrono.Stop();
        ArreterNarration();
        CoucheA.ArreterLectures();
        CoucheB.ArreterLectures();

        if (_presentateur is not null)
        {
            var fenetre = _presentateur;
            _presentateur = null;
            fenetre.Close();
        }
    }

    private async void Executer(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            AfficherStatut($"Erreur pendant le diaporama : {ex.Message}");
        }
    }

    private void SurMenuSuivante(object sender, RoutedEventArgs e) => Avancer();

    private void SurMenuPrecedente(object sender, RoutedEventArgs e) => Reculer();

    private void SurMenuEcranNoir(object sender, RoutedEventArgs e) => BasculerEcran(noir: true);

    private void SurMenuEcranBlanc(object sender, RoutedEventArgs e) => BasculerEcran(noir: false);

    private void SurMenuLaser(object sender, RoutedEventArgs e) => BasculerLaser();

    private void SurMenuPresentateur(object sender, RoutedEventArgs e) => OuvrirPresentateur();

    private void SurMenuTerminer(object sender, RoutedEventArgs e) => Terminer();
}
