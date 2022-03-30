using System.Windows;

namespace Ona_Pix
{
    public partial class App : Application
    {
        //程序启动事件
        protected override void OnStartup(StartupEventArgs e)
        {
            Define.MAIN_WINDOW = new(e.Args);
            Define.MAIN_WINDOW.Show();
        }
    }
}