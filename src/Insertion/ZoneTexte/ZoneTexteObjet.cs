using System;
using System.Drawing;

namespace PowerPointClone.Insertion.ZoneTexte
{
    public enum OrientationTexte
    {
        Horizontale,
        Verticale
    }

    public class ZoneTexteObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Texte { get; set; } = "Nouvelle zone de texte";
        public Rectangle Zone { get; set; }
        public Font Police { get; set; } = new Font("Segoe UI", 14, FontStyle.Regular);
        public Color CouleurTexte { get; set; } = Color.Black;
        public Color? CouleurFond { get; set; } = null;
        public OrientationTexte Orientation { get; set; } = OrientationTexte.Horizontale;

        public void Dessiner(Graphics g)
        {
            if (g == null) return;

            if (CouleurFond.HasValue)
            {
                using (var fondBrush = new SolidBrush(CouleurFond.Value))
                    g.FillRectangle(fondBrush, Zone);
            }

            using (var texteBrush = new SolidBrush(CouleurTexte))
            using (var format = new StringFormat())
            {
                if (Orientation == OrientationTexte.Verticale)
                {
                    format.FormatFlags = StringFormatFlags.DirectionVertical;
                }

                g.DrawString(Texte, Police, texteBrush, Zone, format);
            }
        }
    }
}