namespace PowerPointClone.InterfaceQualite.BarreAccesRapide;
public sealed class BarreAccesRapideService
{
    private readonly List<string> _commandes = ["Nouveau", "Ouvrir", "Enregistrer", "Annuler", "Retablir"];
    public IReadOnlyList<string> Commandes => _commandes.AsReadOnly();
    public void Ajouter(string commande) { Verifier(commande); if (!_commandes.Contains(commande)) _commandes.Add(commande); }
    public void Retirer(string commande) => _commandes.Remove(commande);
    public void Deplacer(string commande, int nouvelIndex)
    { int i = _commandes.IndexOf(commande); if (i < 0) throw new ArgumentException("Commande inconnue."); _commandes.RemoveAt(i); _commandes.Insert(Math.Clamp(nouvelIndex, 0, _commandes.Count), commande); }
    private static void Verifier(string s) { if (string.IsNullOrWhiteSpace(s)) throw new ArgumentException("Nom obligatoire."); }
}