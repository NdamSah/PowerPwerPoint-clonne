using System;
using System.Collections.Generic;
using System.Drawing;

namespace PowerPointClone.Insertion.Tableau
{
    public class CelluleTableau
    {
        public string Texte { get; set; } = string.Empty;
        public int LigneSpan { get; set; } = 1;
        public int ColonneSpan { get; set; } = 1;
        public bool EstMasqueeParFusion { get; set; } = false;
        public Color CouleurFond { get; set; } = Color.White;
    }

    public class TableauObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Point Origine { get; set; }
        public List<int> LargeursColonnes { get; set; } = new List<int>();
        public List<int> HauteursLignes { get; set; } = new List<int>();
        public CelluleTableau[,] Cellules { get; private set; }
        public Color CouleurBordure { get; set; } = Color.DimGray;
        public float EpaisseurBordure { get; set; } = 1.5f;

        public TableauObjet(int lignes, int colonnes, Point origine, int largeurCol = 110, int hauteurLig = 36)
        {
            Origine = origine;
            Cellules = new CelluleTableau[lignes, colonnes];

            for (int c = 0; c < colonnes; c++) LargeursColonnes.Add(largeurCol);
            for (int r = 0; r < lignes; r++) HauteursLignes.Add(hauteurLig);

            for (int r = 0; r < lignes; r++)
                for (int c = 0; c < colonnes; c++)
                    Cellules[r, c] = new CelluleTableau();
        }

        public void FusionnerPlage(int rDebut, int cDebut, int rFin, int cFin)
        {
            int lignes = Cellules.GetLength(0);
            int cols = Cellules.GetLength(1);
            if (rDebut < 0 || cDebut < 0 || rFin >= lignes || cFin >= cols) return;

            var principale = Cellules[rDebut, cDebut];
            principale.LigneSpan = rFin - rDebut + 1;
            principale.ColonneSpan = cFin - cDebut + 1;
            principale.EstMasqueeParFusion = false;

            for (int r = rDebut; r <= rFin; r++)
            {
                for (int c = cDebut; c <= cFin; c++)
                {
                    if (r == rDebut && c == cDebut) continue;
                    Cellules[r, c].EstMasqueeParFusion = true;
                }
            }
        }

        public void FractionnerCellule(int r, int c)
        {
            var cell = Cellules[r, c];
            int rFin = Math.Min(Cellules.GetLength(0) - 1, r + cell.LigneSpan - 1);
            int cFin = Math.Min(Cellules.GetLength(1) - 1, c + cell.ColonneSpan - 1);

            for (int i = r; i <= rFin; i++)
                for (int j = c; j <= cFin; j++)
                    Cellules[i, j].EstMasqueeParFusion = false;

            cell.LigneSpan = 1;
            cell.ColonneSpan = 1;
        }
    }
}