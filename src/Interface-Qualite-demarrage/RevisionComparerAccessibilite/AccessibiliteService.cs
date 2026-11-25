namespace PowerPointClone.InterfaceQualite.RevisionComparerAccessibilite;
public sealed record ProblemeAccessibilite(string Element, string Probleme, string Correction);
public sealed class AccessibiliteService
{
    public List<ProblemeAccessibilite> Analyser(IEnumerable<(string Type, string TexteAlternatif, string Texte)> elements)
    {
        var result = new List<ProblemeAccessibilite>();
        foreach (var e in elements)
        {
            if (e.Type.Equals("Image", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(e.TexteAlternatif))
                result.Add(new("Image", "Texte alternatif absent", "Ajouter une description concise de l'image."));
            if (e.Type.Equals("Texte", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(e.Texte))
                result.Add(new("Zone de texte", "Zone vide", "Supprimer la zone ou ajouter son contenu."));
        }
        return result;
    }
}