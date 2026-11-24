using System;
using System.Collections.Generic;
using System.Drawing;

namespace PowerPointClone.Insertion.Symbole
{
    public class SymboleObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Caractere { get; set; } = "©";
        public string NomPolice { get; set; } = "Segoe UI Symbol";
        public float TaillePolice { get; set; } = 24f;
        public Point Position { get; set; }
        public Color Couleur { get; set; } = Color.Black;
        public List<string> HistoriqueRecents { get; set; } = new List<string>();

        public void EnregistrerDansHistorique(string symbole, int maxElements = 10)
        {
            if (string.IsNullOrEmpty(symbole)) return;
            HistoriqueRecents.Remove(symbole);
            HistoriqueRecents.Insert(0, symbole);
            if (HistoriqueRecents.Count > maxElements)
            {
                HistoriqueRecents.RemoveAt(HistoriqueRecents.Count - 1);
            }
        }

        public void Dessiner(Graphics g)
        {
            if (g == null || string.IsNullOrEmpty(Caractere)) return;

            using (var font = new Font(NomPolice, TaillePolice, FontStyle.Regular))
            using (var brush = new SolidBrush(Couleur))
            {
                g.DrawString(Caractere, font, brush, Position.X, Position.Y);
            }
        }
    }
}