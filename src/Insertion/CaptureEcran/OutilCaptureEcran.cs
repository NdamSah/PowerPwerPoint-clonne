using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PowerPointClone.Insertion.CaptureEcran
{
    public class OutilCaptureEcran
    {
        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public Bitmap CapturerZone(Rectangle zoneEcran)
        {
            if (zoneEcran.Width <= 0 || zoneEcran.Height <= 0)
                throw new ArgumentException("La zone de capture doit avoir une largeur et une hauteur positives.");

            var resultat = new Bitmap(zoneEcran.Width, zoneEcran.Height);
            using (Graphics g = Graphics.FromImage(resultat))
            {
                g.CopyFromScreen(zoneEcran.Location, Point.Empty, zoneEcran.Size);
            }
            return resultat;
        }

        public Bitmap CapturerFenetre(IntPtr handleFenetre, Form fenetreAMasquer = null)
        {
            bool etaitVisible = false;
            if (fenetreAMasquer != null && fenetreAMasquer.Visible)
            {
                etaitVisible = true;
                fenetreAMasquer.Hide();
                Application.DoEvents();
                System.Threading.Thread.Sleep(150);
            }

            try
            {
                if (!GetWindowRect(handleFenetre, out RECT rect))
                    throw new InvalidOperationException("Impossible de lire les dimensions de la fenêtre cible.");

                var zone = Rectangle.FromLTRB(rect.Left, rect.Top, rect.Right, rect.Bottom);
                return CapturerZone(zone);
            }
            finally
            {
                if (etaitVisible && fenetreAMasquer != null)
                {
                    fenetreAMasquer.Show();
                }
            }
        }
    }
}