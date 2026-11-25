using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PowerPointClone.InterfaceQualite.AffichageReglesGrilleZoom;

public sealed class AffichageReglesGrilleZoomViewModel : INotifyPropertyChanged
{
    private bool _reglesVisibles = true, _grilleVisible, _reperesVisibles = true;
    private double _zoom = 100;
    public bool ReglesVisibles { get => _reglesVisibles; set => Set(ref _reglesVisibles, value); }
    public bool GrilleVisible { get => _grilleVisible; set => Set(ref _grilleVisible, value); }
    public bool ReperesVisibles { get => _reperesVisibles; set => Set(ref _reperesVisibles, value); }
    public double Zoom { get => _zoom; set => Set(ref _zoom, Math.Clamp(value, 10, 400)); }
    public void ZoomAvant() => Zoom += 10;
    public void ZoomArriere() => Zoom -= 10;
    public void ZoomReinitialiser() => Zoom = 100;
    public void AdapterFenetre(double largeurDisponible, double hauteurDisponible, double largeurDiapo, double hauteurDiapo)
    {
        if (largeurDiapo <= 0 || hauteurDiapo <= 0) return;
        Zoom = Math.Clamp(Math.Min(largeurDisponible / largeurDiapo, hauteurDisponible / hauteurDiapo) * 100, 10, 400);
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    { if (Equals(field, value)) return; field = value; PropertyChanged?.Invoke(this, new(name)); }
}