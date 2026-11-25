namespace PowerPointClone.InterfaceQualite.RevisionOrthographe;
public sealed class OrthographeService
{
    // Vérification locale minimale ; remplacer/compléter par un dictionnaire français lors de l'intégration.
    private static readonly HashSet<string> MotsConnus = new(StringComparer.OrdinalIgnoreCase)
    { "bonjour", "présentation", "diapositive", "projet", "équipe", "texte", "exemple", "powerpoint", "logiciel", "ordinateur" };
    public IReadOnlyList<string> TrouverMotsInconnus(string texte)
    {
        var mots = System.Text.RegularExpressions.Regex.Matches(texte ?? "", @"[\p{L}']+").Select(m => m.Value);
        return mots.Where(m => m.Length > 2 && !MotsConnus.Contains(m)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }
}