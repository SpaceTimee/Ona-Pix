using System.Windows;
using System.Windows.Threading;
using Ona_Pix.Utils;

namespace Ona_Pix;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Define.MAIN_WINDOW = new(e.Args);
        Define.MAIN_WINDOW.Show();
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        Define.MAIN_WINDOW!.Title = "操作执行失败";
        Define.MAIN_WINDOW.SetControlsEnabled();

        if (!Define.BEHAVIOR_PAGE.DisableExceptionToggle.IS_TOGGLED)
            MessageBox.Show($"Error: {e.Exception.Message}");

        e.Handled = true;
    }
}