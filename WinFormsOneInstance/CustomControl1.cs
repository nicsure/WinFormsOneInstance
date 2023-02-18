using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinFormsOneInstance
{
    /// <summary>
    /// Example implementation of a Custom Control used with OneInstance
    /// </summary>
    public partial class CustomControl1 : Control, IIconControl // Must implement IIconControl
    {
        public CustomControl1()
        {
            InitializeComponent();
        }

        // IIconControl.CloseDown interface method implementation
        public void CloseDown()
        {
            // first check if the Control is not already Disposed, if not then just call Dispose()
            // This is needed to remove the icon from the notification area of the desktop.
            if (!IsDisposed) Dispose();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
        }

        private void ToolStripMenuItemOpen_Click(object sender, EventArgs e) // fires when the user clicks the Open item in the notify icon's context menu
        {
            OneInstance.Signal(); // send a signal to open/show the main Form window
        }

        private void ToolStripMenuItemQuit_Click(object sender, EventArgs e) // fires when the user clicks the Quit item in the notify icon's context menu
        {
            OneInstance.Quit(); // send a request to shut everything down
        }

        private void NotifyIcon1_DoubleClick(object sender, EventArgs e) // fires when the user double clicks the notify icon, acts the same way as the Open menu item
        {
            OneInstance.Signal(); // send a signal to open/show the main Form window
            // This method is functionally identical to ToolStripMenuItemOpen_Click and you can indeed point the DoubleClick event to that method instead of
            // creating another one like this. I left this in for illustrative purposes.
        }
    }
}
