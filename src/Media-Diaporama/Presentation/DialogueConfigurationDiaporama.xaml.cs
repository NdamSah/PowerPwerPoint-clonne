using System.Windows;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;
using PowerPwerPoint.Media.ViewModels;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Boîte « Configurer le diaporama » : type, options d'affichage et plage.</summary>
public partial class DialogueConfigurationDiaporama : Window
{
    private readonly DiaporamaOptions _options;
    private readonly PresentationMedia _presentation;

    public DialogueConfigurationDiaporama(DiaporamaOptions options, PresentationMedia presentation)
    {
        InitializeComponent();
        _options = options.Copier();
        _presentation = presentation;

        Type.ItemsSource = OptionsEnum.Creer<TypeDiaporama>(LibellesEnum.LibelleTypeDiaporama);
        Type.SelectedValue = _options.Type;
        Boucle.IsChecked = _options.BoucleContinue;
        SansAnimation.IsChecked = _options.SansAnimation;
        SansNarration.IsChecked = _options.SansNarration;
        Presentateur.IsChecked = _options.ModePresentateur;
        Debut.Text = _options.Debut.ToString();
        Fin.Text = _options.Fin.ToString();

        var personnalises = new List<OptionDiaporamaPersonnalise> { new(null, "(Aucun — utiliser la plage)") };
        personnalises.AddRange(presentation.DiaporamasPersonnalises.Select(d => new OptionDiaporamaPersonnalise(d.Nom, d.Nom)));
        Personnalise.ItemsSource = personnalises;
        Personnalise.SelectedValue = _options.DiaporamaPersonnalise;

        Resultat = null;
    }

    public DiaporamaOptions? Resultat { get; private set; }

    private void SurValider(object sender, RoutedEventArgs e)
    {
        int.TryParse(Debut.Text, out int debut);
        int.TryParse(Fin.Text, out int fin);

        int total = _presentation.Diapositives.Count;
        if (debut > total || fin > total)
        {
            MessageBox.Show(this, $"La présentation contient {total} diapositive(s).", "Diaporama", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (debut > 0 && fin > 0 && fin < debut)
        {
            MessageBox.Show(this, "La diapositive de fin doit suivre celle du début.", "Diaporama", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        _options.Type = Type.SelectedValue is TypeDiaporama type ? type : TypeDiaporama.Intervenant;
        _options.BoucleContinue = Boucle.IsChecked == true;
        _options.SansAnimation = SansAnimation.IsChecked == true;
        _options.SansNarration = SansNarration.IsChecked == true;
        _options.ModePresentateur = Presentateur.IsChecked == true;
        _options.Debut = Math.Max(0, debut);
        _options.Fin = Math.Max(0, fin);
        _options.DiaporamaPersonnalise = Personnalise.SelectedValue as string;

        Resultat = _options;
        DialogResult = true;
    }

    private void SurAnnuler(object sender, RoutedEventArgs e) => DialogResult = false;
}

/// <summary>Option de la liste des diaporamas personnalisés.</summary>
public sealed record OptionDiaporamaPersonnalise(string? Nom, string Libelle);
