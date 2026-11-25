using System.Collections.ObjectModel;
using System.Windows;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;
using PowerPwerPoint.Media.ViewModels;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Boîte de dialogue d'un zoom : type, titre et diapositives ciblées.</summary>
public partial class DialogueZoom : Window
{
    private readonly ObjetZoom _modele;
    private readonly PresentationMedia _presentation;

    public DialogueZoom(ObjetZoom? modele, PresentationMedia presentation)
    {
        InitializeComponent();
        _presentation = presentation;
        _modele = modele?.CopierZoom() ?? new ObjetZoom { TypeZoom = TypeZoom.Diapositive };

        Titre.Text = _modele.Titre;
        ComboType.ItemsSource = OptionsEnum.Creer<TypeZoom>(LibellesEnum.LibelleTypeZoom);
        ComboType.SelectedValue = _modele.TypeZoom;

        var lignes = new ObservableCollection<LigneSelectionDiapositive>();
        int numero = 1;
        foreach (var diapositive in presentation.Diapositives)
        {
            lignes.Add(new LigneSelectionDiapositive(
                diapositive.Id,
                $"{numero}. {diapositive.Titre}",
                modele?.Cibles.Contains(diapositive.Id) == true));
            numero++;
        }

        Diapositives.ItemsSource = lignes;
        Resultat = null;
    }

    /// <summary>Zoom validé (nouvel objet si création).</summary>
    public ObjetZoom? Resultat { get; private set; }

    private void SurValider(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Titre.Text))
        {
            MessageBox.Show(this, "Donnez un titre au zoom.", "Zoom", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var cibles = (Diapositives.ItemsSource as IEnumerable<LigneSelectionDiapositive> ?? Array.Empty<LigneSelectionDiapositive>())
            .Where(l => l.Selectionnee)
            .Select(l => l.Id)
            .ToList();

        if (cibles.Count == 0)
        {
            MessageBox.Show(this, "Sélectionnez au moins une diapositive à cibler.", "Zoom", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _modele.Titre = Titre.Text.Trim();
        _modele.TypeZoom = ComboType.SelectedValue is TypeZoom type ? type : TypeZoom.Diapositive;
        _modele.Cibles = cibles;
        Resultat = _modele;
        DialogResult = true;
    }

    private void SurAnnuler(object sender, RoutedEventArgs e) => DialogResult = false;
}

internal static class ExtensionsZoom
{
    public static ObjetZoom CopierZoom(this ObjetZoom modele)
        => new()
        {
            Id = modele.Id,
            Nom = modele.Nom,
            Titre = modele.Titre,
            TypeZoom = modele.TypeZoom,
            Cibles = new List<string>(modele.Cibles),
            CouleurFond = modele.CouleurFond,
            X = modele.X,
            Y = modele.Y,
            Largeur = modele.Largeur,
            Hauteur = modele.Hauteur
        };
}
