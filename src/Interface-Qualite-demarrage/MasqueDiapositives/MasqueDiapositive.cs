namespace PowerPointClone.InterfaceQualite.MasqueDiapositives;
public sealed class MasqueDiapositive
{
    public string Nom { get; set; } = "Masque principal";
    public string PoliceTitre { get; set; } = "Aptos Display";
    public double TailleTitre { get; set; } = 30;
    public string CouleurFond { get; set; } = "#FFFFFF";
    public bool AfficherPiedDePage { get; set; } = true;
    public bool AfficherNumero { get; set; } = true;
}