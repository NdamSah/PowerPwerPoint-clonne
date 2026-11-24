using System;
using System.Drawing;

namespace PowerPointClone.Insertion.Equation
{
    public enum TypeExpressionMath
    {
        TexteSimple,
        Fraction,
        Puissance,
        RacineCarree
    }

    public class EquationObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public TypeExpressionMath Type { get; set; } = TypeExpressionMath.TexteSimple;
        public string NumerateurOuBase { get; set; } = "x";
        public string DenominateurOuExposant { get; set; } = "2";
        public Point Position { get; set; }
        public float TaillePolice { get; set; } = 18f;
        public Color Couleur { get; set; } = Color.Black;

        public string FormaterTexte()
        {
            switch (Type)
            {
                case TypeExpressionMath.Fraction:
                    return $"({NumerateurOuBase}) / ({DenominateurOuExposant})";
                case TypeExpressionMath.Puissance:
                    return $"{NumerateurOuBase}^{DenominateurOuExposant}";
                case TypeExpressionMath.RacineCarree:
                    return $"√({NumerateurOuBase})";
                default:
                    return NumerateurOuBase;
            }
        }

        public void Dessiner(Graphics g)
        {
            if (g == null) return;

            using (var font = new Font("Cambria Math", TaillePolice, FontStyle.Italic))
            using (var brush = new SolidBrush(Couleur))
            {
                if (Type == TypeExpressionMath.Fraction)
                {
                    SizeF numSize = g.MeasureString(NumerateurOuBase, font);
                    SizeF denSize = g.MeasureString(DenominateurOuExposant, font);
                    float largeurBarre = Math.Max(numSize.Width, denSize.Width) + 12f;

                    g.DrawString(NumerateurOuBase, font, brush, Position.X + (largeurBarre - numSize.Width) / 2f, Position.Y);
                    float yBarre = Position.Y + numSize.Height + 2f;
                    using (var pen = new Pen(Couleur, 1.5f))
                        g.DrawLine(pen, Position.X, yBarre, Position.X + largeurBarre, yBarre);
                    g.DrawString(DenominateurOuExposant, font, brush, Position.X + (largeurBarre - denSize.Width) / 2f, yBarre + 3f);
                }
                else
                {
                    g.DrawString(FormaterTexte(), font, brush, Position.X, Position.Y);
                }
            }
        }
    }
}