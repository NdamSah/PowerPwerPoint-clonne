using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Enregistrement d'une zone de l'écran. Les images sont capturées par GDI+ à la cadence
/// demandée puis encodées en JPEG dans un AVI MJPEG (voir <see cref="AviWriter"/>).
/// Le microphone peut être enregistré en parallèle dans un WAV séparé, attaché à la vidéo.
/// </summary>
public sealed class EnregistreurEcran : IDisposable
{
    private readonly object _verrou = new();
    private Thread? _fil;
    private volatile bool _enCours;
    private AviWriter? _ecriture;
    private EnregistreurAudio? _micro;
    private Rectangle _zone;
    private int _imagesParSeconde;
    private int _imagesCapturees;
    private bool _dispose;

    public bool EnCours => _enCours;

    public string? CheminVideo { get; private set; }

    public string? CheminAudio { get; private set; }

    public int ImagesCapturees => _imagesCapturees;

    public TimeSpan Duree { get; private set; }

    public event EventHandler<string>? Erreur;

    /// <summary>Démarre la capture. <paramref name="cheminAudio"/> est optionnel (microphone).</summary>
    public void Demarrer(Rectangle zone, int imagesParSeconde, string cheminVideo, string? cheminAudio = null)
    {
        if (_enCours)
        {
            throw new InvalidOperationException("Un enregistrement est déjà en cours.");
        }

        if (zone.Width <= 8 || zone.Height <= 8)
        {
            throw new ArgumentException("La zone de capture est trop petite.", nameof(zone));
        }

        _zone = zone;
        _imagesParSeconde = Math.Clamp(imagesParSeconde, 1, 30);
        _imagesCapturees = 0;
        Duree = TimeSpan.Zero;
        CheminVideo = cheminVideo;
        CheminAudio = null;

        // Les dimensions doivent être paires pour un encodage MJPEG propre.
        _zone.Width -= _zone.Width % 2;
        _zone.Height -= _zone.Height % 2;

        _ecriture = new AviWriter(cheminVideo, _zone.Width, _zone.Height, _imagesParSeconde);

        if (!string.IsNullOrWhiteSpace(cheminAudio))
        {
            try
            {
                _micro = new EnregistreurAudio();
                _micro.Demarrer(cheminAudio);
                CheminAudio = cheminAudio;
            }
            catch (Exception ex)
            {
                CheminAudio = null;
                Erreur?.Invoke(this, $"Capture vidéo sans audio : {ex.Message}");
            }
        }

        _enCours = true;
        _fil = new Thread(BoucleCapture)
        {
            IsBackground = true,
            Name = "CaptureEcran"
        };
        _fil.Start();
    }

    /// <summary>Arrête la capture et finalise les fichiers. Retourne le chemin de la vidéo.</summary>
    public string? Arreter()
    {
        if (!_enCours)
        {
            return CheminVideo;
        }

        _enCours = false;
        _fil?.Join(TimeSpan.FromSeconds(5));
        _fil = null;

        lock (_verrou)
        {
            _ecriture?.Terminer();
            _ecriture = null;
        }

        if (_micro is not null)
        {
            try
            {
                _micro.Arreter();
                _micro.Dispose();
            }
            catch
            {
                // Périphérique audio déjà libéré.
            }

            _micro = null;
        }

        return CheminVideo;
    }

    private void BoucleCapture()
    {
        var chrono = Stopwatch.StartNew();
        double intervalle = 1000.0 / _imagesParSeconde;
        int numero = 0;

        try
        {
            using var bitmap = new Bitmap(_zone.Width, _zone.Height, PixelFormat.Format24bppRgb);
            using var graphiques = Graphics.FromImage(bitmap);
            using var parametres = new EncoderParameters(1);
            parametres.Param[0] = new EncoderParameter(Encoder.Quality, 70L);
            ImageCodecInfo? encodeur = ImageCodecInfo.GetImageEncoders()
                .FirstOrDefault(c => c.FormatID == ImageFormat.Jpeg.Guid);

            while (_enCours)
            {
                graphiques.CopyFromScreen(_zone.Left, _zone.Top, 0, 0, _zone.Size, CopyPixelOperation.SourceCopy);

                using var memoire = new MemoryStream();
                if (encodeur is not null)
                {
                    bitmap.Save(memoire, encodeur, parametres);
                }
                else
                {
                    bitmap.Save(memoire, ImageFormat.Jpeg);
                }

                lock (_verrou)
                {
                    _ecriture?.AjouterImage(memoire.ToArray());
                }

                numero++;
                _imagesCapturees = numero;
                Duree = chrono.Elapsed;

                double attente = (numero * intervalle) - chrono.Elapsed.TotalMilliseconds;
                if (attente > 0)
                {
                    Thread.Sleep((int)attente);
                }
            }
        }
        catch (Exception ex)
        {
            _enCours = false;
            Erreur?.Invoke(this, $"Capture d'écran interrompue : {ex.Message}");
        }
        finally
        {
            lock (_verrou)
            {
                if (_ecriture is not null)
                {
                    _ecriture.Terminer();
                    _ecriture = null;
                }
            }
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
        Erreur = null;
    }
}
