using System.Drawing.Design;
using System.Windows.Forms.Design;

namespace SwapSelection
{
    [System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
    public class mColorEditor : System.Drawing.Design.UITypeEditor
    {
        public mColorEditor()
        {
        }

        public override System.Drawing.Design.UITypeEditorEditStyle GetEditStyle(System.ComponentModel.ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(System.ComponentModel.ITypeDescriptorContext context, System.IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (edSvc != null)
            {
                ColorListBox ColorControl = new ColorListBox(edSvc);
                edSvc.DropDownControl(ColorControl);
                if (ColorControl.Text != string.Empty) value = ColorControl.Text;
            }
            return value;
        }

        public override void PaintValue(System.Drawing.Design.PaintValueEventArgs e)
        {
            using (System.Drawing.Brush br = new System.Drawing.SolidBrush(System.Drawing.Color.FromName(e.Value.ToString())))
            {
                e.Graphics.FillRectangle(br, e.Bounds);
            }
        }

        public override bool GetPaintValueSupported(System.ComponentModel.ITypeDescriptorContext context)
        {
            return true;
        }
    }

}
