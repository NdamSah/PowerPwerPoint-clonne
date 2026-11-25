namespace PowerPwerPoint.Media.ViewModels;

/// <summary>Élément de liste pour une énumération, avec son libellé français.</summary>
public sealed record OptionEnum<T>(T Valeur, string Libelle) where T : struct, Enum;

public static class OptionsEnum
{
    public static IReadOnlyList<OptionEnum<T>> Creer<T>(Func<T, string> libelle) where T : struct, Enum
        => Enum.GetValues<T>().Select(valeur => new OptionEnum<T>(valeur, libelle(valeur))).ToList();
}

/// <summary>Entrée de la liste des sons de transition.</summary>
public sealed record OptionSon(string Nom, string? Chemin);
