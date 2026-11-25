using System.Windows.Input;

namespace PowerPwerPoint.Media.ViewModels;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _executer;
    private readonly Func<object?, bool>? _peutExecuter;

    public RelayCommand(Action executer, Func<bool>? peutExecuter = null)
        : this(_ => executer(), peutExecuter is null ? null : _ => peutExecuter())
    {
    }

    public RelayCommand(Action<object?> executer, Func<object?, bool>? peutExecuter = null)
    {
        _executer = executer ?? throw new ArgumentNullException(nameof(executer));
        _peutExecuter = peutExecuter;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parametre) => _peutExecuter?.Invoke(parametre) ?? true;

    public void Execute(object? parametre)
    {
        if (CanExecute(parametre))
        {
            _executer(parametre);
        }
    }

    public void SignalerChangement() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
