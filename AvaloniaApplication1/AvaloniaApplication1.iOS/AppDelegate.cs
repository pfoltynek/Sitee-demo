using System;
using Avalonia;
using Avalonia.iOS;
using Avalonia.ReactiveUI;
using AvaloniaApplication1.iOS.Services;
using AvaloniaApplication1.Services;
using Foundation;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AvaloniaApplication1.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register("AppDelegate")]
public partial class AppDelegate : AvaloniaAppDelegate<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .AfterSetup(appBuilder =>
            {
                if (appBuilder.Instance is not App app)
                    throw new InvalidOperationException("Instance builderu není typu App.");

                app.RegisterPlatformServices = services =>
                {
                    services.TryAddSingleton<ISystemNotificationService, MacNotificationService>();
                };  
            })
            .WithInterFont()
            .UseReactiveUI();
    }
}
