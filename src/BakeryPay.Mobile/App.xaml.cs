namespace BakeryPay.Mobile;

public partial class App : Application
{
    private readonly AppShell _appShell;

    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _appShell = serviceProvider.GetRequiredService<AppShell>();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_appShell);
    }
}
