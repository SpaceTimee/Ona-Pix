using System.Windows;

namespace Ona_Pix
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Define.MAIN_WINDOW = new(e.Args);
            Define.MAIN_WINDOW.Show();
        }
    }
}