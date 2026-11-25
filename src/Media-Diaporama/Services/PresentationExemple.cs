using PowerPwerPoint.Media.Modele;

namespace PowerPwerPoint.Media.Services;

/// <summary>
/// Présentation de démonstration utilisée au démarrage du module.
/// Elle sert de jeu d'essai pour les transitions, les animations, les zooms, les liens,
/// les actions et le mode présentateur.
/// </summary>
public static class PresentationExemple
{
    public static PresentationMedia Creer()
    {
        var presentation = new PresentationMedia { Titre = "Démonstration Média & Diaporama" };

        var accueil = Diapositive("Bienvenue", "Démonstration du module Média, Transitions, Animations et Diaporama.");
        var titre = Element("Titre", "PowerPoint — Module Média", 80, 150, 800, 110, "#FF1F2430", "#FFFFFFFF", 44, true);
        var sousTitre = Element("Sous-titre", "Transitions, animations, vidéo, audio, zoom, liens et diaporama",
            80, 275, 800, 70, "#FF2F6FED", "#FFFFFFFF", 22, true);
        accueil.Elements.Add(titre);
        accueil.Elements.Add(sousTitre);
        accueil.Animations.Add(new AnimationEffet
        {
            CibleId = titre.Id,
            Effet = EffetAnimation.FonduEntree,
            Duree = 0.7,
            Declencheur = DeclencheurAnimation.AuClic
        });
        accueil.Animations.Add(new AnimationEffet
        {
            CibleId = sousTitre.Id,
            Effet = EffetAnimation.VolerEntree,
            Sens = SensTransition.Bas,
            Duree = 0.6,
            Declencheur = DeclencheurAnimation.ApresLaPrecedente
        });
        accueil.Transition = new TransitionDiapositive { Effet = TransitionEffet.Fondu, Duree = 0.8 };
        presentation.Diapositives.Add(accueil);

        var objectifs = Diapositive("Objectifs", "Chaque point apparaît au clic, avec un effet d'entrée différent.");
        objectifs.Elements.Add(Element("Titre", "Ce que couvre ce module", 60, 45, 840, 70, "#FF1F2430", "#FFFFFFFF", 30, true));
        var points = new[]
        {
            "Transitions et options d'effet",
            "Animations et volet Animation",
            "Vidéo, audio et capture d'écran",
            "Diaporama et mode Présentateur"
        };
        var effets = new[] { EffetAnimation.Apparition, EffetAnimation.VolerEntree, EffetAnimation.ZoomEntree, EffetAnimation.BalayageEntree };
        for (int i = 0; i < points.Length; i++)
        {
            var carte = Element($"Point {i + 1}", points[i], 80, 150 + (i * 85), 800, 65, "#FFE8EFFD", "#FF1F2430", 20, false);
            objectifs.Elements.Add(carte);
            objectifs.Animations.Add(new AnimationEffet
            {
                CibleId = carte.Id,
                Effet = effets[i],
                Duree = 0.5,
                Sens = SensTransition.Gauche
            });
        }

        objectifs.Transition = new TransitionDiapositive
        {
            Effet = TransitionEffet.Poussee,
            Sens = SensTransition.Droite,
            Duree = 0.7
        };
        presentation.Diapositives.Add(objectifs);

        var medias = Diapositive("Médias", "Depuis le panneau Insertion : vidéo, audio, capture d'écran, zoom, lien et action.");
        medias.Elements.Add(Element("Titre", "Insérer un média", 60, 45, 840, 70, "#FF1F2430", "#FFFFFFFF", 30, true));
        medias.Elements.Add(Element("Astuce",
            "Insérez une vidéo puis choisissez « Rogner » pour définir les points d'entrée et de sortie sans réencoder le fichier.",
            80, 160, 800, 140, "#FFFFF3CD", "#FF5C4A00", 18, true));
        medias.Transition = new TransitionDiapositive { Effet = TransitionEffet.Balayage, Sens = SensTransition.Gauche, Duree = 0.8 };
        presentation.Diapositives.Add(medias);

        var zoomSlide = Diapositive("Zoom de section", "Cliquez sur la vignette de zoom pour accéder à la section, Échap pour revenir.");
        zoomSlide.Elements.Add(Element("Titre", "Zoom de section", 60, 45, 840, 70, "#FF1F2430", "#FFFFFFFF", 30, true));
        zoomSlide.Objets.Add(new ObjetZoom
        {
            Titre = "Section : interactions",
            TypeZoom = TypeZoom.Section,
            X = 80,
            Y = 150,
            Largeur = 800,
            Hauteur = 300,
            Cibles = new List<string>()
        });
        zoomSlide.Transition = new TransitionDiapositive { Effet = TransitionEffet.Aucune };
        presentation.Diapositives.Add(zoomSlide);

        var interactions = Diapositive("Liens et actions", "Un clic sur la carte ouvre le site ; l'autre passe à la diapositive suivante.");
        interactions.Elements.Add(Element("Titre", "Liens et actions", 60, 45, 840, 70, "#FF1F2430", "#FFFFFFFF", 30, true));
        var zoneLien = Element("Zone lien", "Ouvrir la documentation WPF", 80, 160, 380, 120, "#FF2F6FED", "#FFFFFFFF", 20, false);
        var zoneAction = Element("Zone action", "Aller à la diapositive suivante", 500, 160, 380, 120, "#FF1F2430", "#FFFFFFFF", 20, false);
        interactions.Elements.Add(zoneLien);
        interactions.Elements.Add(zoneAction);
        interactions.Objets.Add(new ObjetLien
        {
            ElementId = zoneLien.Id,
            TypeLien = TypeLien.PageWeb,
            Cible = "https://learn.microsoft.com/dotnet/desktop/wpf/",
            InfoBulle = "Ouvrir la documentation WPF",
            Nom = "Lien documentation WPF"
        });
        interactions.Objets.Add(new ObjetAction
        {
            ElementId = zoneAction.Id,
            TypeAction = TypeAction.DiapositiveSuivante,
            Declencheur = DeclencheurAction.Clic,
            Nom = "Aller à la suite"
        });
        interactions.Transition = new TransitionDiapositive { Effet = TransitionEffet.Fractionnement, Duree = 0.6 };
        presentation.Diapositives.Add(interactions);

        var animations = Diapositive("Animations", "Emphase puis sortie : cliquez pour déclencher chaque étape.");
        animations.Elements.Add(Element("Titre", "Emphase et sortie", 60, 45, 840, 70, "#FF1F2430", "#FFFFFFFF", 30, true));
        var cibleEmphase = Element("Cible emphase", "Pulsation", 120, 200, 320, 140, "#FF2F6FED", "#FFFFFFFF", 24, true);
        var cibleSortie = Element("Cible sortie", "Disparaître", 520, 200, 320, 140, "#FF8B5CF6", "#FFFFFFFF", 24, true);
        animations.Elements.Add(cibleEmphase);
        animations.Elements.Add(cibleSortie);
        animations.Animations.Add(new AnimationEffet
        {
            CibleId = cibleEmphase.Id,
            Effet = EffetAnimation.Pulsation,
            Duree = 0.8,
            Repetition = 2
        });
        animations.Animations.Add(new AnimationEffet
        {
            CibleId = cibleSortie.Id,
            Effet = EffetAnimation.Disparition,
            Declencheur = DeclencheurAnimation.ApresLaPrecedente,
            Duree = 0.2
        });
        animations.Transition = new TransitionDiapositive { Effet = TransitionEffet.Cercle, Duree = 0.9 };
        presentation.Diapositives.Add(animations);

        var annexe = Diapositive("Annexe", "Diapositive masquée : elle n'apparaît pas dans le diaporama.");
        annexe.Masquee = true;
        annexe.Elements.Add(Element("Titre", "Annexe masquée", 60, 45, 840, 70, "#FF6A7180", "#FFFFFFFF", 30, true));
        presentation.Diapositives.Add(annexe);

        // Le zoom de section cible les diapositives « Liens et actions » et « Animations ».
        var zoom = (ObjetZoom)zoomSlide.Objets[0];
        zoom.Cibles.Add(interactions.Id);
        zoom.Cibles.Add(animations.Id);

        presentation.Options.ModePresentateur = true;
        return presentation;
    }

    private static DiapositiveMedia Diapositive(string titre, string notes)
        => new() { Titre = titre, Notes = notes };

    private static ElementDiapositive Element(
        string nom,
        string texte,
        double x,
        double y,
        double largeur,
        double hauteur,
        string fond,
        string texteCouleur,
        double taille,
        bool zoneTexte)
        => new()
        {
            Nom = nom,
            Texte = texte,
            X = x,
            Y = y,
            Largeur = largeur,
            Hauteur = hauteur,
            CouleurFond = fond,
            CouleurTexte = texteCouleur,
            TaillePolice = taille,
            EstZoneTexte = zoneTexte
        };
}
