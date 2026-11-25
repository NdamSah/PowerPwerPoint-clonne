using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace PowerPointClone.InterfaceQualite.AffichageVues;
public enum ModeAffichage { Normal, Trieuse, Lecture, Notes, Masque }
public sealed class AffichageVuesViewModel : INotifyPropertyChanged
{
	private ModeAffichage _mode = ModeAffichage.Normal;
	public ModeAffichage Mode { get => _mode; set { if (_mode == value) return; _mode = value; PropertyChanged?.Invoke(this, new(nameof(Mode))); } }
	public event PropertyChangedEventHandler? PropertyChanged;
}