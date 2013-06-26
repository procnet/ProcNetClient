using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.DirectoryServices;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.AccessControl;


namespace ProcNetClient
{
    public partial class FormProcNetClient : Form
    {
        const string regPathProcNetClient = @"Software\ProcNetClient";
        const string regNameClientId = "ClientId";
        const string regNameShowNewUsers = "ShowNewUsers";
        const string regNameDefaultBrowser = "DefaultBrowser";
        const string regNamePathBrowser = "PathBrowser";

        const string regPathAutoLoad = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string regNameAutoLoad = "ProcNetClient";

        

        Random rand = new Random((int)DateTime.Now.Ticks);


        bool closed = false;

        bool running1 = false;
        bool running2 = false;


        private SortedDictionary<int, Process> nowRunning = new SortedDictionary<int, Process>();

        //object lSend = new object();


        Queue<OneUpdate> updatesForSend = new Queue<OneUpdate>();
        object lUpdatesForSend = new object();




        string linkStartSession = null;

        bool loaded = false;

        
        int lastCountNewMessages = 0;

        string linkBalloonTip = null;



        int countSendStat = 0;

        bool authorise = false;

        delegate void DelSetLabelCountQueue(int count);
        DelSetLabelCountQueue dSetLabelCountQueue;

        delegate void DelEnableControls(bool enable);
        DelEnableControls dEnableControls;


        bool fullSend = false;




        string[] browsers = 
        {
            @"C:\Program Files\Internet Explorer\iexplore.exe",
            @"C:\Program Files\Mozilla Firefox\firefox.exe",
            @"C:\Program Files\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files\Opera\opera.exe",
            @"C:\Program Files\Safari\Safari.exe",
            @"C:\Program Files (x86)\Internet Explorer\iexplore.exe",
            @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe",
            @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
            @"C:\Program Files (x86)\Opera\opera.exe",
            @"C:\Program Files (x86)\Safari\Safari.exe",

        };

        ProcNetSiteClient client = new ProcNetSiteClient();




        public FormProcNetClient()
        {
            InitializeComponent();


            Assembly a = Assembly.GetExecutingAssembly();
            client.VersionFull = a.GetName().Version.ToString();

            this.Text = notifyIcon1.Text = "ProcNetClient " + client.VersionFull;


            this.labelRestart.Visible = false;

            foreach (string p1 in browsers)
            {
                if (File.Exists(p1))
                {
                    comboBoxPathBrowser.Items.Add(p1);
                }
            }

            dSetLabelCountQueue = new DelSetLabelCountQueue(SetLabelCountQueue);
            dEnableControls = new DelEnableControls(EnableControls);
        }
        
        private void FormProcNetClient_Load(object sender, EventArgs e)
        {
            System.Diagnostics.Process[] prs2 = System.Diagnostics.Process.GetProcesses();
            System.Diagnostics.Process thisProc = System.Diagnostics.Process.GetCurrentProcess();

            foreach (System.Diagnostics.Process p1 in prs2)
            {
                try
                {
                    if (p1.MainModule.FileVersionInfo.InternalName == thisProc.MainModule.FileVersionInfo.InternalName &&
                        p1.MainModule.FileVersionInfo.ProductName == thisProc.MainModule.FileVersionInfo.ProductName &&
                        p1.MainModule.FileVersionInfo.OriginalFilename == thisProc.MainModule.FileVersionInfo.OriginalFilename)
                    {
                        if (p1.Id != thisProc.Id)
                        {
                            Close();
                            return;
                        }
                    }
                }
                catch
                {
                }
            }

            RegistryKey key3 = Registry.CurrentUser.OpenSubKey(regPathAutoLoad);
            if (key3 != null)
            {
                object path3 = key3.GetValue(regNameAutoLoad);

                if (path3 != null && path3.GetType() == typeof(string) && (string)path3 == Application.ExecutablePath)
                {
                    checkBoxAutoLoad.Checked = true;
                }

                key3.Close();
            }


            RegistryKey key4 = Registry.CurrentUser.OpenSubKey(regPathProcNetClient, true);

            if (key4 != null)
            {
                if (key4.GetValue(regNameDefaultBrowser) != null)
                {
                    checkBoxDefaultBrowser.Checked = (string)key4.GetValue(regNameDefaultBrowser) == "1";
                }

                if (key4.GetValue(regNameShowNewUsers) != null)
                {
                    checkBoxShowNewUsers.Checked = (string)key4.GetValue(regNameShowNewUsers) == "1";
                }

                if (key4.GetValue(regNamePathBrowser) != null)
                {
                    comboBoxPathBrowser.Text = (string)key4.GetValue(regNamePathBrowser);

                    if (!File.Exists(comboBoxPathBrowser.Text))
                    {
                        checkBoxDefaultBrowser.Checked = true;
                    }
                }
                else
                {
                    checkBoxDefaultBrowser.Checked = true;
                }

                comboBoxPathBrowser.Enabled = buttonBrowse.Enabled = !checkBoxDefaultBrowser.Checked;

                key4.Close();
            }



            Application.DoEvents();


            if (client.CheckVersion())
            {
                UpdateVersion();
                Close();
                return;
            }



            if (client.Login(regPathProcNetClient, regNameClientId))
            {
                Thread th1 = new Thread(new ThreadStart(ThreadViewProcesses));
                th1.Start();

                Thread.Sleep(500);

                Thread th2 = new Thread(new ThreadStart(ThreadSendUpdates));
                th2.Start();

                Application.DoEvents();


                if (checkBoxAutoLoad.Checked)
                {
                    BeginInvoke(new MethodInvoker(delegate
                    {
                        Hide();
                    }));
                }

            }
            else
            {
                Close();
            }


            loaded = true;
        }


        /// <summary>
        /// Открытие сайта в браузере
        /// </summary>
        /// <param name="balloon">"true" - если был щелчок по всплывающему сообщению</param>
        void OpenSite(bool balloon)
        {
            if (!authorise && linkStartSession == null)
            {
                MessageBox.Show("Вы неавторизованны!", "Ошибка 1");
                return;
            }

            if (linkStartSession != null)
            {
                OpenBrowser(linkStartSession);
                linkStartSession = null;
                authorise = true;
            }
            else if (balloon && linkBalloonTip != null)
            {
                OpenBrowser(client.ServerBaseUrl + linkBalloonTip);
            }
            else
            {
                OpenBrowser(client.ServerBaseUrl + "processes/");
            }
        }


        /// <summary>
        /// Открытие адреса в браузере
        /// </summary>
        /// <param name="url">Ссылка</param>
        void OpenBrowser(string url)
        {
            if (checkBoxDefaultBrowser.Checked)
            {
                Process.Start(url);
            }
            else
            {
                Process.Start(comboBoxPathBrowser.Text, url);
            }
        }


        private void FormProcNetClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            closed = true;
            client.Closed = true;

            EnableControls(false);

            comboBoxPathBrowser.Enabled = false;
            buttonBrowse.Enabled = false;


            while (running1 || running2)
            {
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }


        /// <summary>
        /// Поток проверки процессов
        /// </summary>
        void ThreadViewProcesses()
        {
            running1 = true;

            while (!closed)
            {
                if (client.SendAllProcesses)
                {
                    nowRunning.Clear();
                }

                Process[] ps1 = Process.GetProcesses();

                List<OneUpdate> updates1 = AnalizeProcesses(ps1);

                if (updates1.Count > 0)
                {
                    lock (lUpdatesForSend)
                    {
                        foreach (OneUpdate u1 in updates1)
                        {
                            updatesForSend.Enqueue(u1);
                        }

                        fullSend = true;
                    }
                }

                if (client.SendAllProcesses)
                {
                    client.SendAllProcesses = false;
                }

                Thread.Sleep(200);
            }

            while (running2)
            {
                Thread.Sleep(50);
            }

            client.Logout();

            running1 = false;
        }


        /// <summary>
        /// Поток отсылки информации о процессах на сайт
        /// </summary>
        void ThreadSendUpdates()
        {
            running2 = true;

            DateTime timeNextSend = DateTime.Now.AddMinutes(-1);

            bool norm = true;

            while (!closed)
            {
                if ((updatesForSend.Count > 0 && DateTime.Now > timeNextSend) ||
                    (DateTime.Now > timeNextSend.AddMinutes(5)))
                {
                    if (!norm)
                    {
                        MessageBox.Show("Ошибка 2");
                    }

                    int prevUsersOnline = client.UsersOnline;

                    while (true)
                    {
                        List<OneUpdate> updates2 = new List<OneUpdate>();

                        labelCountInQuerySend.Invoke(dSetLabelCountQueue, updatesForSend.Count);

                        Application.DoEvents();

                        

                        lock (lUpdatesForSend)
                        {
                            while (updatesForSend.Count > 0 && updates2.Count < 10)
                            {
                                updates2.Add(updatesForSend.Dequeue());
                            }
                        }


                        for (int c = 0; c < 3 && !closed; c++)
                        {
                            int c3;
                            norm = client.SendUpdatesProcNetClient(updates2, out c3);

                            if (c3 > lastCountNewMessages)
                            {
                                ShowBalloon("ProcNetClient", "Новые сообщения: " + c3, "messages/");
                            }

                            if (c3 != lastCountNewMessages)
                            {
                                lastCountNewMessages = c3;

                                labelCountNewMessages.Text = "Новые сообщения: " + c3.ToString();
                                if (c3 > 0)
                                {
                                    labelCountNewMessages.ForeColor = Color.Red;
                                }
                                else
                                {
                                    labelCountNewMessages.ForeColor = SystemColors.ControlText;
                                }
                            }

                            
                            if (norm || countSendStat == 0)
                            {
                                break;
                            }

                            Thread.Sleep(300);
                        }

                        if (!norm)
                        {
                            break;
                        }

                        lock (lUpdatesForSend)
                        {
                            if ((countSendStat > 0 && !fullSend) || updatesForSend.Count == 0)
                            {
                                break;
                            }
                        }
                    }

                    if (fullSend)
                    {
                        fullSend = false;
                    }

                    //labelCountInQuerySend.Text = "В очереди отправки: 0";
                    labelCountInQuerySend.Invoke(dSetLabelCountQueue, updatesForSend.Count);

                    countSendStat++;

                    if (countSendStat == 1)
                    {
                        linkStartSession = client.ServerBaseUrl + "browser/startsession/id/" + client.TempHash + "/";


                        this.Invoke(dEnableControls, true);

                        if (!checkBoxAutoLoad.Checked)
                        {
                            OpenSite(false);
                        }
                    }

                    if (client.UsersOnline != prevUsersOnline)
                    {
                        notifyIcon1.Text = "ProcNetClient " + client.VersionFull + "\r\n" +
                            "Пользователей онлайн: " + client.UsersOnline.ToString();

                        if (countSendStat > 1 && client.UsersOnline > prevUsersOnline && checkBoxShowNewUsers.Checked)
                        {
                            ShowBalloon("Новые пользователи", "Пользователей онлайн: " + client.UsersOnline.ToString(), "users/");
                        }

                        prevUsersOnline = client.UsersOnline;
                    }


                    timeNextSend = DateTime.Now.AddSeconds(rand.Next(5) + 3);
                }
                else
                {
                    Thread.Sleep(200);
                }
            }

            running2 = false;
        }


        /// <summary>
        /// Показ кол-ва процессов в очереди отправки
        /// </summary>
        /// <param name="count">Кол-во процессов</param>
        void SetLabelCountQueue(int count)
        {
            labelCountInQuerySend.Text = "Процессов в очереди отправки: " + count.ToString();
        }


        /// <summary>
        /// Включение и отключение элементов управления
        /// </summary>
        /// <param name="enable">"true" - включить</param>
        void EnableControls(bool enable)
        {
            linkLabelOpenSite.Enabled = enable;
            checkBoxAutoLoad.Enabled = enable;
            checkBoxDefaultBrowser.Enabled = enable;
            checkBoxShowNewUsers.Enabled = enable;
        }


        /// <summary>
        /// Проверка наличия новых и закрытых процессов
        /// </summary>
        /// <param name="ps2">Работащие процессы</param>
        /// <returns>Изменения</returns>
        List<OneUpdate> AnalizeProcesses(Process[] ps2)
        {
            SortedDictionary<int, Process> tempRunning = new SortedDictionary<int, Process>();

            List<OneUpdate> updates = new List<OneUpdate>();

            foreach (Process p2 in ps2)
            {
                if (!nowRunning.ContainsKey(p2.Id))
                {
                    nowRunning.Add(p2.Id, p2);

                    updates.Add(new OneUpdate(OneUpdate.ModeUpdate.AddProcess, p2));
                }

                if (!tempRunning.ContainsKey(p2.Id))
                {
                    tempRunning.Add(p2.Id, nowRunning[p2.Id]);
                }
            }

            List<int> closedProcesses = new List<int>();

            foreach (KeyValuePair<int, Process> kv3 in nowRunning)
            {
                if (!tempRunning.ContainsKey(kv3.Key))
                {
                    closedProcesses.Add(kv3.Key);

                    updates.Add(new OneUpdate(OneUpdate.ModeUpdate.RemoveProcess, kv3.Value));
                }
            }

            foreach (int id4 in closedProcesses)
            {
                if (nowRunning.ContainsKey(id4))
                {
                    nowRunning.Remove(id4);
                }
            }

            return updates;
        }


        /// <summary>
        /// Загрузка и обновление новой версии программы с сайта
        /// </summary>
        void UpdateVersion()
        {
            string exeTemp = Application.ExecutablePath.Insert(Application.ExecutablePath.Length - 4, "-temp");

            if (client.UpdateVersion(exeTemp))
            {
                Process.Start(exeTemp);

                Close();
            }
        }

        private void FormProcNetClient_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void idShowWindow_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            this.Activate();
        }

        private void idClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            OpenSite(false);
        }

        private void idAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ProcNet.ru\r\n\r\nСеть компьютерных процессов", "О программе");
        }

        private void linkLabelOpenSite_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenSite(false);
        }


        /// <summary>
        /// Помещение программы в автозагрузку
        /// </summary>
        /// <param name="autoload">"true" - авторазагрузка программы при старте ОС</param>
        void SetAutoLoad(bool autoload)
        {
            RegistryKey key2 = Registry.CurrentUser.OpenSubKey(regPathAutoLoad, true);

            if (key2 != null)
            {
                if (autoload)
                {
                    key2.SetValue(regNameAutoLoad, Application.ExecutablePath);
                }
                else
                {
                    key2.DeleteValue(regNameAutoLoad);
                }

                key2.Close();
            }
        }

        private void checkBoxAutoLoad_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                if (checkBoxAutoLoad.Checked)
                {
                    MessageBox.Show(
                        "После каждой загрузки компьютера, для работы с сайтом\r\n" +
                        "первый раз Вам нужно будет открыть его через программу (для авторизации).\r\n" +
                        "Достаточно сделать двойной щелчок по значку программы или по ссылке в окне.", "Обратите внимание");
                }

                SetAutoLoad(checkBoxAutoLoad.Checked);
            }
        }

        private void idOpenBrowser_Click(object sender, EventArgs e)
        {
            OpenSite(false);
        }


        /// <summary>
        /// Показ всплывающего сообщения
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="text">Текст</param>
        /// <param name="url">Ссылка, которая откроется после щелчка</param>
        void ShowBalloon(string title, string text, string url)
        {
            linkBalloonTip = url;
            notifyIcon1.ShowBalloonTip(10 * 1000, title, text, ToolTipIcon.Info);
        }

        private void notifyIcon1_BalloonTipClicked(object sender, EventArgs e)
        {
            OpenSite(true);
            linkBalloonTip = null;
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog exe = new OpenFileDialog();
            exe.Filter = "Exe-files|*.exe";

            if (File.Exists(comboBoxPathBrowser.Text))
            {
                exe.FileName = comboBoxPathBrowser.Text;
            }

            if (exe.ShowDialog() == DialogResult.OK)
            {
                comboBoxPathBrowser.Text = exe.FileName;
            }
        }


        /// <summary>
        /// Установка выбранного браузера
        /// </summary>
        /// <param name="defBrowser">"true" - использовать браузер в системе по-умолчанию</param>
        /// <param name="pathOther">Путь к exe-файлу браузера</param>
        void SetBrowser(bool defBrowser, string pathOther)
        {
            RegistryKey key3 = Registry.CurrentUser.OpenSubKey(regPathProcNetClient, true);

            if (key3 != null)
            {
                if (File.Exists(pathOther))
                {
                    key3.SetValue(regNameDefaultBrowser, (defBrowser ? "1" : "0"));
                    key3.SetValue(regNamePathBrowser, pathOther);
                }
                else
                {
                    key3.SetValue(regNameDefaultBrowser, "1");
                }

                key3.Close();
            }
        }

        private void checkBoxDefaultBrowser_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                labelRestart.Visible = true;
                linkLabelOpenSite.Enabled = false;

                comboBoxPathBrowser.Enabled = buttonBrowse.Enabled = !checkBoxDefaultBrowser.Checked;

                SetBrowser(checkBoxDefaultBrowser.Checked, comboBoxPathBrowser.Text);
            }
        }

        private void comboBoxPathBrowser_TextChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                labelRestart.Visible = true;
                linkLabelOpenSite.Enabled = false;

                SetBrowser(checkBoxDefaultBrowser.Checked, comboBoxPathBrowser.Text);
            }
        }

        private void checkBoxShowNewUsers_CheckedChanged(object sender, EventArgs e)
        {
            if (loaded)
            {
                RegistryKey key5 = Registry.CurrentUser.OpenSubKey(regPathProcNetClient, true);

                if (key5 != null)
                {
                    key5.SetValue(regNameShowNewUsers, (checkBoxShowNewUsers.Checked ? "1" : "0"));
                }
            }
        }

    }
}
