using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Ona_Pix.Pages;
using OnaCore;
using WpfAnimatedGif;
using MessageBox = System.Windows.MessageBox;

namespace Ona_Pix
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer IN_TIMER = new(), OUT_TIMER = new();
        private bool IS_ACTIVE = false, IS_FIXED = false;

        public MainWindow()
        {
            InitializeComponent();

            IN_TIMER.Interval = new TimeSpan(1);
            IN_TIMER.Tick += IN_TIMER_Tick;
            OUT_TIMER.Interval = new TimeSpan(1);
            OUT_TIMER.Tick += OUT_TIMER_Tick;
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Directory.Exists(Define.CACHE_PATH))
                    new DirectoryInfo(Define.CACHE_PATH).Delete(true);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDialog = new();
                #region 配置openDialog的参数
                openDialog.Title = "Ona Importer";
                openDialog.Multiselect = false; //不允许选择多个文件
                openDialog.RestoreDirectory = true; //自动填充用户上次选择的目录
                openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openDialog.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif|所有文件 (*.*)|*.*";
                openDialog.FilterIndex = 1; //默认png
                openDialog.AddExtension = true; //无后缀时自动增加后缀
                openDialog.CheckFileExists = true;  //检查文件是否正确
                openDialog.CheckPathExists = true;  //检查路径是否正确
                openDialog.ReadOnlyChecked = true; //设定只读
                openDialog.ShowReadOnly = false;    //不向用户显示只读选项
                openDialog.SupportMultiDottedExtensions = false; //不支持多拓展名
                openDialog.AutoUpgradeEnabled = true;   //自动升级对话框样式
                #endregion 配置openDialog的参数
                if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    ActiveSearchBox.Text = openDialog.FileName.ToString();
                    InactiveSearchBox.Text = openDialog.FileName.ToString();
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = "正在搜索图片";

                await PickInput();

                if (File.Exists((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
                    return;

                Title = "图片搜索完成";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = "正在下载图片";

                SaveFileDialog saveDialog = new();
                #region 配置saveDialog的参数
                saveDialog.Title = "Ona Saver";
                saveDialog.RestoreDirectory = true; //自动填充用户上次选择的目录
                saveDialog.FileName = "无题";   //默认文件名
                saveDialog.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif";
                saveDialog.FilterIndex = 1; //默认png
                saveDialog.AddExtension = true; //无后缀时自动增加后缀
                saveDialog.CheckFileExists = false;  //不检查文件是否正确
                saveDialog.CheckPathExists = true;  //检查路径是否正确
                saveDialog.SupportMultiDottedExtensions = false; //不支持多拓展名
                saveDialog.AutoUpgradeEnabled = true;   //自动升级对话框样式
                #endregion 配置saveDialog的参数
                if (saveDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                { Title = "操作正常取消"; return; }

                if (ShowImage.Source == null)
                    await PickInput();

                dynamic bitmapEncoder = null!;
                string imageExtension = Path.GetExtension(saveDialog.FileName);
                if (imageExtension == ".png")
                    bitmapEncoder = new PngBitmapEncoder();
                else if (imageExtension == ".jpg")
                    bitmapEncoder = new JpegBitmapEncoder();
                else if (imageExtension == ".gif")
                    bitmapEncoder = new GifBitmapEncoder();
                else
                    throw new Exception("Unexpected Extension");

                bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ShowImage.Source!));
                using FileStream imageFileStream = new(saveDialog.FileName.ToString(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                bitmapEncoder.Save(imageFileStream);

                Title = "图片下载完成";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private async void LuckyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = "正在获取图片";

                //将Json转换为JObject
                JObject LuckyImageJObject = JObject.Parse(await Http.GetAsync<string>(@"https://api.lolicon.app/setu/v2?r18=2&proxy=null", Define.MAIN_CLIENT));

                //提取并运行
                ActiveSearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();
                InactiveSearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();

                await PickInput();

                Title = "图片获取完成";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private void SettingButton_Click(object sender, RoutedEventArgs e)
        {
            try { Define.SETTING_WINDOW.ShowDialog(); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AboutWindow aboutWindow = new(((AppearancePage)Define.SETTING_WINDOW.Resources["appearancePage"]).DarkModeToggle.IS_TOGGLED);
                aboutWindow.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        private void ActiveSearchBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IS_FIXED = true;
        }
        private void ActiveSpace_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            IN_TIMER.Stop();
            OUT_TIMER.Start();

            ActiveSearchBox.Focus();

            IS_FIXED = false;
        }
        private void ActiveSpace_MouseIn(object sender, System.Windows.Input.MouseEventArgs e)
        {
            OUT_TIMER.Stop();
            IN_TIMER.Start();

            ActiveSearchBox.Focus();
        }
        private void ActiveSpace_MouseOut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IS_FIXED)
            {
                IN_TIMER.Stop();
                OUT_TIMER.Start();
            }
        }

        private void IN_TIMER_Tick(object? sender, EventArgs e)
        {
            if (ActiveRightGrid.Margin.Right < -10)
            {
                ActiveRightGrid.Margin = new Thickness(0, 60, ActiveRightGrid.Margin.Right + 0.3, 0);
                ActiveTopGrid.Margin = new Thickness(0, ActiveTopGrid.Margin.Top + 0.3, 0, 0);
            }
            else
                IN_TIMER.Stop();
        }
        private void OUT_TIMER_Tick(object? sender, EventArgs e)
        {
            if (ActiveRightGrid.Margin.Right > -65)
            {
                ActiveRightGrid.Margin = new Thickness(0, 60, ActiveRightGrid.Margin.Right - 0.3, 0);
                ActiveTopGrid.Margin = new Thickness(0, ActiveTopGrid.Margin.Top - 0.3, 0, 0);
            }
            else
                OUT_TIMER.Stop();
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text == "")
            {
                ActiveSearchButton.IsEnabled = false;
                ActiveDownloadButton.IsEnabled = false;
                InactiveSearchButton.IsEnabled = false;
                InactiveDownloadButton.IsEnabled = false;
            }
            else
            {
                ActiveSearchButton.IsEnabled = true;
                ActiveDownloadButton.IsEnabled = true;
                InactiveSearchButton.IsEnabled = true;
                InactiveDownloadButton.IsEnabled = true;
            }
        }

        private async void Smms_SetImageUrl(dynamic value)
        {
            await Dispatcher.Invoke(async () =>
            {
                try
                {
                    ActiveSearchBox.Text = value;
                    InactiveSearchBox.Text = value;

                    await PickInput();  //await IsUri();

                    Title = "图片搜索完成";
                }
                catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
            });
        }
        private void Smms_SetMainWindowTitle(dynamic value)
        {
            Dispatcher.Invoke(() => Title = value);
        }
        private void Smms_SetControlsEnabled()
        {
            Dispatcher.Invoke(SetControlsEnabled);
        }
        private void Smms_ShowError(dynamic value)
        {
            Dispatcher.Invoke(() =>
            {
                if (!((BehaviorPage)Define.SETTING_WINDOW.Resources["behaviorPage"]).DisableExceptionToggle.IS_TOGGLED)
                    MessageBox.Show("Error: " + value);
            });
        }

        private async Task PickInput()
        {
            try
            {
                Title = "正在识别输入";

                ActiveSearchBox.IsEnabled = false;
                ActiveSearchButton.IsEnabled = false;
                ActiveDownloadButton.IsEnabled = false;
                InactiveSearchBox.IsEnabled = false;
                InactiveSearchButton.IsEnabled = false;
                InactiveDownloadButton.IsEnabled = false;

                if (File.Exists((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
                { IsFilePath(); return; }
            }
            catch
            { SetControlsEnabled(); throw; }

            try
            {
                if (Regex.IsMatch((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text, "^([0-9]*)-?[0-9]*$"))
                    await IsPixivID();  //Pixiv ID
                else if (new Regex(Define.URI_REGEX).IsMatch((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
                    await IsUri();  //Uri
                else
                    await IsKeyWord();  //关键词

                Title = "输入识别完成";
            }
            catch { throw; }
            finally { SetControlsEnabled(); }
        }

        private async Task IsUri()
        {
            Title = "正在解析链接";

            if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains(@"www.pixiv.net/artworks")) //Pixiv Url
            {
                Exception exception = new();
                foreach (string fileSuffix in Define.FILE_SUFFIXES)
                {
                    try
                    {
                        await GetImage((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Replace(@"www.pixiv.net/artworks", @"pixiv.re") + fileSuffix);

                        Title = "链接解析完成";

                        return;
                    }
                    catch (Exception ex) { exception = ex; }
                }
                throw exception;
            }
            else if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains(@"www.pixiv.net/member_illust.php?") && (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains("illust_id"))  //Pixiv Illust Url
            {
                NameValueCollection paramCollection = GetParamCollection(new Uri((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text).Query);

                Exception exception = new();
                foreach (string fileSuffix in Define.FILE_SUFFIXES)
                {
                    try
                    {
                        await GetImage(@"https://pixiv.re/" + paramCollection["illust_id"]! + fileSuffix);

                        Title = "链接解析完成";

                        return;
                    }
                    catch (Exception ex) { exception = ex; }
                }
                throw exception;
            }
            else    //其他Uri(包括Pximg Url)
            {
                await GetImage((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Replace(@"pximg.net", @"pixiv.re"));
            }

            Title = "链接解析完成";
        }
        private async Task IsPixivID()
        {
            Title = "正在解析PixivID";

            Exception exception = new();
            foreach (string fileSuffix in Define.FILE_SUFFIXES)
            {
                try
                {
                    await GetImage(@"https://pixiv.re/" + (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text + fileSuffix);

                    Title = "PixivID解析完成";

                    return;
                }
                catch (Exception ex) { exception = ex; }
            }
            throw exception;
        }
        private void IsFilePath()
        {
            Title = "正在初始化请求";

            Smms smms = new();
            smms.SetImageUrl += Smms_SetImageUrl;
            smms.SetMainWindowTitle += Smms_SetMainWindowTitle;
            smms.SetControlsEnabled += Smms_SetControlsEnabled;
            smms.ShowError += Smms_ShowError;
            smms.ShellRun(Directory.GetCurrentDirectory(), (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text);

            Title = "正在解析文件";   //此时上传工作正在后台运行
        }
        private async Task IsKeyWord()
        {
            Title = "正在解析关键词";

            //将Json转换为JObject
            JObject LuckyImageJObject = JObject.Parse(await Http.GetAsync<string>($@"https://api.lolicon.app/setu/v2?r18=2&proxy=null&tag={(IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text}", Define.MAIN_CLIENT));

            //提取并运行
            ActiveSearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();
            InactiveSearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();
            await PickInput();

            Title = "关键词解析完成";
        }

        private async Task GetImage(string imageUri)
        {
            Title = "正在获取图片";

            HttpResponseMessage imageMessage = await Http.GetAsync<HttpResponseMessage>(imageUri, Define.MAIN_CLIENT, HttpCompletionOption.ResponseContentRead);
            imageMessage.EnsureSuccessStatusCode();

            SetImage(imageMessage);

            Title = "图片获取完成";
        }
        private void SetImage(HttpResponseMessage imageMessage)
        {
            Title = "正在读取图片";

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = imageMessage.Content.ReadAsStream();
            bitmapImage.EndInit();
            ImageBehavior.SetAnimatedSource(ShowImage, bitmapImage);

            if (!IS_ACTIVE)
            {
                ActiveSearchBox.Text = InactiveSearchBox.Text;
                InactiveGrid.Visibility = Visibility.Hidden;
                ActiveGrid.Visibility = Visibility.Visible;
                IS_ACTIVE = true;
            }

            Title = "图片读取完成";
        }

        //将URL参数分离为键值对集合
        private NameValueCollection GetParamCollection(string queryString)
        {
            Title = "正在解析链接";

            queryString = queryString.Replace("?", "");
            NameValueCollection paramCollection = new(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(queryString))
            {
                int count = queryString.Length;
                for (int i = 0; i < count; ++i)
                {
                    int startIndex = i;
                    int index = -1;
                    while (i < count)
                    {
                        char item = queryString[i];
                        if (item == '=')
                        {
                            if (index < 0)
                                index = i;
                        }
                        else if (item == '&')
                            break;

                        ++i;
                    }

                    string key;
                    string value = null!;
                    if (index >= 0)
                    {
                        key = queryString[startIndex..index];
                        value = queryString.Substring(index + 1, i - index - 1);
                    }
                    else
                    {
                        key = queryString[startIndex..i];
                    }
                    paramCollection[HttpUtility.UrlDecode(key, Encoding.UTF8)] = HttpUtility.UrlDecode(value, Encoding.UTF8);
                    if ((i == (count - 1)) && (queryString[i] == '&'))
                        paramCollection[key] = string.Empty;
                }
            }

            Title = "链接解析完成";

            return paramCollection;
        }
        //解锁控件
        private void SetControlsEnabled()
        {
            ActiveSearchBox.IsEnabled = true;
            ActiveSearchButton.IsEnabled = true;
            ActiveDownloadButton.IsEnabled = true;
            InactiveSearchBox.IsEnabled = true;
            InactiveSearchButton.IsEnabled = true;
            InactiveDownloadButton.IsEnabled = true;
        }

        ////检测文本编码
        //private static Encoding MyUrlDeCode(string str)
        //{
        //    //将原文本与用utf-8解码再编码的文本对比
        //    return str == HttpUtility.UrlEncode(HttpUtility.UrlDecode(str.ToUpper(), Encoding.UTF8), Encoding.UTF8) ? Encoding.UTF8 : Encoding.GetEncoding("gb2312");
        //}
    }
}