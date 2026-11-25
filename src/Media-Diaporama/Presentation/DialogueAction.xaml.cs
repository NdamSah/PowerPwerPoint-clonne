using System.Windows;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;
using PowerPwerPoint.Media.ViewModels;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Boîte de dialogue d'une action : navigation, fin du diaporama, programme ou son.</summary>
public partial class DialogueAction : Window
{
    private readonly ObjetAction _modele;
    private readonly PresentationMedia _presentation;

    public DialogueAction(ObjetAction modele, DiapositiveMedia diapositive, PresentationMedia presentation)
    {
        InitializeComponent();
        _presentation = presentation;
        _modele = new ObjetAction
        {
            Id = modele.Id,
            Nom = modele.Nom,
            ElementId = modele.ElementId,
            TypeAction = modele.TypeAction,
            Parametre = modele.Parametre,
            Arguments = modele.Arguments,
            Declencheur = modele.Declencheur,
            X = modele.X,
            Y = modele.Y,
            Largeur = modele.Largeur,
            Hauteur = modele.Hauteur
        };

        ComboTypeAction.ItemsSource = OptionsEnum.Creer<TypeAction>(LibellesEnum.LibelleAction);
        ComboTypeAction.SelectedValue = _modele.TypeAction == TypeAction.Aucune ? TypeAction.DiapositiveSuivante : _modele.TypeAction;

        var lignes = presentation.Diapositives
            .Select((d, i) => new { d.Id, Libelle = $"{i + 1}. {d.Titre}" })
            .ToList();
        DiapositiveCible.ItemsSource = lignes;
        DiapositiveCible.SelectedValue = _modele.Parametre;

        var elements = new List<OptionElementLien> { new(string.Empty, "(Toute la diapositive)") };
        elements.AddRange(diapositive.Elements.Select(e => new OptionElementLien(e.Id, e.Nom)));
        Element.ItemsSource = elements;
        Element.SelectedValue = _modele.ElementId;
        if (Element.SelectedItem is null)
        {
            Element.SelectedIndex = 0;
        }

        Declencheur.ItemsSource = OptionsEnum.Creer<DeclencheurAction>(LibellesEnum.LibelleDeclencheurAction);
        Declencheur.SelectedValue = _modele.Declencheur;

        Parametre.Text = _modele.Parametre;
        Arguments.Text = _modele.Arguments;

        Resultat = null;
        MettreAJourChamps();
    }

    public ObjetAction? Resultat { get; private set; }

    private void SurTypeModifie(object sender, System.Windows.Controls.SelectionChangedEventArgs e) => MettreAJourChamps();

    private void MettreAJourChamps()
    {
        if (EtiquetteDiapositive is null)
        {
            return;
        }

        var type = ComboTypeAction.SelectedValue is TypeAction valeur ? valeur : TypeAction.DiapositiveSuivante;

        bool besoinDiapositive = type == TypeAction.AllerDiapositive;
        bool besoinChemin = type is TypeAction.LancerProgramme or TypeAction.LireSon;
        bool besoinArguments = type == TypeAction.LancerProgramme;

        EtiquetteDiapositive.Visibility = besoinDiapositive ? Visibility.Visible : Visibility.Collapsed;
        DiapositiveCible.Visibility = besoinDiapositive ? Visibility.Visible : Visibility.Collapsed;
        EtiquetteParametre.Visibility = besoinChemin ? Visibility.Visible : Visibility.Collapsed;
        Parametre.Visibility = besoinChemin ? Visibility.Visible : Visibility.Collapsed;
        EtiquetteArguments.Visibility = besoinArguments ? Visibility.Visible : Visibility.Collapsed;
        Arguments.Visibility = besoinArguments ? Visibility.Visible : Visibility.Collapsed;

        EtiquetteParametre.Text = type == TypeAction.LancerProgramme
            ? "Programme à lancer"
            : "Fichier son (WAV, MP3…)";
    }

    private void SurParcourir(object sender, RoutedEventArgs e)
    {
        var type = ComboTypeAction.SelectedValue is TypeAction valeur ? valeur : TypeAction.LancerProgramme;
        string filtre = type == TypeAction.LancerProgramme
            ? "Programmes|*.exe;*.bat;*.cmd|Tous les fichiers|*.*"
            : StockageMedia.FiltreAudio;

        string? chemin = ServiceDialoguesWpf.DemanderFichier("Choisir un fichier", filtre);
        if (chemin is not null)
        {
            Parametre.Text = chemin;
        }
    }

    private void SurValider(object sender, RoutedEventArgs e)
    {
        var type = ComboTypeAction.SelectedValue is TypeAction valeur ? valeur : TypeAction.DiapositiveSuivante;
        string parametre = string.Empty;

        switch (type)
        {
            case TypeAction.AllerDiapositive:
                parametre = DiapositiveCible.SelectedValue as string ?? string.Empty;
                if (string.IsNullOrEmpty(parametre))
                {
                    MessageBox.Show(this, "Choisissez la diapositive cible.", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                break;

            case TypeAction.LancerProgramme:
            case TypeAction.LireSon:
                parametre = Parametre.Text.Trim();
                if (string.IsNullOrEmpty(parametre))
                {
                    MessageBox.Show(this, "Choisissez le fichier à utiliser.", "Action", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (type == TypeAction.LireSon)
                {
                    parametre = StockageMedia.Importer(parametre, _presentation);
                }

                break;
        }

        _modele.TypeAction = type;
        _modele.Parametre = parametre;
        _modele.Arguments = Arguments.Text.Trim();
        _modele.Declencheur = Declencheur.SelectedValue is DeclencheurAction declencheur ? declencheur : DeclencheurAction.Clic;
        _modele.ElementId = Element.SelectedValue as string ?? string.Empty;
        _modele.Nom = $"Action : {LibellesEnum.LibelleAction(type)}";
        Resultat = _modele;
        DialogResult = true;
    }

    private void SurAnnuler(object sender, RoutedEventArgs e) => DialogResult = false;
}
