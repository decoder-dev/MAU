using System.Windows.Forms;

namespace NetworkMonitoringSystem.Helpers
{
    public static class ToolTipHelper
    {
        private static ToolTip? _toolTip;

        public static ToolTip GetToolTip()
        {
            if (_toolTip == null)
            {
                _toolTip = new ToolTip
                {
                    IsBalloon = false,
                    ToolTipTitle = "Подсказка",
                    ToolTipIcon = ToolTipIcon.Info,
                    AutoPopDelay = 5000,
                    InitialDelay = 500,
                    ReshowDelay = 100
                };
            }
            return _toolTip;
        }

        public static void SetToolTip(Control control, string text)
        {
            GetToolTip().SetToolTip(control, text);
        }
    }
}

