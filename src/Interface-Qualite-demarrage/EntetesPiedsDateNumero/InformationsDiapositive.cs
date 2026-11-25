namespace PowerPointClone.InterfaceQualite.EntetesPiedsDateNumero;
public sealed class InformationsDiapositive
{
    public string EnTete { get; set; } = "";
    public string PiedDePage { get; set; } = "";
    public bool AfficherDate { get; set; } = true;
    public bool DateAutomatique { get; set; } = true;
    public DateTime DateFixe { get; set; } = DateTime.Today;
    public bool AfficherNumero { get; set; } = true;
    public string ObtenirDate() => (DateAutomatique ? DateTime.Today : DateFixe).ToString("dd/MM/yyyy");
    public string ObtenirNumero(int index) => AfficherNumero ? (index + 1).ToString() : "";
}