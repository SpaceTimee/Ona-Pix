using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Ona_Pix.Pages;
using WpfAnimatedGif;

namespace Ona_Pix
{
    public partial class AboutWindow : Window
    {
        public static readonly string[] TIPS = new string[]
        {
            "主人","主...主人?","好的，主人","是，主人","收到，主人",
            "主人累了吗","休息一下吧，主人","注意休息，主人","注意身体，主人",
            "女仆的工作★","一起喝咖啡吧，主人","咖啡Suki★","咖啡Daisuki★",
            "那里不可以，主人","啊，主人好棒","主人，❤",
            "不可以瑟瑟","瑟瑟打咩!","H是不可以的!",
            "Ona Pix","Ona Piksu?","Ona Piku★","Ona...阿嚏!",
            "A B C D","A C G N","S S S S","Y A P T","L O V E",
            "关于: 关于:",
            "❤","✨","★",
            "(｡･ω･｡)ﾉ♡","(๑• . •๑)","•ᴗ•",
            "Esc?","Ctrl+W?"
        };

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

            if (!((BehaviorPage)Define.SETTING_WINDOW.Resources["behaviorPage"]).DisableTipsToggle.IS_TOGGLED)
            {
                Random random = new(Convert.ToInt32(DateTime.Now.Ticks & 0x0000FFFF));
                Title = $"关于: {TIPS[random.Next(0, TIPS.Length)]}";

                #region Tips 测试代码
                //System.Threading.Tasks.Task.Run(() =>
                //{
                //    Dispatcher.Invoke(() =>
                //    {
                //        for (int i = 0; i < TIPS.Length; i++)
                //        {
                //            Title = $"关于: {TIPS[i]}";
                //            System.Threading.Thread.Sleep(1000);
                //        }
                //    });
                //});
                #endregion Tips 测试代码
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