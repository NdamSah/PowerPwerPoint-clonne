using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>Un effet positionné dans un pas d'animation.</summary>
public sealed class EntreePas
{
    public required AnimationEffet Effet { get; init; }

    public TimeSpan Decalage { get; init; }

    public TimeSpan Fin => Decalage + MoteurAnimations.DureePlanifiee(Effet);
}

/// <summary>
/// Pas d'animation : un ensemble d'effets démarrés ensemble, déclenché par un clic
/// ou lancé automatiquement pour les enchaînements « après la précédente ».
/// </summary>
public sealed class PasAnimation
{
    public bool DeclencheParClic { get; init; }

    /// <summary>Vrai si l'étape démarre seule (premier effet « avec/après la précédente »).</summary>
    public bool DemarrageAutomatique { get; init; }

    public List<EntreePas> Entrees { get; } = new();

    public TimeSpan Duree => Entrees.Count == 0
        ? TimeSpan.Zero
        : Entrees.Max(e => e.Fin);
}

/// <summary>
/// Moteur des animations : construit le plan de lecture (au clic / avec / après la précédente,
/// délais, répétitions), prépare l'état initial des éléments et anime les cibles WPF.
/// </summary>
public static class MoteurAnimations
{
    private const double DistanceHorizontale = 480;
    private const double DistanceVerticale = 270;

    public static string NomCategorie(AnimationEffet effet)
        => AnimationCatalogue.LibelleCategorie(AnimationCatalogue.CategorieDe(effet.Effet));

    /// <summary>Durée totale d'un effet, répétitions incluses (une répétition infinie compte pour une passe).</summary>
    public static TimeSpan DureePlanifiee(AnimationEffet effet)
    {
        double duree = Math.Max(0.05, effet.Duree);
        int passes = effet.Repetition switch
        {
            < 0 => 1,
            0 => 1,
            _ => effet.Repetition
        };

        return TimeSpan.FromSeconds(duree * passes);
    }

    public static List<PasAnimation> ConstruirePlan(IEnumerable<AnimationEffet> effets)
    {
        var pas = new List<PasAnimation>();
        PasAnimation? courant = null;

        foreach (var effet in effets)
        {
            bool nouveauPas = effet.Declencheur == DeclencheurAnimation.AuClic || courant is null;
            if (nouveauPas)
            {
                courant = new PasAnimation
                {
                    DeclencheParClic = effet.Declencheur == DeclencheurAnimation.AuClic,
                    DemarrageAutomatique = effet.Declencheur != DeclencheurAnimation.AuClic
                };
                pas.Add(courant);
            }

            TimeSpan decalage = TimeSpan.Zero;
            if (courant!.Entrees.Count > 0)
            {
                var precedente = courant.Entrees[^1];
                decalage = effet.Declencheur switch
                {
                    DeclencheurAnimation.AvecLaPrecedente => precedente.Decalage,
                    DeclencheurAnimation.ApresLaPrecedente => precedente.Fin,
                    _ => TimeSpan.Zero
                };
            }

            decalage += TimeSpan.FromSeconds(Math.Max(0, effet.Delai));
            courant.Entrees.Add(new EntreePas { Effet = effet, Decalage = decalage });
        }

        return pas;
    }

    /// <summary>
    /// Prépare la diapositive : les cibles dont la première animation est une entrée
    /// sont masquées jusqu'à leur tour.
    /// </summary>
    public static void PreparerDiapositive(
        DiapositiveMedia diapositive,
        Func<string, FrameworkElement?> resolveur)
    {
        foreach (var groupe in diapositive.Animations.GroupBy(a => a.CibleId))
        {
            var cible = resolveur(groupe.Key);
            if (cible is null)
            {
                continue;
            }

            TransformsElements.Reinitialiser(cible);
            var premiere = groupe.First();
            if (AnimationCatalogue.CategorieDe(premiere.Effet) == CategorieAnimation.Entree)
            {
                cible.Opacity = 0;
            }
        }
    }

    /// <summary>Remet toutes les cibles des effets dans leur état neutre.</summary>
    public static void Reinitialiser(
        IEnumerable<AnimationEffet> effets,
        Func<string, FrameworkElement?> resolveur)
    {
        foreach (var cibleId in effets.Select(e => e.CibleId).Distinct())
        {
            var cible = resolveur(cibleId);
            if (cible is not null)
            {
                TransformsElements.Reinitialiser(cible);
            }
        }
    }

    /// <summary>Construit le storyboard d'un effet (sans le jouer).</summary>
    public static Storyboard ConstruireStoryboard(AnimationEffet effet, FrameworkElement cible, TimeSpan decalage)
    {
        var storyboard = new Storyboard();
        var (echelle, rotation, translation) = TransformsElements.Obtenir(cible);

        double dureeSecondes = Math.Max(0.05, effet.Duree);
        var duree = new Duration(TimeSpan.FromSeconds(dureeSecondes));
        var douce = new CubicEase { EasingMode = EasingMode.EaseOut };
        RepeatBehavior? repetition = effet.Repetition switch
        {
            < 0 => RepeatBehavior.Forever,
            > 0 => new RepeatBehavior(effet.Repetition),
            _ => null
        };

        double facteur = effet.Ampleur <= 0 ? 1 : effet.Ampleur;
        var (decalageEntree, decalageSortie) = Offset(effet.Sens, facteur);

        void Animer(DependencyObject cibleAnimation, string chemin, AnimationTimeline animation)
        {
            animation.BeginTime = decalage;
            if (repetition is { } valeur)
            {
                animation.RepeatBehavior = valeur;
            }

            Storyboard.SetTarget(animation, cibleAnimation);
            Storyboard.SetTargetProperty(animation, new PropertyPath(chemin));
            storyboard.Children.Add(animation);
        }

        switch (effet.Effet)
        {
            // ---------- Entrées ----------
            case EffetAnimation.Apparition:
                cible.Opacity = 0;
                Animer(cible, "Opacity", new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(0.01))));
                break;

            case EffetAnimation.FonduEntree:
                cible.Opacity = 0;
                Animer(cible, "Opacity", new DoubleAnimation(0, 1, duree) { EasingFunction = douce });
                break;

            case EffetAnimation.VolerEntree:
                cible.Opacity = 0;
                Animer(cible, "Opacity", new DoubleAnimation(0, 1, new Duration(TimeSpan.FromSeconds(Math.Min(0.3, dureeSecondes)))));
                Animer(translation, "X", new DoubleAnimation(decalageEntree.X, 0, duree) { EasingFunction = douce });
                Animer(translation, "Y", new DoubleAnimation(decalageEntree.Y, 0, duree) { EasingFunction = douce });
                break;

            case EffetAnimation.ZoomEntree:
                cible.Opacity = 0;
                Animer(cible, "Opacity", new DoubleAnimation(0, 1, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleX", new DoubleAnimation(0.3, 1, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleY", new DoubleAnimation(0.3, 1, duree) { EasingFunction = douce });
                break;

            case EffetAnimation.BalayageEntree:
            {
                cible.Opacity = 1;
                double largeur = cible.ActualWidth > 1 ? cible.ActualWidth : 320;
                double hauteur = cible.ActualHeight > 1 ? cible.ActualHeight : 120;
                var (depart, arrivee) = MoteurTransitions.RectangleRevelation(effet.Sens, largeur, hauteur);
                cible.Clip = new RectangleGeometry(depart);
                Animer(cible, "Clip.Rect",
                    new RectAnimation(depart, arrivee, duree) { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseInOut } });
                break;
            }

            case EffetAnimation.RotationEntree:
                cible.Opacity = 0;
                Animer(cible, "Opacity", new DoubleAnimation(0, 1, duree));
                Animer(rotation, "Angle", new DoubleAnimation(-180, 0, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleX", new DoubleAnimation(0.5, 1, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleY", new DoubleAnimation(0.5, 1, duree) { EasingFunction = douce });
                break;

            // ---------- Emphases ----------
            case EffetAnimation.Pulsation:
            {
                double sommet = 1 + (0.15 * facteur);
                var clesX = new DoubleAnimationUsingKeyFrames { Duration = duree };
                var clesY = new DoubleAnimationUsingKeyFrames { Duration = duree };
                clesX.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0)));
                clesX.KeyFrames.Add(new EasingDoubleKeyFrame(sommet, KeyTime.FromPercent(0.5), douce));
                clesX.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1), douce));
                clesY.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(0)));
                clesY.KeyFrames.Add(new EasingDoubleKeyFrame(sommet, KeyTime.FromPercent(0.5), douce));
                clesY.KeyFrames.Add(new EasingDoubleKeyFrame(1, KeyTime.FromPercent(1), douce));
                Animer(echelle, "ScaleX", clesX);
                Animer(echelle, "ScaleY", clesY);
                break;
            }

            case EffetAnimation.Grossir:
                Animer(echelle, "ScaleX", new DoubleAnimation(1, 1 + (0.3 * facteur), duree) { EasingFunction = douce });
                Animer(echelle, "ScaleY", new DoubleAnimation(1, 1 + (0.3 * facteur), duree) { EasingFunction = douce });
                break;

            case EffetAnimation.Retrecir:
                Animer(echelle, "ScaleX", new DoubleAnimation(1, 1 - (0.25 * facteur), duree) { EasingFunction = douce });
                Animer(echelle, "ScaleY", new DoubleAnimation(1, 1 - (0.25 * facteur), duree) { EasingFunction = douce });
                break;

            case EffetAnimation.Pivot:
                Animer(rotation, "Angle", new DoubleAnimation(0, 360 * facteur, duree));
                break;

            case EffetAnimation.Transparence:
                Animer(cible, "Opacity", new DoubleAnimation(1, Math.Max(0, 1 - (0.6 * facteur)), duree));
                break;

            // ---------- Sorties ----------
            case EffetAnimation.Disparition:
                Animer(cible, "Opacity", new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(0.01))));
                break;

            case EffetAnimation.FonduSortie:
                Animer(cible, "Opacity", new DoubleAnimation(1, 0, duree) { EasingFunction = douce });
                break;

            case EffetAnimation.VolerSortie:
                Animer(cible, "Opacity", new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(Math.Min(0.3, dureeSecondes)))));
                Animer(translation, "X", new DoubleAnimation(0, decalageSortie.X, duree) { EasingFunction = douce });
                Animer(translation, "Y", new DoubleAnimation(0, decalageSortie.Y, duree) { EasingFunction = douce });
                break;

            case EffetAnimation.ZoomSortie:
                Animer(cible, "Opacity", new DoubleAnimation(1, 0, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleX", new DoubleAnimation(1, 0.3, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleY", new DoubleAnimation(1, 0.3, duree) { EasingFunction = douce });
                break;

            case EffetAnimation.Reduire:
                Animer(echelle, "ScaleX", new DoubleAnimation(1, 0.2, duree) { EasingFunction = douce });
                Animer(echelle, "ScaleY", new DoubleAnimation(1, 0.2, duree) { EasingFunction = douce });
                Animer(cible, "Opacity", new DoubleAnimation(1, 0.1, duree));
                break;

            // ---------- Trajectoires ----------
            case EffetAnimation.TrajectoireLigne:
            case EffetAnimation.TrajectoireArc:
            case EffetAnimation.TrajectoireCercle:
            case EffetAnimation.TrajectoireDiagonale:
            {
                var geometrie = GeometrieTrajectoire(effet, facteur);
                Animer(translation, "X",
                    new DoubleAnimationUsingPath { PathGeometry = geometrie, Source = PathAnimationSource.X, Duration = duree });
                Animer(translation, "Y",
                    new DoubleAnimationUsingPath { PathGeometry = geometrie, Source = PathAnimationSource.Y, Duration = duree });
                break;
            }
        }

        return storyboard;
    }

    /// <summary>Joue un pas d'animation et attend sa fin (ou l'annulation).</summary>
    public static async Task ExecuterPasAsync(
        PasAnimation pas,
        Func<string, FrameworkElement?> resolveur,
        CancellationToken jeton = default)
    {
        ArgumentNullException.ThrowIfNull(pas);
        ArgumentNullException.ThrowIfNull(resolveur);

        var storyboards = new List<(Storyboard Storyboard, FrameworkElement Conteneur)>();
        TimeSpan fin = TimeSpan.Zero;

        foreach (var entree in pas.Entrees)
        {
            var cible = resolveur(entree.Effet.CibleId);
            if (cible is null)
            {
                continue;
            }

            var storyboard = ConstruireStoryboard(entree.Effet, cible, entree.Decalage);
            if (storyboard.Children.Count == 0)
            {
                continue;
            }

            storyboards.Add((storyboard, cible));
            if (entree.Fin > fin)
            {
                fin = entree.Fin;
            }
        }

        if (storyboards.Count == 0)
        {
            return;
        }

        foreach (var (storyboard, conteneur) in storyboards)
        {
            storyboard.Begin(conteneur, isControllable: true);
        }

        if (fin > TimeSpan.Zero)
        {
            try
            {
                await Task.Delay(fin, jeton).ConfigureAwait(true);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        foreach (var (storyboard, conteneur) in storyboards)
        {
            try
            {
                storyboard.Stop(conteneur);
            }
            catch
            {
                // Storyboard déjà arrêté (fenêtre fermée).
            }
        }
    }

    /// <summary>Joue l'intégralité du plan (aperçu dans l'éditeur).</summary>
    public static async Task ExecuterPlanAsync(
        IEnumerable<AnimationEffet> effets,
        Func<string, FrameworkElement?> resolveur,
        CancellationToken jeton = default)
    {
        foreach (var pas in ConstruirePlan(effets))
        {
            if (jeton.IsCancellationRequested)
            {
                return;
            }

            await ExecuterPasAsync(pas, resolveur, jeton).ConfigureAwait(true);
            await Task.Delay(TimeSpan.FromMilliseconds(200), jeton).ConfigureAwait(true);
        }
    }

    private static (Point Entree, Point Sortie) Offset(SensTransition sens, double facteur)
        => sens switch
        {
            SensTransition.Gauche => (new Point(-DistanceHorizontale * facteur, 0), new Point(-DistanceHorizontale * facteur, 0)),
            SensTransition.Droite => (new Point(DistanceHorizontale * facteur, 0), new Point(DistanceHorizontale * facteur, 0)),
            SensTransition.Haut => (new Point(0, -DistanceVerticale * facteur), new Point(0, -DistanceVerticale * facteur)),
            SensTransition.Bas => (new Point(0, DistanceVerticale * facteur), new Point(0, DistanceVerticale * facteur)),
            _ => (new Point(0, -DistanceVerticale * facteur), new Point(0, DistanceVerticale * facteur))
        };

    private static PathGeometry GeometrieTrajectoire(AnimationEffet effet, double facteur)
    {
        var (sens, _) = Offset(effet.Sens, facteur);
        var geometrie = new PathGeometry();
        var figure = new PathFigure { StartPoint = new Point(0, 0), IsClosed = false, IsFilled = false };

        switch (effet.Effet)
        {
            case EffetAnimation.TrajectoireCercle:
            {
                double rayon = 120 * facteur;
                figure.Segments.Add(new ArcSegment(new Point(2 * rayon, 0), new Size(rayon, rayon), 0, false,
                    SweepDirection.Clockwise, true));
                figure.Segments.Add(new ArcSegment(new Point(0, 0), new Size(rayon, rayon), 0, false,
                    SweepDirection.Clockwise, true));
                break;
            }

            case EffetAnimation.TrajectoireArc:
            {
                var arrivee = new Point(sens.X * 1.2, sens.Y * 0.3);
                var controle = new Point(sens.X * 0.5, sens.Y * 1.6);
                figure.Segments.Add(new QuadraticBezierSegment(controle, arrivee, true));
                break;
            }

            default:
                figure.Segments.Add(new LineSegment(new Point(sens.X, sens.Y), true));
                break;
        }

        geometrie.Figures.Add(figure);
        return geometrie;
    }
}
