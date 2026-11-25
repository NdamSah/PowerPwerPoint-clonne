using System.Diagnostics;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Chronomètre du diaporama : temps total, temps de la diapositive courante, pause et remise à zéro.
/// Utilisé par le mode Présentateur et par la répétition des minutages.
/// </summary>
public sealed class ChronometreDiaporama
{
    private readonly Stopwatch _total = new();
    private readonly Stopwatch _diapositive = new();

    public TimeSpan Total => _total.Elapsed;

    public TimeSpan DiapositiveCourante => _diapositive.Elapsed;

    public bool EnPause { get; private set; }

    public bool Demarre { get; private set; }

    public void Demarrer()
    {
        DemarrerDiapositive();
        _total.Start();
        Demarre = true;
        EnPause = false;
    }

    public void DemarrerDiapositive()
    {
        _diapositive.Reset();
        if (!EnPause)
        {
            _diapositive.Start();
        }
    }

    public void Pause()
    {
        if (EnPause)
        {
            return;
        }

        EnPause = true;
        _total.Stop();
        _diapositive.Stop();
    }

    public void Reprendre()
    {
        if (!EnPause)
        {
            return;
        }

        EnPause = false;
        _total.Start();
        _diapositive.Start();
    }

    public void BasculerPause()
    {
        if (EnPause)
        {
            Reprendre();
        }
        else
        {
            Pause();
        }
    }

    public void Reinitialiser()
    {
        _total.Reset();
        _diapositive.Reset();
        Demarre = false;
        EnPause = false;
    }

    public static string Formater(TimeSpan duree)
        => duree.TotalHours >= 1
            ? $"{(int)duree.TotalHours}:{duree.Minutes:00}:{duree.Seconds:00}"
            : $"{duree.Minutes:00}:{duree.Seconds:00}";
}
