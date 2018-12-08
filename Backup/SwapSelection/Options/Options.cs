using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Media;

namespace SwapSelection
{
    public class Options : DialogPage
    {
        [Category("General")]
        [DisplayName("Arrow Color")]
        [Description("Set the Color of the Arrow")]
        [DefaultValue(typeof(Color), "Yellow")]
        [Editor(typeof(mColorEditor), typeof(UITypeEditor))]
        public string ArrowColor { get; set; } = "Yellow";

        [Category("General")]
        [DisplayName("Adornment Type")]
        [Description("Set the Type of Visual Adornment 1-Curved Arrow 2-Square Arrow 3-ArrowHead")]
        [DefaultValue(3)]
        public int AdornmentType { get; set; } = 3;

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (AdornmentType > 3)
                AdornmentType = 3;
            else if (AdornmentType < 1)
                AdornmentType = 1;

            base.OnApply(e);
        }
    }

}
