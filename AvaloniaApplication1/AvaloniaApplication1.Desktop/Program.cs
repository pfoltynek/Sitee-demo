using System;
using Avalonia;
using Avalonia.ReactiveUI;
using AvaloniaApplication1.Desktop.Services;
using AvaloniaApplication1.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AvaloniaApplication1.Desktop;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .AfterSetup(builder =>
            {
                if (builder.Instance is not App app)
                    throw new InvalidOperationException("Instance builderu není typu App.");

                app.RegisterPlatformServices = services =>
                {
                    services.TryAddSingleton<ISystemNotificationService, WindowsNotificationService>();
                };
            })
            .LogToTrace()
            .UseReactiveUI();
}
