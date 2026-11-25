using System.IO;
using System.Media;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Moteur d'exécution des transitions : construit un <see cref="Storyboard"/> qui anime la
/// diapositive entrante (et, pour les effets de type poussée, la diapositive sortante).
/// Le sens (<see cref="SensTransition"/>) indique d'où arrive la nouvelle diapositive.
/// </summary>
public static class MoteurTransitions
{
    public const double LargeurReference = 960;
    public const double HauteurReference = 540;

    private static readonly List<SoundPlayer> SonsActifs = new();

    public static TimeSpan DureeReelle(TransitionDiapositive transition)
    {
        if (transition.Effet == TransitionEffet.Aucune)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromSeconds(Math.Clamp(transition.Duree, 0.1, 10));
    }

    /// <summary>Joue la transition puis laisse la diapositive entrante dans son état normal.</summary>
    public static async Task ExecuterAsync(
        TransitionDiapositive transition,
        FrameworkElement? sortante,
        FrameworkElement entrante,
        CancellationToken jeton = default)
    {
        ArgumentNullException.ThrowIfNull(transition);
        ArgumentNullException.ThrowIfNull(entrante);

        TransformsElements.Reinitialiser(entrante);
        if (sortante is not null)
        {
            TransformsElements.Reinitialiser(sortante);
        }

        var duree = DureeReelle(transition);
        if (duree == TimeSpan.Zero || jeton.IsCancellationRequested)
        {
            return;
        }

        JouerSon(transition);

        var storyboard = Construire(transition, sortante, entrante, duree);
        await JouerAsync(storyboard, entrante, duree + TimeSpan.FromSeconds(0.3), jeton).ConfigureAwait(true);

        TransformsElements.Reinitialiser(entrante);
        if (sortante is not null)
        {
            TransformsElements.Reinitialiser(sortante);
        }
    }

    /// <summary>Construit le storyboard sans le jouer (utilisé par les tests et l'aperçu).</summary>
    public static Storyboard Construire(
        TransitionDiapositive transition,
        FrameworkElement? sortante,
        FrameworkElement entrante,
        TimeSpan duree)
    {
        var storyboard = new Storyboard();
        var (echelleEntrante, rotationEntrante, translationEntrante) = TransformsElements.Obtenir(entrante);

        double largeur = LargeurDe(entrante);
        double hauteur = HauteurDe(entrante);
        var d = new Duration(duree);
        var douce = new CubicEase { EasingMode = EasingMode.EaseInOut };

        ScaleTransform? echelleSortante = null;
        TranslateTransform? translationSortante = null;
        if (sortante is not null)
        {
            (echelleSortante, _, translationSortante) = TransformsElements.Obtenir(sortante);
        }

        switch (transition.Effet)
        {
            case TransitionEffet.Fondu:
                entrante.Opacity = 0;
                Animer(storyboard, entrante, "Opacity", new DoubleAnimation(0, 1, d));
                break;

            case TransitionEffet.FonduEnchaine:
                entrante.Opacity = 0;
                Animer(storyboard, entrante, "Opacity", new DoubleAnimation(0, 1, d));
                if (sortante is not null)
                {
                    Animer(storyboard, sortante, "Opacity", new DoubleAnimation(1, 0, d));
                }

                break;

            case TransitionEffet.Poussee:
            {
                var (debut, _) = Deplacement(transition.Sens, largeur, hauteur);
                entrante.Opacity = 1;
                Animer(storyboard, translationEntrante, "X", new DoubleAnimation(debut.X, 0, d) { EasingFunction = douce });
                Animer(storyboard, translationEntrante, "Y", new DoubleAnimation(debut.Y, 0, d) { EasingFunction = douce });
                if (translationSortante is not null)
                {
                    Animer(storyboard, translationSortante, "X", new DoubleAnimation(0, -debut.X, d) { EasingFunction = douce });
                    Animer(storyboard, translationSortante, "Y", new DoubleAnimation(0, -debut.Y, d) { EasingFunction = douce });
                }

                break;
            }

            case TransitionEffet.Diapositive:
            {
                var (debut, _) = Deplacement(transition.Sens, largeur, hauteur);
                entrante.Opacity = 1;
                Animer(storyboard, translationEntrante, "X", new DoubleAnimation(debut.X, 0, d));
                Animer(storyboard, translationEntrante, "Y", new DoubleAnimation(debut.Y, 0, d));
                if (translationSortante is not null)
                {
                    Animer(storyboard, translationSortante, "X",
                        new DoubleAnimation(0, debut.X * 0.25, d) { EasingFunction = douce });
                    Animer(storyboard, translationSortante, "Y",
                        new DoubleAnimation(0, debut.Y * 0.25, d) { EasingFunction = douce });
                }

                break;
            }

            case TransitionEffet.Balayage:
            {
                entrante.Opacity = 1;
                var (depart, arrivee) = RectangleRevelation(transition.Sens, largeur, hauteur);
                RevelerAvecClip(storyboard, entrante, depart, arrivee, d);
                break;
            }

            case TransitionEffet.Volet:
            {
                entrante.Opacity = 1;
                var depart = new Rect(largeur / 2, 0, 0, hauteur);
                RevelerAvecClip(storyboard, entrante, depart, new Rect(0, 0, largeur, hauteur), d);
                break;
            }

            case TransitionEffet.Fractionnement:
            {
                entrante.Opacity = 1;
                var depart = new Rect(0, hauteur / 2, largeur, 0);
                RevelerAvecClip(storyboard, entrante, depart, new Rect(0, 0, largeur, hauteur), d);
                break;
            }

            case TransitionEffet.Cercle:
            {
                entrante.Opacity = 1;
                double rayon = Math.Sqrt((largeur * largeur) + (hauteur * hauteur)) / 2;
                var centre = new Point(largeur / 2, hauteur / 2);
                entrante.Clip = new EllipseGeometry(centre, 0, 0);
                Animer(storyboard, entrante, "Clip.RadiusX", new DoubleAnimation(0, rayon, d));
                Animer(storyboard, entrante, "Clip.RadiusY", new DoubleAnimation(0, rayon, d));
                break;
            }

            case TransitionEffet.Ouverture:
                entrante.Opacity = 0;
                Animer(storyboard, entrante, "Opacity", new DoubleAnimation(0, 1, d));
                Animer(storyboard, echelleEntrante, "ScaleX", new DoubleAnimation(0.4, 1, d) { EasingFunction = douce });
                Animer(storyboard, echelleEntrante, "ScaleY", new DoubleAnimation(0.4, 1, d) { EasingFunction = douce });
                break;

            case TransitionEffet.Fermeture:
                entrante.Opacity = 0;
                Animer(storyboard, entrante, "Opacity", new DoubleAnimation(0, 1, d));
                Animer(storyboard, echelleEntrante, "ScaleX", new DoubleAnimation(1.6, 1, d) { EasingFunction = douce });
                Animer(storyboard, echelleEntrante, "ScaleY", new DoubleAnimation(1.6, 1, d) { EasingFunction = douce });
                break;

            case TransitionEffet.Tourbillon:
                entrante.Opacity = 0;
                Animer(storyboard, entrante, "Opacity", new DoubleAnimation(0, 1, d));
                Animer(storyboard, rotationEntrante, "Angle", new DoubleAnimation(-170, 0, d) { EasingFunction = douce });
                Animer(storyboard, echelleEntrante, "ScaleX", new DoubleAnimation(0.5, 1, d) { EasingFunction = douce });
                Animer(storyboard, echelleEntrante, "ScaleY", new DoubleAnimation(0.5, 1, d) { EasingFunction = douce });
                break;

            case TransitionEffet.Zoom:
                entrante.Opacity = 0;
                Animer(storyboard, entrante, "Opacity", new DoubleAnimation(0, 1, d));
                Animer(storyboard, echelleEntrante, "ScaleX", new DoubleAnimation(0.7, 1, d) { EasingFunction = douce });
                Animer(storyboard, echelleEntrante, "ScaleY", new DoubleAnimation(0.7, 1, d) { EasingFunction = douce });
                if (echelleSortante is not null)
                {
                    Animer(storyboard, echelleSortante, "ScaleX", new DoubleAnimation(1, 1.25, d));
                    Animer(storyboard, echelleSortante, "ScaleY", new DoubleAnimation(1, 1.25, d));
                }

                break;

            case TransitionEffet.Aucune:
            default:
                break;
        }

        return storyboard;
    }

    /// <summary>Vecteur de déplacement d'entrée pour un sens donné (d'où arrive la diapositive).</summary>
    public static (Point Debut, Point FinSortie) Deplacement(SensTransition sens, double largeur, double hauteur)
        => sens switch
        {
            SensTransition.Gauche => (new Point(-largeur, 0), new Point(largeur, 0)),
            SensTransition.Droite => (new Point(largeur, 0), new Point(-largeur, 0)),
            SensTransition.Haut => (new Point(0, -hauteur), new Point(0, hauteur)),
            SensTransition.Bas => (new Point(0, hauteur), new Point(0, -hauteur)),
            _ => (new Point(0, 0), new Point(0, 0))
        };

    /// <summary>Rectangle de départ et d'arrivée de la découpe pour un balayage.</summary>
    public static (Rect Depart, Rect Arrivee) RectangleRevelation(SensTransition sens, double largeur, double hauteur)
        => sens switch
        {
            SensTransition.Gauche => (new Rect(0, 0, 0, hauteur), new Rect(0, 0, largeur, hauteur)),
            SensTransition.Droite => (new Rect(largeur, 0, 0, hauteur), new Rect(0, 0, largeur, hauteur)),
            SensTransition.Haut => (new Rect(0, 0, largeur, 0), new Rect(0, 0, largeur, hauteur)),
            SensTransition.Bas => (new Rect(0, hauteur, largeur, 0), new Rect(0, 0, largeur, hauteur)),
            _ => (new Rect(largeur / 2, hauteur / 2, 0, 0), new Rect(0, 0, largeur, hauteur))
        };

    public static void JouerSon(TransitionDiapositive transition)
    {
        if (string.IsNullOrWhiteSpace(transition.Son) || !File.Exists(transition.Son))
        {
            return;
        }

        try
        {
            var lecteur = new SoundPlayer(transition.Son);
            if (transition.LireEnBoucleSon)
            {
                lecteur.PlayLooping();
            }
            else
            {
                lecteur.Play();
            }

            SonsActifs.Add(lecteur);
            if (SonsActifs.Count > 8)
            {
                SonsActifs.RemoveRange(0, SonsActifs.Count - 8);
            }
        }
        catch
        {
            // Un son de transition manquant ne doit jamais bloquer le diaporama.
        }
    }

    private static void RevelerAvecClip(Storyboard storyboard, FrameworkElement element, Rect depart, Rect arrivee, Duration duree)
    {
        element.Clip = new RectangleGeometry(depart);
        Animer(storyboard, element, "Clip.Rect",
            new RectAnimation(depart, arrivee, duree) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } });
    }

    private static void Animer(Storyboard storyboard, DependencyObject cible, string chemin, AnimationTimeline animation)
    {
        Storyboard.SetTarget(animation, cible);
        Storyboard.SetTargetProperty(animation, new PropertyPath(chemin));
        storyboard.Children.Add(animation);
    }

    private static async Task JouerAsync(Storyboard storyboard, FrameworkElement conteneur, TimeSpan securite, CancellationToken jeton)
    {
        if (storyboard.Children.Count == 0)
        {
            return;
        }

        var fin = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        storyboard.Completed += (_, _) => fin.TrySetResult();

        using var inscription = jeton.Register(() =>
        {
            try
            {
                storyboard.Stop(conteneur);
            }
            catch
            {
                // Le storyboard a pu être arrêté par la fermeture de la fenêtre.
            }

            fin.TrySetResult();
        });

        storyboard.Begin(conteneur, isControllable: true);

        var delai = Task.Delay(securite, jeton);
        await Task.WhenAny(fin.Task, delai).ConfigureAwait(true);
    }

    private static double LargeurDe(FrameworkElement element)
        => element.ActualWidth > 1 ? element.ActualWidth : element.Width > 1 ? element.Width : LargeurReference;

    private static double HauteurDe(FrameworkElement element)
        => element.ActualHeight > 1 ? element.ActualHeight : element.Height > 1 ? element.Height : HauteurReference;
}
