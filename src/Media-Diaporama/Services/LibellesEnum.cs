using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Libellés français des énumérations affichées dans les panneaux et boîtes de dialogue.
/// Les méthodes évitent volontairement de porter le nom d'une énumération : une méthode
/// homonyme masque le type et casse la résolution des membres (CS0119 / CS0411).
/// </summary>
public static class LibellesEnum
{
    public static string Demarrage(DemarrageMedia valeur) => valeur switch
    {
        DemarrageMedia.AuClic => "Au clic",
        DemarrageMedia.Automatiquement => "Automatiquement",
        _ => "Après le précédent"
    };

    public static string Sens(SensTransition valeur) => valeur switch
    {
        SensTransition.Gauche => "Depuis la gauche",
        SensTransition.Droite => "Depuis la droite",
        SensTransition.Haut => "Depuis le haut",
        SensTransition.Bas => "Depuis le bas",
        _ => "Centre"
    };

    public static string LibelleDeclencheurAnimation(DeclencheurAnimation valeur) => valeur switch
    {
        DeclencheurAnimation.AuClic => "Au clic",
        DeclencheurAnimation.AvecLaPrecedente => "Avec la précédente",
        _ => "Après la précédente"
    };

    public static string LibelleTypeZoom(TypeZoom valeur) => valeur switch
    {
        TypeZoom.Resume => "Résumé",
        TypeZoom.Section => "Section",
        _ => "Diapositive"
    };

    public static string LibelleTypeLien(TypeLien valeur) => valeur switch
    {
        TypeLien.PageWeb => "Page web",
        TypeLien.Fichier => "Fichier existant",
        TypeLien.Diapositive => "Diapositive de la présentation",
        _ => "Adresse e-mail"
    };

    public static string LibelleAction(TypeAction valeur) => valeur switch
    {
        TypeAction.Aucune => "Aucune action",
        TypeAction.AllerDiapositive => "Aller à la diapositive…",
        TypeAction.DiapositiveSuivante => "Diapositive suivante",
        TypeAction.DiapositivePrecedente => "Diapositive précédente",
        TypeAction.PremiereDiapositive => "Première diapositive",
        TypeAction.DerniereDiapositive => "Dernière diapositive",
        TypeAction.FinDiaporama => "Terminer le diaporama",
        TypeAction.LancerProgramme => "Lancer un programme…",
        _ => "Lire un son…"
    };

    public static string LibelleDeclencheurAction(DeclencheurAction valeur)
        => valeur == DeclencheurAction.Clic ? "Au clic de la souris" : "Au survol de la souris";

    public static string LibelleTypeDiaporama(TypeDiaporama valeur) => valeur switch
    {
        TypeDiaporama.Intervenant => "Présenté par un intervenant (plein écran)",
        TypeDiaporama.ParcoursIndividuel => "Parcouru individuellement (fenêtre)",
        _ => "Parcouru dans une borne (plein écran)"
    };

    public static string LibelleCategorieAnimation(CategorieAnimation valeur) => valeur switch
    {
        CategorieAnimation.Entree => "Entrée",
        CategorieAnimation.Emphase => "Emphase",
        CategorieAnimation.Sortie => "Sortie",
        _ => "Trajectoire"
    };
}
