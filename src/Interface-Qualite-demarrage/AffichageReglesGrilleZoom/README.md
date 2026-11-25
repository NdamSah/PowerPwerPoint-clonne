# AffichageReglesGrilleZoom

Module WPF indépendant : affichage des règles, de la grille et réglage du zoom.

## Fichiers
- `AffichageReglesGrilleZoomViewModel.cs` : état MVVM et commandes de zoom.
- `AffichageReglesGrilleZoomView.xaml` : interface du module.
- `AffichageReglesGrilleZoomView.xaml.cs` : contrôle WPF qui reçoit le contenu de la diapositive.

## Intégration
1. Ajouter les trois fichiers au projet WPF .NET 8 de l'application.
2. Conserver le namespace `PowerPointClone.InterfaceQualite.AffichageReglesGrilleZoom` ou adapter uniformément le namespace au nom réel du projet.
3. Dans la fenêtre d'édition, utiliser le contrôle :

```xml
<qualite:AffichageReglesGrilleZoomView SlideContent="{Binding DiapositiveActuelleVue}" />
```

avec la déclaration :

```xml
xmlns:qualite="clr-namespace:PowerPointClone.InterfaceQualite.AffichageReglesGrilleZoom"
```

`DiapositiveActuelleVue` doit être le contrôle WPF qui dessine la diapositive. Le contrôle fournit son propre ViewModel. La grille est une surcouche visuelle et ne modifie pas le contenu de la diapositive.

## Limites à compléter lors de l'intégration
- Le bouton « Adapter » remet provisoirement le zoom à 100 % ; le calcul exact dépendra de la taille réelle du viewport et du format de diapositive défini dans le noyau.
- Les règles sont actuellement des repères visuels gradués ; leur calibration en unités réelles devra être raccordée aux dimensions du document.
