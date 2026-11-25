using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Gère le stockage physique des médias : les fichiers importés sont copiés dans un dossier
/// <c>&lt;présentation&gt;_media</c> afin de voyager avec le document, comme le fait PowerPoint.
/// </summary>
public static class StockageMedia
{
    public static readonly string[] ExtensionsVideo =
    {
        ".mp4", ".wmv", ".avi", ".mov", ".mkv", ".webm", ".m4v"
    };

    public static readonly string[] ExtensionsAudio =
    {
        ".wav", ".mp3", ".wma", ".m4a", ".aac", ".flac", ".ogg"
    };

    public static string FiltreVideo
        => "Fichiers vidéo|" + string.Join(";", ExtensionsVideo.Select(e => "*" + e)) + "|Tous les fichiers|*.*";

    public static string FiltreAudio
        => "Fichiers audio|" + string.Join(";", ExtensionsAudio.Select(e => "*" + e)) + "|Tous les fichiers|*.*";

    public static string FiltreSon
        => "Sons (*.wav)|*.wav|Tous les fichiers|*.*";

    /// <summary>Dossier média associé à la présentation, ou dossier temporaire si non enregistrée.</summary>
    public static string DossierPour(PresentationMedia presentation)
    {
        string? dossier = presentation.DossierMedia;
        if (!string.IsNullOrEmpty(dossier))
        {
            Directory.CreateDirectory(dossier);
            return dossier;
        }

        string temp = Path.Combine(Path.GetTempPath(), "PowerPwerPointClone", "Media");
        Directory.CreateDirectory(temp);
        return temp;
    }

    /// <summary>Copie un fichier externe dans le dossier média et retourne le chemin de la copie.</summary>
    public static string Importer(string cheminSource, PresentationMedia presentation)
    {
        if (string.IsNullOrWhiteSpace(cheminSource) || !File.Exists(cheminSource))
        {
            throw new FileNotFoundException("Le fichier média est introuvable.", cheminSource);
        }

        string dossier = DossierPour(presentation);
        string nom = Path.GetFileName(cheminSource);
        string destination = Path.Combine(dossier, nom);
        int suffixe = 1;

        while (File.Exists(destination) &&
               !string.Equals(Path.GetFullPath(destination), Path.GetFullPath(cheminSource), StringComparison.OrdinalIgnoreCase))
        {
            destination = Path.Combine(dossier, $"{Path.GetFileNameWithoutExtension(nom)}_{suffixe++}{Path.GetExtension(nom)}");
        }

        if (!string.Equals(Path.GetFullPath(destination), Path.GetFullPath(cheminSource), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(cheminSource, destination, overwrite: true);
        }

        return destination;
    }

    /// <summary>Chemin absolu exploitable pour la lecture, ou <c>null</c> si le fichier a disparu.</summary>
    public static string? Resoudre(string? chemin, PresentationMedia presentation)
    {
        if (string.IsNullOrWhiteSpace(chemin))
        {
            return null;
        }

        if (Path.IsPathRooted(chemin))
        {
            return File.Exists(chemin) ? chemin : null;
        }

        string? dossier = presentation.DossierMedia;
        if (string.IsNullOrEmpty(dossier))
        {
            return null;
        }

        string combine = Path.Combine(dossier, chemin);
        return File.Exists(combine) ? combine : null;
    }
}
