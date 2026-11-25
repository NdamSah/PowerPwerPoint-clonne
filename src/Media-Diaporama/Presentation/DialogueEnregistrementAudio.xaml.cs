using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Threading;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Boîte « Enregistrer un son » : capture micro, indicateur de niveau, réécoute et validation.</summary>
public partial class DialogueEnregistrementAudio : Window
{
    private readonly PresentationMedia _presentation;
    private readonly DispatcherTimer _minuterie = new() { Interval = TimeSpan.FromMilliseconds(120) };
    private EnregistreurAudio? _enregistreur;
    private string? _chemin;

    public DialogueEnregistrementAudio(PresentationMedia presentation)
    {
        InitializeComponent();
        _presentation = presentation;
        _minuterie.Tick += (_, _) => RafraichirChrono();
        _minuterie.Start();
        Closed += (_, _) =>
        {
            _minuterie.Stop();
            _enregistreur?.Dispose();
        };
    }

    /// <summary>Chemin du WAV retenu, si l'utilisateur valide.</summary>
    public string? CheminRetenu => _chemin;

    private void SurEnregistrer(object sender, RoutedEventArgs e)
    {
        try
        {
            string dossier = StockageMedia.DossierPour(_presentation);
            _chemin = Path.Combine(dossier, $"son_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

            _enregistreur = new EnregistreurAudio();
            _enregistreur.NiveauChange += (_, niveau) => Niveau.Value = niveau * 100;
            _enregistreur.Erreur += (_, message) => Message.Text = message;
            _enregistreur.Demarrer(_chemin);

            BoutonEnregistrer.IsEnabled = false;
            BoutonArreter.IsEnabled = true;
            BoutonValider.IsEnabled = false;
            Message.Text = "Enregistrement en cours…";
        }
        catch (Exception ex)
        {
            Message.Text = $"Microphone indisponible : {ex.Message}";
        }
    }

    private void SurArreter(object sender, RoutedEventArgs e)
    {
        _enregistreur?.Arreter();
        BoutonEnregistrer.IsEnabled = true;
        BoutonArreter.IsEnabled = false;
        BoutonLire.IsEnabled = _chemin is not null;
        BoutonValider.IsEnabled = _chemin is not null;
        Message.Text = _chemin is null ? "Aucun enregistrement." : $"Enregistré : {Path.GetFileName(_chemin)}";
    }

    private void SurLire(object sender, RoutedEventArgs e)
    {
        if (_chemin is null)
        {
            return;
        }

        using var lecteur = new SoundPlayer(_chemin);
        lecteur.Play();
        Message.Text = "Réécoute…";
    }

    private void SurValider(object sender, RoutedEventArgs e)
    {
        if (_chemin is null || !File.Exists(_chemin))
        {
            Message.Text = "Enregistrez d'abord un son.";
            return;
        }

        _enregistreur?.Arreter();
        DialogResult = true;
    }

    private void SurAnnuler(object sender, RoutedEventArgs e)
    {
        _enregistreur?.Arreter();
        if (_chemin is not null && File.Exists(_chemin))
        {
            try
            {
                File.Delete(_chemin);
            }
            catch
            {
                // Fichier temporaire non supprimable : sans conséquence.
            }
        }

        DialogResult = false;
    }

    private void RafraichirChrono()
    {
        var duree = _enregistreur?.Duree ?? TimeSpan.Zero;
        Chrono.Text = ChronometreDiaporama.Formater(duree);
    }
}
