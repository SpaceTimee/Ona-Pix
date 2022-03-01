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


        private void PrivacyLink_Click(object sender, RoutedEventArgs e)
        {
            try { Define.StartProcess(PrivacyLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void AgreementLink_Click(object sender, RoutedEventArgs e)
        {
            try { Define.StartProcess(AgreementLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }

        private void OpenSourceLink_Click(object sender, RoutedEventArgs e)
        {
            try { Define.StartProcess(OpenSourceLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); return; }
        }
        private void LanzouLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("密码: ddvs");
                Define.StartProcess(LanzouLink.NavigateUri.ToString());
            }
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