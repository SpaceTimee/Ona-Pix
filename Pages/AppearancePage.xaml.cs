using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace Ona_Pix.Pages
{
    public partial class AppearancePage : UserControl
    {
        public AppearancePage()
        {
            InitializeComponent();
        }

        private void DarkModeToggle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((BundledTheme)Application.Current.Resources.MergedDictionaries[0]).BaseTheme =
                DarkModeToggle.IS_TOGGLED ? BaseTheme.Dark : BaseTheme.Light;
        }
        private void LockAnimationToggle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LockAnimationToggle.IS_TOGGLED)
                Define.MAIN_WINDOW.ActiveSpace_MouseIn(this, null!);
            else
                Define.MAIN_WINDOW.ActiveSpace_MouseOut(this, null!);
        }
    }
}