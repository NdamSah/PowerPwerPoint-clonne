using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.ViewModels;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Gestionnaire des diaporamas personnalisés (création, modification, suppression).</summary>
public partial class DialogueDiaporamasPersonnalises : Window
{
    private readonly PresentationMedia _presentation;
    private readonly ObservableCollection<LigneSelectionDiapositive> _lignes = new();
    private bool _modifie;

    public DialogueDiaporamasPersonnalises(PresentationMedia presentation)
    {
        InitializeComponent();
        _presentation = presentation;

        Liste.ItemsSource = presentation.DiaporamasPersonnalises;
        Diapositives.ItemsSource = _lignes;

        ReconstruireLignes(null);

        if (presentation.DiaporamasPersonnalises.Count > 0)
        {
            Liste.SelectedIndex = 0;
        }
    }

    /// <summary>Vrai si la collection a été modifiée.</summary>
    public bool Modifie => _modifie;

    private void ReconstruireLignes(IEnumerable<string>? selection)
    {
        var selectionIds = selection?.ToHashSet() ?? new HashSet<string>();
        _lignes.Clear();

        int numero = 1;
        foreach (var diapositive in _presentation.Diapositives)
        {
            _lignes.Add(new LigneSelectionDiapositive(
                diapositive.Id,
                $"{numero}. {diapositive.Titre}",
                selectionIds.Contains(diapositive.Id)));
            numero++;
        }
    }

    private void SurSelectionChange(object sender, SelectionChangedEventArgs e)
    {
        if (Liste.SelectedItem is not DiaporamaPersonnalise diaporama)
        {
            return;
        }

        Nom.Text = diaporama.Nom;
        ReconstruireLignes(diaporama.Diapositives);
    }

    private void SurNouveau(object sender, RoutedEventArgs e)
    {
        Liste.SelectedItem = null;
        Nom.Text = $"Diaporama {_presentation.DiaporamasPersonnalises.Count + 1}";
        ReconstruireLignes(null);
        Nom.Focus();
    }

    private void SurEnregistrer(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Nom.Text))
        {
            MessageBox.Show(this, "Donnez un nom au diaporama.", "Diaporamas personnalisés", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var ids = _lignes.Where(l => l.Selectionnee).Select(l => l.Id).ToList();
        if (ids.Count == 0)
        {
            MessageBox.Show(this, "Sélectionnez au moins une diapositive.", "Diaporamas personnalisés", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (Liste.SelectedItem is DiaporamaPersonnalise existant)
        {
            existant.Nom = Nom.Text.Trim();
            existant.Diapositives = ids;
        }
        else
        {
            _presentation.DiaporamasPersonnalises.Add(new DiaporamaPersonnalise
            {
                Nom = Nom.Text.Trim(),
                Diapositives = ids
            });
        }

        _modifie = true;
        Liste.Items.Refresh();
    }

    private void SurSupprimer(object sender, RoutedEventArgs e)
    {
        if (Liste.SelectedItem is not DiaporamaPersonnalise diaporama)
        {
            return;
        }

        _presentation.DiaporamasPersonnalises.Remove(diaporama);
        _modifie = true;
        Nom.Text = string.Empty;
        ReconstruireLignes(null);
    }

    private void SurFermer(object sender, RoutedEventArgs e)
    {
        DialogResult = _modifie;
        Close();
    }
}
