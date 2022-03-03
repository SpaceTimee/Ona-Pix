using System;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Windows;
using Newtonsoft.Json.Linq;
using OnaCore;

namespace Ona_Pix
{
    public partial class AboutWindow
    {
        private readonly NameValueCollection RELEASE_LIST = new();
        private readonly HttpClient GITHUB_CLIENT = new();

        private void PartialAboutWindow()
        {
            GITHUB_CLIENT.DefaultRequestHeaders.Add("Accept", Define.ACCEPT_HEADER);
            GITHUB_CLIENT.DefaultRequestHeaders.Add("User-Agent", Define.USER_AGENT_HEADER);
        }

        private async void UpdateLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Title = "正在获取更新";

                JObject releaseJObject = JObject.Parse(await Http.GetAsync<string>(Define.GITHUB_RELEASE_API_URL, GITHUB_CLIENT));

                if (Define.CURRENT_VERSION != releaseJObject["name"]!.ToString())
                {
                    Title = "有可用更新";

                    MessageBoxResult messageBoxResult = MessageBox.Show("有可用更新，是否自动更新 (否: 手动更新)", "", MessageBoxButton.YesNoCancel);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        foreach (var releaseList in releaseJObject["assets"]!)
                            RELEASE_LIST.Add(releaseList["name"]!.ToString(), releaseList["browser_download_url"]!.ToString());

                        ReleaseListBox.ItemsSource = RELEASE_LIST.Keys;

                        //调整窗口最小宽度
                        Dispatcher.Invoke(new Action(() =>
                        {
                            MinWidth = 250 + ReleaseListBoxColumn.ActualWidth;
                        }), System.Windows.Threading.DispatcherPriority.Loaded);

                        Title = "请选择更新文件";

                        //接下来的正常流程: 等待用户反馈后转至 ReleaseListBox_SelectionChanged()
                    }
                    else if (messageBoxResult == MessageBoxResult.No)
                        LanzouLink_Click(UpdateLink, new RoutedEventArgs());
                }
                else
                    Title = "当前是最新版本";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; Close(); }
        }
        private async void ReleaseListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Title = "继续执行更新";

            try
            {
                Title = "正在下载更新";

                ReleaseListBox.IsEnabled = false;
                UpdateLink.IsEnabled = false;

                byte[] ReleaseBytes = await Http.GetAsync<byte[]>(RELEASE_LIST[(string)ReleaseListBox.SelectedItem], GITHUB_CLIENT);

                Title = "正在保存更新";

                new DirectoryInfo(Define.CACHE_PATH).Create();
                FileStream fileStream = new(
                    Path.Combine(Define.CACHE_PATH, (string)ReleaseListBox.SelectedItem),
                    FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                fileStream.Write(ReleaseBytes, 0, ReleaseBytes.Length);
                fileStream.Close();

                Title = "正在打开更新";

                Define.StartProcess(Path.Combine(Define.CACHE_PATH, (string)ReleaseListBox.SelectedItem));
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
            finally { Close(); }
        }
    }
}