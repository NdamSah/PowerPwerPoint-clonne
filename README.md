# Clone PowerPoint (C# / WPF)

Reproduction de Microsoft PowerPoint en C#. Chaque onglet, chaque bouton et chaque raccourci clavier doivent fonctionner.
Projet d'équipe de 5 personnes.

## Technologies
- C# / .NET 8
- WPF (MVVM) pour l'interface
- Fluent.Ribbon pour le ruban
- DocumentFormat.OpenXml pour les fichiers .pptx
- NAudio pour l'audio et la vidéo
- LiveCharts2 pour les graphiques
- xUnit pour les tests

## Équipe et branches

### Moustapha : Noyau + onglet Fichier
- Branche : `moustapha-noyau-fichier`
- Dossier : `src/Noyau-Fichier/`

### Bevolo : Accueil + éditeur de diapositive
- Branche : `bevolo-accueil-editeur`
- Dossier : `src/Accueil-Editeur/`

### Ndam : Insertion (objets graphiques)
- Branche : `ndam-insertion-objets-graphiques`
- Dossier : `src/Insertion/`

### Noukam : Média, transitions, animations, diaporama
- Branche : `noukam-media-transitions-animations-diaporama`
- Dossier : `src/Media-Diaporama/`

### Etoundi : Interface, conception, révision, affichage, qualité
- Branche : `etoundi-interface-conception-revision-affichage`
- Dossier : `src/Interface-Qualite/`

Chaque tâche a son sous-dossier, par exemple `src/Insertion/Tableau/`.

## Démarrer
```
git clone https://github.com/NdamSah/PowerPwerPoint-clonne.git
cd PowerPwerPoint-clonne
git fetch
git checkout NOM-DE-TA-BRANCHE
```

## Travailler
```
git pull origin main
git merge main
(coder dans le dossier de ta tâche)
git add .
git commit -m "feat(tableau): insertion d'un tableau"
git push
```

Messages de commit :
- `feat(...)` : nouvelle fonctionnalité
- `fix(...)` : correction
- `docs:` : documentation
- `chore:` : organisation

## Fusionner dans main
1. Sur GitHub : Pull requests, puis New.
2. Choisir base `main` et compare `ta-branche`.
3. Un autre membre relit, puis clique sur Merge.
4. Jamais de push direct sur `main`.

## Règles
- Chacun travaille uniquement dans son dossier.
- On met sa branche à jour chaque jour.
- On commit souvent et on teste avant de fusionner.

## Un bouton est terminé quand
- [ ] La commande existe et le bouton du ruban est branché
- [ ] Annuler / Rétablir fonctionne
- [ ] Le raccourci clavier est enregistré
- [ ] Enregistrer puis rouvrir le .pptx conserve le résultat
- [ ] Un test xUnit existe
