using System.Globalization;
using System.Windows;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Boîte « Vérification du minutage » : liste des durées et application aux transitions.</summary>
public partial class DialogueMinutage : Window
{
    private readonly PresentationMedia _presentation;

    public DialogueMinutage(PresentationMedia presentation)
    {
        InitializeComponent();
        _presentation = presentation;
        Rafraichir();
    }

    /// <summary>Vrai si les minutages ont été appliqués aux transitions.</summary>
    public bool Applique { get; private set; }

    private void Rafraichir()
    {
        var lignes = new List<LigneMinutage>();
        int numero = 1;

        foreach (var diapositive in _presentation.Diapositives)
        {
            var minutage = _presentation.Minutages.FirstOrDefault(m => m.DiapositiveId == diapositive.Id);
            lignes.Add(new LigneMinutage(
                numero,
                diapositive.Titre,
                minutage?.Secondes ?? 0,
                minutage is null ? "—" : $"{minutage.Secondes:0.0} s"));
            numero++;
        }

        Lignes.ItemsSource = lignes;

        var total = TimeSpan.FromSeconds(_presentation.DureeTotaleMinutage());
        Total.Text = $"Total : {ChronometreDiaporama.Formater(total)} · {_presentation.Minutages.Count} diapositive(s) minutée(s)";
    }

    private void SurAppliquer(object sender, RoutedEventArgs e)
    {
        if (_presentation.Minutages.Count == 0)
        {
            MessageBox.Show(this, "Aucun minutage à appliquer.", "Minutage", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        foreach (var minutage in _presentation.Minutages)
        {
            var diapositive = _presentation.TrouverDiapositive(minutage.DiapositiveId);
            if (diapositive is null)
            {
                continue;
            }

            diapositive.Transition.AvanceAutomatique = true;
            diapositive.Transition.DelaiAvance = Math.Clamp(minutage.Secondes, 0.5, 3600);
        }

        Applique = true;
        MessageBox.Show(this, "Les minutages ont été appliqués aux transitions.", "Minutage", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SurEffacer(object sender, RoutedEventArgs e)
    {
        _presentation.Minutages.Clear();
        Rafraichir();
    }

    private void SurFermer(object sender, RoutedEventArgs e) => Close();
}

/// <summary>Ligne affichée dans la vérification du minutage.</summary>
public sealed record LigneMinutage(int Numero, string Titre, double Secondes, string DureeTexte);
