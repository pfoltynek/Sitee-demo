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

public class App : Application
{
    private DateTime _lastClick = DateTime.MinValue;
    // Double-click práh – dolaď podle platformy/požadavků (typicky 300–500 ms)
    private readonly TimeSpan _doubleClickThreshold = TimeSpan.FromMilliseconds(400);
    private PixelPoint? _lastWindowPosition = null;
    private WindowState _lastWindowState = WindowState.Normal;

    public Action<ServiceCollection>? RegisterPlatformServices;
    
    /// <summary>
    /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
    /// </summary>
    public IServiceProvider? Services { get; private set; }
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        CreateServiceProvider();
        MainViewModel vm = GetViewModel();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm,
            };

            HandleWindowClosing(desktop);
            ManageWindowStateChanges(desktop);
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

    private void HandleWindowClosing(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (desktop.MainWindow != null)
            desktop.MainWindow.Closing += (s, e) =>
            {
                Window window = (Window)s!;
                window.WindowState = WindowState.Minimized;
                window.ShowInTaskbar = false;
                e.Cancel = true; // Zabrání skutečnému zavření okna
            };
    }

     private void ManageWindowStateChanges(IClassicDesktopStyleApplicationLifetime desktop)
     {
         if (desktop.MainWindow != null)
             desktop.MainWindow.PropertyChanged += (s, e) =>
             {
                 if (e.Property != Window.WindowStateProperty) return;

                 var window = (Window)s!;
                 var state = window.WindowState;

                 if (state == WindowState.Normal)
                     _lastWindowPosition = window.Position;

                 if (state != WindowState.Minimized)
                     _lastWindowState = state;

                 window.ShowInTaskbar = state != WindowState.Minimized;
             };
     }

    private MainViewModel GetViewModel()
    {
        if(Services == null)
            throw new InvalidOperationException("Services not initialized.");
        
        return Services.GetRequiredService<MainViewModel>();
    }
    
    private void CreateServiceProvider()
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
            Window? mainWindow = desktop.MainWindow;

            if (mainWindow != null && mainWindow.WindowState == WindowState.Minimized)
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
        var notification = Services?.GetRequiredService<ISystemNotificationService>();
        if (notification != null) 
            notification.ShowNotification("Option 2 clicked", "Hello, world!");
    }
}
