using System;
using System.Drawing;

namespace PowerPointClone.Insertion.Images
{
    public enum StyleImageType
    {
        Aucun,
        BordureSimple,
        OmbrePortee,
        CoinsArrondis,
        CadreEpais
    }

    public class ImageObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string SourceCheminOuUrl { get; set; } = string.Empty;
        public bool EstEnLigne { get; set; }
        public Bitmap ImageOriginale { get; set; }
        public Rectangle ZoneRecadrage { get; set; }
        public Rectangle PositionEtTaille { get; set; }
        public float Luminosite { get; set; } = 1.0f;
        public float Contraste { get; set; } = 1.0f;
        public StyleImageType Style { get; set; } = StyleImageType.Aucun;
    }
}