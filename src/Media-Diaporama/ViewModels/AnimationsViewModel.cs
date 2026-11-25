using System.Collections.ObjectModel;
using System.Windows.Input;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>Cible possible d'une animation (élément de la diapositive ou objet média).</summary>
public sealed record CibleAnimation(string Id, string Nom, string Categorie)
{
    public override string ToString() => $"{Categorie} : {Nom}";
}

/// <summary>
/// Panneau Animations : galerie par catégorie, volet Animation (ordre, déclencheur, durée,
/// délai, répétition), choix de la cible et aperçu.
/// </summary>
public sealed class AnimationsViewModel : ViewModelBase
{
    private readonly MediaWorkspaceViewModel _espace;
    private readonly List<AnimationItemViewModel> _items = new();
    private AnimationItemViewModel? _selection;
    private CibleAnimation? _cibleChoisie;
    private IReadOnlyList<CibleAnimation> _cibles = Array.Empty<CibleAnimation>();

    public AnimationsViewModel(MediaWorkspaceViewModel espace)
    {
        _espace = espace;
        Declencheurs = OptionsEnum.Creer<DeclencheurAnimation>(LibellesEnum.LibelleDeclencheurAnimation);
        SensPossibles = OptionsEnum.Creer<SensTransition>(LibellesEnum.Sens);

        Entrees = AnimationCatalogue.ParCategorie(CategorieAnimation.Entree).ToList();
        Emphases = AnimationCatalogue.ParCategorie(CategorieAnimation.Emphase).ToList();
        Sorties = AnimationCatalogue.ParCategorie(CategorieAnimation.Sortie).ToList();
        Trajectoires = AnimationCatalogue.ParCategorie(CategorieAnimation.Trajectoire).ToList();

        AjouterAnimationCommand = new RelayCommand(p =>
        {
            if (p is EntreeAnimation entree)
            {
                Ajouter(entree);
            }
        });

        SupprimerCommand = new RelayCommand(Supprimer, () => _selection is not null);
        MonterCommand = new RelayCommand(() => Deplacer(-1), () => PeutDeplacer(-1));
        DescendreCommand = new RelayCommand(() => Deplacer(1), () => PeutDeplacer(1));
        ApercuCommand = new RelayCommand(() => _espace.DemanderApercuAnimations(null), () => ADesAnimations);
        ApercuSelectionCommand = new RelayCommand(
            () => _espace.DemanderApercuAnimations(_selection?.Effet),
            () => _selection is not null && ADesAnimations);
    }

    public ObservableCollection<AnimationItemViewModel> Elements { get; } = new();

    public IReadOnlyList<EntreeAnimation> Entrees { get; }

    public IReadOnlyList<EntreeAnimation> Emphases { get; }

    public IReadOnlyList<EntreeAnimation> Sorties { get; }

    public IReadOnlyList<EntreeAnimation> Trajectoires { get; }

    public IReadOnlyList<OptionEnum<DeclencheurAnimation>> Declencheurs { get; }

    public IReadOnlyList<OptionEnum<SensTransition>> SensPossibles { get; }

    public IReadOnlyList<CibleAnimation> Cibles
    {
        get => _cibles;
        private set => SetProperty(ref _cibles, value);
    }

    public CibleAnimation? CibleChoisie
    {
        get => _cibleChoisie;
        set
        {
            if (SetProperty(ref _cibleChoisie, value))
            {
                Notifier(nameof(ResumeCible));
            }
        }
    }

    /// <summary>Cible imposée par la sélection dans l'aperçu (propagée par l'espace de travail).</summary>
    public ElementDiapositive? CibleParDefaut
    {
        set
        {
            if (value is null)
            {
                return;
            }

            var cible = Cibles.FirstOrDefault(c => c.Id == value.Id);
            if (cible is not null)
            {
                CibleChoisie = cible;
            }
        }
    }

    public string ResumeCible => CibleChoisie is null
        ? "Choisissez la cible de l'animation"
        : $"Cible : {CibleChoisie.Nom}";

    public AnimationItemViewModel? Selection
    {
        get => _selection;
        set
        {
            if (!SetProperty(ref _selection, value))
            {
                return;
            }

            Notifier(nameof(AUneSelection));
            SignalerCommandes();
        }
    }

    public bool AUneSelection => _selection is not null;

    public bool ADesAnimations => Elements.Count > 0;

    public string ResumePlan
    {
        get
        {
            if (Elements.Count == 0)
            {
                return "Aucune animation sur cette diapositive.";
            }

            var plan = MoteurAnimations.ConstruirePlan(Elements.Select(e => e.Effet));
            int etapes = plan.Count;
            return $"{Elements.Count} effet(s) · {etapes} étape(s) au clic";
        }
    }

    public ICommand AjouterAnimationCommand { get; }

    public ICommand SupprimerCommand { get; }

    public ICommand MonterCommand { get; }

    public ICommand DescendreCommand { get; }

    public ICommand ApercuCommand { get; }

    public ICommand ApercuSelectionCommand { get; }

    public void Rafraichir()
    {
        var diapositive = _espace.DiapositiveCourante;
        var cibles = new List<CibleAnimation>();

        if (diapositive is not null)
        {
            cibles.AddRange(diapositive.Elements.Select(e => new CibleAnimation(e.Id, e.Nom, "Élément")));
            cibles.AddRange(diapositive.Objets.Select(o => new CibleAnimation(o.Id, o.Libelle, "Objet")));
        }

        Cibles = cibles;
        if (CibleChoisie is not null)
        {
            CibleChoisie = cibles.FirstOrDefault(c => c.Id == CibleChoisie.Id);
        }

        var effetSelectionne = _selection?.Effet;

        foreach (var item in _items)
        {
            item.Dispose();
        }

        _items.Clear();
        Elements.Clear();

        if (diapositive is not null)
        {
            int numero = 1;
            foreach (var effet in diapositive.Animations)
            {
                var item = new AnimationItemViewModel(effet, numero, diapositive.NomCible(effet.CibleId));
                _items.Add(item);
                Elements.Add(item);
                numero++;
            }
        }

        Selection = _items.FirstOrDefault(i => ReferenceEquals(i.Effet, effetSelectionne));

        Notifier(nameof(ADesAnimations));
        Notifier(nameof(ResumePlan));
        Notifier(nameof(Cibles));

        if (CibleChoisie is null && _cibles.Count > 0)
        {
            CibleChoisie = _cibles[0];
        }

        Notifier(nameof(ResumeCible));
        SignalerCommandes();
    }

    private void Ajouter(EntreeAnimation entree)
    {
        var diapositive = _espace.DiapositiveCourante;
        if (diapositive is null)
        {
            return;
        }

        var cible = CibleChoisie ?? Cibles.FirstOrDefault();
        if (cible is null)
        {
            _espace.ServiceDialogues.AfficherInformation("La diapositive ne contient aucun élément à animer.");
            return;
        }

        var effet = new AnimationEffet
        {
            CibleId = cible.Id,
            Effet = entree.Effet,
            Duree = entree.Categorie == CategorieAnimation.Trajectoire ? 1.5 : 0.5,
            Sens = SensTransition.Gauche
        };

        diapositive.Animations.Add(effet);
        _espace.MarquerModifie($"Animation « {entree.Nom} » ajoutée sur {cible.Nom}.");
        Rafraichir();
        Selection = _items.FirstOrDefault(i => ReferenceEquals(i.Effet, effet));
    }

    private void Supprimer()
    {
        var diapositive = _espace.DiapositiveCourante;
        if (diapositive is null || _selection is null)
        {
            return;
        }

        var effet = _selection.Effet;
        diapositive.Animations.Remove(effet);
        _espace.MarquerModifie("Animation supprimée.");
        Selection = null;
        Rafraichir();
    }

    private bool PeutDeplacer(int direction)
    {
        if (_selection is null)
        {
            return false;
        }

        int index = _items.IndexOf(_selection);
        int cible = index + direction;
        return index >= 0 && cible >= 0 && cible < _items.Count;
    }

    private void Deplacer(int direction)
    {
        var diapositive = _espace.DiapositiveCourante;
        if (diapositive is null || _selection is null || !PeutDeplacer(direction))
        {
            return;
        }

        int index = diapositive.Animations.IndexOf(_selection.Effet);
        int cible = index + direction;
        diapositive.Animations.Move(index, cible);

        _espace.MarquerModifie("Ordre des animations modifié.");
        Rafraichir();
        Selection = _items.ElementAtOrDefault(cible);
    }

    private void SignalerCommandes()
    {
        (SupprimerCommand as RelayCommand)?.SignalerChangement();
        (MonterCommand as RelayCommand)?.SignalerChangement();
        (DescendreCommand as RelayCommand)?.SignalerChangement();
        (ApercuCommand as RelayCommand)?.SignalerChangement();
        (ApercuSelectionCommand as RelayCommand)?.SignalerChangement();
    }
}
