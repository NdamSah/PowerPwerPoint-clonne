namespace PowerPointClone.InterfaceQualite.OptionsFenetre;
public enum ThemeApplication { Clair, Sombre, Systeme }
public sealed class OptionsFenetreService
{
    public ThemeApplication Theme { get; set; } = ThemeApplication.Systeme;
    public bool ToujoursAuPremierPlan { get; set; }
    public bool ConfirmerFermeture { get; set; } = true;
    public bool AfficherInfoDemarrage { get; set; } = true;
    public double Largeur { get; set; } = 1200;
    public double Hauteur { get; set; } = 800;
    public void DefinirTaille(double largeur, double hauteur)
    { if (largeur < 640 || hauteur < 480) throw new ArgumentOutOfRangeException("Taille minimale : 640 x 480."); Largeur = largeur; Hauteur = hauteur; }
}