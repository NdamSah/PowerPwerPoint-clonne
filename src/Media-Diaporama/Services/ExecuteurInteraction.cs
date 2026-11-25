using System.Diagnostics;
using System.Media;
using System.Windows.Media;
using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Exécution des liens hypertexte et des actions pendant le diaporama.
/// Toute erreur est convertie en message lisible : le diaporama ne doit jamais planter
/// à cause d'une cible manquante (fichier déplacé, programme absent, URL invalide).
/// </summary>
public static class ExecuteurInteraction
{
    private static readonly List<MediaPlayer> LecteursActifs = new();

    public static bool ExecuterLien(
        ObjetLien lien,
        PresentationMedia presentation,
        Action<int> allerADiapositive,
        out string message)
    {
        message = string.Empty;

        switch (lien.TypeLien)
        {
            case TypeLien.PageWeb:
                return OuvrirExterne(lien.Cible, out message);

            case TypeLien.Email:
                return OuvrirExterne($"mailto:{lien.Cible}", out message);

            case TypeLien.Fichier:
            {
                string? chemin = StockageMedia.Resoudre(lien.Cible, presentation);
                if (chemin is null && File.Exists(lien.Cible))
                {
                    chemin = lien.Cible;
                }

                if (chemin is null)
                {
                    message = $"Fichier lié introuvable : {lien.Cible}";
                    return false;
                }

                return OuvrirExterne(chemin, out message);
            }

            case TypeLien.Diapositive:
            {
                int index = presentation.Diapositives.ToList().FindIndex(d => d.Id == lien.Cible);
                if (index < 0)
                {
                    message = "La diapositive liée n'existe plus.";
                    return false;
                }

                allerADiapositive(index);
                message = "Navigation vers la diapositive liée.";
                return true;
            }

            default:
                return false;
        }
    }

    public static bool ExecuterAction(
        ObjetAction action,
        PresentationMedia presentation,
        int indexCourant,
        Action<int> allerADiapositive,
        Action terminerDiaporama,
        out string message)
    {
        message = string.Empty;

        switch (action.TypeAction)
        {
            case TypeAction.AllerDiapositive:
            {
                int index = presentation.Diapositives.ToList().FindIndex(d => d.Id == action.Parametre);
                if (index < 0)
                {
                    message = "Diapositive cible introuvable.";
                    return false;
                }

                allerADiapositive(index);
                return true;
            }

            case TypeAction.DiapositiveSuivante:
                allerADiapositive(indexCourant + 1);
                return true;

            case TypeAction.DiapositivePrecedente:
                allerADiapositive(indexCourant - 1);
                return true;

            case TypeAction.PremiereDiapositive:
                allerADiapositive(0);
                return true;

            case TypeAction.DerniereDiapositive:
                allerADiapositive(presentation.Diapositives.Count - 1);
                return true;

            case TypeAction.FinDiaporama:
                terminerDiaporama();
                return true;

            case TypeAction.LancerProgramme:
            {
                if (!File.Exists(action.Parametre))
                {
                    message = $"Programme introuvable : {action.Parametre}";
                    return false;
                }

                try
                {
                    var info = new ProcessStartInfo(action.Parametre)
                    {
                        UseShellExecute = true,
                        Arguments = action.Arguments ?? string.Empty
                    };
                    Process.Start(info);
                    return true;
                }
                catch (Exception ex)
                {
                    message = $"Impossible de lancer le programme : {ex.Message}";
                    return false;
                }
            }

            case TypeAction.LireSon:
            {
                string? chemin = StockageMedia.Resoudre(action.Parametre, presentation);
                if (chemin is null)
                {
                    message = $"Son introuvable : {action.Parametre}";
                    return false;
                }

                JouerSon(chemin);
                return true;
            }

            default:
                return false;
        }
    }

    public static bool OuvrirExterne(string cible, out string message)
    {
        message = string.Empty;
        if (string.IsNullOrWhiteSpace(cible))
        {
            message = "Cible vide.";
            return false;
        }

        try
        {
            Process.Start(new ProcessStartInfo(cible) { UseShellExecute = true });
            return true;
        }
        catch (Exception ex)
        {
            message = $"Impossible d'ouvrir « {cible} » : {ex.Message}";
            return false;
        }
    }

    /// <summary>Joue un son (WAV via SoundPlayer, autres formats via MediaPlayer).</summary>
    public static void JouerSon(string chemin, int volume = 90)
    {
        try
        {
            if (chemin.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
            {
                using var lecteur = new SoundPlayer(chemin);
                lecteur.Play();
                return;
            }

            var media = new MediaPlayer();
            media.Open(new Uri(chemin, UriKind.Absolute));
            media.Volume = Math.Clamp(volume / 100.0, 0, 1);
            media.MediaFailed += (_, _) => LecteursActifs.Remove(media);
            media.MediaEnded += (_, _) => LecteursActifs.Remove(media);
            LecteursActifs.Add(media);
            media.Play();
        }
        catch
        {
            // Un son manquant ne doit jamais interrompre le diaporama.
        }
    }
}
