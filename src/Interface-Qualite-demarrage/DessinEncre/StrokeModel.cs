namespace PowerPointClone.InterfaceQualite.DessinEncre;
public sealed record PointEncre(double X, double Y);
public sealed class StrokeModel
{
    public List<PointEncre> Points { get; } = [];
    public string Couleur { get; set; } = "#000000";
    public double Epaisseur { get; set; } = 2;
    public void AjouterPoint(double x, double y) { if (!double.IsFinite(x) || !double.IsFinite(y)) return; Points.Add(new(x, y)); }
    public void Effacer() => Points.Clear();
}