using System;
using System.Reflection;
using System.Windows;

namespace Ona_Pix
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void AboutWin_Loaded(object sender, RoutedEventArgs e)
        {
            try { VersionLabel.Content = "版本号: " + Assembly.GetExecutingAssembly().GetName().Version!.ToString()[0..^2]; }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void HomePageLink_Click(object sender, RoutedEventArgs e)
        {
            try { Define.StartProcess(HomePageLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void BlogLink_Click(object sender, RoutedEventArgs e)
        {
            try { Define.StartProcess(BlogLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void EmailLink_Click(object sender, RoutedEventArgs e)
        {
            try { Define.StartProcess("mailto:" + EmailLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
    }
}