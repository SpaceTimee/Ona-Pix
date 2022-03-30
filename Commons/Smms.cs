using System;
using System.Diagnostics;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using OnaCore;
using OnaPixSecret;
using SauceNET;
using SheasCore;

namespace Ona_Pix
{
    internal class Smms : Proc
    {
        internal event Define.SET_WINDOW_HANDLER_P? SetImageUrl, SetMainWindowTitle, ShowError;
        internal event Define.SET_WINDOW_HANDLER? SetControlsEnabled;

        public Smms() : base("Ona-Pix-Smms.exe")
        {
            //Control.CheckForIllegalCrossThreadCalls = false;
        }

        //Smms程序调用事件
        public override async void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            //输出事件(输出结果)
            try
            {
                if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
                {
                    //将Json转换为JObject
                    JObject SmmsJObject = JObject.Parse(e.Data!);
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
                        SetImageUrl!(sauce.Results[0].SourceURL);
                    }
                    catch { throw; }
                    finally
                    {
                        try { (await Http.GetAsync<HttpResponseMessage>(deleteUrl, Define.MAIN_CLIENT)).EnsureSuccessStatusCode(); }
                        catch { MessageBox.Show("Error: 图片清理失败，该图片暂时被锁定，请尽快向开发者汇报此错误，谢谢配合！"); }
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); SetMainWindowTitle!("操作执行失败"); return; }
        }
        public override void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            //报错事件(输出错误)
            if (!string.IsNullOrEmpty(e.Data) && !string.IsNullOrWhiteSpace(e.Data))
            {
                ShowError!(e.Data);
                SetMainWindowTitle!("操作执行失败");
            }
        }
        public override void Process_Exited(object sender, EventArgs e)
        {
            //退出事件
            SetControlsEnabled!();
        }
    }
}