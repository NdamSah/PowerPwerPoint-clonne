using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.Http;
using System.Threading.Tasks;

namespace PowerPointClone.Insertion.Images
{
    public class GestionnaireImages
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public ImageObjet ChargerDepuisFichier(string cheminFichier, Point positionInitiale)
        {
            using (var temp = Image.FromFile(cheminFichier))
            {
                var bmp = new Bitmap(temp);
                var tailleReduite = AjusterTailleMax(bmp.Size, new Size(400, 300));

                return new ImageObjet
                {
                    SourceCheminOuUrl = cheminFichier,
                    EstEnLigne = false,
                    ImageOriginale = bmp,
                    ZoneRecadrage = new Rectangle(Point.Empty, bmp.Size),
                    PositionEtTaille = new Rectangle(positionInitiale, tailleReduite)
                };
            }
        }

        public async Task<ImageObjet> ChargerDepuisUrlAsync(string url, Point positionInitiale)
        {
            byte[] data = await _httpClient.GetByteArrayAsync(url);
            using (var ms = new System.IO.MemoryStream(data))
            using (var temp = Image.FromStream(ms))
            {
                var bmp = new Bitmap(temp);
                var tailleReduite = AjusterTailleMax(bmp.Size, new Size(400, 300));

                return new ImageObjet
                {
                    SourceCheminOuUrl = url,
                    EstEnLigne = true,
                    ImageOriginale = bmp,
                    ZoneRecadrage = new Rectangle(Point.Empty, bmp.Size),
                    PositionEtTaille = new Rectangle(positionInitiale, tailleReduite)
                };
            }
        }

        public void AppliquerRecadrage(ImageObjet objet, Rectangle nouveauRecadrage)
        {
            if (objet?.ImageOriginale == null) return;

            Rectangle valide = Rectangle.Intersect(
                nouveauRecadrage,
                new Rectangle(Point.Empty, objet.ImageOriginale.Size)
            );

            if (valide.Width > 0 && valide.Height > 0)
            {
                objet.ZoneRecadrage = valide;
            }
        }

        public void Redimensionner(ImageObjet objet, int largeur, int hauteur)
        {
            if (objet == null || largeur <= 10 || hauteur <= 10) return;
            objet.PositionEtTaille = new Rectangle(
                objet.PositionEtTaille.Location,
                new Size(largeur, hauteur)
            );
        }

        public void Dessiner(Graphics g, ImageObjet objet)
        {
            if (g == null || objet?.ImageOriginale == null) return;

            Bitmap portion = objet.ImageOriginale.Clone(
                objet.ZoneRecadrage,
                objet.ImageOriginale.PixelFormat
            );

            using (portion)
            using (var attributes = CreerAttributsCorrection(objet.Luminosite, objet.Contraste))
            {
                g.DrawImage(
                    portion,
                    objet.PositionEtTaille,
                    0,
                    0,
                    portion.Width,
                    portion.Height,
                    GraphicsUnit.Pixel,
                    attributes
                );

                DessinerStyle(g, objet);
            }
        }

        private ImageAttributes CreerAttributsCorrection(float luminosite, float contraste)
        {
            float t = (1.0f - contraste) / 2.0f;
            float lumOffset = luminosite - 1.0f;

            float[][] matrice =
            {
                new float[] { contraste, 0, 0, 0, 0 },
                new float[] { 0, contraste, 0, 0, 0 },
                new float[] { 0, 0, contraste, 0, 0 },
                new float[] { 0, 0, 0, 1, 0 },
                new float[] { t + lumOffset, t + lumOffset, t + lumOffset, 0, 1 }
            };

            var attrs = new ImageAttributes();
            attrs.SetColorMatrix(new ColorMatrix(matrice));
            return attrs;
        }

        private void DessinerStyle(Graphics g, ImageObjet objet)
        {
            Rectangle rect = objet.PositionEtTaille;

            switch (objet.Style)
            {
                case StyleImageType.BordureSimple:
                    using (var pen = new Pen(Color.DodgerBlue, 2))
                        g.DrawRectangle(pen, rect);
                    break;

                case StyleImageType.CadreEpais:
                    using (var pen = new Pen(Color.FromArgb(30, 41, 59), 8))
                        g.DrawRectangle(pen, rect);
                    break;

                case StyleImageType.OmbrePortee:
                    var ombre = new Rectangle(rect.X + 6, rect.Y + 6, rect.Width, rect.Height);
                    using (var brush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                        g.FillRectangle(brush, ombre);
                    break;
            }
        }

        private Size AjusterTailleMax(Size source, Size max)
        {
            double ratioX = (double)max.Width / source.Width;
            double ratioY = (double)max.Height / source.Height;
            double ratio = Math.Min(ratioX, ratioY);
            if (ratio > 1) ratio = 1;

            return new Size(
                Math.Max(20, (int)(source.Width * ratio)),
                Math.Max(20, (int)(source.Height * ratio))
            );
        }
    }
}