using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>Résultat d'un enregistrement d'écran (vidéo AVI + piste micro optionnelle).</summary>
public sealed record ResultatCaptureEcran(string CheminVideo, string? CheminAudio, TimeSpan Duree);

/// <summary>
/// Abstraction des boîtes de dialogue et des acquisitions de médias.
/// Les view models restent testables (aucune dépendance directe à WPF) et la couche vue
/// fournit l'implémentation concrète.
/// </summary>
public interface IServiceDialogues
{
    string? OuvrirPresentation();

    /// <summary>Enregistre la présentation ; retourne le chemin retenu ou <c>null</c> si annulé.</summary>
    string? EnregistrerPresentation(string? cheminActuel, string titreDefaut, bool choisirEmplacement);

    string? ChoisirFichier(string titre, string filtre);

    /// <summary>Ouvre la boîte « Enregistrer un son » et retourne le WAV produit.</summary>
    string? EnregistrerSon(PresentationMedia presentation);

    /// <summary>Capture une zone de l'écran (sélection + enregistrement).</summary>
    ResultatCaptureEcran? CapturerEcran(PresentationMedia presentation);

    /// <summary>Ouvre le lecteur vidéo intégré (lecture, pause, rognage).</summary>
    void AfficherVideo(ObjetVideo video, PresentationMedia presentation);

    ObjetZoom? EditerZoom(ObjetZoom? modele, PresentationMedia presentation);

    ObjetLien? EditerLien(ObjetLien? modele, DiapositiveMedia diapositive, PresentationMedia presentation);

    ObjetAction? EditerAction(ObjetAction? modele, DiapositiveMedia diapositive, PresentationMedia presentation);

    DiaporamaOptions? ConfigurerDiaporama(DiaporamaOptions options, PresentationMedia presentation);

    bool EditerDiaporamasPersonnalises(PresentationMedia presentation);

    bool AfficherMinutage(PresentationMedia presentation);

    void AfficherAideRaccourcis();

    void AfficherInformation(string message);

    void AfficherErreur(string message);

    bool Confirmer(string message, string titre);
}
