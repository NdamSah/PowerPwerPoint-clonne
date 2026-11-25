using NAudio.Wave;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Enregistrement d'un son depuis le microphone par défaut (WAV PCM 16 bits mono 44,1 kHz).
/// Expose le niveau courant pour l'indicateur visuel de la boîte « Enregistrer un son ».
/// </summary>
public sealed class EnregistreurAudio : IDisposable
{
    private WaveInEvent? _entree;
    private WaveFileWriter? _ecriture;
    private bool _dispose;

    public bool EnCours { get; private set; }

    public string? DernierFichier { get; private set; }

    public TimeSpan Duree { get; private set; }

    /// <summary>Niveau du signal entre 0 et 1 (crête du dernier tampon).</summary>
    public event EventHandler<float>? NiveauChange;

    public event EventHandler<string>? Erreur;

    public void Demarrer(string cheminWav)
    {
        if (EnCours)
        {
            throw new InvalidOperationException("Un enregistrement est déjà en cours.");
        }

        string? dossier = Path.GetDirectoryName(Path.GetFullPath(cheminWav));
        if (!string.IsNullOrEmpty(dossier))
        {
            Directory.CreateDirectory(dossier);
        }

        try
        {
            var format = new WaveFormat(44100, 16, 1);
            _ecriture = new WaveFileWriter(cheminWav, format);
            _entree = new WaveInEvent
            {
                WaveFormat = format,
                BufferMilliseconds = 50,
                NumberOfBuffers = 3
            };

            _entree.DataAvailable += SurDonnees;
            _entree.RecordingStopped += SurArret;
            _entree.StartRecording();

            DernierFichier = cheminWav;
            Duree = TimeSpan.Zero;
            EnCours = true;
        }
        catch (Exception ex)
        {
            Liberer();
            Erreur?.Invoke(this, $"Microphone indisponible : {ex.Message}");
            throw;
        }
    }

    public void Arreter()
    {
        if (!EnCours)
        {
            return;
        }

        EnCours = false;

        try
        {
            _entree?.StopRecording();
        }
        catch
        {
            // Arrêt déjà effectué ou périphérique retiré.
        }

        Liberer();
    }

    private void SurDonnees(object? sender, WaveInEventArgs e)
    {
        if (_ecriture is null)
        {
            return;
        }

        _ecriture.Write(e.Buffer, 0, e.BytesRecorded);
        Duree += TimeSpan.FromSeconds((double)e.BytesRecorded / (44100 * 2));

        float crete = 0;
        for (int i = 0; i + 1 < e.BytesRecorded; i += 2)
        {
            short echantillon = (short)(e.Buffer[i] | (e.Buffer[i + 1] << 8));
            float valeur = Math.Abs(echantillon / 32768f);
            if (valeur > crete)
            {
                crete = valeur;
            }
        }

        NiveauChange?.Invoke(this, Math.Min(1f, crete));
    }

    private void SurArret(object? sender, StoppedEventArgs e)
    {
        if (e.Exception is not null)
        {
            Erreur?.Invoke(this, $"Enregistrement interrompu : {e.Exception.Message}");
        }
    }

    private void Liberer()
    {
        if (_entree is not null)
        {
            _entree.DataAvailable -= SurDonnees;
            _entree.RecordingStopped -= SurArret;
            _entree.Dispose();
            _entree = null;
        }

        if (_ecriture is not null)
        {
            _ecriture.Flush();
            _ecriture.Dispose();
            _ecriture = null;
        }
    }

    public void Dispose()
    {
        if (_dispose)
        {
            return;
        }

        _dispose = true;
        Arreter();
        NiveauChange = null;
        Erreur = null;
    }
}
