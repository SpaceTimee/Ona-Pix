using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Ona_Pix.Controls;
using OnaCore;
using OnaPixSecret;
using SauceNET;
using SauceNET.Model;
using WpfAnimatedGif;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;

namespace Ona_Pix
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient MAIN_CLIENT = new();    //当前窗口使用的唯一的 HttpClient
        private BitmapImage? CURRENT_IMAGE; //当前窗口显示的图片 (便于以后改为图片缓存形式)
        private bool IS_ACTIVE = false, IS_FIXED = false;   //当前窗口的状态信息

        public MainWindow(string[] args)
        {
            InitializeComponent();

            //填充被拖入到图标打开的文件的路径
            if (args.Length >= 1 && File.Exists(args[0]))
            {
                if (Array.IndexOf(Define.FILE_SUFFIXES, Path.GetExtension(args[0]).ToLower()) != -1)
                    InactiveSearchBox.Text = args[0];
                else
                    throw new Exception("里面被塞入了奇怪的东西...");
            }
        }
        protected override void OnSourceInitialized(EventArgs e) => IconRemover.RemoveIcon(this);
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            //检查是否是第一次运行
            if (Properties.Settings.Default.IsFirstRun)
                Welcome();
            else
                PickSettings();

            //清理自动更新留下的安装包
            ClearCache();
        }
        private static void Welcome()
        {
            //第一次运行显示提示和关于窗口
            MessageBox.Show
            (
@"欢迎回家，主人! 我是 Ona Pix，你的专属女仆★
关于我的所有信息都可以在接下来的关于窗口中找到噢！随便逛逛吧，主人。
参观结束后就可以关闭关于窗口啦，我在主窗口等你!"
            );

            new AboutWindow(false).ShowDialog();

            Properties.Settings.Default.IsFirstRun = false;
            Properties.Settings.Default.Save();
        }
        private void PickSettings()
        {
            //检查是否需要还原设置
            if (Properties.Settings.Default.IsDarkMode)
                RestoreSettings(Define.APPEARANCE_PAGE.DarkModeToggle, Define.APPEARANCE_PAGE.DarkModeToggle_MouseDown);
            if (Properties.Settings.Default.IsIconButton)
                RestoreSettings(Define.APPEARANCE_PAGE.IconButtonToggle, Define.APPEARANCE_PAGE.IconButtonToggle_MouseDown);
            if (Properties.Settings.Default.IsAnimationLocked)
                RestoreSettings(Define.APPEARANCE_PAGE.LockAnimationToggle, Define.APPEARANCE_PAGE.LockAnimationToggle_MouseDown);
            if (Properties.Settings.Default.IsR18Disabled)
                RestoreSettings(Define.BEHAVIOR_PAGE.DisableR18Toggle, Define.BEHAVIOR_PAGE.DisableR18Toggle_MouseDown);
            if (Properties.Settings.Default.IsPixivCat)
                RestoreSettings(Define.BEHAVIOR_PAGE.PixivCatToggle, Define.BEHAVIOR_PAGE.PixivCatToggle_MouseDown);
            if (Properties.Settings.Default.IsExceptionDisabled)
                RestoreSettings(Define.BEHAVIOR_PAGE.DisableExceptionToggle, Define.BEHAVIOR_PAGE.DisableExceptionToggle_MouseDown);
            if (Properties.Settings.Default.IsTipsDisabled)
                RestoreSettings(Define.BEHAVIOR_PAGE.DisableTipsToggle, Define.BEHAVIOR_PAGE.DisableTipsToggle_MouseDown);

            Define.APPEARANCE_PAGE.OpacitySlider.Value = Properties.Settings.Default.MenuOpacity;
        }
        private void RestoreSettings(ToggleSwitch toggle, Action<object, MouseButtonEventArgs> Toggle_MouseDown)
        {
            //还原设置
            toggle.SwitchStatus();
            Toggle_MouseDown(this, null!);
        }
        private static void ClearCache()
        {
            //清理更新时下载的安装包
            try
            {
                if (Directory.Exists(Define.CACHE_PATH))
                    new DirectoryInfo(Define.CACHE_PATH).Delete(true);
            }
            catch
            {
                //Ona Pix 自己在里面，清理不了
                if (AppDomain.CurrentDomain.SetupInformation.ApplicationBase == Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"Ona Pix Cache\"))
                {
                    if (MessageBox.Show("主人，我被困在 " + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + " 临时文件夹里了，主人能帮我离开这个地方吗?", "求助", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer", "/select," + Environment.ProcessPath);
                        Environment.Exit(0);
                    }
                }
                else throw;
            }
        }

        //菜单动画激活事件
        internal void ActiveSpace_MouseIn(object sender, MouseEventArgs e)
        {
            //悬浮菜单栏浮出
            AnimateActiveGrid(new Thickness(0, 60, -10, 0), new Thickness(0, -10, 0, 0));
            ActiveSearchBox.Focus();
        }
        private void ActiveSearchBox_PreviewMouseDown(object sender, MouseButtonEventArgs e) => IS_FIXED = true;
        internal void ActiveSpace_MouseOut(object sender, MouseEventArgs e)
        {
            //悬浮菜单栏收回
            if (!Define.APPEARANCE_PAGE.LockAnimationToggle.IS_TOGGLED && !IS_FIXED)
                AnimateActiveGrid(new Thickness(0, 60, -65, 0), new Thickness(0, -65, 0, 0));
        }
        private void InactiveSpace_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //点击空白区域后悬浮菜单栏收回
            if (!Define.APPEARANCE_PAGE.LockAnimationToggle.IS_TOGGLED)
            {
                AnimateActiveGrid(new Thickness(0, 60, -65, 0), new Thickness(0, -65, 0, 0));
                IS_FIXED = false;
            }
        }

        //执行菜单动画
        private void AnimateActiveGrid(Thickness activeRightGridThickness, Thickness activeTopGridThickness)
        {
            ActiveRightGrid.BeginAnimation(MarginProperty, new ThicknessAnimation(activeRightGridThickness, TimeSpan.FromSeconds(0.25)));
            ActiveTopGrid.BeginAnimation(MarginProperty, new ThicknessAnimation(activeTopGridThickness, TimeSpan.FromSeconds(0.25)));
        }

        //文件拖入事件
        private void ReceivingSpace_DragEnter(object sender, DragEventArgs e)
        {
            //文件拖入主窗口时的鼠标变化
            e.Handled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;
            else
                e.Effects = DragDropEffects.None;
        }
        private void ReceivingSpace_Drop(object sender, DragEventArgs e)
        {
            //文件拖入主窗口后的文件路径处理
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] path = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (Array.IndexOf(Define.FILE_SUFFIXES, Path.GetExtension(path[0]).ToLower()) != -1)
                    ActiveSearchBox.Text = InactiveSearchBox.Text = path[0];
                else
                    throw new Exception("里面被塞入了奇怪的东西...");
            }
        }

        //按钮点击事件
        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            //浏览
            OpenFileDialog openDialog = new();
            #region 配置openDialog的参数
            openDialog.Title = "Ona Importer";
            openDialog.Multiselect = false; //不允许选择多个文件
            openDialog.RestoreDirectory = true; //自动填充用户上次选择的目录
            openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            openDialog.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif";
            openDialog.FilterIndex = 1; //默认 png
            openDialog.AddExtension = true; //无后缀时自动增加后缀
            openDialog.CheckFileExists = true;  //检查文件是否正确
            openDialog.CheckPathExists = true;  //检查路径是否正确
            openDialog.ReadOnlyChecked = true; //设定只读
            openDialog.ShowReadOnly = false;    //不向用户显示只读选项
            #endregion 配置openDialog的参数

            if (openDialog.ShowDialog() == true)
                ActiveSearchBox.Text = InactiveSearchBox.Text = openDialog.FileName.ToString();
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            //搜索
            Title = "正在搜索图片";

            await PickInput();

            Title = "图片搜索完成";
        }
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            //下载
            Title = "正在下载图片";

            SaveFileDialog saveDialog = new();
            #region 配置saveDialog的参数
            saveDialog.Title = "Ona Saver";
            saveDialog.RestoreDirectory = true; //自动填充用户上次选择的目录
            saveDialog.FileName = "无题" + DateTime.Now.ToString("yMdHms")[1..];   //默认文件名
            saveDialog.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif";
            saveDialog.FilterIndex = 1; //默认 png
            saveDialog.AddExtension = true; //无后缀时自动增加后缀
            saveDialog.CheckFileExists = false;  //不检查文件是否正确
            saveDialog.CheckPathExists = true;  //检查路径是否正确
            #endregion 配置saveDialog的参数

            if (saveDialog.ShowDialog() != true)
            {
                Title = "操作正常取消";
                return;
            }

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
                throw new Exception("图片保存后缀不合法");

            bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ShowImage.Source!));
            using FileStream imageFileStream = new(saveDialog.FileName.ToString(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
            bitmapEncoder.Save(imageFileStream);

            Title = "图片下载完成";
        }
        private async void LuckyButton_Click(object sender, RoutedEventArgs e)
        {
            //一图
            Title = "正在获取图片";

            //触发瑟瑟彩蛋
            if (InactiveSearchBox.Text == "只要瑟瑟" || InactiveSearchBox.Text == "只要色色" && MessageBox.Show("啊...真的要这样吗...", "彩蛋: ONLY R18", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                if (Define.BEHAVIOR_PAGE.DisableR18Toggle.IS_TOGGLED)
                    RestoreSettings(Define.BEHAVIOR_PAGE.DisableR18Toggle, Define.BEHAVIOR_PAGE.DisableR18Toggle_MouseDown);

                Define.R18 = '1';
            }

            //将 Lolicon 响应的 Json 数据转换为 JObject
            JObject LuckyImageJObject = JObject.Parse(await Http.GetAsync<string>(@$"https://api.lolicon.app/setu/v2?r18={Define.R18}&proxy=null", MAIN_CLIENT));

            //提取并运行
            ActiveSearchBox.Text = InactiveSearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();

            await PickInput();

            Title = "图片获取完成";
        }
        private void SettingButton_Click(object sender, RoutedEventArgs e) => Define.SETTING_WINDOW.ShowDialog();
        private void AboutButton_Click(object sender, RoutedEventArgs e) => new AboutWindow(Define.APPEARANCE_PAGE.DarkModeToggle.IS_TOGGLED).ShowDialog();

        //判断输入内容
        private async Task PickInput()
        {
            Title = "正在识别输入";

            ActiveSearchBox.IsEnabled = ActiveSearchButton.IsEnabled = ActiveDownloadButton.IsEnabled =
            InactiveSearchBox.IsEnabled = InactiveSearchButton.IsEnabled = InactiveDownloadButton.IsEnabled = false;

            if (File.Exists((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
                await IsFilePath(); //图片路径
            else if (Regex.IsMatch((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text, "^([0-9]*)-?[0-9]*$"))
                await IsPixivID();  //Pixiv ID
            else if (new Regex(Define.URL_REGEX).IsMatch((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
            {
                if (!(IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.StartsWith("https://") && !(IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.StartsWith("http://"))
                    (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text = "https://" + (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text;
                await IsUri();  //Uri
            }
            else
                await IsKeyWord();  //关键词

            Title = "输入识别完成";
        }

        //处理输入内容
        private async Task IsUri()
        {
            Title = "正在解析链接";

            if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains(@"www.pixiv.net/artworks")) //Pixiv Url
                await GetImage((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Replace(@"www.pixiv.net/artworks", @"pixiv.nl") + ".png");
            else if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains(@"www.pixiv.net/member_illust.php?") && (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains("illust_id"))  //Pixiv Illust Url
            {
                NameValueCollection paramCollection = GetParamCollection(new Uri((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text).Query);
                await GetImage(@"https://pixiv.nl/" + paramCollection["illust_id"]! + ".png");
            }
            else    //其他 Uri (包括 Pximg Url)
                await GetImage((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Replace(@"i.pximg.net", Define.BEHAVIOR_PAGE.PixivCatToggle.IS_TOGGLED ? @"i.pixiv.nl" : @"prox.spacetimee.xyz"));

            Title = "链接解析完成";
        }
        private async Task IsPixivID()
        {
            Title = "正在解析 PixivID";

            await GetImage(@"https://pixiv.nl/" + (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text + ".png");

            Title = "PixivID 解析完成";
        }
        private async Task IsFilePath()
        {
            Title = "正在解析图片";

            string smmsJson = await UploadFile((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text);
            if (string.IsNullOrWhiteSpace(smmsJson))
                throw new Exception("未接收到有效响应");

            //将 smms 响应的 Json 数据转换为 JObject
            JObject smmsJObject = JObject.Parse(smmsJson!);
            if (!(bool)smmsJObject["success"]!)
                throw new Exception(smmsJObject["code"]!.ToString());

            //提取所需数据
            string imageUrl = smmsJObject["data"]!["url"]!.ToString();
            string deleteUrl = smmsJObject["data"]!["delete"]!.ToString();

            try
            {
                SauceNETClient sauceNETClient = new(Secret.GetSauceNaoApiKey());
                //获取相似图片的链接
                Sauce sauce = await sauceNETClient.GetSauceAsync(imageUrl);
                //查找最相似且链接不为空的图片的索引
                int i = 0;
                while (sauce.Results[i].SourceURL == null) ++i;
                //填充查找到的链接并再次搜索响应结果
                ActiveSearchBox.Text = InactiveSearchBox.Text = sauce.Results[i].SourceURL;
                await PickInput();  //await IsUri();
            }
            catch { throw; }
            finally
            {
                try { (await Http.GetAsync<HttpResponseMessage>(deleteUrl, MAIN_CLIENT)).EnsureSuccessStatusCode(); }
                catch { MessageBox.Show("Error: 图片清理失败，该图片可能暂时无法再次解析"); }
            }

            Title = "图片解析完成";
        }
        private async Task IsKeyWord()
        {
            Title = "正在解析关键词";

            //将 Lolicon 响应的 Json 数据转换为 JObject
            JObject luckyImageJObject = JObject.Parse(await Http.GetAsync<string>($@"https://api.lolicon.app/setu/v2?r18=2&proxy=null&tag={(IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text}", MAIN_CLIENT));

            //判断是否有查找到的结果
            if (!luckyImageJObject["data"]!.HasValues)
                throw new Exception("关键词太冷门啦，没有找到相关图片哦");

            //提取和填充所需数据并再次搜索响应结果
            ActiveSearchBox.Text = InactiveSearchBox.Text = luckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();
            await PickInput();

            Title = "关键词解析完成";
        }

        //将URL的参数分离成键值对集合
        private static NameValueCollection GetParamCollection(string queryString)
        {
            //去掉开头的'?'字符并在末尾添加一个'&'字符标志结束
            queryString = queryString.Replace("?", string.Empty) + '&';

            //分离所有参数并记录
            NameValueCollection paramCollection = new(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrWhiteSpace(queryString))
            {
                int startIndex = 0, equalIndex = 0;
                for (int endIndex = 0; endIndex < queryString.Length; ++endIndex)
                {
                    if (queryString[endIndex] == '=')
                        equalIndex = endIndex;
                    else if (queryString[endIndex] == '&')
                    {
                        paramCollection[HttpUtility.UrlDecode(queryString[startIndex..equalIndex], Encoding.UTF8)] = HttpUtility.UrlDecode(queryString[(equalIndex + 1)..endIndex], Encoding.UTF8);
                        startIndex = endIndex + 1;
                    }
                }
            }

            //返回包含所有被分离的参数的键值对集合
            return paramCollection;
        }

        //上传图片文件至 smms
        public static async Task<string> UploadFile(string filePath)
        {
            //将数据写入数据流
            string boundary = "------WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x");
            string header = $"Content-Disposition: form-data; name=\"smfile\"; filename=\"{filePath}\"\r\n" + "Content-Type: application/octet-stream\r\n\r\n";
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            byte[] fileBytes = new byte[81920];    //最优 80kb 文件读取缓存
            byte[] endBoundaryBytes = Encoding.ASCII.GetBytes("\r\n--" + boundary + "--");
            int fileBytesLength;
            using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            using MemoryStream memoryStream = new();
            memoryStream.Write(boundaryBytes, 0, boundaryBytes.Length);
            memoryStream.Write(headerBytes, 0, headerBytes.Length);
            while ((fileBytesLength = fileStream.Read(fileBytes, 0, fileBytes.Length)) != 0)
                memoryStream.Write(fileBytes, 0, fileBytesLength);
            memoryStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);

            //配置图片上传请求的参数
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create("https://smms.app/api/v2/upload");
            webRequest.Method = "POST";
            webRequest.Headers.Add("Authorization", Secret.GetSmmsApiKey());
            webRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            webRequest.ContentLength = memoryStream.Length;

            //将数据流写入图片上传请求
            byte[] tempBytes = new byte[memoryStream.Length];
            using Stream requestStream = webRequest.GetRequestStream();
            memoryStream.Position = 0;
            memoryStream.Read(tempBytes, 0, tempBytes.Length);
            requestStream.Write(tempBytes, 0, tempBytes.Length);

            //返回 smms 响应结果
            return new StreamReader((await webRequest.GetResponseAsync()).GetResponseStream()).ReadToEnd();
        }

        //获取和显示图片
        private async Task GetImage(string imageUri)
        {
            Title = "正在获取图片";

            HttpResponseMessage responseMessage = await Http.GetAsync<HttpResponseMessage>(imageUri, MAIN_CLIENT, HttpCompletionOption.ResponseContentRead);
            responseMessage.EnsureSuccessStatusCode();

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = responseMessage.Content.ReadAsStream();
            try { bitmapImage.EndInit(); }
            catch (NotSupportedException) { throw new NotSupportedException("无法显示该结果"); }

            CURRENT_IMAGE = bitmapImage;

            if (IS_ACTIVE)
                SetImage();
            else
            {
                ThicknessAnimation inactiveRightGridAnimation = new(new Thickness(Width - 10, 0, -(Width - 10), 0), TimeSpan.FromSeconds(0.3));
                ThicknessAnimation inactiveTopGridAnimation = new(new Thickness(0, -(InactiveTopGrid.ActualHeight + 10), 0, InactiveTopGrid.ActualHeight + 10), TimeSpan.FromSeconds(0.3));
                inactiveRightGridAnimation.Completed += ThicknessAnimation_Completed;
                InactiveRightGrid.BeginAnimation(MarginProperty, inactiveRightGridAnimation);
                InactiveTopGrid.BeginAnimation(MarginProperty, inactiveTopGridAnimation);
            }

            Title = "图片获取完成";
        }
        private void SetImage()
        {
            Title = "正在读取图片";

            ImageBehavior.SetAnimatedSource(ShowImage, CURRENT_IMAGE);
            SetControlsEnabled();

            Title = "图片读取完成";
        }

        //初始界面切换动画完成事件
        private void ThicknessAnimation_Completed(object? sender, EventArgs e)
        {
            SetImage();

            ActiveSearchBox.Text = InactiveSearchBox.Text;
            InactiveGrid.Visibility = Visibility.Collapsed;
            ActiveGrid.Visibility = Visibility.Visible;

            IS_ACTIVE = true;
        }

        //切换输入框和按钮的 IsEnabled 属性
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //输入框中无内容时禁用搜索和下载按钮，否则启用
            if (string.IsNullOrWhiteSpace((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
                ActiveSearchButton.IsEnabled = ActiveDownloadButton.IsEnabled =
                InactiveSearchButton.IsEnabled = InactiveDownloadButton.IsEnabled = false;
            else
                ActiveSearchButton.IsEnabled = ActiveDownloadButton.IsEnabled =
                InactiveSearchButton.IsEnabled = InactiveDownloadButton.IsEnabled = true;
        }
        public void SetControlsEnabled()
        {
            //启用在搜索时被临时禁用的搜索框和按钮
            ActiveSearchBox.IsEnabled = ActiveSearchButton.IsEnabled = ActiveDownloadButton.IsEnabled =
            InactiveSearchBox.IsEnabled = InactiveSearchButton.IsEnabled = InactiveDownloadButton.IsEnabled = true;
        }

        //窗口关闭事件
        protected override void OnClosing(CancelEventArgs e) => Environment.Exit(0);    //强制关闭

        //窗口热键
        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
                Environment.Exit(0);
        }

        //处理异常
        private void HandleException(Exception ex)
        {
            Title = "操作执行失败";

            if (!Define.BEHAVIOR_PAGE.DisableExceptionToggle.IS_TOGGLED)
                MessageBox.Show("Error: " + ex.Message);

            SetControlsEnabled();
        }
    }
}