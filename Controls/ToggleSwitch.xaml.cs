using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Ona_Pix.Controls
{
    public partial class ToggleSwitch : UserControl
    {
        public bool IS_TOGGLED = false;

        private readonly Thickness LEFT_SIDE_MARGIN = new(-11, 0, 0, 0);
        private readonly Thickness RIGHT_SIDE_MARGIN = new(0, 0, -11, 0);
        private readonly SolidColorBrush OFF_COLOR_BRUSH = new(Color.FromRgb(160, 160, 160));
        private readonly SolidColorBrush ON_COLOR_BRUSH = new(Color.FromRgb(33, 150, 243));

        public ToggleSwitch()
        {
            InitializeComponent();
        }

        //点击事件
        private void ForeDot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SwitchStatus();
        }
        private void BackRec_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SwitchStatus();
        }

        //切换Toggle状态
        internal void SwitchStatus()
        {
            if (!IS_TOGGLED)
            {
                BackRec.Fill = ON_COLOR_BRUSH;
                ForeDot.Margin = RIGHT_SIDE_MARGIN;
            }
            else
            {
                BackRec.Fill = OFF_COLOR_BRUSH;
                ForeDot.Margin = LEFT_SIDE_MARGIN;
            }

            IS_TOGGLED = !IS_TOGGLED;
        }
    }
}