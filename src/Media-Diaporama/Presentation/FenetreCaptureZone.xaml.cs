using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Drawing = System.Drawing;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>
/// Calque plein écran permettant de dessiner la zone d'écran à enregistrer.
/// Retourne un rectangle en pixels physiques, prêt pour GDI+.
/// </summary>
public partial class FenetreCaptureZone : Window
{
    private Point _origine;
    private bool _selectionEnCours;

    public FenetreCaptureZone()
    {
        InitializeComponent();

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                DialogResult = false;
            }
        };

        Loaded += (_, _) =>
        {
            Activate();
            Focus();
        };
    }

    /// <summary>Zone retenue (pixels physiques), ou <c>null</c> si annulé.</summary>
    public Drawing.Rectangle? ZoneRetenue { get; private set; }

    private void SurAppui(object sender, MouseButtonEventArgs e)
    {
        _origine = e.GetPosition(Zone);
        _selectionEnCours = true;
        Selection.Visibility = Visibility.Visible;
        Instruction.Visibility = Visibility.Collapsed;
        PositionnerSelection(_origine, _origine);
    }

    private void SurMouvement(object sender, MouseEventArgs e)
    {
        if (!_selectionEnCours)
        {
            return;
        }

        PositionnerSelection(_origine, e.GetPosition(Zone));
    }

    private void SurRelachement(object sender, MouseButtonEventArgs e)
    {
        if (!_selectionEnCours)
        {
            return;
        }

        _selectionEnCours = false;
        var fin = e.GetPosition(Zone);
        PositionnerSelection(_origine, fin);

        Point hautGauche = PointToScreen(new Point(Math.Min(_origine.X, fin.X), Math.Min(_origine.Y, fin.Y)));
        Point basDroite = PointToScreen(new Point(Math.Max(_origine.X, fin.X), Math.Max(_origine.Y, fin.Y)));

        var zone = new Drawing.Rectangle(
            (int)Math.Round(hautGauche.X),
            (int)Math.Round(hautGauche.Y),
            (int)Math.Round(basDroite.X - hautGauche.X),
            (int)Math.Round(basDroite.Y - hautGauche.Y));

        if (zone.Width < 24 || zone.Height < 24)
        {
            Instruction.Visibility = Visibility.Visible;
            Selection.Visibility = Visibility.Collapsed;
            return;
        }

        ZoneRetenue = zone;
        DialogResult = true;
    }

    private void PositionnerSelection(Point a, Point b)
    {
        double gauche = Math.Min(a.X, b.X);
        double haut = Math.Min(a.Y, b.Y);

        Canvas.SetLeft(Selection, gauche);
        Canvas.SetTop(Selection, haut);
        Selection.Width = Math.Abs(a.X - b.X);
        Selection.Height = Math.Abs(a.Y - b.Y);
    }
}
