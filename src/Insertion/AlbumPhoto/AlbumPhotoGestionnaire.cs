using System;
using System.Collections.Generic;
using System.Drawing;

namespace PowerPointClone.Insertion.AlbumPhoto
{
    public enum MiseEnPageAlbum
    {
        UnePhotoParDiapo,
        DeuxPhotosParDiapo,
        QuatrePhotosGrille
    }

    public class DiapoAlbum
    {
        public string Titre { get; set; } = "Album photo";
        public List<(string CheminImage, Rectangle Zone)> Elements { get; set; } = new List<(string, Rectangle)>();
    }

    public class AlbumPhotoGestionnaire
    {
        public List<DiapoAlbum> GenererAlbum(List<string> cheminsImages, MiseEnPageAlbum miseEnPage, Size tailleDiapo)
        {
            var diapos = new List<DiapoAlbum>();
            if (cheminsImages == null || cheminsImages.Count == 0) return diapos;

            int parDiapo = miseEnPage == MiseEnPageAlbum.UnePhotoParDiapo ? 1 :
                           miseEnPage == MiseEnPageAlbum.DeuxPhotosParDiapo ? 2 : 4;

            for (int i = 0; i < cheminsImages.Count; i += parDiapo)
            {
                var diapo = new DiapoAlbum { Titre = $"Album - Page {(i / parDiapo) + 1}" };
                int count = Math.Min(parDiapo, cheminsImages.Count - i);
                var zones = CalculerZones(count, tailleDiapo);

                for (int j = 0; j < count; j++)
                {
                    diapo.Elements.Add((cheminsImages[i + j], zones[j]));
                }
                diapos.Add(diapo);
            }

            return diapos;
        }

        private List<Rectangle> CalculerZones(int nbPhotos, Size taille)
        {
            int marge = 40;
            int w = taille.Width - marge * 2;
            int h = taille.Height - marge * 2;
            var zones = new List<Rectangle>();

            if (nbPhotos == 1)
            {
                zones.Add(new Rectangle(marge, marge, w, h));
            }
            else if (nbPhotos == 2)
            {
                int demiW = (w - 20) / 2;
                zones.Add(new Rectangle(marge, marge, demiW, h));
                zones.Add(new Rectangle(marge + demiW + 20, marge, demiW, h));
            }
            else
            {
                int demiW = (w - 20) / 2;
                int demiH = (h - 20) / 2;
                zones.Add(new Rectangle(marge, marge, demiW, demiH));
                zones.Add(new Rectangle(marge + demiW + 20, marge, demiW, demiH));
                zones.Add(new Rectangle(marge, marge + demiH + 20, demiW, demiH));
                zones.Add(new Rectangle(marge + demiW + 20, marge + demiH + 20, demiW, demiH));
            }
            return zones;
        }
    }
}