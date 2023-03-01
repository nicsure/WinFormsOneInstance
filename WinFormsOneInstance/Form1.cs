namespace WinFormsOneInstance
{
    /// <summary>
    ///  Example implementation of a Form used with OneInstance as the application's main form.
    /// </summary>
    public partial class Form1 : Form, IMainForm // Form must implement IMainForm
    {
        static decimal myValue = 50; // static value used with the example NumericUpDown Control on the form
        private FormWindowState lastWindowState = FormWindowState.Normal;

        static Form1()
        {
            OneInstance.Signal(); // required to show the form when the application is first started (Optional)
        }

        public Form1()
        {
            InitializeComponent();
        }

        // IMainForm.IsDead interface method implementation, returns if the Form has been disposed
        public bool IsDead => IsDisposed;

        // IMainForm.CloseDown interface method implementation, closes and disposed the Form
        public void CloseDown()
        {
            if (!IsDead) Close(); // check if form is not already disposed, if not just call Close();
        }

        // IMainForm.Open interface method implementation, shows the new Form to the user and perform initialization.
        public void Open()
        {
            numericUpDown1.Value = myValue; // set the example controls's value to our static value
            Show();
        }

        // IMainForm.Restore interface method implementation, shows an already open Form to the user (in case it's behind other windows or minimized)
        public void Restore()
        {
            if (!IsDead) // check the Form is not disposed
            {
                if (WindowState == FormWindowState.Minimized)
                    WindowState = lastWindowState; // unminimize Form
                BringToFront(); // Bring Form to front.
            }
        }

        private void NumericUpDown1_ValueChanged(object sender, EventArgs e) // fires when the example control's value is changed
        {
            myValue = numericUpDown1.Value; // update our static value to the new value of the example control.
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Minimized)
                lastWindowState = WindowState;
        }
    }
}