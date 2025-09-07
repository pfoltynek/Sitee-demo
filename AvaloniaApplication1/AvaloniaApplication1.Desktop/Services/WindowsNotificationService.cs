using AvaloniaApplication1.Services;
using CommunityToolkit.WinUI.Notifications;

namespace AvaloniaApplication1.Desktop.Services;

public class WindowsNotificationService : ISystemNotificationService
{
    public void ShowNotification(string title, string message)
    {
        var toast = new ToastContentBuilder()
            .AddText(title)
            .AddText(message);
        
        toast.Show();
    }
}