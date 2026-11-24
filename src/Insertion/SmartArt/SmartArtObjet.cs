using System;
using System.Collections.Generic;
using System.Drawing;

namespace PowerPointClone.Insertion.SmartArt
{
    public enum DispositionSmartArt
    {
        ListeVerticale,
        ProcessusHorizontal,
        Cycle,
        Hierarchie
    }

    public class NoeudSmartArt
    {
        public string Texte { get; set; } = "Élément";
        public Rectangle Zone { get; set; }
        public Color CouleurFond { get; set; } = Color.DodgerBlue;
    }

    public class SmartArtObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DispositionSmartArt Disposition { get; set; } = DispositionSmartArt.ProcessusHorizontal;
        public Rectangle ZoneGlobale { get; set; }
        public List<NoeudSmartArt> Noeuds { get; set; } = new List<NoeudSmartArt>();

        public void CalculerDisposition()
        {
            if (Noeuds == null || Noeuds.Count == 0) return;

            int count = Noeuds.Count;
            if (Disposition == DispositionSmartArt.ListeVerticale)
            {
                int espacement = 10;
                int hauteurNoeud = Math.Max(28, (ZoneGlobale.Height - espacement * (count - 1)) / count);
                for (int i = 0; i < count; i++)
                {
                    int y = ZoneGlobale.Y + i * (hauteurNoeud + espacement);
                    Noeuds[i].Zone = new Rectangle(ZoneGlobale.X, y, ZoneGlobale.Width, hauteurNoeud);
                }
            }
            else
            {
                int espacement = 18;
                int largeurNoeud = Math.Max(50, (ZoneGlobale.Width - espacement * (count - 1)) / count);
                for (int i = 0; i < count; i++)
                {
                    int x = ZoneGlobale.X + i * (largeurNoeud + espacement);
                    Noeuds[i].Zone = new Rectangle(x, ZoneGlobale.Y, largeurNoeud, ZoneGlobale.Height);
                }
            }
        }

        public void Dessiner(Graphics g)
        {
            if (g == null) return;
            CalculerDisposition();

            using (var font = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                for (int i = 0; i < Noeuds.Count; i++)
                {
                    var noeud = Noeuds[i];
                    using (var brush = new SolidBrush(noeud.CouleurFond))
                        g.FillRectangle(brush, noeud.Zone);

                    g.DrawString(noeud.Texte, font, Brushes.White, noeud.Zone, sf);

                    if (Disposition == DispositionSmartArt.ProcessusHorizontal && i < Noeuds.Count - 1)
                    {
                        var suivant = Noeuds[i + 1];
                        int midY = noeud.Zone.Y + noeud.Zone.Height / 2;
                        using (var pen = new Pen(Color.DimGray, 2f))
                            g.DrawLine(pen, noeud.Zone.Right + 2, midY, suivant.Zone.Left - 2, midY);
                    }
                }
            }
        }
    }
}