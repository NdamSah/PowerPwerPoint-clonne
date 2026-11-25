using System.IO;
using System.Windows;
using System.Windows.Threading;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>
/// Contrôle de l'enregistrement d'écran : démarrage, chronomètre, arrêt et restitution
/// du fichier AVI (+ piste WAV du microphone) pour insertion dans la présentation.
/// </summary>
public partial class FenetreEnregistrementEcran : Window
{
    private readonly Drawing.Rectangle _zone;
    private readonly PresentationMedia _presentation;
    private readonly DispatcherTimer _minuterie = new() { Interval = TimeSpan.FromMilliseconds(200) };
    private EnregistreurEcran? _enregistreur;
    private string? _cheminAudio;

    public FenetreEnregistrementEcran(Drawing.Rectangle zone, PresentationMedia presentation)
    {
        InitializeComponent();
        _zone = zone;
        _presentation = presentation;

        Details.Text = $"Zone : {zone.Width} × {zone.Height} px · 15 images/s";
        _minuterie.Tick += (_, _) => Rafraichir();

        Loaded += (_, _) =>
        {
            PositionnerHorsZone();
            Activate();
        };

        Closed += (_, _) =>
        {
            _minuterie.Stop();
            _enregistreur?.Dispose();
            _enregistreur = null;
        };
    }

    /// <summary>Résultat disponible lorsque l'utilisateur arrête l'enregistrement.</summary>
    public ResultatCaptureEcran? Resultat { get; private set; }

    private void SurDemarrer(object sender, RoutedEventArgs e)
    {
        try
        {
            string dossier = StockageMedia.DossierPour(_presentation);
            string horodatage = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string video = Path.Combine(dossier, $"capture_{horodatage}.avi");
            _cheminAudio = Micro.IsChecked == true
                ? Path.Combine(dossier, $"capture_{horodatage}.wav")
                : null;

            _enregistreur = new EnregistreurEcran();
            _enregistreur.Erreur += (_, message) => Message.Text = message;
            _enregistreur.Demarrer(_zone, 15, video, _cheminAudio);

            _minuterie.Start();
            BoutonDemarrer.IsEnabled = false;
            BoutonArreter.IsEnabled = true;
            Micro.IsEnabled = false;
            Message.Text = "Enregistrement en cours…";
        }
        catch (Exception ex)
        {
            Message.Text = $"Impossible de démarrer : {ex.Message}";
        }
    }

    private void SurArreter(object sender, RoutedEventArgs e)
    {
        if (_enregistreur is null)
        {
            DialogResult = false;
            return;
        }

        Message.Text = "Finalisation du fichier…";
        var duree = _enregistreur.Duree;
        string? video = _enregistreur.Arreter();

        if (video is null || !File.Exists(video))
        {
            Message.Text = "Aucune image capturée.";
            BoutonArreter.IsEnabled = false;
            BoutonDemarrer.IsEnabled = true;
            Micro.IsEnabled = true;
            return;
        }

        Resultat = new ResultatCaptureEcran(
            video,
            _cheminAudio is not null && File.Exists(_cheminAudio) ? _cheminAudio : null,
            duree);

        DialogResult = true;
    }

    private void SurAnnuler(object sender, RoutedEventArgs e)
    {
        _enregistreur?.Arreter();
        DialogResult = false;
    }

    private void Rafraichir()
    {
        if (_enregistreur is null)
        {
            return;
        }

        Chrono.Text = ChronometreDiaporama.Formater(_enregistreur.Duree);
        Details.Text = $"{_enregistreur.ImagesCapturees} image(s) · {_zone.Width} × {_zone.Height} px";
    }

    /// <summary>Évite de capturer la fenêtre de contrôle elle-même.</summary>
    private void PositionnerHorsZone()
    {
        var ecran = WinForms.Screen.FromRectangle(_zone).WorkingArea;
        double largeur = ActualWidth > 0 ? ActualWidth : Width;
        double hauteur = ActualHeight > 0 ? ActualHeight : Height;

        double x = ecran.Right - largeur - 12;
        double y = ecran.Bottom - hauteur - 12;

        bool chevauche = x < _zone.Right && x + largeur > _zone.Left && y < _zone.Bottom && y + hauteur > _zone.Top;
        if (chevauche)
        {
            x = ecran.Left + 12;
            y = ecran.Top + 12;
        }

        Left = x;
        Top = y;
    }
}
