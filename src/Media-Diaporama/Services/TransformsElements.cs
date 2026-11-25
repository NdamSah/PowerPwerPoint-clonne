using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Fournit (et mémorise) un groupe de transformations standard par élément rendu :
/// échelle, rotation puis translation. Les moteurs d'animation peuvent ainsi animer
/// des propriétés indépendantes sans écraser les transformations du rendu.
/// </summary>
public static class TransformsElements
{
    private sealed class GroupeTransforms
    {
        public GroupeTransforms(ScaleTransform echelle, RotateTransform rotation, TranslateTransform translation)
        {
            Echelle = echelle;
            Rotation = rotation;
            Translation = translation;
        }

        public ScaleTransform Echelle { get; }

        public RotateTransform Rotation { get; }

        public TranslateTransform Translation { get; }
    }

    private static readonly ConditionalWeakTable<FrameworkElement, GroupeTransforms> Cache = new();

    public static (ScaleTransform Echelle, RotateTransform Rotation, TranslateTransform Translation) Obtenir(FrameworkElement element)
    {
        if (Cache.TryGetValue(element, out var existant))
        {
            return (existant.Echelle, existant.Rotation, existant.Translation);
        }

        var echelle = new ScaleTransform(1, 1);
        var rotation = new RotateTransform(0);
        var translation = new TranslateTransform(0, 0);

        var groupe = new TransformGroup();
        groupe.Children.Add(echelle);
        groupe.Children.Add(rotation);
        groupe.Children.Add(translation);

        element.RenderTransform = groupe;
        element.RenderTransformOrigin = new Point(0.5, 0.5);

        var paquet = new GroupeTransforms(echelle, rotation, translation);
        Cache.Add(element, paquet);
        return (echelle, rotation, translation);
    }

    /// <summary>Remet l'élément dans son état neutre (opacité, transformations, découpe).</summary>
    public static void Reinitialiser(FrameworkElement element)
    {
        element.Opacity = 1;
        element.Clip = null;
        element.Visibility = Visibility.Visible;

        var (echelle, rotation, translation) = Obtenir(element);
        echelle.ScaleX = 1;
        echelle.ScaleY = 1;
        rotation.Angle = 0;
        translation.X = 0;
        translation.Y = 0;
    }
}
