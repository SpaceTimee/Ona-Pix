using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using OnaCore;

namespace Ona_Pix.Wins
{
    public partial class AboutWin
    {
        private readonly NameValueCollection RELEASE_LIST = [];
        private readonly HttpClient GITHUB_CLIENT = new();

        private void PartialAboutWindow()
        {
            GITHUB_CLIENT.DefaultRequestHeaders.Add("Accept", Define.GITHUB_RELEASE_API_ACCEPT_HEADER);
            GITHUB_CLIENT.DefaultRequestHeaders.Add("User-Agent", Define.GITHUB_RELEASE_API_USER_AGENT_HEADER);
        }

        //更新交互事件
        private async void UpdateLink_Click(object sender, RoutedEventArgs e)
        {
            //用户点击关于页面的版本号后触发
            Title = "正在获取更新";

            JObject releaseJObject = JObject.Parse(await Http.GetAsync<string>(Define.GITHUB_RELEASE_API_URL, GITHUB_CLIENT));

            if (Define.CURRENT_VERSION != releaseJObject["name"]!.ToString())
            {
                Title = "有可用更新";

                MessageBoxResult messageBoxResult = MessageBox.Show("有可用更新，是否自动更新 (否: 手动更新)", string.Empty, MessageBoxButton.YesNoCancel);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    //记录最新版的 Github Release 中的所有文件并列出
                    foreach (JObject releaseList in releaseJObject["assets"]!.Cast<JObject>())
                        RELEASE_LIST.Add(releaseList["name"]!.ToString(), releaseList["browser_download_url"]!.ToString());

                    ReleaseListBox.ItemsSource = RELEASE_LIST.Keys;

                    //调整窗口最小宽度
                    Dispatcher.Invoke(new Action(() => { MinWidth = 250 + ReleaseListBoxColumn.ActualWidth; }), System.Windows.Threading.DispatcherPriority.Loaded);

                    Title = "请选择更新文件";

                    //接下来的正常流程: 等待用户选择需要下载的文件后转至 ReleaseListBox_SelectionChanged()
                }
                else if (messageBoxResult == MessageBoxResult.No)
                    HyperLink_Click(LanzouLink, new RoutedEventArgs());
            }
            else
                Title = "当前是最新版本";
        }
        private async void ReleaseListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //用户选择需要下载的文件后触发
            try
            {
                Title = "正在下载更新";

                ReleaseListBox.IsEnabled = UpdateLink.IsEnabled = false;

                byte[] ReleaseBytes = await Http.GetAsync<byte[]>(RELEASE_LIST[(string)ReleaseListBox.SelectedItem], GITHUB_CLIENT);

                Title = "正在保存更新";

                new DirectoryInfo(Define.CACHE_PATH).Create();  //创建文件夹
                using (FileStream fileStream = new(Path.Combine(Define.CACHE_PATH, (string)ReleaseListBox.SelectedItem),
                    FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                    fileStream.Write(ReleaseBytes, 0, ReleaseBytes.Length);

                Title = "正在打开更新";

                Define.StartProcess(Path.Combine(Define.CACHE_PATH, (string)ReleaseListBox.SelectedItem));
            }
            catch { throw; }
            finally { Close(); }
        }
    }
}