using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaApplication1.ViewModels;
using AvaloniaApplication1.Views;
using System;
using Avalonia.Data.Core.Plugins;
using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaApplication1;

public partial class App : Application
{
    private DateTime _lastClick = DateTime.MinValue;
    // Double-click práh – dolaď podle platformy/požadavků (typicky 300–500 ms)
    private readonly TimeSpan _doubleClickThreshold = TimeSpan.FromMilliseconds(400);
    private PixelPoint? _lastWindowPosition = null;
    private WindowState _lastWindowState = WindowState.Normal;

    public Action<ServiceCollection>? RegisterPlatformServices;
    public IServiceProvider Services { get; set; } = default!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // // If you use CommunityToolkit, line below is needed to remove Avalonia data validation.
        // // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        ServiceCollection collection = new ServiceCollection();
        collection.AddTransient<MainViewModel>();

        if (RegisterPlatformServices != null) 
            RegisterPlatformServices(collection);
        
        // Creates a ServiceProvider containing services from the provided IServiceCollection
        Services = collection.BuildServiceProvider();
        
        MainViewModel vm = Services.GetRequiredService<MainViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };
            
            desktop.MainWindow.Closing += (s, e) =>
            {
                Window window = (Window)s!;
                window.WindowState = WindowState.Minimized;
                window.ShowInTaskbar = false;
                e.Cancel = true; // Zabrání skutečnému zavření okna
            };
            
            // Minimalizace okna a skrytí z hlavního panelu
            desktop.MainWindow.PropertyChanged += (s, e) =>
            {
                if (e.Property == Window.WindowStateProperty)
                {
                    Window window = (Window)s!;
                    // Ulož pozici pouze pokud je okno v normálním stavu
                    if (window.WindowState == WindowState.Normal)
                    {
                        _lastWindowPosition = window.Position;
                    }

                    if (window.WindowState != WindowState.Minimized)
                    {
                        // Ulož poslední stav okna
                        _lastWindowState = window.WindowState;
                    }

                    // Při minimalizaci skryj okno ze stavového řádku
                    if (window.WindowState == WindowState.Minimized)
                    {
                        window.ShowInTaskbar = false;
                    }
                }
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void TrayIcon_Clicked(object? sender, EventArgs e)
    {
        DateTime now = DateTime.Now;

        if (now - _lastClick <= _doubleClickThreshold)
        {
            _lastClick = DateTime.MinValue;
            OnTrayDoubleClicked();
        }
        else
        {
            _lastClick = now;
            // Pokud potřebuješ i single-click akci, zvaž krátké opoždění
            // a spuštění jen pokud mezitím nepřišel druhý klik.
            // Zde pro jednoduchost single-click akci neděláme.
        }
    }

    private void OnTrayDoubleClicked()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Získání hlavního okna
            var mainWindow = desktop.MainWindow;

            if (mainWindow != null)
            {
                // Obnovení okna a jeho zobrazení
                mainWindow.WindowState = _lastWindowState;
                mainWindow.ShowInTaskbar = true;
                if (_lastWindowPosition.HasValue && mainWindow.WindowState == WindowState.Normal)
                    mainWindow.Position = _lastWindowPosition.Value;

                // Zajistí, že okno bude v popředí a aktivní
                mainWindow.Topmost = true;
                mainWindow.Activate();
                mainWindow.Topmost = false;
            }
        }
    }

    private void Option2_Clicked(object? sender, EventArgs e)
    {
        var notification = Services.GetRequiredService<ISystemNotificationService>();
        notification.ShowNotification("Option 2 clicked", "Hello, world!");
    }
}
