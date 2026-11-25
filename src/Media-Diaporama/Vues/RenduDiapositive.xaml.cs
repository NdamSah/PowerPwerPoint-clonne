using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.Vues;

public enum ModeRendu
{
    Edition,
    Diaporama
}

public partial class RenduDiapositive : UserControl
{
    private readonly Dictionary<string, FrameworkElement> _visuels = new();
    private readonly List<LecteurVideo> _lecteurs = new();
    private readonly List<MediaPlayer> _sons = new();
    private DiapositiveMedia? _diapositive;
    private ElementDiapositive? _elementSelectionne;

    public RenduDiapositive()
    {
        InitializeComponent();
    }

    /// <summary>Mode de rendu : édition (aperçu) ou diaporama (lecture des médias et interactions).</summary>
    public ModeRendu Mode { get; set; } = ModeRendu.Edition;

    public PresentationMedia? Presentation { get; set; }

    public DiapositiveMedia? Diapositive => _diapositive;

    /// <summary>Élément cliqué dans l'aperçu (sélection pour les animations).</summary>
    public event EventHandler<ElementDiapositive?>? ElementClique;

    /// <summary>Objet média cliqué en édition (vidéo, zoom…).</summary>
    public event EventHandler<ObjetMedia>? ObjetClique;

    /// <summary>Interaction à exécuter pendant le diaporama (lien ou action).</summary>
    public event EventHandler<InteractionMedia>? InteractionDemandee;

    /// <summary>Zoom activé pendant le diaporama.</summary>
    public event EventHandler<ObjetZoom>? ZoomActive;

    public event EventHandler<string>? Statut;

    public FrameworkElement? Resoudre(string? id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        return _visuels.TryGetValue(id, out var visuel) ? visuel : null;
    }

    public void Charger(DiapositiveMedia? diapositive, PresentationMedia? presentation, ModeRendu mode)
    {
        ArreterLectures();
        Mode = mode;
        Presentation = presentation;
        _diapositive = diapositive;

        Zone.Children.Clear();
        _visuels.Clear();
        _elementSelectionne = null;

        if (diapositive is null)
        {
            return;
        }

        foreach (var element in diapositive.Elements)
        {
            var visuel = CreerVisuelElement(element);
            Zone.Children.Add(visuel);
            _visuels[element.Id] = visuel;
        }

        foreach (var objet in diapositive.Objets)
        {
            var visuel = CreerVisuelObjet(objet);
            if (visuel is null)
            {
                continue;
            }

            Zone.Children.Add(visuel);
            _visuels[objet.Id] = visuel;
        }

        EnregistrerInteractions();
    }

    /// <summary>Prépare les animations (masque les cibles dont la première animation est une entrée).</summary>
    public void PreparerAnimations()
    {
        if (_diapositive is null)
        {
            return;
        }

        MoteurAnimations.PreparerDiapositive(_diapositive, Resoudre);
    }

    public void MettreEnEvidence(ElementDiapositive? element)
    {
        _elementSelectionne = element;
        foreach (var pair in _visuels)
        {
            if (pair.Value is Border bordure && bordure.Tag is ElementDiapositive modele)
            {
                bordure.BorderThickness = new Thickness(element is not null && modele.Id == element.Id ? 3 : 0);
                bordure.BorderBrush = Couleurs.Pinceau("#FF2F6FED");
            }
        }
    }

    public void DemarrerMediasAutomatiques()
    {
        if (_diapositive is null)
        {
            return;
        }

        foreach (var objet in _diapositive.Objets)
        {
            switch (objet)
            {
                case ObjetVideo video when video.Demarrage != DemarrageMedia.AuClic:
                    LireVideo(video);
                    break;
                case ObjetAudio audio when audio.Demarrage != DemarrageMedia.AuClic:
                    LireAudio(audio);
                    break;
            }
        }

        JouerNarration();
    }

    public void ArreterLectures()
    {
        foreach (var lecteur in _lecteurs)
        {
            lecteur.Arreter();
        }

        _lecteurs.Clear();

        foreach (var son in _sons)
        {
            try
            {
                son.Stop();
                son.Close();
            }
            catch
            {
                // Lecteur déjà libéré.
            }
        }

        _sons.Clear();
    }

    /// <summary>Coupe ou relance tout le son (touche muet du diaporama).</summary>
    public void BasculerMuet(bool muet)
    {
        foreach (var lecteur in _lecteurs)
        {
            lecteur.DefinirMuet(muet);
        }

        foreach (var son in _sons)
        {
            son.IsMuted = muet;
        }
    }

    private FrameworkElement CreerVisuelElement(ElementDiapositive element)
    {
        var bordure = new Border
        {
            Width = element.Largeur,
            Height = element.Hauteur,
            Background = Couleurs.Pinceau(element.CouleurFond, Colors.Transparent),
            CornerRadius = new CornerRadius(6),
            Tag = element,
            Cursor = Cursors.Hand
        };

        var texte = new TextBlock
        {
            Text = element.Texte,
            Foreground = Couleurs.Pinceau(element.CouleurTexte, Colors.Black),
            FontSize = element.TaillePolice,
            FontFamily = new FontFamily("Segoe UI"),
            FontWeight = element.TaillePolice >= 28 ? FontWeights.SemiBold : FontWeights.Normal,
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = element.AlignementTexte?.ToLowerInvariant() switch
            {
                "left" or "gauche" => TextAlignment.Left,
                "right" or "droite" => TextAlignment.Right,
                _ => TextAlignment.Center
            },
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(14, 8, 14, 8)
        };

        bordure.Child = texte;
        Canvas.SetLeft(bordure, element.X);
        Canvas.SetTop(bordure, element.Y);

        bordure.MouseLeftButtonDown += (_, e) =>
        {
            if (Mode != ModeRendu.Edition)
            {
                return;
            }

            e.Handled = true;
            MettreEnEvidence(element);
            ElementClique?.Invoke(this, element);
        };

        return bordure;
    }

    private FrameworkElement? CreerVisuelObjet(ObjetMedia objet)
    {
        switch (objet)
        {
            case ObjetVideo video:
                return Mode == ModeRendu.Diaporama ? CreerVideoDiaporama(video) : CreerVideoEdition(video);
            case ObjetAudio audio:
                return CreerAudio(audio);
            case ObjetZoom zoom:
                return CreerZoom(zoom);
            default:
                return null;
        }
    }

    private FrameworkElement CreerVideoEdition(ObjetVideo video)
    {
        var grille = new Grid { Width = video.Largeur, Height = video.Hauteur, Cursor = Cursors.Hand };
        grille.Children.Add(new Border
        {
            Background = Couleurs.Pinceau("#FF15192A"),
            CornerRadius = new CornerRadius(8)
        });

        var pile = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center
        };
        pile.Children.Add(new TextBlock
        {
            Text = "\uE714",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 34,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        pile.Children.Add(new TextBlock
        {
            Text = Path.GetFileName(video.Chemin),
            Foreground = Brushes.White,
            FontSize = 13,
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = Math.Max(120, video.Largeur - 30)
        });
        pile.Children.Add(new TextBlock
        {
            Text = video.EstRogne ? video.ResumeRognage : "Cliquez pour lire ou rogner",
            Foreground = Couleurs.Pinceau("#FFB9C1D4"),
            FontSize = 11,
            Margin = new Thickness(0, 4, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        grille.Children.Add(pile);

        grille.MouseLeftButtonDown += (_, e) =>
        {
            e.Handled = true;
            ObjetClique?.Invoke(this, video);
        };

        Canvas.SetLeft(grille, video.X);
        Canvas.SetTop(grille, video.Y);
        return grille;
    }

    private FrameworkElement CreerVideoDiaporama(ObjetVideo video)
    {
        var conteneur = new Grid
        {
            Width = video.Largeur,
            Height = video.Hauteur,
            Background = Couleurs.Pinceau("#FF000000")
        };

        string? chemin = StockageMedia.Resoudre(video.Chemin, Presentation ?? new PresentationMedia());
        var media = new MediaElement
        {
            LoadedBehavior = MediaState.Manual,
            UnloadedBehavior = MediaState.Manual,
            Stretch = Stretch.Uniform,
            Volume = video.Muet ? 0 : video.Volume / 100.0
        };

        if (chemin is not null)
        {
            media.Source = new Uri(chemin, UriKind.Absolute);
        }

        conteneur.Children.Add(media);

        var bouton = new Border
        {
            Background = Couleurs.Pinceau("#66000000"),
            CornerRadius = new CornerRadius(6),
            Width = 64,
            Height = 64,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Visibility = video.AfficherCommandes ? Visibility.Visible : Visibility.Collapsed,
            Cursor = Cursors.Hand
        };
        bouton.Child = new TextBlock
        {
            Text = "\uE768",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 26,
            Foreground = Brushes.White,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        conteneur.Children.Add(bouton);

        var lecteur = new LecteurVideo(media, video, bouton, chemin);
        lecteur.Statut += (_, message) => Statut?.Invoke(this, message);
        _lecteurs.Add(lecteur);

        bouton.MouseLeftButtonDown += (_, e) =>
        {
            e.Handled = true;
            lecteur.Basculer();
        };

        conteneur.MouseLeftButtonDown += (_, e) => e.Handled = true;
        conteneur.MouseLeftButtonUp += (_, e) => e.Handled = true;
        bouton.MouseLeftButtonUp += (_, e) => e.Handled = true;

        Canvas.SetLeft(conteneur, video.X);
        Canvas.SetTop(conteneur, video.Y);
        return conteneur;
    }

    private FrameworkElement CreerAudio(ObjetAudio audio)
    {
        var grille = new Grid
        {
            Width = Math.Min(audio.Largeur, 320),
            Height = Math.Min(audio.Hauteur, 90),
            Cursor = Cursors.Hand,
            Visibility = Mode == ModeRendu.Diaporama ? Visibility.Collapsed : Visibility.Visible
        };

        grille.Children.Add(new Border
        {
            Background = Couleurs.Pinceau("#FF1F2430"),
            CornerRadius = new CornerRadius(8)
        });

        var pile = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 12, 0)
        };
        pile.Children.Add(new TextBlock
        {
            Text = "\uE8D6",
            FontFamily = new FontFamily("Segoe MDL2 Assets"),
            FontSize = 22,
            Foreground = Brushes.White,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        });

        var textes = new StackPanel();
        textes.Children.Add(new TextBlock
        {
            Text = audio.Libelle,
            Foreground = Brushes.White,
            FontSize = 12,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 230
        });
        textes.Children.Add(new TextBlock
        {
            Text = audio.LireEnArrierePlan ? "Arrière-plan" : "Sur la diapositive",
            Foreground = Couleurs.Pinceau("#FFB9C1D4"),
            FontSize = 11
        });
        pile.Children.Add(textes);
        grille.Children.Add(pile);

        grille.MouseLeftButtonDown += (_, e) =>
        {
            e.Handled = true;
            if (Mode == ModeRendu.Edition)
            {
                ObjetClique?.Invoke(this, audio);
            }
        };

        Canvas.SetLeft(grille, audio.X);
        Canvas.SetTop(grille, audio.Y);
        return grille;
    }

    private FrameworkElement CreerZoom(ObjetZoom zoom)
    {
        var grille = new Grid
        {
            Width = zoom.Largeur,
            Height = zoom.Hauteur,
            Cursor = Cursors.Hand
        };

        grille.Children.Add(new Border
        {
            Background = Couleurs.Pinceau(zoom.CouleurFond, Colors.Black),
            CornerRadius = new CornerRadius(10)
        });

        var pile = new StackPanel
        {
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(20)
        };
        pile.Children.Add(new TextBlock
        {
            Text = zoom.Libelle,
            Foreground = Brushes.White,
            FontSize = 26,
            FontWeight = FontWeights.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center
        });
        pile.Children.Add(new TextBlock
        {
            Text = $"{LibellesEnum.LibelleTypeZoom(zoom.TypeZoom)} · {zoom.ResumeCibles}",
            Foreground = Couleurs.Pinceau("#FFB9C1D4"),
            FontSize = 14,
            Margin = new Thickness(0, 6, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        pile.Children.Add(new TextBlock
        {
            Text = Mode == ModeRendu.Diaporama ? "Cliquez pour accéder à la section" : "Double-clic pour modifier la cible",
            Foreground = Couleurs.Pinceau("#FF7FA6FF"),
            FontSize = 12,
            Margin = new Thickness(0, 14, 0, 0),
            HorizontalAlignment = HorizontalAlignment.Center
        });
        grille.Children.Add(pile);

        grille.MouseLeftButtonDown += (_, e) =>
        {
            e.Handled = true;
            if (Mode == ModeRendu.Diaporama)
            {
                ZoomActive?.Invoke(this, zoom);
            }
            else
            {
                ObjetClique?.Invoke(this, zoom);
            }
        };

        Canvas.SetLeft(grille, zoom.X);
        Canvas.SetTop(grille, zoom.Y);
        return grille;
    }

    private void EnregistrerInteractions()
    {
        if (_diapositive is null)
        {
            return;
        }

        foreach (var lien in _diapositive.Objets.OfType<ObjetLien>())
        {
            var support = string.IsNullOrEmpty(lien.ElementId) ? Zone : Resoudre(lien.ElementId);
            if (support is null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(lien.InfoBulle))
            {
                support.ToolTip = lien.InfoBulle;
            }

            support.Cursor = Cursors.Hand;
            var cible = support;
            cible.MouseLeftButtonDown += (_, e) =>
            {
                if (Mode != ModeRendu.Diaporama)
                {
                    return;
                }

                e.Handled = true;
                InteractionDemandee?.Invoke(this, new InteractionMedia(lien, null));
            };
        }

        foreach (var action in _diapositive.Objets.OfType<ObjetAction>())
        {
            var support = string.IsNullOrEmpty(action.ElementId) ? Zone : Resoudre(action.ElementId);
            if (support is null)
            {
                continue;
            }

            if (action.Declencheur == DeclencheurAction.Clic)
            {
                support.MouseLeftButtonDown += (_, e) =>
                {
                    if (Mode != ModeRendu.Diaporama)
                    {
                        return;
                    }

                    e.Handled = true;
                    InteractionDemandee?.Invoke(this, new InteractionMedia(null, action));
                };
            }
            else
            {
                support.MouseEnter += (_, _) =>
                {
                    if (Mode == ModeRendu.Diaporama)
                    {
                        InteractionDemandee?.Invoke(this, new InteractionMedia(null, action));
                    }
                };
            }
        }
    }

    private void JouerNarration()
    {
        if (Presentation?.Options.SansNarration == true || string.IsNullOrEmpty(_diapositive?.Narration))
        {
            return;
        }

        string? chemin = _diapositive.Narration;
        if (chemin is null || !File.Exists(chemin))
        {
            return;
        }

        try
        {
            var lecteur = new MediaPlayer();
            lecteur.Open(new Uri(chemin, UriKind.Absolute));
            lecteur.Play();
            _sons.Add(lecteur);
        }
        catch
        {
            // Narration illisible : le diaporama continue silencieusement.
        }
    }

    public void LireVideo(ObjetVideo video)
    {
        var lecteur = _lecteurs.FirstOrDefault(l => ReferenceEquals(l.Modele, video));
        lecteur?.Lire();
    }

    public void LireAudio(ObjetAudio audio)
    {
        string? chemin = StockageMedia.Resoudre(audio.Chemin, Presentation ?? new PresentationMedia());
        if (chemin is null)
        {
            return;
        }

        try
        {
            var lecteur = new MediaPlayer();
            lecteur.Open(new Uri(chemin, UriKind.Absolute));
            lecteur.Volume = audio.Muet ? 0 : audio.Volume / 100.0;
            if (audio.LireEnBoucle)
            {
                lecteur.MediaEnded += (_, _) =>
                {
                    lecteur.Position = TimeSpan.Zero;
                    lecteur.Play();
                };
            }

            lecteur.Play();
            _sons.Add(lecteur);
        }
        catch
        {
            StaticStatut("Lecture audio impossible.");
        }
    }

    private void StaticStatut(string message) => Statut?.Invoke(this, message);

    private void SurClicZone(object sender, MouseButtonEventArgs e)
    {
        if (Mode == ModeRendu.Edition)
        {
            MettreEnEvidence(null);
            ElementClique?.Invoke(this, null);
        }
    }

    /// <summary>Gestion de lecture d'une vidéo avec rognage et options.</summary>
    private sealed class LecteurVideo
    {
        private readonly MediaElement _media;
        private readonly Border _bouton;
        private readonly DispatcherTimer _minuterie;
        private readonly string? _chemin;
        private bool _enLecture;

        public LecteurVideo(MediaElement media, ObjetVideo modele, Border bouton, string? chemin)
        {
            _media = media;
            Modele = modele;
            _bouton = bouton;
            _chemin = chemin;

            _media.MediaOpened += (_, _) => AppliquerPositionDebut();
            _media.MediaEnded += (_, _) => FinDeLecture();

            _minuterie = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(200) };
            _minuterie.Tick += (_, _) => SurveillerRognage();
            _minuterie.Start();
        }

        public ObjetVideo Modele { get; }

        public event EventHandler<string>? Statut;

        public void Lire()
        {
            if (_chemin is null)
            {
                Statut?.Invoke(this, $"Vidéo introuvable : {Path.GetFileName(Modele.Chemin)}");
                return;
            }

            try
            {
                if (_media.Position == TimeSpan.Zero || _media.Position < Modele.DebutRognage)
                {
                    AppliquerPositionDebut();
                }

                _media.Play();
                _enLecture = true;
                _bouton.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Statut?.Invoke(this, $"Lecture vidéo impossible : {ex.Message}");
            }
        }

        public void Basculer()
        {
            if (_enLecture)
            {
                _media.Pause();
                _enLecture = false;
                _bouton.Visibility = Modele.AfficherCommandes ? Visibility.Visible : Visibility.Collapsed;
                return;
            }

            Lire();
        }

        public void DefinirMuet(bool muet)
        {
            _media.Volume = muet ? 0 : Modele.Volume / 100.0;
        }

        public void Arreter()
        {
            _minuterie.Stop();
            try
            {
                _media.Stop();
                _media.Close();
            }
            catch
            {
                // Média déjà fermé.
            }
        }

        private TimeSpan Fin()
            => Modele.FinRognage ?? (_media.NaturalDuration.HasTimeSpan ? _media.NaturalDuration.TimeSpan : TimeSpan.MaxValue);

        private void AppliquerPositionDebut()
        {
            if (_media.NaturalDuration.HasTimeSpan && Modele.DebutRognage < _media.NaturalDuration.TimeSpan)
            {
                _media.Position = Modele.DebutRognage;
            }
        }

        private void SurveillerRognage()
        {
            if (!_media.NaturalDuration.HasTimeSpan)
            {
                return;
            }

            var fin = Modele.FinRognage ?? _media.NaturalDuration.TimeSpan;
            if (_media.Position < fin || _media.Position == TimeSpan.Zero)
            {
                return;
            }

            if (Modele.LireEnBoucle)
            {
                _media.Position = Modele.DebutRognage;
                _media.Play();
                return;
            }

            FinDeLecture();
        }

        private void FinDeLecture()
        {
            _enLecture = false;
            _media.Pause();
            if (Modele.RembobinerApresLecture)
            {
                _media.Position = Modele.DebutRognage;
            }

            _bouton.Visibility = Modele.AfficherCommandes ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}

/// <summary>Interaction demandée par un clic sur un élément porteur de lien ou d'action.</summary>
public sealed record InteractionMedia(ObjetLien? Lien, ObjetAction? Action);
