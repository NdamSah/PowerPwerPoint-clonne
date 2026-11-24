using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace PowerPointClone.Insertion.Graphique
{
    public enum TypeGraphique
    {
        Colonnes,
        Barres,
        Courbes,
        Secteurs,
        NuageDePoints
    }

    public class GraphiqueObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titre { get; set; } = "Graphique";
        public TypeGraphique Type { get; set; } = TypeGraphique.Colonnes;
        public Rectangle Zone { get; set; }
        public List<string> Categories { get; set; } = new List<string> { "T1", "T2", "T3", "T4" };
        public List<double> Valeurs { get; set; } = new List<double> { 25, 40, 30, 55 };
        public bool AfficherTableauDonnees { get; set; } = true;

        public void Dessiner(Graphics g)
        {
            if (g == null || Valeurs.Count == 0) return;

            using (var penCadre = new Pen(Color.LightGray, 1))
                g.DrawRectangle(penCadre, Zone);

            using (var fontTitre = new Font("Segoe UI", 11, FontStyle.Bold))
                g.DrawString(Titre, fontTitre, Brushes.Black, Zone.X + 10, Zone.Y + 8);

            Rectangle zoneTrace = new Rectangle(Zone.X + 40, Zone.Y + 35, Zone.Width - 60, Zone.Height - 75);
            double max = Math.Max(1, Valeurs.Max());

            if (Type == TypeGraphique.Colonnes)
            {
                int largeurBarre = Math.Max(10, (zoneTrace.Width / Valeurs.Count) - 12);
                for (int i = 0; i < Valeurs.Count; i++)
                {
                    int hauteur = (int)((Valeurs[i] / max) * zoneTrace.Height);
                    int x = zoneTrace.X + i * (largeurBarre + 12);
                    int y = zoneTrace.Bottom - hauteur;

                    using (var brush = new SolidBrush(Color.SteelBlue))
                        g.FillRectangle(brush, x, y, largeurBarre, hauteur);
                }
            }
            else if (Type == TypeGraphique.Secteurs)
            {
                double total = Math.Max(1, Valeurs.Sum());
                float angleDepart = 0f;
                Color[] palette = { Color.SteelBlue, Color.Coral, Color.MediumSeaGreen, Color.Goldenrod, Color.MediumPurple };

                for (int i = 0; i < Valeurs.Count; i++)
                {
                    float balayage = (float)((Valeurs[i] / total) * 360.0);
                    using (var brush = new SolidBrush(palette[i % palette.Length]))
                        g.FillPie(brush, zoneTrace, angleDepart, balayage);
                    angleDepart += balayage;
                }
            }
        }
    }
}