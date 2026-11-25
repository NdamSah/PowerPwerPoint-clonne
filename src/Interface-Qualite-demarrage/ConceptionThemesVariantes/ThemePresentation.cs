namespace PowerPointClone.InterfaceQualite.ConceptionThemesVariantes;
public sealed record ThemePresentation(string Nom, string CouleurFond, string CouleurTexte, string CouleurAccent, string Police = "Aptos")
{
    public static ThemePresentation Clair => new("Clair", "#FFFFFF", "#202020", "#4472C4");
    public static ThemePresentation Sombre => new("Sombre", "#202124", "#FFFFFF", "#8AB4F8");
    public static ThemePresentation Bleu => new("Bleu", "#EAF2F8", "#17365D", "#2F75B5");
}