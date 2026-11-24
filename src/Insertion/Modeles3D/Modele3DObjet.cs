using System;
using System.Drawing;
using System.IO;

namespace PowerPointClone.Insertion.Modeles3D
{
    public class Modele3DObjet
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string NomFichier { get; set; } = string.Empty;
        public string CheminFichier { get; set; } = string.Empty;
        public Rectangle ZoneAffichage { get; set; }
        public float RotationX { get; set; } = 15f;
        public float RotationY { get; set; } = -25f;
        public float RotationZ { get; set; } = 0f;
        public string VueSelectionnee { get; set; } = "Vue avant";

        public bool ChargerModele(string chemin)
        {
            if (string.IsNullOrWhiteSpace(chemin) || !File.Exists(chemin))
                return false;

            string ext = Path.GetExtension(chemin).ToLowerInvariant();
            if (ext != ".glb" && ext != ".gltf" && ext != ".obj" && ext != ".fbx" && ext != ".stl")
                return false;

            CheminFichier = chemin;
            NomFichier = Path.GetFileName(chemin);
            return true;
        }

        public void Pivoter(float deltaX, float deltaY, float deltaZ = 0f)
        {
            RotationX = (RotationX + deltaX) % 360f;
            RotationY = (RotationY + deltaY) % 360f;
            RotationZ = (RotationZ + deltaZ) % 360f;
        }

        public void DessinerApercu(Graphics g)
        {
            if (g == null) return;

            using (var fond = new SolidBrush(Color.FromArgb(238, 244, 250)))
            using (var bordure = new Pen(Color.RoyalBlue, 1.5f))
            using (var fontTitre = new Font("Segoe UI", 10, FontStyle.Bold))
            using (var fontInfo = new Font("Segoe UI", 8.5f))
            {
                g.FillRectangle(fond, ZoneAffichage);
                g.DrawRectangle(bordure, ZoneAffichage);

                string titre = string.IsNullOrEmpty(NomFichier) ? "Modèle 3D" : $"🧊 {NomFichier}";
                g.DrawString(titre, fontTitre, Brushes.Navy, ZoneAffichage.X + 10, ZoneAffichage.Y + 10);

                string angles = $"{VueSelectionnee} | X:{RotationX:0}° Y:{RotationY:0}° Z:{RotationZ:0}°";
                g.DrawString(angles, fontInfo, Brushes.DarkSlateGray, ZoneAffichage.X + 10, ZoneAffichage.Y + 32);
            }
        }
    }
}