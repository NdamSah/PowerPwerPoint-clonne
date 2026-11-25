using System.Windows.Media;

namespace PowerPwerPoint.Media.Services;

/// <summary>Conversion sûre des couleurs hexadécimales du modèle vers des pinceaux WPF gelés.</summary>
public static class Couleurs
{
    public static SolidColorBrush Pinceau(string? hexadecimal, Color? defaut = null)
    {
        Color couleur = defaut ?? Colors.Transparent;

        if (!string.IsNullOrWhiteSpace(hexadecimal))
        {
            try
            {
                if (ColorConverter.ConvertFromString(hexadecimal) is Color convertie)
                {
                    couleur = convertie;
                }
            }
            catch
            {
                // Couleur invalide : on conserve la valeur par défaut.
            }
        }

        var pinceau = new SolidColorBrush(couleur);
        pinceau.Freeze();
        return pinceau;
    }
}
