using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;

namespace Ona_Pix.Pages
{
    public partial class AppearancePage : UserControl
    {
        public AppearancePage()
        {
            InitializeComponent();
        }

        private void DarkModeToggle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ((BundledTheme)Application.Current.Resources.MergedDictionaries[0]).BaseTheme =
                DarkModeToggle.IS_TOGGLED ? BaseTheme.Dark : BaseTheme.Light;

            SetButtonContent();
        }
        private void IconButtonToggle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            SetButtonContent();
        }
        private void LockAnimationToggle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (LockAnimationToggle.IS_TOGGLED)
                Define.MAIN_WINDOW!.ActiveSpace_MouseIn(this, null!);
            else
                Define.MAIN_WINDOW!.ActiveSpace_MouseOut(this, null!);
        }
        private void HideBorderToggle_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Define.MAIN_WINDOW!.SizeToContent = SizeToContent.Manual;

            if (HideBorderToggle.IS_TOGGLED)
            {
                MessageBox.Show("Warning: 开启后可能导致主窗口无法正常关闭，请记住关闭主窗口的热键 Ctrl+W");
                Define.MAIN_WINDOW!.WindowStyle = WindowStyle.None;
            }
            else
                Define.MAIN_WINDOW!.WindowStyle = WindowStyle.SingleBorderWindow;

            Define.MAIN_WINDOW!.SizeToContent = SizeToContent.WidthAndHeight;
        }

        private void SetButtonContent()
        {
            if (IconButtonToggle.IS_TOGGLED)
            {
                if (!DarkModeToggle.IS_TOGGLED)
                {
                    Define.MAIN_WINDOW!.ActiveViewButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.View.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveSearchButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Search.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveDownloadButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Download.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveLuckyButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Lucky.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveSettingButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Setting.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveAboutButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.About.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                }
                else
                {
                    Define.MAIN_WINDOW!.ActiveViewButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.View_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveSearchButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Search_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveDownloadButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Download_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveLuckyButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Lucky_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveSettingButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.Setting_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                    Define.MAIN_WINDOW!.ActiveAboutButton.Content = new Image()
                    {
                        Source = Imaging.CreateBitmapSourceFromHBitmap(Properties.Resources.About_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                    };
                }

                Define.MAIN_WINDOW!.ActiveViewButton.ToolTip = "浏览";
                Define.MAIN_WINDOW!.ActiveSearchButton.ToolTip = "搜索";
                Define.MAIN_WINDOW!.ActiveDownloadButton.ToolTip = "下载";
                Define.MAIN_WINDOW!.ActiveLuckyButton.ToolTip = "一图";
                Define.MAIN_WINDOW!.ActiveSettingButton.ToolTip = "设置";
                Define.MAIN_WINDOW!.ActiveAboutButton.ToolTip = "关于";
            }
            else
            {
                Define.MAIN_WINDOW!.ActiveViewButton.Content = "浏览";
                Define.MAIN_WINDOW!.ActiveSearchButton.Content = "搜索";
                Define.MAIN_WINDOW!.ActiveDownloadButton.Content = "下载";
                Define.MAIN_WINDOW!.ActiveLuckyButton.Content = "一图";
                Define.MAIN_WINDOW!.ActiveSettingButton.Content = "设置";
                Define.MAIN_WINDOW!.ActiveAboutButton.Content = "关于";

                Define.MAIN_WINDOW!.ActiveViewButton.ToolTip = null;
                Define.MAIN_WINDOW!.ActiveSearchButton.ToolTip = null;
                Define.MAIN_WINDOW!.ActiveDownloadButton.ToolTip = null;
                Define.MAIN_WINDOW!.ActiveLuckyButton.ToolTip = null;
                Define.MAIN_WINDOW!.ActiveSettingButton.ToolTip = null;
                Define.MAIN_WINDOW!.ActiveAboutButton.ToolTip = null;
            }
        }
    }
}