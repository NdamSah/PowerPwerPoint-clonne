using System;
using System.Drawing;
using System.IO;

namespace PowerPointClone.Insertion.Objet
{
    public enum ModeObjetExterne
    {
        CreerNouveau,
        DepuisFichier,
        LienVersFichier
    }

    public class ObjetExterne
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TitreAffichage { get; set; } = "Objet intégré";
        public string CheminFichier { get; set; } = string.Empty;
        public ModeObjetExterne Mode { get; set; } = ModeObjetExterne.DepuisFichier;
        public Rectangle Zone { get; set; }
        public bool AfficherEnTantQuIcone { get; set; } = true;
        public byte[] ContenuIntegre { get; set; }

        public void ChargerContenuDepuisFichier()
        {
            if (string.IsNullOrWhiteSpace(CheminFichier) || !File.Exists(CheminFichier))
                throw new FileNotFoundException("Le fichier source de l'objet est introuvable.", CheminFichier);

            if (string.IsNullOrWhiteSpace(TitreAffichage) || TitreAffichage == "Objet intégré")
                TitreAffichage = Path.GetFileName(CheminFichier);

            if (Mode != ModeObjetExterne.LienVersFichier)
            {
                ContenuIntegre = File.ReadAllBytes(CheminFichier);
            }
        }

        public void Dessiner(Graphics g)
        {
            if (g == null) return;

            using (var fondBrush = new SolidBrush(Color.FromArgb(245, 248, 252)))
            using (var bordurePen = new Pen(Color.SteelBlue, 1.5f))
            using (var fontTitre = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var fontInfo = new Font("Segoe UI", 8.5f, FontStyle.Regular))
            {
                g.FillRectangle(fondBrush, Zone);
                g.DrawRectangle(bordurePen, Zone);

                string icone = AfficherEnTantQuIcone ? "📎" : "📄";
                g.DrawString($"{icone} {TitreAffichage}", fontTitre, Brushes.Black, Zone.X + 8, Zone.Y + 8);

                string sousTitre = Mode == ModeObjetExterne.LienVersFichier
                    ? "Lié au fichier externe"
                    : (ContenuIntegre != null ? $"Intégré ({ContenuIntegre.Length} octets)" : "Objet intégré");

                g.DrawString(sousTitre, fontInfo, Brushes.DimGray, Zone.X + 8, Zone.Y + 28);
            }
        }
    }
}