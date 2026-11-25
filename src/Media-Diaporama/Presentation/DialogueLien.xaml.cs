using System.Windows;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;
using PowerPwerPoint.Media.ViewModels;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Boîte de dialogue d'un lien : page web, fichier, diapositive ou adresse e-mail.</summary>
public partial class DialogueLien : Window
{
    private readonly ObjetLien _modele;
    private readonly PresentationMedia _presentation;
    private readonly DiapositiveMedia _diapositive;

    public DialogueLien(ObjetLien modele, DiapositiveMedia diapositive, PresentationMedia presentation)
    {
        InitializeComponent();
        _presentation = presentation;
        _diapositive = diapositive;
        _modele = new ObjetLien
        {
            Id = modele.Id,
            Nom = modele.Nom,
            ElementId = modele.ElementId,
            TypeLien = modele.TypeLien,
            Cible = modele.Cible,
            InfoBulle = modele.InfoBulle,
            X = modele.X,
            Y = modele.Y,
            Largeur = modele.Largeur,
            Hauteur = modele.Hauteur
        };

        Cible.Text = _modele.Cible;
        InfoBulle.Text = _modele.InfoBulle;

        var options = OptionsEnum.Creer<TypeLien>(LibellesEnum.LibelleTypeLien);
        ComboTypeLien.ItemsSource = options;
        ComboTypeLien.SelectedValue = _modele.TypeLien;

        var elements = new List<OptionElementLien> { new(string.Empty, "(Toute la diapositive)") };
        elements.AddRange(diapositive.Elements.Select(e => new OptionElementLien(e.Id, e.Nom)));
        Element.ItemsSource = elements;
        Element.SelectedValue = _modele.ElementId;
        if (Element.SelectedItem is null)
        {
            Element.SelectedIndex = 0;
        }

        Resultat = null;
        MettreAJourChamps();
    }

    public ObjetLien? Resultat { get; private set; }

    private void SurTypeModifie(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        => MettreAJourChamps();

    private void MettreAJourChamps()
    {
        var type = ComboTypeLien.SelectedValue is TypeLien valeur ? valeur : TypeLien.PageWeb;

        if (BoutonParcourir is null)
        {
            return;
        }

        BoutonParcourir.Visibility = type == TypeLien.Fichier ? Visibility.Visible : Visibility.Collapsed;

        var diapositiveCible = type == TypeLien.Diapositive ? DiapositiveCible() : null;
        if (diapositiveCible is not null)
        {
            Cible.Text = diapositiveCible.Id;
        }

        Cible.IsReadOnly = type == TypeLien.Diapositive;
    }

    private DiapositiveMedia? DiapositiveCible()
    {
        var premiere = _presentation.Diapositives.FirstOrDefault(d => d.Id != _diapositive.Id);
        return premiere;
    }

    private void SurParcourir(object sender, RoutedEventArgs e)
    {
        string? chemin = ServiceDialoguesWpf.DemanderFichier("Choisir le fichier lié", "Tous les fichiers|*.*");
        if (chemin is not null)
        {
            Cible.Text = chemin;
        }
    }

    private void SurValider(object sender, RoutedEventArgs e)
    {
        var type = ComboTypeLien.SelectedValue is TypeLien valeur ? valeur : TypeLien.PageWeb;

        if (string.IsNullOrWhiteSpace(Cible.Text))
        {
            MessageBox.Show(this, "Renseignez la cible du lien.", "Lien", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (type == TypeLien.PageWeb && !Cible.Text.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            Cible.Text = "https://" + Cible.Text.Trim();
        }

        _modele.TypeLien = type;
        _modele.Cible = Cible.Text.Trim();
        _modele.InfoBulle = InfoBulle.Text.Trim();
        _modele.ElementId = Element.SelectedValue as string ?? string.Empty;
        _modele.Nom = string.IsNullOrWhiteSpace(_modele.InfoBulle) ? _modele.Cible : _modele.InfoBulle;
        Resultat = _modele;
        DialogResult = true;
    }

    private void SurAnnuler(object sender, RoutedEventArgs e) => DialogResult = false;
}

/// <summary>Élément sélectionnable comme porteur de lien ou d'action.</summary>
public sealed record OptionElementLien(string Id, string Nom);
