using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Ona_Pix.Wins
{
    public partial class AboutWin : Window
    {
        public static readonly string[] Tips =
        [
            "主人","主...主人?","好的，主人","是，主人","收到，主人",
            "主人累了吗","休息一下吧，主人","注意休息，主人","注意身体，主人",
            "女仆的工作★","一起喝咖啡吧，主人","咖啡 Suki★","咖啡 Daisuki★",
            "那里不可以，主人","啊，主人好棒","主人，❤",
            "不可以瑟瑟","瑟瑟打咩!","H 是不可以的!",
            "Ona Pix","Ona Piksu?","Ona Piku ★","Ona...阿嚏!",
            "A B C D","A C G N","S S S S","Y A P T","L O V E",
            "关于: 关于:",
            "❤","✨","★",
            "(｡･ω･｡)ﾉ♡","(๑• . •๑)","•ᴗ•",
            "Esc?","Ctrl+W?"
        ];

        public AboutWin(bool isDarkMode)
        {
            InitializeComponent();
            PartialAboutWindow();

            //检查暗色模式
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

            //显示随机一言 (Tips)
            if (!Define.BEHAVIOR_PAGE.DisableTipsToggle.IS_TOGGLED)
            {
                Random random = new(Convert.ToInt32(DateTime.Now.Ticks & 0x0000FFFF));
                Title = $"关于: {Tips[random.Next(0, Tips.Length)]}";

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
        protected override void OnSourceInitialized(EventArgs e) => IconRemover.RemoveIcon(this);
        private void AboutWin_Loaded(object sender, RoutedEventArgs e) => UpdateRun.Text = "版本号: " + Define.CURRENT_VERSION;

        //链接点击事件
        private void HyperLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开链接";

            string prefix = string.Empty;

            if (sender == EmailLink)
                prefix = "mailto:";
            else if (sender == LanzouLink)
                MessageBox.Show("密码: ddvs");

            Define.StartProcess(prefix + ((Hyperlink)sender).NavigateUri.ToString());

            Title = "链接打开完成";
        }
        //private async void UpdateLink_Click(object sender, RoutedEventArgs e) 位于Updater.cs

        //窗口热键
        private void AboutWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                Close();    //关闭窗口
        }
    }
}