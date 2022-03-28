using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
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

        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = "正在打开链接";

                string prefix = "";

                if (sender == EmailLink)
                    prefix = "mailto:";
                else if (sender == LanzouLink)
                    MessageBox.Show("密码: ddvs");

                Define.StartProcess(prefix + ((Hyperlink)sender).NavigateUri.ToString());

                Title = "链接打开完成";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        //private async void UpdateLink_Click(object sender, RoutedEventArgs e) 位于Updater.cs

        private void AboutWin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();    //关闭窗口
        }
    }
}