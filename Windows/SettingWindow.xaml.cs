using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace Ona_Pix
{
    public partial class SettingWindow : Window
    {
        public SettingWindow()
        {
            InitializeComponent();
        }
        private void SettingWin_Loaded(object sender, RoutedEventArgs e)
        {
            //默认选中菜单栏的第1个选项
            PaneListBox.SelectedIndex = 0;
            PaneListBox.Focus();
        }

        //窗口关闭事件
        protected override void OnClosing(CancelEventArgs e)
        {
            //仅隐藏
            e.Cancel = true;
            Hide();
        }

        //菜单切换事件
        private void PaneListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //随菜单栏选项变化切换显示页面
            PageBorder.Child = PaneListBox.SelectedIndex switch
            {
                0 => (UIElement)Resources["appearancePage"],
                1 => (UIElement)Resources["behaviorPage"],
                _ => (UIElement)Resources["appearancePage"],
            };
        }

        private void SettingWin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();    //关闭窗口
        }
    }
}