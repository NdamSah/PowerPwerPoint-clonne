using System.Windows;

namespace PowerPwerPoint.Media.Presentation;

/// <summary>Fenêtre d'aide rappelant les raccourcis du diaporama.</summary>
public partial class DialogueAideRaccourcis : Window
{
    public DialogueAideRaccourcis()
    {
        InitializeComponent();
    }

    private void SurFermer(object sender, RoutedEventArgs e) => Close();
}
