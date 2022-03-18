using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Ona_Pix
{
    public partial class AboutWindow : Window
    {
        public AboutWindow(bool isDarkMode)
        {
            InitializeComponent();
            PartialAboutWindow();

            if (isDarkMode)
            {
                MemoryStream memoryStream = new();
                Properties.Resources.Pixiv_Tan_Dark.Save(memoryStream, Properties.Resources.Pixiv_Tan_Dark.RawFormat);

                BitmapImage bitmapImage = new();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memoryStream;
                bitmapImage.EndInit();

                ImageBehavior.SetAnimatedSource(TanImage, bitmapImage);
            }
        }
        private void AboutWin_Loaded(object sender, RoutedEventArgs e)
        {
            try { UpdateRun.Text = "版本号: " + Define.CURRENT_VERSION; }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        private void DeveloperLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开Github";

            try { Define.StartProcess(DeveloperLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "Github打开完成";
        }
        //private async void UpdateLink_Click(object sender, RoutedEventArgs e) 位于Updater.cs
        private void PrivacyLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开Thought";

            try { Define.StartProcess(PrivacyLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "Thought打开完成";
        }

        private void OpenSourceLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开Github";

            try { Define.StartProcess(OpenSourceLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "Github打开完成";
        }
        private void LanzouLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开蓝奏云";

            try
            {
                MessageBox.Show("密码: ddvs");
                Define.StartProcess(LanzouLink.NavigateUri.ToString());
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "蓝奏云打开完成";
        }
        private void HomePageLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开主页";

            try { Define.StartProcess(HomePageLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "主页打开完成";
        }
        private void BlogLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开博客";

            try { Define.StartProcess(BlogLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "博客打开完成";
        }
        private void EmailLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开邮箱";

            try { Define.StartProcess("mailto:" + EmailLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "邮箱打开完成";
        }
        private void AgreementLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开Thought";

            try { Define.StartProcess(AgreementLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "Thought打开完成";
        }
    }
}