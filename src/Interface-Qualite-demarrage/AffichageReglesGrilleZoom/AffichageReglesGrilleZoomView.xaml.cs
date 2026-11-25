using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace PowerPointClone.InterfaceQualite.AffichageReglesGrilleZoom;

public partial class AffichageReglesGrilleZoomView : UserControl
{
    public static readonly DependencyProperty SlideContentProperty = DependencyProperty.Register(
        nameof(SlideContent), typeof(object), typeof(AffichageReglesGrilleZoomView),
        new PropertyMetadata(null));

    /// <summary>Élément visuel représentant la diapositive éditée.</summary>
    public object? SlideContent
    {
        get => GetValue(SlideContentProperty);
        set => SetValue(SlideContentProperty, value);
    }

    public AffichageReglesGrilleZoomView()
    {
        InitializeComponent();
        DataContext = new AffichageReglesGrilleZoomViewModel();
        SlideHost.SetBinding(ContentControl.ContentProperty,
            new Binding(nameof(SlideContent)) { Source = this });
    }
}
