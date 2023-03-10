using System.Reflection;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using ToolTip = System.Windows.Controls.ToolTip;

namespace Media_Player.Utils
{
    public class MySlider : Slider
    {
        private ToolTip _autoToolTip;

        protected override void OnThumbDragStarted(DragStartedEventArgs e)
        {
            base.OnThumbDragStarted(e);
            _formatAutoToolTipContent();
            AutoToolTip.Content = "=))";
        }

        protected override void OnThumbDragDelta(DragDeltaEventArgs e)
        {
            base.OnThumbDragDelta(e);
            _formatAutoToolTipContent();
        }

        private void _formatAutoToolTipContent()
        {
            string? rawStr = AutoToolTip.Content.ToString();
            if (rawStr == null) return;
            int rawVal = int.Parse(rawStr);
            AutoToolTip.Content = TimeUtils.FormatTime(rawVal);
        }

        private ToolTip AutoToolTip
        {
            get
            {
                if (_autoToolTip == null)
                {
                    FieldInfo field = typeof(Slider).GetField(
                        "_autoToolTip", BindingFlags.NonPublic | BindingFlags.Instance);
                    _autoToolTip = field.GetValue(this) as ToolTip;
                }
                return _autoToolTip;
            }
        }
    }
}
