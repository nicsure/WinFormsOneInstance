using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace WinFormsOneInstance
{
    /// <summary>
    /// A static class that deals with the handling of single instance WinForms applications.<br/>
    /// To use, replace the Application.Run(Form) call in Program.cs with the Create method of this class.<br/><br/>
    /// You will need two classes<br/><br/>
    /// A class that extends Form and implements OneInstance.IMainForm. This is your application's main form.<br/>
    /// A CustomControl class containing a NotififyIcon (with a ContextMenuStrip probably) that implements OneInstance.IIconControl<br/><br/>
    /// Note: By default the main form will NOT appear on startup. To make the form appear when the application is first started you should call OneInstance.Signal() from the static constructor of the Form.<br/>
    /// Note: If the application is already running and another attempt to open the application occurs, the second instance will terminate and the running instance will display its main Form
    /// </summary>
    public static class OneInstance
    {
        private static Type uiType = typeof(object);
        private static IIconControl? iconInstance = null;
        private static IMainForm? uiInstance = null;
        private static EventWaitHandle? waitHandle = null;
        private static readonly object msgSync = new();
        private static bool quit = false;

        /// <summary>
        /// Use this method in Program.cs in place of Application.Run(Form)<br/>
        /// Example: OneInstance.Create(typeof(Form1), typeof(CustomControl1), "some-random-text");
        /// </summary>
        /// <param name="mainFormType">The Type of the application's main form. This Type must be of a class that implements OneInstance.IMainForm</param>
        /// <param name="iconControlType">The type of the CustomControl that holds the notification icon. This type must be of a class that implements OneInstance.IIconControl</param>
        /// <param name="guid">A unique string to identify this Application (can be anything really)</param>
        public static void Create(Type mainFormType, Type iconControlType, string guid)
        {
            uiType = mainFormType;
            waitHandle = new(false, EventResetMode.ManualReset, guid, out bool isNew);
            if (isNew)
            {
                if (uiType.GetInterface("IMainForm") != null)
                {
                    if (Activator.CreateInstance(iconControlType) is IIconControl control)
                    {
                        iconInstance = control;
                        SignalLoop();
                        RuntimeHelpers.RunClassConstructor(uiType.TypeHandle);
                        Application.Run();
                    }
                }
            }
            else
                Signal();
        }

        private static async void SignalLoop()
        {
            _ = Task.Run(() => 
            {
                while (waitHandle?.WaitOne() ?? false && !quit)
                {
                    waitHandle?.Reset();
                    Pulse();
                }
                quit = true;
                Pulse();
            });
            while (true)
            { 
                await Task.Run(Hold);
                if (quit) break; 
                if (uiInstance == null || uiInstance.IsDead) 
                {
                    object? obj = Activator.CreateInstance(uiType); 
                    if (obj is not null and IMainForm) 
                    {
                        uiInstance = (IMainForm)obj; 
                        uiInstance.Open(); 
                    }
                }
                else uiInstance?.Restore(); 
            }
            Exit();
        }

        /// <summary>
        /// Call this when you want the application to exit
        /// </summary>
        public static void Quit()
        {
            quit = true;
            Signal();
            EmergencyExit();
        }

        /// <summary>
        /// Call this when you want the application's main form to appear
        /// </summary>
        public static void Signal()
        {
            if (waitHandle != null)
            {
                Task.Run(() =>
                {
                    EventWaitHandle.SignalAndWait(waitHandle, waitHandle, 100, true);
                }).Wait();
            }
        }

        private static async void EmergencyExit()
        {
            await Task.Delay(5000);
            Application.Exit();
        }

        private static void Exit()
        {
            iconInstance?.CloseDown();
            uiInstance?.CloseDown();
            Application.Exit();
        }

        private static void Pulse() { lock (msgSync) Monitor.Pulse(msgSync); }

        private static void Hold() { lock (msgSync) Monitor.Wait(msgSync); }
    }

    /// <summary>
    /// This interface should be implemented by a Custom Control that contains a NotifyIcon. Remember to set the Text property of the NotifyIcon to the name of the Application
    /// </summary>
    public interface IIconControl
    {
        /// <summary>
        /// This method should deal with disposing of the Custom Control. Easiest way is to just call this.Dispose()
        /// </summary>
        void CloseDown();
    }

    /// <summary>
    /// This interface should be implemented by a class that extends Form, basically your appplication's main form
    /// </summary>
    public interface IMainForm
    {
        /// <summary>
        /// This method should deal with showing the Form to the user. Easiest way is to just call this.Show(). There's no need to check this.IsDisposed here as the Form will have only just been created. This method can also be used to perform initialization of the form such as filling in of Control data
        /// </summary>
        void Open();

        /// <summary>
        /// This method should deal with closing the Form. Easiest way is by calling this.Close() but check this.IsDisposed is not true first.
        /// </summary>
        void CloseDown();

        /// <summary>
        /// This property should just return this.IsDisposed
        /// </summary>
        bool IsDead { get; }

        /// <summary>
        /// This method should deal with showing an already open Form to the user. Basically, bring the Form window to the front and/or unminimize it, but check this.IsDisposed is not true first.
        /// </summary>
        void Restore();
    }

}
