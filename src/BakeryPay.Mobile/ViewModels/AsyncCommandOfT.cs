using System.Windows.Input;

namespace BakeryPay.Mobile.ViewModels;

public class AsyncCommandOfT<T> : ICommand
{
    private readonly Func<T?, Task> _execute;

    public AsyncCommandOfT(Func<T?, Task> execute)
    {
        _execute = execute;
    }

    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public async void Execute(object? parameter)
    {
        if (parameter is T typed)
        {
            await _execute(typed);
            return;
        }

        await _execute(default);
    }
}
