namespace PowerPwerPoint.Media.Services;

public enum ActionNavigation
{
    Aucune,
    Suivante,
    Precedente,
    Premiere,
    Derniere,
    Terminer,
    EcranNoir,
    EcranBlanc,
    PointeurLaser
}

/// <summary>Résultat de l'interprétation d'une touche pendant le diaporama.</summary>
/// <param name="Action">Action à exécuter.</param>
/// <param name="Index">Index cible (0-based) pour les actions de navigation.</param>
public sealed record ResultatNavigation(ActionNavigation Action, int Index = -1)
{
    public static ResultatNavigation Rien => new(ActionNavigation.Aucune);
}

/// <summary>
/// Interprétation pure des raccourcis du diaporama : flèches, Page précédente/suivante,
/// Espace, Entrée, Début/Fin, Échap, B, W, L, plus la saisie « numéro + Entrée ».
/// </summary>
public static class NavigateurDiaporama
{
    public static ResultatNavigation Interpreter(string? touche, int indexCourant, int total)
    {
        if (total <= 0)
        {
            return ResultatNavigation.Rien;
        }

        indexCourant = Math.Clamp(indexCourant, 0, total - 1);

        switch (touche)
        {
            case "Right":
            case "Down":
            case "PageDown":
            case "Space":
            case "Enter":
            case "Return":
            case "N":
                return indexCourant + 1 >= total
                    ? new ResultatNavigation(ActionNavigation.Terminer)
                    : new ResultatNavigation(ActionNavigation.Suivante, indexCourant + 1);

            case "Left":
            case "Up":
            case "PageUp":
            case "Back":
            case "P":
                return new ResultatNavigation(ActionNavigation.Precedente, Math.Max(0, indexCourant - 1));

            case "Home":
                return new ResultatNavigation(ActionNavigation.Premiere, 0);

            case "End":
                return new ResultatNavigation(ActionNavigation.Derniere, total - 1);

            case "Escape":
                return new ResultatNavigation(ActionNavigation.Terminer);

            case "B":
                return new ResultatNavigation(ActionNavigation.EcranNoir);

            case "W":
                return new ResultatNavigation(ActionNavigation.EcranBlanc);

            case "L":
                return new ResultatNavigation(ActionNavigation.PointeurLaser);

            default:
                return ResultatNavigation.Rien;
        }
    }

    public static bool EstChiffre(string? touche)
        => !string.IsNullOrEmpty(touche) && touche.Length == 1 && char.IsDigit(touche[0]);

    public static bool EstToucheDeSaisie(string? touche)
        => touche is "D0" or "D1" or "D2" or "D3" or "D4" or "D5" or "D6" or "D7" or "D8" or "D9"
            or "NumPad0" or "NumPad1" or "NumPad2" or "NumPad3" or "NumPad4"
            or "NumPad5" or "NumPad6" or "NumPad7" or "NumPad8" or "NumPad9";

    /// <summary>Chiffre correspondant à une touche WPF (<c>D5</c>, <c>NumPad5</c>), ou <c>null</c>.</summary>
    public static string? ChiffreDepuisTouche(string? touche)
    {
        if (string.IsNullOrEmpty(touche))
        {
            return null;
        }

        if (touche.Length == 2 && touche[0] == 'D' && char.IsDigit(touche[1]))
        {
            return touche[1].ToString();
        }

        if (touche.StartsWith("NumPad", StringComparison.Ordinal) && touche.Length == 7 && char.IsDigit(touche[6]))
        {
            return touche[6].ToString();
        }

        return null;
    }

    /// <summary>Convertit une saisie « 3 » ou « 12 » en index 0-based, ou <c>null</c> si hors plage.</summary>
    public static int? ResoudreSaisie(string? saisie, int total)
    {
        if (string.IsNullOrWhiteSpace(saisie) || total <= 0)
        {
            return null;
        }

        if (!int.TryParse(saisie, out int numero))
        {
            return null;
        }

        if (numero < 1 || numero > total)
        {
            return null;
        }

        return numero - 1;
    }
}
