using System.IO;
using System.Windows;
using Microsoft.Win32;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Presentation;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Implémentation WPF des dialogues du module : sélecteurs de fichiers, boîtes média,
/// configuration du diaporama et messages d'erreur / confirmation.
/// </summary>
public sealed class ServiceDialoguesWpf : IServiceDialogues
{
    public const string FiltrePresentation =
        "Présentation média (*.media.json)|*.media.json|Tous les fichiers|*.*";

    private static Window? Proprietaire
        => Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive)
           ?? Application.Current?.Windows.OfType<Window>().FirstOrDefault()
           ?? Application.Current?.MainWindow;

    /// <summary>Sélecteur de fichier réutilisable (également utilisé par les boîtes de dialogue).</summary>
    public static string? DemanderFichier(string titre, string filtre)
    {
        var dialogue = new OpenFileDialog
        {
            Title = titre,
            Filter = filtre,
            CheckFileExists = true
        };

        return dialogue.ShowDialog(Proprietaire) == true ? dialogue.FileName : null;
    }

    public string? OuvrirPresentation()
    {
        var dialogue = new OpenFileDialog
        {
            Title = "Ouvrir une présentation (module média)",
            Filter = FiltrePresentation,
            CheckFileExists = true
        };

        return dialogue.ShowDialog(Proprietaire) == true ? dialogue.FileName : null;
    }

    public string? EnregistrerPresentation(string? cheminActuel, string titreDefaut, bool choisirEmplacement)
    {
        if (!choisirEmplacement && !string.IsNullOrWhiteSpace(cheminActuel))
        {
            return cheminActuel;
        }

        var dialogue = new SaveFileDialog
        {
            Title = "Enregistrer la présentation (module média)",
            Filter = FiltrePresentation,
            FileName = NomFichierSur(titreDefaut) + SerialisationMedia.Extension,
            AddExtension = true,
            DefaultExt = SerialisationMedia.Extension
        };

        if (!string.IsNullOrWhiteSpace(cheminActuel) && File.Exists(cheminActuel))
        {
            dialogue.InitialDirectory = Path.GetDirectoryName(cheminActuel);
            dialogue.FileName = Path.GetFileName(cheminActuel);
        }

        return dialogue.ShowDialog(Proprietaire) == true ? dialogue.FileName : null;
    }

    public string? ChoisirFichier(string titre, string filtre) => DemanderFichier(titre, filtre);

    public string? EnregistrerSon(PresentationMedia presentation)
    {
        var dialogue = new DialogueEnregistrementAudio(presentation) { Owner = Proprietaire };
        return dialogue.ShowDialog() == true ? dialogue.CheminRetenu : null;
    }

    public ResultatCaptureEcran? CapturerEcran(PresentationMedia presentation)
    {
        var selection = new FenetreCaptureZone();
        if (selection.ShowDialog() != true || selection.ZoneRetenue is not { } zone)
        {
            return null;
        }

        var enregistrement = new FenetreEnregistrementEcran(zone, presentation);
        return enregistrement.ShowDialog() == true ? enregistrement.Resultat : null;
    }

    public void AfficherVideo(ObjetVideo video, PresentationMedia presentation)
    {
        var fenetre = new LecteurVideoWindow(video, presentation) { Owner = Proprietaire };
        fenetre.ShowDialog();
    }

    public ObjetZoom? EditerZoom(ObjetZoom? modele, PresentationMedia presentation)
    {
        var dialogue = new DialogueZoom(modele, presentation) { Owner = Proprietaire };
        return dialogue.ShowDialog() == true ? dialogue.Resultat : null;
    }

    public ObjetLien? EditerLien(ObjetLien? modele, DiapositiveMedia diapositive, PresentationMedia presentation)
    {
        if (modele is null)
        {
            return null;
        }

        var dialogue = new DialogueLien(modele, diapositive, presentation) { Owner = Proprietaire };
        return dialogue.ShowDialog() == true ? dialogue.Resultat : null;
    }

    public ObjetAction? EditerAction(ObjetAction? modele, DiapositiveMedia diapositive, PresentationMedia presentation)
    {
        if (modele is null)
        {
            return null;
        }

        var dialogue = new DialogueAction(modele, diapositive, presentation) { Owner = Proprietaire };
        return dialogue.ShowDialog() == true ? dialogue.Resultat : null;
    }

    public DiaporamaOptions? ConfigurerDiaporama(DiaporamaOptions options, PresentationMedia presentation)
    {
        var dialogue = new DialogueConfigurationDiaporama(options, presentation) { Owner = Proprietaire };
        return dialogue.ShowDialog() == true ? dialogue.Resultat : null;
    }

    public bool EditerDiaporamasPersonnalises(PresentationMedia presentation)
    {
        var dialogue = new DialogueDiaporamasPersonnalises(presentation) { Owner = Proprietaire };
        dialogue.ShowDialog();
        return dialogue.Modifie;
    }

    public bool AfficherMinutage(PresentationMedia presentation)
    {
        var dialogue = new DialogueMinutage(presentation) { Owner = Proprietaire };
        dialogue.ShowDialog();
        return dialogue.Applique;
    }

    public void AfficherAideRaccourcis()
    {
        var dialogue = new DialogueAideRaccourcis { Owner = Proprietaire };
        dialogue.ShowDialog();
    }

    public void AfficherInformation(string message)
        => MessageBox.Show(message, "Média, Transitions, Animations & Diaporama", MessageBoxButton.OK, MessageBoxImage.Information);

    public void AfficherErreur(string message)
        => MessageBox.Show(message, "Média, Transitions, Animations & Diaporama", MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirmer(string message, string titre)
        => MessageBox.Show(message, titre, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;

    private static string NomFichierSur(string titre)
    {
        string nom = string.IsNullOrWhiteSpace(titre) ? "Presentation" : titre.Trim();
        foreach (char interdit in Path.GetInvalidFileNameChars())
        {
            nom = nom.Replace(interdit, '_');
        }

        return nom;
    }
}
