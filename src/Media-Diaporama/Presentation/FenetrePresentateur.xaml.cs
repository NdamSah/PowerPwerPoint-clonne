using System.Windows;
using System.Windows.Threading;
using PowerPwerPoint.Media.Services;
using PowerPwerPoint.Media.Vues;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>
/// Fenêtre du mode Présentateur : diapositive courante, diapositive suivante, notes,
/// chronomètres et commandes de navigation. Se place sur le second écran si disponible.
/// </summary>
public partial class FenetrePresentateur : Window
{
    private readonly FenetreDiaporama _diaporama;
    private readonly DispatcherTimer _minuterie = new() { Interval = TimeSpan.FromMilliseconds(250) };

    public FenetrePresentateur(FenetreDiaporama diaporama)
    {
        InitializeComponent();
        _diaporama = diaporama;

        _minuterie.Tick += (_, _) => MettreAJourChrono();
        _minuterie.Start();

        _diaporama.DiapositiveChangee += (_, _) => Rafraichir();
        Loaded += (_, _) => Rafraichir();
        Closed += (_, _) => _minuterie.Stop();
    }

    private void Rafraichir()
    {
        var courante = _diaporama.DiapositiveCourante;
        var suivante = _diaporama.DiapositiveSuivante;

        RenduCourant.Presentation = _diaporama.Presentation;
        RenduCourant.Charger(courante, _diaporama.Presentation, ModeRendu.Edition);

        RenduSuivant.Presentation = _diaporama.Presentation;
        RenduSuivant.Charger(suivante, _diaporama.Presentation, ModeRendu.Edition);

        NotesDiapositive.Text = string.IsNullOrWhiteSpace(courante?.Notes)
            ? "Aucune note pour cette diapositive."
            : courante.Notes;

        NumeroDiapositive.Text = $"Diapositive {_diaporama.Index + 1} / {_diaporama.Total}";

        BoutonSuivant.IsEnabled = _diaporama.Index + 1 < _diaporama.Total;
        BoutonPrecedent.IsEnabled = _diaporama.Index > 0;

        MettreAJourChrono();
    }

    private void MettreAJourChrono()
    {
        ChronoTotal.Text = ChronometreDiaporama.Formater(_diaporama.Chronometre.Total);
        ChronoDiapositive.Text = $"diapositive {ChronometreDiaporama.Formater(_diaporama.Chronometre.DiapositiveCourante)}";
        BoutonPause.Content = _diaporama.Chronometre.EnPause ? "Reprendre" : "Pause";
    }

    private void SurSuivant(object sender, RoutedEventArgs e)
    {
        _diaporama.Avancer();
        RendreFocus();
    }

    private void SurPrecedent(object sender, RoutedEventArgs e)
    {
        _diaporama.Reculer();
        RendreFocus();
    }

    private void SurPause(object sender, RoutedEventArgs e)
    {
        _diaporama.BasculerPause();
        MettreAJourChrono();
        RendreFocus();
    }

    private void SurTerminer(object sender, RoutedEventArgs e) => _diaporama.TerminerDiaporama();

    /// <summary>Le diaporama doit garder le clavier : on lui rend le focus après chaque commande.</summary>
    private void RendreFocus()
    {
        _diaporama.Activate();
        _diaporama.Focus();
    }
}
