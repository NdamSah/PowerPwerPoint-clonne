using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace PowerPointClone.Insertion.Formes
{
    public enum TypeForme
    {
        Rectangle,
        Ellipse,
        Triangle,
        FlecheDroite,
        PolygonePersonnalise
    }

    public class FormeGeometrique
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TypeForme Type { get; set; } = TypeForme.Rectangle;
        public Rectangle Bornes { get; set; }
        public List<PointF> SommetsPersonnalises { get; set; } = new List<PointF>();
        public Color CouleurRemplissage { get; set; } = Color.SkyBlue;
        public Color CouleurBordure { get; set; } = Color.Navy;
        public float EpaisseurBordure { get; set; } = 2f;
        public string TexteInterieur { get; set; } = string.Empty;
        public Font PoliceTexte { get; set; } = new Font("Segoe UI", 11, FontStyle.Bold);
        public Color CouleurTexte { get; set; } = Color.White;

        public void ModifierPoint(int index, PointF nouvellePosition)
        {
            if (index >= 0 && index < SommetsPersonnalises.Count)
            {
                SommetsPersonnalises[index] = nouvellePosition;
            }
        }

        public GraphicsPath ConstruireChemin()
        {
            var path = new GraphicsPath();

            switch (Type)
            {
                case TypeForme.Rectangle:
                    path.AddRectangle(Bornes);
                    break;

                case TypeForme.Ellipse:
                    path.AddEllipse(Bornes);
                    break;

                case TypeForme.Triangle:
                    Point[] triangle =
                    {
                        new Point(Bornes.Left + Bornes.Width / 2, Bornes.Top),
                        new Point(Bornes.Right, Bornes.Bottom),
                        new Point(Bornes.Left, Bornes.Bottom)
                    };
                    path.AddPolygon(triangle);
                    break;

                case TypeForme.FlecheDroite:
                    int midY = Bornes.Top + Bornes.Height / 2;
                    int corpsHaut = Bornes.Top + Bornes.Height / 4;
                    int corpsBas = Bornes.Bottom - Bornes.Height / 4;
                    int pointeX = Bornes.Left + (Bornes.Width * 2) / 3;

                    Point[] fleche =
                    {
                        new Point(Bornes.Left, corpsHaut),
                        new Point(pointeX, corpsHaut),
                        new Point(pointeX, Bornes.Top),
                        new Point(Bornes.Right, midY),
                        new Point(pointeX, Bornes.Bottom),
                        new Point(pointeX, corpsBas),
                        new Point(Bornes.Left, corpsBas)
                    };
                    path.AddPolygon(fleche);
                    break;

                case TypeForme.PolygonePersonnalise:
                    if (SommetsPersonnalises.Count >= 3)
                        path.AddPolygon(SommetsPersonnalises.ToArray());
                    else
                        path.AddRectangle(Bornes);
                    break;
            }

            return path;
        }

        public void Dessiner(Graphics g)
        {
            if (g == null) return;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (var path = ConstruireChemin())
            using (var brush = new SolidBrush(CouleurRemplissage))
            using (var pen = new Pen(CouleurBordure, EpaisseurBordure))
            {
                g.FillPath(brush, path);
                g.DrawPath(pen, path);
            }

            if (!string.IsNullOrWhiteSpace(TexteInterieur))
            {
                using (var textBrush = new SolidBrush(CouleurTexte))
                using (var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                })
                {
                    g.DrawString(TexteInterieur, PoliceTexte, textBrush, Bornes, sf);
                }
            }
        }
    }
}