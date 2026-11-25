using System.IO;
using System.Windows;
using System.Windows.Threading;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>
/// Lecteur vidéo intégré : lecture, pause, déplacement et définition des points de rognage
/// (début / fin) enregistrés dans le modèle sans réencoder le fichier.
/// </summary>
public partial class LecteurVideoWindow : Window
{
    private readonly ObjetVideo _video;
    private readonly PresentationMedia _presentation;
    private readonly DispatcherTimer _minuterie = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private bool _deplacementEnCours;
    private bool _initialise;

    public LecteurVideoWindow(ObjetVideo video, PresentationMedia presentation)
    {
        // Affectés avant InitializeComponent : le Slider de volume déclenche ValueChanged
        // pendant le chargement du XAML, et le gestionnaire lit _video.
        _video = video;
        _presentation = presentation;

        InitializeComponent();

        NomFichier.Text = string.IsNullOrEmpty(video.Chemin) ? "Aucun fichier" : video.Chemin;
        Volume.Value = video.Volume;
        Boucle.IsChecked = video.LireEnBoucle;
        ResumeRognage.Text = video.ResumeRognage;

        _minuterie.Tick += (_, _) => MettreAJourPosition();
        _minuterie.Start();

        Loaded += SurChargement;
        Closed += (_, _) =>
        {
            _minuterie.Stop();
            try
            {
                Lecteur.Stop();
                Lecteur.Close();
            }
            catch
            {
                // Lecteur déjà libéré.
            }
        };
    }

    private void SurChargement(object sender, RoutedEventArgs e)
    {
        string? chemin = StockageMedia.Resoudre(_video.Chemin, _presentation);
        if (chemin is null)
        {
            MessageBox.Show(this, "Le fichier vidéo est introuvable.", "Vidéo", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Lecteur.Source = new Uri(chemin, UriKind.Absolute);
        Lecteur.Volume = _video.Volume / 100.0;
        Lecteur.MediaOpened += (_, _) => InitialiserBornes();
        Lecteur.MediaEnded += (_, _) => FinDeLecture();
    }

    private void InitialiserBornes()
    {
        if (!Lecteur.NaturalDuration.HasTimeSpan)
        {
            return;
        }

        double total = Lecteur.NaturalDuration.TimeSpan.TotalSeconds;
        Position.Maximum = total;
        BorneDebut.Maximum = total;
        BorneFin.Maximum = total;
        BorneDebut.Value = Math.Clamp(_video.DebutRognage.TotalSeconds, 0, total);
        BorneFin.Value = Math.Clamp(_video.FinRognage?.TotalSeconds ?? total, 0, total);
        TempsTotal.Text = Formater(TimeSpan.FromSeconds(total));
        _initialise = true;
        ResumeRognage.Text = _video.ResumeRognage;

        if (BorneDebut.Value > 0)
        {
            Lecteur.Position = TimeSpan.FromSeconds(BorneDebut.Value);
        }
    }

    private void MettreAJourPosition()
    {
        if (!_initialise || _deplacementEnCours)
        {
            return;
        }

        Position.Value = Lecteur.Position.TotalSeconds;
        TempsCourant.Text = Formater(Lecteur.Position);

        double fin = BorneFin.Value;
        if (fin > 0 && Lecteur.Position.TotalSeconds >= fin)
        {
            FinDeLecture();
        }
    }

    private void FinDeLecture()
    {
        Lecteur.Pause();
        _ = Dispatcher.BeginInvoke(new Action(() =>
        {
            Lecteur.Position = TimeSpan.FromSeconds(BorneDebut.Value);
        }));
    }

    private static string Formater(TimeSpan duree) => $"{(int)duree.TotalMinutes:00}:{duree.Seconds:00}";

    private void SurDebutDeplacement(object sender, RoutedEventArgs e) => _deplacementEnCours = true;

    private void SurFinDeplacement(object sender, RoutedEventArgs e)
    {
        _deplacementEnCours = false;
        if (_initialise)
        {
            Lecteur.Position = TimeSpan.FromSeconds(Position.Value);
        }
    }

    private void SurBorneModifiee(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!_initialise)
        {
            return;
        }

        if (BorneDebut.Value >= BorneFin.Value)
        {
            if (ReferenceEquals(sender, BorneDebut))
            {
                BorneFin.Value = Math.Min(BorneFin.Maximum, BorneDebut.Value + 0.5);
            }
            else
            {
                BorneDebut.Value = Math.Max(0, BorneFin.Value - 0.5);
            }
        }

        _video.DebutRognage = TimeSpan.FromSeconds(BorneDebut.Value);
        _video.FinRognage = BorneFin.Value >= BorneFin.Maximum - 0.05
            ? null
            : TimeSpan.FromSeconds(BorneFin.Value);
        ResumeRognage.Text = _video.ResumeRognage;
    }

    private void SurDefinirDebut(object sender, RoutedEventArgs e)
    {
        BorneDebut.Value = Math.Max(0, Lecteur.Position.TotalSeconds);
        ResumeRognage.Text = _video.ResumeRognage;
    }

    private void SurDefinirFin(object sender, RoutedEventArgs e)
    {
        BorneFin.Value = Math.Min(BorneFin.Maximum, Lecteur.Position.TotalSeconds + 0.1);
        ResumeRognage.Text = _video.ResumeRognage;
    }

    private void SurReinitialiser(object sender, RoutedEventArgs e)
    {
        BorneDebut.Value = 0;
        BorneFin.Value = BorneFin.Maximum;
        _video.DebutRognage = TimeSpan.Zero;
        _video.FinRognage = null;
        ResumeRognage.Text = _video.ResumeRognage;
    }

    private void SurLecture(object sender, RoutedEventArgs e)
    {
        if (Lecteur.Position.TotalSeconds < BorneDebut.Value || Lecteur.Position.TotalSeconds >= BorneFin.Value)
        {
            Lecteur.Position = TimeSpan.FromSeconds(BorneDebut.Value);
        }

        Lecteur.Play();
        BoutonLecture.Content = "⏸ Pause";
    }

    private void SurArreter(object sender, RoutedEventArgs e)
    {
        Lecteur.Stop();
        BoutonLecture.Content = "▶ Lire";
    }

    private void SurVolumeModifie(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Lecteur is null)
        {
            return;
        }

        Lecteur.Volume = Volume.Value / 100.0;
        _video.Volume = (int)Volume.Value;
    }

    private void SurFermer(object sender, RoutedEventArgs e)
    {
        _video.LireEnBoucle = Boucle.IsChecked == true;
        Close();
    }
}
