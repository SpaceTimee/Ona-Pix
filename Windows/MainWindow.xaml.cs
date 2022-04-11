using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Ona_Pix.Controls;
using OnaCore;
using WpfAnimatedGif;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using MessageBox = System.Windows.MessageBox;

namespace Ona_Pix
{
    public partial class MainWindow : Window
    {
        private readonly DispatcherTimer ACTIVATE_TIMER = new(), IN_TIMER = new(), OUT_TIMER = new();
        private HttpResponseMessage IMAGE_MESSAGE = new();
        private bool IS_ACTIVE = false, IS_FIXED = false;
        internal char R18 = '2';

        public MainWindow(string[] args)
        {
            try
            {
                InitializeComponent();

                //填充拖入图标的文件路径
                if (args.Length >= 1 && File.Exists(args[0]))
                {
                    if (Array.IndexOf(Define.FILE_SUFFIXES, Path.GetExtension(args[0])) != -1)
                        InactiveSearchBox.Text = args[0];
                    else
                        throw new Exception("里面被塞入了奇怪的东西...");
                }

                ACTIVATE_TIMER.Interval = new TimeSpan(1);
                ACTIVATE_TIMER.Tick += ACTIVATE_TIMER_Tick;
                IN_TIMER.Interval = new TimeSpan(1);
                IN_TIMER.Tick += IN_TIMER_Tick;
                OUT_TIMER.Interval = new TimeSpan(1);
                OUT_TIMER.Tick += OUT_TIMER_Tick;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; }
        }
        private void MainWin_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //检查是否是第一次运行
                if (Properties.Settings.Default.IsFirstRun)
                    Welcome();
                else
                    PickSettings();

                //清理安装包
                ClearCache();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        //部分初始化操作
        private static void Welcome()
        {
            //第一次运行显示提示和关于窗口
            MessageBox.Show
            (
@"欢迎回家，主人! 我是 Ona Pix，你的专属女仆★
关于我的所有信息都可以在接下来的关于窗口中找到噢！随便逛逛吧，主人。
参观结束后就可以关闭关于窗口啦，我会在主窗口等你!"
            );

            AboutWindow aboutWindow = new(false);
            aboutWindow.ShowDialog();

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
                //Ona Pix自己在里面，清理不了
                if (AppDomain.CurrentDomain.SetupInformation.ApplicationBase == Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Ona Pix Cache\\"))
                {
                    if (MessageBox.Show("主人，我被困在 " + AppDomain.CurrentDomain.SetupInformation.ApplicationBase + " 临时文件夹里了，主人能帮我离开这个地方吗?", "求助", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        Process.Start("explorer", "/select," + Process.GetCurrentProcess().MainModule!.FileName);
                        Environment.Exit(0);
                    }
                }
                else throw;
            }
        }

        //窗口关闭事件
        protected override void OnClosing(CancelEventArgs e)
        {
            //强制结束
            Environment.Exit(0);
        }

        //按钮点击事件
        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            //浏览
            try
            {
                OpenFileDialog openDialog = new();
                #region 配置openDialog的参数
                openDialog.Title = "Ona Importer";
                openDialog.Multiselect = false; //不允许选择多个文件
                openDialog.RestoreDirectory = true; //自动填充用户上次选择的目录
                openDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                openDialog.Filter = "PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|GIF (*.gif)|*.gif";
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
            //搜索
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
            //下载
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
            //一图
            try
            {
                Title = "正在获取图片";

                //选择瑟瑟等级
                if (InactiveSearchBox.Text == "只要瑟瑟" || InactiveSearchBox.Text == "只要色色")
                {
                    if (MessageBox.Show("啊...真的要这样吗...", "彩蛋: ONLY R18", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        if (Define.BEHAVIOR_PAGE.DisableR18Toggle.IS_TOGGLED)
                            RestoreSettings(Define.BEHAVIOR_PAGE.DisableR18Toggle, Define.BEHAVIOR_PAGE.DisableR18Toggle_MouseDown);
                        R18 = '1';
                    }
                }

                //将Json转换为JObject
                JObject LuckyImageJObject = JObject.Parse(await Http.GetAsync<string>(@$"https://api.lolicon.app/setu/v2?r18={R18}&proxy=null", Define.MAIN_CLIENT));

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
            //设置
            try { Define.SETTING_WINDOW.ShowDialog(); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            //关于
            try
            {
                AboutWindow aboutWindow = new(Define.APPEARANCE_PAGE.DarkModeToggle.IS_TOGGLED);
                aboutWindow.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        //判断输入内容
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
            catch { SetControlsEnabled(); throw; }

            try
            {
                if (Regex.IsMatch((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text, "^([0-9]*)-?[0-9]*$"))
                    await IsPixivID();  //Pixiv ID
                else if (new Regex(Define.URI_REGEX).IsMatch((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text))
                {
                    if (!(IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.StartsWith("https://") && !(IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.StartsWith("http://"))
                        (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text = "https://" + (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text;
                    await IsUri();  //Uri
                }
                else
                    await IsKeyWord();  //关键词

                Title = "输入识别完成";
            }
            catch { throw; }
            finally { SetControlsEnabled(); }
        }

        //不同输入内容的处理
        private async Task IsUri()
        {
            Title = "正在解析链接";

            if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains(@"www.pixiv.net/artworks")) //Pixiv Url
                await GetImage((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Replace(@"www.pixiv.net/artworks", @"pixiv.re") + ".png");
            else if ((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains(@"www.pixiv.net/member_illust.php?") && (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Contains("illust_id"))  //Pixiv Illust Url
            {
                NameValueCollection paramCollection = GetParamCollection(new Uri((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text).Query);
                await GetImage(@"https://pixiv.re/" + paramCollection["illust_id"]! + ".png");
            }
            else    //其他Uri(包括Pximg Url)
                await GetImage((IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text.Replace(@"i.pximg.net", Define.BEHAVIOR_PAGE.PixivCatToggle.IS_TOGGLED ? @"i.pixiv.re" : @"pximg.moezx.cc"));

            Title = "链接解析完成";
        }
        private async Task IsPixivID()
        {
            Title = "正在解析PixivID";

            await GetImage(@"https://pixiv.re/" + (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text + ".png");

            Title = "PixivID解析完成";
        }
        private void IsFilePath()
        {
            Title = "正在初始化请求";

            Smms smms = new();
            smms.SetImageUrl += Smms_SetImageUrl;
            smms.SetMainWindowTitle += Smms_SetMainWindowTitle;
            smms.SetControlsEnabled += Smms_SetControlsEnabled;
            smms.ShowError += Smms_ShowError;
            smms.ShellRun(Directory.GetCurrentDirectory(), "\"" + (IS_ACTIVE ? ActiveSearchBox : InactiveSearchBox).Text + "\"");

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

        //Smms线程委托事件
        private async void Smms_SetImageUrl(dynamic value)
        {
            //Smms查找到图片链接后引发的事件
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
            //设置主窗口标题
            Dispatcher.Invoke(() => Title = value);
        }
        private void Smms_SetControlsEnabled()
        {
            //激活临时禁用的按钮
            Dispatcher.Invoke(SetControlsEnabled);
        }
        private void Smms_ShowError(dynamic value)
        {
            //报告异常
            Dispatcher.Invoke(() =>
            {
                if (!Define.BEHAVIOR_PAGE.DisableExceptionToggle.IS_TOGGLED)
                    MessageBox.Show("Error: " + value);
            });
        }

        //图片获取和显示
        private async Task GetImage(string imageUri)
        {
            //获取图片
            Title = "正在获取图片";

            HttpResponseMessage imageMessage = await Http.GetAsync<HttpResponseMessage>(imageUri, Define.MAIN_CLIENT, HttpCompletionOption.ResponseContentRead);
            imageMessage.EnsureSuccessStatusCode();
            IMAGE_MESSAGE = imageMessage;

            if (IS_ACTIVE)
                SetImage();
            else
                ACTIVATE_TIMER.Start();

            Title = "图片获取完成";
        }
        private void SetImage()
        {
            //显示图片
            Title = "正在读取图片";

            BitmapImage bitmapImage = new();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = IMAGE_MESSAGE.Content.ReadAsStream();
            bitmapImage.EndInit();
            ImageBehavior.SetAnimatedSource(ShowImage, bitmapImage);

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

        //切换控件禁用状态
        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //切换按钮禁用状态(无内容时禁用按钮)
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
        private void SetControlsEnabled()
        {
            //解锁搜索时临时禁用的控件
            ActiveSearchBox.IsEnabled = true;
            ActiveSearchButton.IsEnabled = true;
            ActiveDownloadButton.IsEnabled = true;
            InactiveSearchBox.IsEnabled = true;
            InactiveSearchButton.IsEnabled = true;
            InactiveDownloadButton.IsEnabled = true;
        }

        //动画激活事件
        internal void ActiveSpace_MouseIn(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //激活悬浮菜单栏弹出
            OUT_TIMER.Stop();
            IN_TIMER.Start();

            ActiveSearchBox.Focus();
        }
        private void ActiveSearchBox_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //点击搜索框后临时固定悬浮菜单栏
            OUT_TIMER.Stop();
            IN_TIMER.Start();

            ActiveSearchBox.Focus();

            IS_FIXED = true;
        }
        internal void ActiveSpace_MouseOut(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //激活悬浮菜单栏收回
            if (!Define.APPEARANCE_PAGE.LockAnimationToggle.IS_TOGGLED && !IS_FIXED)
            {
                IN_TIMER.Stop();
                OUT_TIMER.Start();
            }
        }
        private void InactiveSpace_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //点击空白区域后激活悬浮菜单栏收回
            if (!Define.APPEARANCE_PAGE.LockAnimationToggle.IS_TOGGLED)
            {
                IN_TIMER.Stop();
                OUT_TIMER.Start();

                IS_FIXED = false;
            }
        }

        //动画播放事件
        private void ACTIVATE_TIMER_Tick(object? sender, EventArgs e)
        {
            //初始界面切换至图片界面的动画事件
            if (InactiveRightGrid.Margin.Left < Width)
            {
                InactiveRightGrid.Margin = new Thickness(InactiveRightGrid.Margin.Left + 0.5, 0, InactiveRightGrid.Margin.Right - 0.5, 0);
                InactiveTopGrid.Margin = new Thickness(0, InactiveTopGrid.Margin.Top - 0.1, 0, InactiveTopGrid.Margin.Bottom + 0.1);
            }
            else
            {
                ACTIVATE_TIMER.Stop();

                SetImage();

                ActiveSearchBox.Text = InactiveSearchBox.Text;
                InactiveGrid.Visibility = Visibility.Collapsed;
                ActiveGrid.Visibility = Visibility.Visible;

                IS_ACTIVE = true;
            }
        }
        private void IN_TIMER_Tick(object? sender, EventArgs e)
        {
            //悬浮菜单栏弹出的动画事件
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
            //悬浮菜单栏收回的动画事件
            if (ActiveRightGrid.Margin.Right > -65)
            {
                ActiveRightGrid.Margin = new Thickness(0, 60, ActiveRightGrid.Margin.Right - 0.3, 0);
                ActiveTopGrid.Margin = new Thickness(0, ActiveTopGrid.Margin.Top - 0.3, 0, 0);
            }
            else
                OUT_TIMER.Stop();
        }

        //文件拖入事件
        private void ReveivingSpace_DragEnter(object sender, System.Windows.DragEventArgs e)
        {
            //文件拖入主窗口时的鼠标变化
            try
            {
                e.Handled = true;

                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                    e.Effects = DragDropEffects.Copy;
                else
                    e.Effects = DragDropEffects.None;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private void ReveivingSpace_Drop(object sender, System.Windows.DragEventArgs e)
        {
            //文件拖入主窗口后的文件路径处理
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    string[] path = (string[])e.Data.GetData(DataFormats.FileDrop);

                    if (Array.IndexOf(Define.FILE_SUFFIXES, Path.GetExtension(path[0])) != -1)
                    {
                        ActiveSearchBox.Text = path[0];
                        InactiveSearchBox.Text = path[0];
                    }
                    else
                        throw new Exception("里面被塞入了奇怪的东西...");
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        private void MainWin_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control && e.Key == Key.W)
                Environment.Exit(0);
        }
    }
}