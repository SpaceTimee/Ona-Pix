using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Ona_Pix.Props;
using Ona_Pix.Utils;
using static Ona_Pix.Props.Resources;

namespace Ona_Pix.Pages;

public partial class AppearancePage : UserControl
{
    public AppearancePage() => InitializeComponent();

    //Toggle 点击事件
    internal void DarkModeToggle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //暗色模式
        if (DarkModeToggle.IS_TOGGLED)
        {
            ((BundledTheme)Application.Current.Resources.MergedDictionaries[0]).BaseTheme = BaseTheme.Dark;
            Settings.Default.IsDarkMode = true;
        }
        else
        {
            ((BundledTheme)Application.Current.Resources.MergedDictionaries[0]).BaseTheme = BaseTheme.Light;
            Settings.Default.IsDarkMode = false;
        }

        Settings.Default.Save();

        SetButtonContent();
    }
    internal void IconButtonToggle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //按钮图标
        if (IconButtonToggle.IS_TOGGLED)
            Settings.Default.IsIconButton = true;
        else
            Settings.Default.IsIconButton = false;

        Settings.Default.Save();

        SetButtonContent();
    }
    internal void LockAnimationToggle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //固定菜单
        if (LockAnimationToggle.IS_TOGGLED)
        {
            Define.MAIN_WINDOW!.ActiveSpace_MouseIn(this, null!);
            Settings.Default.IsAnimationLocked = true;
        }
        else
        {
            Define.MAIN_WINDOW!.ActiveSpace_MouseOut(this, null!);
            Settings.Default.IsAnimationLocked = false;
        }

        Settings.Default.Save();
    }
    private void HideBorderToggle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        //隐藏边框
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
    private void OpacitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Define.MAIN_WINDOW != null)
        {
            Define.MAIN_WINDOW!.ActiveRightBorder.Opacity = Define.MAIN_WINDOW!.ActiveTopBorder.Opacity = 1 - OpacitySlider.Value / 100;

            Settings.Default.MenuOpacity = OpacitySlider.Value;
            Settings.Default.Save();
        }
    }

    //为按钮应用暗色模式和图标按钮
    private void SetButtonContent()
    {
        if (IconButtonToggle.IS_TOGGLED)
        {
            if (!DarkModeToggle.IS_TOGGLED)
            {
                Define.MAIN_WINDOW!.ActiveViewButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(View.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveSearchButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Search.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveDownloadButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Download.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveLuckyButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Lucky.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveSettingButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Setting.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveAboutButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(About.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
            }
            else
            {
                Define.MAIN_WINDOW!.ActiveViewButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(View_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveSearchButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Search_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveDownloadButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Download_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveLuckyButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Lucky_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveSettingButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(Setting_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
                };
                Define.MAIN_WINDOW!.ActiveAboutButton.Content = new Image()
                {
                    Source = Imaging.CreateBitmapSourceFromHBitmap(About_Dark.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
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

            Define.MAIN_WINDOW!.ActiveViewButton.ToolTip =
            Define.MAIN_WINDOW!.ActiveSearchButton.ToolTip =
            Define.MAIN_WINDOW!.ActiveDownloadButton.ToolTip =
            Define.MAIN_WINDOW!.ActiveLuckyButton.ToolTip =
            Define.MAIN_WINDOW!.ActiveSettingButton.ToolTip =
            Define.MAIN_WINDOW!.ActiveAboutButton.ToolTip = null;
        }
    }
}