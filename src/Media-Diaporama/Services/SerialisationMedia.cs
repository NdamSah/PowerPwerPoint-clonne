using System.Text.Json;
using System.Text.Json.Serialization;
using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Persistance du module média. Format de travail JSON (<c>*.media.json</c>) utilisé tant que
/// l'import/export OpenXML n'est pas branché : il permet de valider la sauvegarde et la
/// réouverture des médias, transitions, animations et minutages.
/// </summary>
public static class SerialisationMedia
{
    public const string Extension = ".media.json";

    public static JsonSerializerOptions Options { get; } = CreerOptions();

    private static JsonSerializerOptions CreerOptions()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter());
        return options;
    }

    public static void Enregistrer(PresentationMedia presentation, string chemin)
    {
        ArgumentNullException.ThrowIfNull(presentation);
        if (string.IsNullOrWhiteSpace(chemin))
        {
            throw new ArgumentException("Le chemin d'enregistrement est requis.", nameof(chemin));
        }

        string? dossier = Path.GetDirectoryName(Path.GetFullPath(chemin));
        if (!string.IsNullOrEmpty(dossier))
        {
            Directory.CreateDirectory(dossier);
        }

        File.WriteAllText(chemin, Serialiser(presentation));
        presentation.CheminFichier = Path.GetFullPath(chemin);
    }

    public static PresentationMedia Charger(string chemin)
    {
        if (!File.Exists(chemin))
        {
            throw new FileNotFoundException("Fichier introuvable.", chemin);
        }

        var presentation = Deserialiser(File.ReadAllText(chemin));
        presentation.CheminFichier = Path.GetFullPath(chemin);
        return presentation;
    }

    public static string Serialiser(PresentationMedia presentation)
        => JsonSerializer.Serialize(presentation, Options);

    public static PresentationMedia Deserialiser(string json)
    {
        var presentation = JsonSerializer.Deserialize<PresentationMedia>(json, Options)
            ?? throw new InvalidDataException("Le contenu du fichier n'est pas une présentation valide.");

        Reparer(presentation);
        return presentation;
    }

    /// <summary>Sécurise les collections après désérialisation (fichiers anciens ou partiels).</summary>
    private static void Reparer(PresentationMedia presentation)
    {
        presentation.Diapositives ??= new();
        presentation.Options ??= new DiaporamaOptions();
        presentation.DiaporamasPersonnalises ??= new();
        presentation.Minutages ??= new();

        foreach (var diapositive in presentation.Diapositives)
        {
            diapositive.Transition ??= new TransitionDiapositive();
            diapositive.Animations ??= new();
            diapositive.Elements ??= new();
            diapositive.Objets ??= new();

            foreach (var effet in diapositive.Animations)
            {
                effet.CibleId ??= string.Empty;
            }
        }
    }
}
