using System;
using System.Collections.Specialized;
using System.Diagnostics;
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
using OnaCore;
using OnaPixSecret;
using SauceNET;
using WpfAnimatedGif;
using MessageBox = System.Windows.MessageBox;

namespace Ona_Pix
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient MAIN_CLIENT = new();

        public MainWindow()
        {
            InitializeComponent();
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
                    SearchBox.Text = openDialog.FileName.ToString();   //文件路径
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }
        private async void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在搜索图片";

            try { await PickInput(); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "图片搜索完成";
        }
        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在下载图片";

            try
            {
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
                    throw new Exception("Unexpected Extension");

                bitmapEncoder.Frames.Add(BitmapFrame.Create((BitmapSource)ShowImage.Source!));
                using FileStream imageFileStream = new(saveDialog.FileName.ToString(), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                bitmapEncoder.Save(imageFileStream);
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "图片下载完成";
        }
        private async void LuckyButton_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在获取图片";

            try
            {
                //将Json转换为JObject
                JObject LuckyImageJObject = JObject.Parse(await Http.GetAsync<string>(@"https://api.lolicon.app/setu/v2?r18=2&proxy=null", MAIN_CLIENT));

                //提取并运行
                SearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();
                await PickInput();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "图片获取完成";
        }
        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AboutWindow aboutWindow = new();
                aboutWindow.ShowDialog();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            try
            {
                if (SearchBox.Text == "")
                {
                    SearchButton.IsEnabled = false;
                    DownloadButton.IsEnabled = false;
                }
                else
                {
                    SearchButton.IsEnabled = true;
                    DownloadButton.IsEnabled = true;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        private async Task PickInput()
        {
            Title = "正在识别输入";

            try
            {
                SearchBox.IsEnabled = false;
                SearchButton.IsEnabled = false;
                DownloadButton.IsEnabled = false;

                if (Regex.IsMatch(SearchBox.Text, "^([0-9]*)-?[0-9]*$"))
                    await IsPixivID();  //Pixiv ID
                else if (new Regex(Define.URI_REGEX).IsMatch(SearchBox.Text))
                    await IsUri();  //Uri
                else if (File.Exists(SearchBox.Text))
                    await IsFilePath(); //文件路径
                else
                    await IsKeyWord();  //关键词
            }
            catch { throw; }
            finally
            {
                SearchBox.IsEnabled = true;
                SearchButton.IsEnabled = true;
                DownloadButton.IsEnabled = true;
            }

            Title = "输入识别完成";
        }
        private async Task IsUri()
        {
            Title = "正在解析链接";

            if (SearchBox.Text.Contains(@"www.pixiv.net/artworks")) //Pixiv Url
            {
                Exception exception = new();
                foreach (string fileSuffix in Define.FILE_SUFFIXES)
                {
                    try
                    {
                        await GetImage(SearchBox.Text.Replace(@"www.pixiv.net/artworks", @"pixiv.re") + fileSuffix);

                        Title = "链接解析完成";

                        return;
                    }
                    catch (Exception ex) { exception = ex; }
                }
                throw exception;
            }
            else if (SearchBox.Text.Contains(@"www.pixiv.net/member_illust.php?") && SearchBox.Text.Contains("illust_id"))  //Pixiv Illust Url
            {
                NameValueCollection paramCollection = GetParamCollection(new Uri(SearchBox.Text).Query);

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
                await GetImage(SearchBox.Text.Replace(@"pximg.net", @"pixiv.re"));
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
                    await GetImage(@"https://pixiv.re/" + SearchBox.Text + fileSuffix);

                    Title = "PixivID解析完成";

                    return;
                }
                catch (Exception ex) { exception = ex; }
            }
            throw exception;
        }
        private async Task IsFilePath()
        {
            Title = "正在解析文件";

            //将Json转换为JObject
            JObject SmmsJObject = JObject.Parse(UploadSmms(SearchBox.Text));

            if (!(bool)SmmsJObject["success"]!)
                throw new Exception(SmmsJObject["code"]!.ToString());

            //提取
            string imageUrl = SmmsJObject["data"]!["url"]!.ToString();
            string deleteUrl = SmmsJObject["data"]!["delete"]!.ToString();

            try
            {
                var sauceNETClient = new SauceNETClient(Secret.GetSauceNaoApiKey());
                //获取相似图片链接
                var sauce = await sauceNETClient.GetSauceAsync(imageUrl);
                //填充其中最相似的图片链接
                Dispatcher.Invoke(() => { SearchBox.Text = sauce.Results[0].SourceURL; });

                await IsUri();
            }
            catch { throw; }
            finally
            {
                try { (await Http.GetAsync<HttpResponseMessage>(deleteUrl, MAIN_CLIENT)).EnsureSuccessStatusCode(); }
                catch { MessageBox.Show("Error: 高危错误，图片清理失败，请尽快向开发者汇报此错误，谢谢配合！"); }
            }

            Title = "文件解析完成";
        }
        private async Task IsKeyWord()
        {
            Title = "正在解析关键词";

            //将Json转换为JObject
            JObject LuckyImageJObject = JObject.Parse(await Http.GetAsync<string>($@"https://api.lolicon.app/setu/v2?r18=2&proxy=null&tag={SearchBox.Text}", MAIN_CLIENT));

            //提取并运行
            SearchBox.Text = LuckyImageJObject["data"]![0]!["urls"]!["original"]!.ToString();
            await PickInput();

            Title = "关键词解析完成";
        }

        private string UploadSmms(string filePath)
        {
            Title = "正在上传图片";

            using Process SmmsProcess = new();
            SmmsProcess.StartInfo.FileName = @"Ona-Pix-Smms.exe";
            SmmsProcess.StartInfo.Arguments = filePath;
            SmmsProcess.StartInfo.UseShellExecute = false;
            SmmsProcess.StartInfo.RedirectStandardInput = true;
            SmmsProcess.StartInfo.RedirectStandardOutput = true;
            SmmsProcess.StartInfo.CreateNoWindow = true;
            SmmsProcess.Start();
            SmmsProcess.WaitForExit();

            Title = "图片上传完成";

            return SmmsProcess.StandardOutput.ReadToEnd();
        }

        private async Task GetImage(string imageUri)
        {
            Title = "正在获取图片";

            HttpResponseMessage imageMessage = await Http.GetAsync<HttpResponseMessage>(imageUri, MAIN_CLIENT, HttpCompletionOption.ResponseContentRead);
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
            ShowImage.Margin = new Thickness(0, 0, 10, 10);
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

        ////检测文本编码
        //private static Encoding MyUrlDeCode(string str)
        //{
        //    //将原文本与用utf-8解码再编码的文本对比
        //    return str == HttpUtility.UrlEncode(HttpUtility.UrlDecode(str.ToUpper(), Encoding.UTF8), Encoding.UTF8) ? Encoding.UTF8 : Encoding.GetEncoding("gb2312");
        //}
    }
}