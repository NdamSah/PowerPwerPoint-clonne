using System.IO;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Écrit un fichier AVI contenant un flux vidéo MJPEG (une image JPEG par image).
/// Implémentation RIFF minimale et autonome : aucune dépendance d'encodage externe n'est requise,
/// ce qui permet d'insérer immédiatement une capture d'écran dans la présentation.
/// </summary>
public sealed class AviWriter : IDisposable
{
    private const uint AvifHasIndex = 0x10;
    private const uint AviifKeyFrame = 0x10;

    private readonly FileStream _flux;
    private readonly BinaryWriter _ecriture;
    private readonly int _largeur;
    private readonly int _hauteur;
    private readonly int _imagesParSeconde;
    private readonly List<(long Position, int Taille)> _index = new();

    private long _positionTailleRiff;
    private long _positionTailleHdrl;
    private long _positionTailleMovi;
    private long _positionTotalImagesAvih;
    private long _positionDureeStrh;
    private long _positionDebutMovi;
    private int _images;
    private bool _termine;

    public AviWriter(string chemin, int largeur, int hauteur, int imagesParSeconde)
    {
        if (largeur <= 0 || hauteur <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(largeur), "Dimensions invalides.");
        }

        if (imagesParSeconde <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(imagesParSeconde), "Cadence invalide.");
        }

        _largeur = largeur;
        _hauteur = hauteur;
        _imagesParSeconde = imagesParSeconde;
        Chemin = chemin;

        string? dossier = Path.GetDirectoryName(Path.GetFullPath(chemin));
        if (!string.IsNullOrEmpty(dossier))
        {
            Directory.CreateDirectory(dossier);
        }

        _flux = new FileStream(chemin, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
        _ecriture = new BinaryWriter(_flux);
        EcrireEntete();
    }

    public string Chemin { get; }

    public int NombreImages => _images;

    public void AjouterImage(ReadOnlySpan<byte> jpeg)
    {
        if (_termine)
        {
            throw new InvalidOperationException("Le fichier AVI est déjà terminé.");
        }

        if (jpeg.Length <= 0)
        {
            return;
        }

        long position = _flux.Position;
        _index.Add((position, jpeg.Length));

        EcrireFourcc("00dc");
        _ecriture.Write(jpeg.Length);
        _ecriture.Write(jpeg);

        if ((jpeg.Length & 1) == 1)
        {
            _ecriture.Write((byte)0);
        }

        _images++;
    }

    /// <summary>Finalise les en-têtes (tailles, nombre d'images, index). Idempotent.</summary>
    public void Terminer()
    {
        if (_termine)
        {
            return;
        }

        _termine = true;

        long finMovi = _flux.Position;
        int tailleMovi = (int)(finMovi - (_positionDebutMovi + 4));

        AllerA(_positionTailleMovi);
        _ecriture.Write(tailleMovi);

        AllerA(finMovi);
        int tailleIndex = _index.Count * 16;
        EcrireFourcc("idx1");
        _ecriture.Write(tailleIndex);
        foreach (var (position, taille) in _index)
        {
            EcrireFourcc("00dc");
            _ecriture.Write(AviifKeyFrame);
            _ecriture.Write((int)(position - _positionDebutMovi));
            _ecriture.Write(taille);
        }

        int tailleHdrl = (int)(_positionDebutMovi - 8 - (_positionTailleHdrl + 4));
        AllerA(_positionTailleHdrl);
        _ecriture.Write(tailleHdrl);

        AllerA(_positionTotalImagesAvih);
        _ecriture.Write(_images);

        AllerA(_positionDureeStrh);
        _ecriture.Write(_images);

        long fin = _flux.Position;
        AllerA(_positionTailleRiff);
        _ecriture.Write((int)(fin - 8));

        AllerA(fin);
        _ecriture.Flush();
        _flux.Flush();
        _ecriture.Dispose();
        _flux.Dispose();
    }

    public void Dispose() => Terminer();

    private void EcrireEntete()
    {
        EcrireFourcc("RIFF");
        _positionTailleRiff = _flux.Position;
        _ecriture.Write(0);
        EcrireFourcc("AVI ");

        long debutHdrl = _flux.Position;
        EcrireFourcc("LIST");
        _positionTailleHdrl = _flux.Position;
        _ecriture.Write(0);
        EcrireFourcc("hdrl");

        // MainAVIHeader (56 octets)
        EcrireFourcc("avih");
        _ecriture.Write(56);
        _ecriture.Write(1_000_000 / _imagesParSeconde);
        _ecriture.Write(0);
        _ecriture.Write(0);
        _ecriture.Write(AvifHasIndex);
        _positionTotalImagesAvih = _flux.Position;
        _ecriture.Write(0);
        _ecriture.Write(0);
        _ecriture.Write(1);
        _ecriture.Write(_largeur * _hauteur * 3);
        _ecriture.Write(_largeur);
        _ecriture.Write(_hauteur);
        _ecriture.Write(0);
        _ecriture.Write(0);
        _ecriture.Write(0);
        _ecriture.Write(0);

        // LIST strl
        long debutStrl = _flux.Position;
        EcrireFourcc("LIST");
        long positionTailleStrl = _flux.Position;
        _ecriture.Write(0);
        EcrireFourcc("strl");

        // AVIStreamHeader (56 octets)
        EcrireFourcc("strh");
        _ecriture.Write(56);
        EcrireFourcc("vids");
        EcrireFourcc("MJPG");
        _ecriture.Write(0);
        _ecriture.Write((short)0);
        _ecriture.Write((short)0);
        _ecriture.Write(0);
        _ecriture.Write(1);
        _ecriture.Write(_imagesParSeconde);
        _ecriture.Write(0);
        _positionDureeStrh = _flux.Position;
        _ecriture.Write(0);
        _ecriture.Write(_largeur * _hauteur * 3);
        _ecriture.Write(unchecked((int)0xFFFFFFFF));
        _ecriture.Write(0);
        _ecriture.Write((short)0);
        _ecriture.Write((short)0);
        _ecriture.Write((short)_largeur);
        _ecriture.Write((short)_hauteur);

        // BITMAPINFOHEADER (40 octets)
        EcrireFourcc("strf");
        _ecriture.Write(40);
        _ecriture.Write(40);
        _ecriture.Write(_largeur);
        _ecriture.Write(_hauteur);
        _ecriture.Write((short)1);
        _ecriture.Write((short)24);
        EcrireFourcc("MJPG");
        _ecriture.Write(_largeur * _hauteur * 3);
        _ecriture.Write(0);
        _ecriture.Write(0);
        _ecriture.Write(0);
        _ecriture.Write(0);

        int tailleStrl = (int)(_flux.Position - (positionTailleStrl + 4));
        long finStrl = _flux.Position;
        AllerA(positionTailleStrl);
        _ecriture.Write(tailleStrl);
        AllerA(finStrl);

        int tailleHdrl = (int)(_flux.Position - (debutHdrl + 8));
        AllerA(_positionTailleHdrl);
        _ecriture.Write(tailleHdrl);
        AllerA(_flux.Position);

        // LIST movi
        EcrireFourcc("LIST");
        _positionTailleMovi = _flux.Position;
        _ecriture.Write(0);
        _positionDebutMovi = _flux.Position;
        EcrireFourcc("movi");
    }

    private void EcrireFourcc(string valeur)
    {
        foreach (char caractere in valeur)
        {
            _ecriture.Write((byte)caractere);
        }
    }

    private void AllerA(long position) => _flux.Seek(position, SeekOrigin.Begin);
}
