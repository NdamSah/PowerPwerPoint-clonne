using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Calcule la séquence de diapositives réellement présentée : elle ignore toujours les
/// diapositives masquées, puis applique soit la plage « de … à … », soit un diaporama personnalisé.
/// </summary>
public static class PlageDiaporama
{
    /// <summary>Applique la plage 1-based <see cref="DiaporamaOptions.Debut"/>/<see cref="DiaporamaOptions.Fin"/> aux diapositives visibles.</summary>
    public static IReadOnlyList<DiapositiveMedia> Resoudre(
        IEnumerable<DiapositiveMedia> diapositives,
        DiaporamaOptions options)
    {
        var visibles = diapositives.Where(d => !d.Masquee).ToList();
        if (visibles.Count == 0)
        {
            return visibles;
        }

        int debut = options.Debut <= 0 ? 1 : Math.Min(options.Debut, visibles.Count);
        int fin = options.Fin <= 0 ? visibles.Count : Math.Min(options.Fin, visibles.Count);

        if (fin < debut)
        {
            (debut, fin) = (fin, debut);
        }

        return visibles.Skip(debut - 1).Take(fin - debut + 1).ToList();
    }

    /// <summary>
    /// Applique un diaporama personnalisé : ordre et contenu de la sélection, en conservant
    /// l'ordre demandé et en ignorant les diapositives masquées ou supprimées.
    /// </summary>
    public static IReadOnlyList<DiapositiveMedia> ResoudrePersonnalise(
        IEnumerable<DiapositiveMedia> diapositives,
        DiaporamaPersonnalise personnalise)
    {
        var visibles = diapositives.Where(d => !d.Masquee).ToList();
        var sequence = new List<DiapositiveMedia>(personnalise.Diapositives.Count);

        foreach (var id in personnalise.Diapositives)
        {
            var diapositive = visibles.FirstOrDefault(d => d.Id == id);
            if (diapositive is not null && !sequence.Contains(diapositive))
            {
                sequence.Add(diapositive);
            }
        }

        return sequence;
    }
}
