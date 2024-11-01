using System.Windows.Controls;
using System.Windows.Input;
using Ona_Pix.Props;
using Ona_Pix.Utils;

namespace Ona_Pix.Pages
{
    public partial class BehaviorPage : UserControl
    {
        public BehaviorPage() => InitializeComponent();

        //Toggle 点击事件
        internal void DisableR18Toggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //禁用瑟瑟
            if (DisableR18Toggle.IS_TOGGLED)
            {
                Define.R18 = '0';
                Settings.Default.IsR18Disabled = true;
            }
            else
            {
                Define.R18 = '2';
                Settings.Default.IsR18Disabled = false;
            }

            Settings.Default.Save();
        }
        internal void PixivCatToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //使用 PixivCat 接口
            if (PixivCatToggle.IS_TOGGLED)
                Settings.Default.IsPixivCat = true;
            else
                Settings.Default.IsPixivCat = false;

            Settings.Default.Save();
        }
        internal void DisableExceptionToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //禁用报错
            if (DisableExceptionToggle.IS_TOGGLED)
                Settings.Default.IsExceptionDisabled = true;
            else
                Settings.Default.IsExceptionDisabled = false;

            Settings.Default.Save();
        }
        internal void DisableTipsToggle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //禁用一言
            if (DisableTipsToggle.IS_TOGGLED)
                Settings.Default.IsTipsDisabled = true;
            else
                Settings.Default.IsTipsDisabled = false;

            Settings.Default.Save();
        }
    }
}