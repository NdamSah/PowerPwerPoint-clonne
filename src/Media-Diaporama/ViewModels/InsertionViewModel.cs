using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using PowerPwerPoint.Media.Modele;
using PowerPwerPoint.Media.Services;

namespace PowerPwerPoint.Media.ViewModels;

/// <summary>
/// Panneau Insertion multimédia : vidéo, audio, enregistrement d'un son, capture d'écran,
/// zoom, lien et action, plus l'édition des propriétés de l'objet sélectionné.
/// </summary>
public sealed class InsertionViewModel : ViewModelBase
{
    private static readonly ObservableCollection<ObjetMedia> Vide = new();

    private readonly MediaWorkspaceViewModel _espace;
    private ObjetMedia? _selection;

    public InsertionViewModel(MediaWorkspaceViewModel espace)
    {
        _espace = espace;
        Demarrages = OptionsEnum.Creer<DemarrageMedia>(LibellesEnum.Demarrage);

        InsererVideoCommand = new RelayCommand(InsererVideo);
        InsererAudioCommand = new RelayCommand(InsererAudio);
        EnregistrerSonCommand = new RelayCommand(EnregistrerSon);
        EnregistrerEcranCommand = new RelayCommand(EnregistrerEcran);
        InsererZoomCommand = new RelayCommand(() => EditerZoom(null));
        InsererLienCommand = new RelayCommand(() => EditerLien(null));
        InsererActionCommand = new RelayCommand(() => EditerAction(null));
        ModifierObjetCommand = new RelayCommand(_ => ModifierObjet(), _ => AUnObjet);
        LireObjetCommand = new RelayCommand(_ => LireObjet(), _ => AUnObjet);
        SupprimerObjetCommand = new RelayCommand(_ => SupprimerObjet(), _ => AUnObjet);
        ChoisirFichierVideoCommand = new RelayCommand(ChoisirFichierVideo);
        ReinitialiserRognageCommand = new RelayCommand(_ => ReinitialiserRognage(), _ => Video is not null);
        ReinitialiserVolumeCommand = new RelayCommand(_ => ReinitialiserVolume(), _ => AUnObjet);
    }

    public ObservableCollection<ObjetMedia> Objets
        => _espace.DiapositiveCourante?.Objets ?? Vide;

    public ObjetMedia? ObjetSelectionne
    {
        get => _selection;
        set
        {
            if (!SetProperty(ref _selection, value))
            {
                return;
            }

            Notifier(nameof(AUnObjet));
            Notifier(nameof(Video));
            Notifier(nameof(Audio));
            Notifier(nameof(Zoom));
            Notifier(nameof(Lien));
            Notifier(nameof(Action));
            Notifier(nameof(EstVideo));
            Notifier(nameof(EstAudio));
            Notifier(nameof(EstZoom));
            Notifier(nameof(EstLien));
            Notifier(nameof(EstAction));
            Notifier(nameof(CheminVideoAffiche));
            Notifier(nameof(ResumeObjet));
            SignalerChangementCommandes();
        }
    }

    public bool AUnObjet => _selection is not null;

    public bool EstVideo => _selection is ObjetVideo;

    public bool EstAudio => _selection is ObjetAudio;

    public bool EstZoom => _selection is ObjetZoom;

    public bool EstLien => _selection is ObjetLien;

    public bool EstAction => _selection is ObjetAction;

    public ObjetVideo? Video => _selection as ObjetVideo;

    public ObjetAudio? Audio => _selection as ObjetAudio;

    public ObjetZoom? Zoom => _selection as ObjetZoom;

    public ObjetLien? Lien => _selection as ObjetLien;

    public ObjetAction? Action => _selection as ObjetAction;

    public string CheminVideoAffiche => Video is null
        ? string.Empty
        : string.IsNullOrEmpty(Video.Chemin) ? "(aucun fichier)" : Path.GetFileName(Video.Chemin);

    public string ResumeObjet => _selection switch
    {
        null => "Aucun objet sélectionné",
        ObjetVideo video => $"{video.TypeLibelle} — {video.Libelle}{(video.EstRogne ? $" · {video.ResumeRognage}" : string.Empty)}",
        ObjetAudio audio => $"{audio.TypeLibelle} — {audio.Libelle}{(audio.LireEnArrierePlan ? " · arrière-plan" : string.Empty)}",
        ObjetZoom zoom => $"{zoom.TypeLibelle} — {zoom.Libelle} · {zoom.ResumeCibles}",
        ObjetLien lien => $"Lien {LibellesEnum.LibelleTypeLien(lien.TypeLien)} — {lien.Cible}",
        ObjetAction action => $"Action — {action.Libelle}",
        _ => _selection.Libelle
    };

    public IReadOnlyList<OptionEnum<DemarrageMedia>> Demarrages { get; }

    public ICommand InsererVideoCommand { get; }

    public ICommand InsererAudioCommand { get; }

    public ICommand EnregistrerSonCommand { get; }

    public ICommand EnregistrerEcranCommand { get; }

    public ICommand InsererZoomCommand { get; }

    public ICommand InsererLienCommand { get; }

    public ICommand InsererActionCommand { get; }

    public ICommand ModifierObjetCommand { get; }

    public ICommand LireObjetCommand { get; }

    public ICommand SupprimerObjetCommand { get; }

    public ICommand ChoisirFichierVideoCommand { get; }

    public ICommand ReinitialiserRognageCommand { get; }

    public ICommand ReinitialiserVolumeCommand { get; }

    internal void ActualiserListe()
    {
        ObjetSelectionne = null;
        Notifier(nameof(Objets));
    }

    internal void NotifierListe() => Notifier(nameof(Objets));

    private IServiceDialogues Dialogues => _espace.ServiceDialogues;

    private DiapositiveMedia? Diapositive => _espace.DiapositiveCourante;

    private void InsererVideo()
    {
        string? chemin = Dialogues.ChoisirFichier("Insérer une vidéo", StockageMedia.FiltreVideo);
        if (string.IsNullOrEmpty(chemin) || Diapositive is null)
        {
            return;
        }

        Executer(() =>
        {
            string copie = StockageMedia.Importer(chemin, _espace.Presentation);
            var objet = new ObjetVideo
            {
                Chemin = copie,
                Nom = Path.GetFileName(chemin)
            };
            Ajouter(objet, $"Vidéo insérée : {objet.Libelle}");
        });
    }

    private void InsererAudio()
    {
        string? chemin = Dialogues.ChoisirFichier("Insérer un fichier audio", StockageMedia.FiltreAudio);
        if (string.IsNullOrEmpty(chemin) || Diapositive is null)
        {
            return;
        }

        Executer(() =>
        {
            string copie = StockageMedia.Importer(chemin, _espace.Presentation);
            var objet = new ObjetAudio
            {
                Chemin = copie,
                Nom = Path.GetFileName(chemin)
            };
            Ajouter(objet, $"Audio inséré : {objet.Libelle}");
        });
    }

    private void EnregistrerSon()
    {
        if (Diapositive is null)
        {
            return;
        }

        string? chemin = Dialogues.EnregistrerSon(_espace.Presentation);
        if (string.IsNullOrEmpty(chemin))
        {
            return;
        }

        var objet = new ObjetAudio
        {
            Chemin = chemin,
            Nom = Path.GetFileName(chemin),
            EnregistreDansApplication = true
        };
        Ajouter(objet, $"Son enregistré : {objet.Libelle}");
    }

    private void EnregistrerEcran()
    {
        if (Diapositive is null)
        {
            return;
        }

        var resultat = Dialogues.CapturerEcran(_espace.Presentation);
        if (resultat is null)
        {
            return;
        }

        var objet = new ObjetVideo
        {
            Chemin = resultat.CheminVideo,
            Nom = "Capture d'écran",
            PisteAudio = resultat.CheminAudio,
            Duree = resultat.Duree,
            Demarrage = DemarrageMedia.Automatiquement,
            MasquerHorsLecture = false
        };
        Ajouter(objet, $"Capture d'écran insérée ({resultat.Duree.TotalSeconds:0.0} s)");
    }

    private void EditerZoom(ObjetZoom? modele)
    {
        if (Diapositive is null)
        {
            return;
        }

        var resultat = Dialogues.EditerZoom(modele, _espace.Presentation);
        if (resultat is null)
        {
            return;
        }

        if (modele is null)
        {
            Ajouter(resultat, $"Zoom inséré : {resultat.Libelle}");
        }
        else
        {
            _espace.MarquerModifie($"Zoom modifié : {resultat.Libelle}");
            ActualiserSelection();
        }
    }

    private void EditerLien(ObjetLien? modele)
    {
        if (Diapositive is null)
        {
            return;
        }

        var gabarit = modele ?? new ObjetLien
        {
            ElementId = _espace.ElementSelectionne?.Id ?? string.Empty
        };

        var resultat = Dialogues.EditerLien(gabarit, Diapositive, _espace.Presentation);
        if (resultat is null)
        {
            return;
        }

        if (modele is null)
        {
            Ajouter(resultat, "Lien inséré.");
        }
        else
        {
            modele.TypeLien = resultat.TypeLien;
            modele.Cible = resultat.Cible;
            modele.InfoBulle = resultat.InfoBulle;
            modele.ElementId = resultat.ElementId;
            modele.Nom = resultat.Nom;
            _espace.MarquerModifie("Lien modifié.");
            ActualiserSelection();
        }
    }

    private void EditerAction(ObjetAction? modele)
    {
        if (Diapositive is null)
        {
            return;
        }

        var gabarit = modele ?? new ObjetAction
        {
            ElementId = _espace.ElementSelectionne?.Id ?? string.Empty
        };

        var resultat = Dialogues.EditerAction(gabarit, Diapositive, _espace.Presentation);
        if (resultat is null)
        {
            return;
        }

        if (modele is null)
        {
            Ajouter(resultat, "Action insérée.");
        }
        else
        {
            modele.TypeAction = resultat.TypeAction;
            modele.Parametre = resultat.Parametre;
            modele.Arguments = resultat.Arguments;
            modele.Declencheur = resultat.Declencheur;
            modele.ElementId = resultat.ElementId;
            modele.Nom = resultat.Nom;
            _espace.MarquerModifie("Action modifiée.");
            ActualiserSelection();
        }
    }

    private void ModifierObjet()
    {
        switch (_selection)
        {
            case ObjetZoom zoom:
                EditerZoom(zoom);
                break;
            case ObjetLien lien:
                EditerLien(lien);
                break;
            case ObjetAction action:
                EditerAction(action);
                break;
            case ObjetVideo video:
                Dialogues.AfficherVideo(video, _espace.Presentation);
                ActualiserSelection();
                break;
            case null:
                break;
        }
    }

    private void LireObjet()
    {
        if (_selection is null)
        {
            return;
        }

        switch (_selection)
        {
            case ObjetVideo video:
                Dialogues.AfficherVideo(video, _espace.Presentation);
                break;
            case ObjetAudio audio:
            {
                string? chemin = StockageMedia.Resoudre(audio.Chemin, _espace.Presentation);
                if (chemin is null)
                {
                    Dialogues.AfficherErreur("Le fichier audio est introuvable.");
                    return;
                }

                ExecuteurInteraction.JouerSon(chemin, audio.Volume);
                _espace.SignalerStatut($"Lecture de {audio.Libelle}.");
                break;
            }
            case ObjetLien lien:
            {
                bool ok = ExecuteurInteraction.ExecuterLien(lien, _espace.Presentation, _ => { }, out string message);
                _espace.SignalerStatut(ok ? "Lien ouvert." : message);
                break;
            }
            default:
                ModifierObjet();
                break;
        }
    }

    private void SupprimerObjet()
    {
        if (_selection is null || Diapositive is null)
        {
            return;
        }

        var objet = _selection;
        Diapositive.Objets.Remove(objet);

        // Nettoyage des animations qui ciblaient l'objet supprimé.
        var effets = Diapositive.Animations.Where(a => a.CibleId == objet.Id).ToList();
        foreach (var effet in effets)
        {
            Diapositive.Animations.Remove(effet);
        }

        ObjetSelectionne = null;
        _espace.MarquerModifie($"{objet.TypeLibelle} supprimé.");
        _espace.ActualiserContenuDiapositive();
    }

    private void ChoisirFichierVideo()
    {
        if (Video is null)
        {
            return;
        }

        string? chemin = Dialogues.ChoisirFichier("Remplacer la vidéo", StockageMedia.FiltreVideo);
        if (string.IsNullOrEmpty(chemin))
        {
            return;
        }

        Executer(() =>
        {
            Video.Chemin = StockageMedia.Importer(chemin, _espace.Presentation);
            Video.DebutRognage = TimeSpan.Zero;
            Video.FinRognage = null;
            _espace.MarquerModifie("Vidéo remplacée.");
            ActualiserSelection();
        });
    }

    private void ReinitialiserRognage()
    {
        if (Video is null)
        {
            return;
        }

        Video.DebutRognage = TimeSpan.Zero;
        Video.FinRognage = null;
        _espace.MarquerModifie("Rognage réinitialisé.");
        ActualiserSelection();
    }

    private void ReinitialiserVolume()
    {
        switch (_selection)
        {
            case ObjetVideo video:
                video.Volume = 80;
                video.Muet = false;
                break;
            case ObjetAudio audio:
                audio.Volume = 80;
                audio.Muet = false;
                break;
            default:
                return;
        }

        _espace.MarquerModifie("Volume réinitialisé.");
        ActualiserSelection();
    }

    private void Ajouter(ObjetMedia objet, string message)
    {
        Diapositive!.Objets.Add(objet);
        objet.PropertyChanged += (_, _) =>
        {
            if (ReferenceEquals(objet, _selection))
            {
                Notifier(nameof(ResumeObjet));
                Notifier(nameof(CheminVideoAffiche));
            }
        };

        ObjetSelectionne = objet;
        _espace.MarquerModifie(message);
        _espace.ActualiserContenuDiapositive();
    }

    private void Executer(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Dialogues.AfficherErreur(ex.Message);
        }
    }

    private void ActualiserSelection()
    {
        Notifier(nameof(Objets));
        Notifier(nameof(ResumeObjet));
        Notifier(nameof(CheminVideoAffiche));
        Notifier(nameof(Video));
        Notifier(nameof(Audio));
        Notifier(nameof(Zoom));
        Notifier(nameof(Lien));
        Notifier(nameof(Action));
        SignalerChangementCommandes();
        _espace.ActualiserContenuDiapositive();
    }

    private void SignalerChangementCommandes()
    {
        (ModifierObjetCommand as RelayCommand)?.SignalerChangement();
        (LireObjetCommand as RelayCommand)?.SignalerChangement();
        (SupprimerObjetCommand as RelayCommand)?.SignalerChangement();
        (ReinitialiserRognageCommand as RelayCommand)?.SignalerChangement();
        (ReinitialiserVolumeCommand as RelayCommand)?.SignalerChangement();
    }
}
