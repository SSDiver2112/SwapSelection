using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Media;

namespace SwapSelection
{
    [ToolboxItem(false)]
    public class ColorListBox : ListBox
    {

        private int _MouseIndex = -1;
        private IWindowsFormsEditorService m_EditorService;

        public ColorListBox(IWindowsFormsEditorService editor_service) : base()
        {
            DrawMode = DrawMode.OwnerDrawFixed;
            ItemHeight = 18;
            m_EditorService = editor_service;
            var colo = typeof(Colors).GetProperties();
            Click += ColorListBox_Click;
            DrawItem += ColorListBox_DrawItem;
            MouseMove += ColorListBox_MouseMove;

            foreach (System.Reflection.PropertyInfo citem in colo)
            {
                Items.Add(citem.Name);
            }
        }

        private void ColorListBox_Click(object sender, System.EventArgs args)
        {
            if (m_EditorService != null)
            {
                m_EditorService.CloseDropDown();
            }
        }

        private void ColorListBox_MouseMove(object sender, MouseEventArgs e)
        {
            int index = IndexFromPoint(e.Location);
            if (index != _MouseIndex)
            {
                _MouseIndex = index;
                Invalidate();
            }
        }
        private void ColorListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index > -1)
            {
                using (var pn = new System.Drawing.Pen(System.Drawing.Color.Black, 1))
                {
                    e.DrawBackground();
                    e.Graphics.FillRectangle(new System.Drawing.SolidBrush(System.Drawing.Color.FromName(this.Items[e.Index].ToString())),
                        new System.Drawing.Rectangle(e.Bounds.X, e.Bounds.Y + 1, 20, e.Bounds.Height - 2));
                    e.Graphics.DrawRectangle(pn, new System.Drawing.Rectangle(e.Bounds.X, e.Bounds.Y + 1, 20, e.Bounds.Height - 2));
                    TextRenderer.DrawText(e.Graphics, this.Items[e.Index].ToString(), Font,
                        new System.Drawing.Rectangle(e.Bounds.X + 20, e.Bounds.Y, e.Bounds.Width - 20, e.Bounds.Height),
                        System.Drawing.Color.Black, TextFormatFlags.Left);
                    if (e.Index == _MouseIndex)
                    {
                        pn.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                        e.Graphics.DrawRectangle(pn, new System.Drawing.Rectangle(e.Bounds.X + 23, e.Bounds.Y, e.Bounds.Width - 26, e.Bounds.Height - 1));
                    }
                }
            }
        }
    }

}
