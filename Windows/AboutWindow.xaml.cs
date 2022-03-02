using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Newtonsoft.Json.Linq;
using OnaCore;

namespace Ona_Pix
{
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void AboutWin_Loaded(object sender, RoutedEventArgs e)
        {
            try { UpdateRun.Text = "版本号: " + Assembly.GetExecutingAssembly().GetName().Version!.ToString()[0..^2]; }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }
        }

        private void DeveloperLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在打开Github";

            try { Define.StartProcess(DeveloperLink.NavigateUri.ToString()); }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "Github打开完成";
        }
        private async void UpdateLink_Click(object sender, RoutedEventArgs e)
        {
            Title = "正在执行更新";

            try
            {
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("Accept", Define.ACCEPT_HEADER);
                httpClient.DefaultRequestHeaders.Add("User-Agent", Define.USER_AGENT_HEADER);

                Title = "正在获取更新";

                JObject releaseJObject = JObject.Parse(await Http.GetAsync<string>(Define.GITHUB_RELEASE_API_URL, httpClient));

                Title = "更新获取完成";

                if (Assembly.GetExecutingAssembly().GetName().Version!.ToString()[0..^2] != releaseJObject["name"]!.ToString())
                {
                    Title = "有可用更新";
                    MessageBoxResult messageBoxResult = MessageBox.Show("有可用更新，是否自动更新(否:手动更新)", "", MessageBoxButton.YesNoCancel);
                    if (messageBoxResult == MessageBoxResult.Yes)
                    {
                        Title = "正在下载更新";

                        string releaseUrl = releaseJObject["assets"]![0]!["browser_download_url"]!.ToString().Contains("Setup") ?
                            releaseJObject["assets"]![0]!["browser_download_url"]!.ToString() : releaseJObject["assets"]![1]!["browser_download_url"]!.ToString();

                        byte[] bytes = await Http.GetAsync<byte[]>(releaseUrl, HttpCompletionOption.ResponseContentRead);

                        Title = "更新下载完成";

                        Title = "正在保存更新";

                        using FileStream saver = new(Environment.GetFolderPath(
                            Environment.SpecialFolder.MyDocuments) + @"\Ona Pix Setup.exe",
                            FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);

                        saver.Write(bytes, 0, bytes.Length);

                        Title = "更新保存完成";

                        Title = "正在打开更新";

                        Define.StartProcess(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Ona Pix Setup.exe");

                        Title = "更新打开完成";
                    }
                    else if (messageBoxResult == MessageBoxResult.No)
                    {
                        Title = "正在打开蓝奏云";

                        MessageBox.Show("密码: ddvs");
                        Define.StartProcess(LanzouLink.NavigateUri.ToString());

                        Title = "蓝奏云打开完成";
                    }
                }
                else
                    Title = "当前是最新版本";
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); Title = "操作执行失败"; return; }

            Title = "更新执行完成";
        }
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