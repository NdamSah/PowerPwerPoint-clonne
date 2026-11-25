namespace PowerPointClone.InterfaceQualite.ConceptionTailleArrierePlan;
public sealed class FormatDiapositive
{
    public double Largeur { get; private set; } = 13.333;
    public double Hauteur { get; private set; } = 7.5;
    public string CouleurArrierePlan { get; private set; } = "#FFFFFF";
    public void DefinirFormat(double largeurPouces, double hauteurPouces)
    { if (largeurPouces <= 0 || hauteurPouces <= 0) throw new ArgumentOutOfRangeException(); Largeur = largeurPouces; Hauteur = hauteurPouces; }
    public void DefinirArrierePlan(string couleurHex)
    { if (!System.Text.RegularExpressions.Regex.IsMatch(couleurHex, "^#[0-9A-Fa-f]{6}$")) throw new ArgumentException("Couleur attendue : #RRGGBB."); CouleurArrierePlan = couleurHex; }
}