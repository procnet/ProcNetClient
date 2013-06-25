using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace ProcNetClient
{
    static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Application.ExecutablePath.EndsWith("-temp.exe"))
            {
                string exeNorm = Application.ExecutablePath.Remove(Application.ExecutablePath.Length - 9, 5);
                if (File.Exists(exeNorm))
                {
                    Thread.Sleep(100);
                    Application.DoEvents();

                    
                    try
                    {
                        File.Delete(exeNorm);
                    }
                    catch
                    { }

                    if (!File.Exists(exeNorm))
                    {
                        try
                        {
                            File.Copy(Application.ExecutablePath, exeNorm);
                            Process.Start(exeNorm);

                            return;
                        }
                        catch
                        { }
                    }
                }
            }

            string exeTemp = Application.ExecutablePath.Insert(Application.ExecutablePath.Length - 4, "-temp");
            if (File.Exists(exeTemp))
            {
                try
                {
                    File.Delete(exeTemp);
                }
                catch
                {
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormProcNetClient());
        }
    }
}
