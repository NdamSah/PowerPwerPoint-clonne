namespace PowerPointClone.InterfaceQualite.RaccourcisGlobaux;
public sealed class RaccourcisService
{
    private readonly Dictionary<string, string> _raccourcis = new(StringComparer.OrdinalIgnoreCase)
    { ["Nouveau"] = "Ctrl+N", ["Ouvrir"] = "Ctrl+O", ["Enregistrer"] = "Ctrl+S", ["Annuler"] = "Ctrl+Z", ["Retablir"] = "Ctrl+Y", ["Diaporama"] = "F5" };
    public IReadOnlyDictionary<string, string> Tous => _raccourcis;
    public void Definir(string action, string combinaison)
    { if (string.IsNullOrWhiteSpace(action) || string.IsNullOrWhiteSpace(combinaison)) throw new ArgumentException("Action et raccourci obligatoires."); _raccourcis[action] = combinaison; }
    public bool EssayerObtenirAction(string combinaison, out string? action)
    { var paire = _raccourcis.FirstOrDefault(p => p.Value.Equals(combinaison, StringComparison.OrdinalIgnoreCase)); action = paire.Key; return action != null; }
}