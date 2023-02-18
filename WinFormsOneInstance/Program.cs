namespace WinFormsOneInstance
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            // The following line replaces the original
            // Application.Run(Form1);
            OneInstance.Create(typeof(Form1), typeof(CustomControl1), "OneInstance08476060387456028735632");
        }
    }
}