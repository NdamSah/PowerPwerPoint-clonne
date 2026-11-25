using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>Demande de lancement d'un diaporama émise par les panneaux.</summary>
public sealed record DemandeDiaporama(
    IReadOnlyList<DiapositiveMedia> Sequence,
    int IndexDepart,
    DiaporamaOptions Options,
    bool ModePresentateur,
    bool EnregistrerDiaporama,
    bool RepetitionMinutage)
{
    public bool EstVide => Sequence.Count == 0;
}

/// <summary>Demande d'aperçu des animations d'une diapositive.</summary>
public sealed record DemandeApercuAnimations(DiapositiveMedia Diapositive, AnimationEffet? Depuis);
