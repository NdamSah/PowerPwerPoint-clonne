using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PowerPointClone.Insertion.WordArt
{
    public enum EffetWordArt
    {
        Aucun,
        ContourEpais,
        DegradeLineaire,
        OmbreDecalee
    }

    public class WordArtObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Texte { get; set; } = "WordArt";
        public Point Position { get; set; }
        public string NomPolice { get; set; } = "Impact";
        public float TaillePolice { get; set; } = 32f;
        public Color CouleurDebut { get; set; } = Color.DeepSkyBlue;
        public Color CouleurFin { get; set; } = Color.DarkBlue;
        public Color CouleurContour { get; set; } = Color.White;
        public float EpaisseurContour { get; set; } = 2f;
        public EffetWordArt Effet { get; set; } = EffetWordArt.DegradeLineaire;

        public void Dessiner(Graphics g)
        {
            if (g == null || string.IsNullOrWhiteSpace(Texte)) return;

            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = new GraphicsPath())
            using (var famille = new FontFamily(NomPolice))
            {
                path.AddString(
                    Texte,
                    famille,
                    (int)FontStyle.Bold,
                    TaillePolice,
                    Position,
                    StringFormat.GenericDefault
                );

                RectangleF bornes = path.GetBounds();
                if (bornes.Width < 1) bornes.Width = 10;
                if (bornes.Height < 1) bornes.Height = 10;

                if (Effet == EffetWordArt.OmbreDecalee)
                {
                    using (var matriceOmbre = new Matrix())
                    {
                        matriceOmbre.Translate(4f, 4f);
                        using (var ombrePath = (GraphicsPath)path.Clone())
                        {
                            ombrePath.Transform(matriceOmbre);
                            using (var ombreBrush = new SolidBrush(Color.FromArgb(110, 0, 0, 0)))
                                g.FillPath(ombreBrush, ombrePath);
                        }
                    }
                }

                if (Effet == EffetWordArt.DegradeLineaire)
                {
                    using (var brush = new LinearGradientBrush(bornes, CouleurDebut, CouleurFin, LinearGradientMode.Vertical))
                        g.FillPath(brush, path);
                }
                else
                {
                    using (var brush = new SolidBrush(CouleurDebut))
                        g.FillPath(brush, path);
                }

                if (Effet == EffetWordArt.ContourEpais || Effet == EffetWordArt.DegradeLineaire)
                {
                    using (var pen = new Pen(CouleurContour, EpaisseurContour))
                        g.DrawPath(pen, path);
                }
            }
        }
    }
}