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
        string VersionFull = "";
        string VersionForCheckUpdates = "1.4";

        const string serverBaseUrl = "http://procnet.ru/";

        const string regProcNetClient = @"Software\ProcNetClient";
        const string regvalClientId = "ClientId";
        const string regvalShowNewUsers = "ShowNewUsers";
        const string regvalDefaultBrowser = "DefaultBrowser";
        const string regvalPathBrowser = "PathBrowser";

        const string regPathAutoLoad = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string regvalAutoLoad = "ProcNetClient";

        

        Random rand = new Random((int)DateTime.Now.Ticks);


        bool closed = false;

        bool running1 = false;
        bool running2 = false;


        private SortedDictionary<int, Process> nowRunning = new SortedDictionary<int, Process>();

        object lSend = new object();

        string guid = null;

        Queue<OneUpdate> updatesForSend = new Queue<OneUpdate>();
        object lUpdatesForSend = new object();

        SortedDictionary<string, string> filenamesIconHashes = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        SHA1 sha = new SHA1Managed();


        string linkStartSession = null;

        bool loaded = false;

        object lWrError = new object();
        
        int lastCountNewMessages = 0;

        string linkBalloonTip = null;


        string tempHash = "";

        int countSendStat = 0;

        bool authorise = false;

        delegate void DelSetLabelCountQueue(int count);
        DelSetLabelCountQueue dSetLabelCountQueue;

        delegate void DelEnableControls(bool enable);
        DelEnableControls dEnableControls;

        bool sendAllProcesses = false;

        bool fullSend = false;

        int usersOnline = 0;

        ushort codeBase = 0x4c9a; // 01001100 10011010


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



        public FormProcNetClient()
        {
            InitializeComponent();


            Assembly a = Assembly.GetExecutingAssembly();
            VersionFull = a.GetName().Version.ToString();

            this.Text = notifyIcon1.Text = "ProcNetClient " + VersionFull;


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

            //checkCode = (ushort)rand.Next(65535);
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
                object path3 = key3.GetValue(regvalAutoLoad);

                if (path3 != null && path3.GetType() == typeof(string) && (string)path3 == Application.ExecutablePath)
                {
                    checkBoxAutoLoad.Checked = true;
                }

                key3.Close();
            }


            RegistryKey key4 = Registry.CurrentUser.OpenSubKey(regProcNetClient, true);

            if (key4 != null)
            {
                if (key4.GetValue(regvalDefaultBrowser) != null)
                {
                    checkBoxDefaultBrowser.Checked = (string)key4.GetValue(regvalDefaultBrowser) == "1";
                }

                if (key4.GetValue(regvalShowNewUsers) != null)
                {
                    checkBoxShowNewUsers.Checked = (string)key4.GetValue(regvalShowNewUsers) == "1";
                }

                if (key4.GetValue(regvalPathBrowser) != null)
                {
                    comboBoxPathBrowser.Text = (string)key4.GetValue(regvalPathBrowser);

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


            if (CheckVersion())
            {
                Close();
                return;
            }

            /*//======================================================
            // test post!!!!
            HttpWebRequest reqTest = CreateWebRequest2("browser/111/", WebRequestMethods.Http.Post);
            byte[] bs = Encoding.UTF8.GetBytes("abcdef");
            WriteWebRequest(reqTest, "application/x-www-form-urlencoded", bs, true);            
            Close();
            return;
            //======================================================*/

            
            if (Login())
            {
                Thread th1 = new Thread(new ThreadStart(ThreadViewProcesses));
                th1.Start();

                Thread.Sleep(500);

                Thread th2 = new Thread(new ThreadStart(ThreadSendUpdates));
                th2.Start();

                Application.DoEvents();

                //Thread.Sleep(100);

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

            //Application.DoEvents();

            loaded = true;
        }

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
                OpenBrowser(serverBaseUrl + linkBalloonTip);
            }
            else
            {
                OpenBrowser(serverBaseUrl + "processes/");
            }
        }

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

            EnableControls(false);

            comboBoxPathBrowser.Enabled = false;
            buttonBrowse.Enabled = false;


            while (running1 || running2)
            {
                Thread.Sleep(50);
                Application.DoEvents();
            }
        }

        void ThreadViewProcesses()
        {
            running1 = true;

            while (!closed)
            {
                if (sendAllProcesses)
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

                if (sendAllProcesses)
                {
                    sendAllProcesses = false;
                }

                Thread.Sleep(200);
            }

            while (running2)
            {
                Thread.Sleep(50);
            }

            Logout(guid);

            running1 = false;
        }

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

                    int prevUsersOnline = usersOnline;

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
                            norm = SendUpdatesProcNetClient(updates2);
                            
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
                        linkStartSession = serverBaseUrl + "browser/startsession/id/" + tempHash + "/";


                        this.Invoke(dEnableControls, true);

                        if (!checkBoxAutoLoad.Checked)
                        {
                            OpenSite(false);
                        }
                    }

                    if (usersOnline != prevUsersOnline)
                    {
                        notifyIcon1.Text = "ProcNetClient " + VersionFull + "\r\n" +
                            "Пользователей онлайн: " + usersOnline.ToString();

                        if (countSendStat > 1 && usersOnline > prevUsersOnline && checkBoxShowNewUsers.Checked)
                        {
                            ShowBalloon("Новые пользователи", "Пользователей онлайн: " + usersOnline.ToString(), "users/");
                        }

                        prevUsersOnline = usersOnline;
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

        void SetLabelCountQueue(int count)
        {
            labelCountInQuerySend.Text = "Процессов в очереди отправки: " + count.ToString();
        }

        void EnableControls(bool enable)
        {
            linkLabelOpenSite.Enabled = enable;
            checkBoxAutoLoad.Enabled = enable;
            checkBoxDefaultBrowser.Enabled = enable;
            checkBoxShowNewUsers.Enabled = enable;



        }

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

        bool SendUpdatesProcNetClient(List<OneUpdate> updates3)
        {
            StringBuilder str = new StringBuilder();

            str.Append("temphash=" + tempHash + "&");

            SortedDictionary<string, byte[]> hashAndIconsData = new SortedDictionary<string, byte[]>();

            List<string> addIconHashes = new List<string>();


            foreach (OneUpdate u1 in updates3)
            {
                string m = null;

                switch (u1.Mode)
                {
                    case OneUpdate.ModeUpdate.AddProcess:
                        try
                        {
                            if (u1.Proc.HasExited)
                            {
                                continue;
                            }
                        }
                        catch
                        {
                        }
                        m = "add";
                        break;

                    case OneUpdate.ModeUpdate.RemoveProcess:
                        m = "rem";
                        break;
                }

                str.Append(m + u1.Proc.Id.ToString() + "=" + u1.Proc.ProcessName + "&");



                if (u1.Mode == OneUpdate.ModeUpdate.AddProcess)
                {
                    try
                    {
                        str.Append("ver" + u1.Proc.Id.ToString() + "=" + u1.Proc.MainModule.FileVersionInfo.FileVersion.Replace("&", "%26") + "&");
                    }
                    catch
                    { }

                    try
                    {
                        str.Append("des" + u1.Proc.Id.ToString() + "=" + u1.Proc.MainModule.FileVersionInfo.FileDescription.Replace("&", "%26") + "&");
                    }
                    catch
                    { }

                    try
                    {
                        str.Append("com" + u1.Proc.Id.ToString() + "=" + u1.Proc.MainModule.FileVersionInfo.CompanyName.Replace("&", "%26") + "&");
                    }
                    catch
                    { }

                    try
                    {
                        str.Append("siz" + u1.Proc.Id.ToString() + "=" + new FileInfo(u1.Proc.MainModule.FileName).Length.ToString() + "&");
                    }
                    catch
                    { }

                    try
                    {
                        string fn = u1.Proc.MainModule.FileName;
                        if (fn.StartsWith("\\??\\"))
                        {
                            fn = fn.Substring(4);
                        }

                        if (!filenamesIconHashes.ContainsKey(fn))
                        {
                            try
                            {
                                Icon ico = Icon.ExtractAssociatedIcon(fn);

                                string file1 = Path.GetTempFileName();// +".png";
                                string file2 = Path.GetTempFileName();// +".png";

                                Bitmap icoBig = new Bitmap(ico.ToBitmap(), new Size(32, 32));
                                Bitmap icoSmall = new Bitmap(ico.ToBitmap(), new Size(16, 16));

                                icoBig.Save(file1);
                                icoSmall.Save(file2);

                                byte[] buffBig = File.ReadAllBytes(file1);
                                byte[] buffSmall = File.ReadAllBytes(file2);

                                try
                                {
                                    File.Delete(file1);
                                }
                                catch
                                { }

                                try
                                {
                                    File.Delete(file2);
                                }
                                catch { }

                                string hashBig = BytesToHexString(sha.ComputeHash(buffBig));
                                string hashSmall = BytesToHexString(sha.ComputeHash(buffSmall));

                                if (!hashAndIconsData.ContainsKey(hashBig))
                                {
                                    hashAndIconsData.Add(hashBig, buffBig);
                                    addIconHashes.Add(hashBig);
                                }

                                if (!hashAndIconsData.ContainsKey(hashSmall))
                                {
                                    hashAndIconsData.Add(hashSmall, buffSmall);
                                    addIconHashes.Add(hashSmall);
                                }

                                str.Append("big" + u1.Proc.Id.ToString() + "=" + hashBig + "&");
                                str.Append("sml" + u1.Proc.Id.ToString() + "=" + hashSmall + "&");
                            }
                            catch
                            { 
                            }

                            try
                            {
                                filenamesIconHashes.Add(u1.Proc.MainModule.FileName, null);
                            }
                            catch
                            { }
                        }
                    }
                    catch
                    { }
                }
            }

            str.Remove(str.Length - 1, 1);

            byte[] bs1 = Encoding.UTF8.GetBytes(str.ToString());

            HttpWebRequest req = CreateWebRequest2("client/setstat/", WebRequestMethods.Http.Post);
            string html = "";
            try
            {
                /*req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = bs1.Length;
                Stream wr2 = req.GetRequestStream();
                wr2.Write(bs1, 0, bs1.Length);
                wr2.Close();*/

                WriteWebRequest(req, "application/x-www-form-urlencoded", bs1, true);

                
                HttpWebResponse resp1 = (HttpWebResponse)req.GetResponse();
                Stream rd4 = resp1.GetResponseStream();
                StreamReader rd3 = new StreamReader(rd4, Encoding.Default);
                html = rd3.ReadToEnd();
                rd3.Close();
                rd4.Close();
                resp1.Close();
            }
            catch (Exception e1)
            {
                WriteError(req.RequestUri.AbsoluteUri, req.Method, str.ToString(), e1.Message);
            }

            bool norm = false;

            string[] vals = html.Split('&');
            foreach (string v1 in vals)
            {
                if (closed)
                {
                    break;
                }

                int iEq = v1.IndexOf('=');
                if (iEq > 0)
                {
                    string name = v1.Substring(0, iEq);
                    string val2 = v1.Substring(iEq + 1);

                    if (name == "geticon")
                    {
                        try
                        {
                            string hash = val2;

                            if (hashAndIconsData.ContainsKey(hash) && hashAndIconsData[hash] != null)
                            {
                                SendIcon(hash, hashAndIconsData[hash]);
                            }
                        }
                        catch
                        { }
                    }
                    else if (name == "mes")
                    {
                        norm = true;

                        int c3 = 0;
                        if (Int32.TryParse(val2, out c3))
                        {
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
                        }
                    }
                    else if (name == "temphash")
                    {
                        tempHash = val2;
                    }
                    else if (name == "getallprocesses" && val2 == "1")
                    {
                        sendAllProcesses = true;
                    }
                    else if (name == "online")
                    {
                        usersOnline = Int32.Parse(val2);
                    }
                    else if (name == "newcode")
                    {
                        codeBase = ushort.Parse(val2);
                    }
                }
            }

            foreach (string h1 in addIconHashes)
            {
                if (hashAndIconsData.ContainsKey(h1))
                {
                    hashAndIconsData[h1] = null;
                }
            }


            return norm;
        }

        bool Login()
        {
            guid = null;

            RegistryKey key1 = Registry.CurrentUser.OpenSubKey(regProcNetClient, true);

            if (key1 == null)
            {
                key1 = Registry.CurrentUser.CreateSubKey(regProcNetClient, RegistryKeyPermissionCheck.Default);
            }

            object id = key1.GetValue(regvalClientId);
            key1.Close();


            if (id == null || id.GetType() != typeof(string) || ((string)id).Length != 40)
            {
                if (!Registration())
                {
                    MessageBox.Show("Ошибка регистрации", "Ошибка");
                    Close();
                    return false;
                }
            }
            else
            {
                guid = (string)id;
            }

            codeBase = (ushort)(guid[guid.Length - 2] * 256 + guid[guid.Length - 1]);


            byte[] bs3 = Encoding.Default.GetBytes("guid=" + guid);

            HttpWebRequest req2 = CreateWebRequest2("client/login/", WebRequestMethods.Http.Post);
            try
            {
                req2.ContentType = "application/x-www-form-urlencoded";
                req2.ContentLength = bs3.Length;
                Stream wr3 = req2.GetRequestStream();
                wr3.Write(bs3, 0, bs3.Length);
                wr3.Close();

                HttpWebResponse resp2 = (HttpWebResponse)req2.GetResponse();
                Stream rd3 = resp2.GetResponseStream();
                StreamReader rd4 = new StreamReader(rd3, Encoding.Default, false);
                tempHash = rd4.ReadLine();
                rd4.Close();
                rd3.Close();
                resp2.Close();
            }
            catch (Exception e1)
            {
                WriteError(req2.RequestUri.AbsoluteUri, req2.Method, bs3.ToString(), e1.Message);
            }

            if (tempHash != null && tempHash.Length == 40)
            {
                return true;
            }

            MessageBox.Show("Ошибка авторизации", "Ошибка");
            return false;
        }

        bool Registration()
        {
            if (MessageBox.Show(
                "Вы - новый пользователь сети комьютерных процессов!\r\n" +
                "Если вы согласны быть одним из участников, нажмите \"Да\"",
                "Добро пожаловать!", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return false;
            }

            string sid = "";

            try
            {
                sid = GetComputerSid();
            }
            catch
            { }

            byte[] sidHash = sha.ComputeHash(Encoding.Default.GetBytes(sid));
            string sidHash2 = BytesToHexString(sidHash);

            string tempGuid = "";
            HttpWebRequest req1 = CreateWebRequest2("client/reqcreatenewuser/", WebRequestMethods.Http.Get);
            try
            {
                HttpWebResponse resp1 = (HttpWebResponse)req1.GetResponse();
                Stream rd1 = resp1.GetResponseStream();
                StreamReader rd2 = new StreamReader(rd1, Encoding.Default);
                tempGuid = rd2.ReadLine();
                rd2.Close();
                rd1.Close();
                resp1.Close();
            }
            catch (Exception e1)
            {
                WriteError(req1.RequestUri.AbsoluteUri, req1.Method, "", e1.Message);
            }

            if (tempGuid.Length == 40)
            {
                string t2 = "sid=" + sidHash2 + "&tempguid=" + tempGuid;
                HttpWebRequest req2 = CreateWebRequest2("client/createnewuser/", WebRequestMethods.Http.Post);
                try
                {
                    req2.ContentType = "application/x-www-form-urlencoded";
                    req2.ContentLength = t2.Length;
                    Stream wr3 = req2.GetRequestStream();
                    StreamWriter wr4 = new StreamWriter(wr3, Encoding.Default); // UTF8 - ошибка!
                    wr4.Write(t2);
                    wr4.Flush();
                    wr4.Close();
                    wr3.Close();

                    HttpWebResponse resp2 = (HttpWebResponse)req2.GetResponse();
                    Stream rd5 = resp2.GetResponseStream();
                    StreamReader rd6 = new StreamReader(rd5, Encoding.Default);
                    guid = rd6.ReadLine();
                    rd6.Close();
                    rd5.Close();
                    resp2.Close();
                }
                catch (Exception e1)
                {
                    WriteError(req2.RequestUri.AbsoluteUri, req2.Method, t2, e1.Message);
                }

                if (guid.Length == 40)
                {
                    RegistryKey key1 = Registry.CurrentUser.OpenSubKey(regProcNetClient, true);

                    if (key1 == null)
                    {
                        key1 = Registry.CurrentUser.CreateSubKey(regProcNetClient);
                        key1.Close();

                        key1 = Registry.CurrentUser.OpenSubKey(regProcNetClient, true);
                    }

                    key1.SetValue(regvalClientId, guid);
                    key1.Close();


                    return true;
                }
            }

            return false;
        }

        void Logout(string guid)
        {
            string post = "userguid=" + guid;
            byte[] bs3 = Encoding.Default.GetBytes(post);

            HttpWebRequest req3 = CreateWebRequest2("client/logout/", WebRequestMethods.Http.Post);
            try
            {
                req3.ContentType = "application/x-www-form-urlencoded";
                req3.ContentLength = bs3.Length;
                Stream wr3 = req3.GetRequestStream();
                wr3.Write(bs3, 0, bs3.Length);
                wr3.Close();

                HttpWebResponse resp2 = (HttpWebResponse)req3.GetResponse();
                Stream rd3 = resp2.GetResponseStream();
                StreamReader rd4 = new StreamReader(rd3, Encoding.Default, false);
                string s = rd4.ReadLine();
                rd4.Close();
                rd3.Close();
                resp2.Close();
            }
            catch (Exception e1)
            {
                WriteError(req3.RequestUri.AbsoluteUri, req3.Method, post, e1.Message);
            }
        }

        HttpWebRequest CreateWebRequest2(string relPageUrl, string method)
        {
            string url = serverBaseUrl + relPageUrl;

            HttpWebRequest req3 = (HttpWebRequest)WebRequest.Create(url);
            req3.Method = method;
            req3.UserAgent = "ProcNetClient-" + VersionFull;

            return req3;
        }

        void WriteWebRequest(HttpWebRequest req1, string contentType, byte[] body, bool coding)
        {
            byte[] c2 = null;

            if (coding)
            {
                ushort h = 0;
                int i = 0;

                List<ushort> temp = new List<ushort>();

                foreach (byte b1 in body)
                {
                    temp.Add(b1);
                    h = (ushort)(h ^ (0xffff & (b1 << (i % 5))));
                    temp.Add(h);
                    h = (ushort)(h ^ (0xffff & (codeBase << (i % 7))));
                    temp.Add(h);

                    i++;
                }

                c2 = Encoding.Default.GetBytes("&c=" + h.ToString());
            }

            req1.ContentType = contentType;
            req1.ContentLength = body.Length + (coding ? c2.Length : 0);
            Stream wr3 = req1.GetRequestStream();
            wr3.Write(body, 0, body.Length);

            if (coding)
            {
                wr3.Write(c2, 0, c2.Length);
            }

            wr3.Close();
        }

        bool CheckVersion()
        {
            HttpWebRequest req1 = CreateWebRequest2("client/checkversion/", WebRequestMethods.Http.Get);
            try
            {
                HttpWebResponse resp1 = (HttpWebResponse)req1.GetResponse();
                Stream rd1 = resp1.GetResponseStream();
                StreamReader rd2 = new StreamReader(rd1, Encoding.Default);
                string ver2 = rd2.ReadLine();
                string downloadUrl = rd2.ReadLine();
                rd2.Close();
                rd1.Close();
                resp1.Close();

                if (ver2 != VersionForCheckUpdates)
                {
                    if (MessageBox.Show("Доступна новая версия " + ver2 + ". Обновить?", "Обновление", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        UpdateVersion(downloadUrl);
                        return true;
                    }
                }
            }
            catch (Exception e1)
            {
                WriteError(req1.RequestUri.AbsoluteUri, req1.Method, "", e1.Message);
            }
            
            return false;
        }

        void UpdateVersion(string downloadUrl)
        {
            HttpWebRequest req2 = CreateWebRequest2(downloadUrl, WebRequestMethods.Http.Get);
            try
            {
                HttpWebResponse resp1 = (HttpWebResponse)req2.GetResponse();
                byte[] buff = new byte[2048];
                Stream rd1 = resp1.GetResponseStream();

                string exeTemp = Application.ExecutablePath.Insert(Application.ExecutablePath.Length - 4, "-temp");
                FileStream wr2 = new FileStream(exeTemp, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024);

                while (true)
                {
                    int len = rd1.Read(buff, 0, 2048);

                    if (len > 0)
                    {
                        wr2.Write(buff, 0, len);
                    }
                    else
                    {
                        break;
                    }
                }
                wr2.Close();
                rd1.Close();
                resp1.Close();

                Process.Start(exeTemp);

                Close();
            }
            catch (Exception e1)
            {
                WriteError(req2.RequestUri.AbsoluteUri, req2.Method, "", e1.Message);

                MessageBox.Show("Обновление не удалось!\r\n\r\nСкачайте программу с сайта вручную", "Ошибка");
            }
        }

        private void FormProcNetClient_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }

        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {

        }

        string GetComputerSid()
        {
            WindowsIdentity iden = WindowsIdentity.GetCurrent();
            return iden.User.AccountDomainSid.ToString();
        }

        string BytesToHexString(byte[] bs)
        {
            StringBuilder str = new StringBuilder();
            foreach (byte b1 in bs)
            {
                str.Append(b1.ToString("x2"));
            }
            return str.ToString();
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

        void SendIcon(string hash, byte[] ico)
        {
            byte[] b2 = Encoding.Default.GetBytes("--AyV04a\r\nContent-Disposition: file; name=\"file\"; filename=\"" + hash + "\"\r\nContent-Transfer-Encoding: binary\r\n\r\n");
            byte[] b3 = Encoding.Default.GetBytes("\r\n--AyV04a--");


            HttpWebRequest req1 = CreateWebRequest2("client/send_icon/" + hash + "/", WebRequestMethods.Http.Post);
            try
            {
                req1.ContentType = "multipart/form-data, boundary=AyV04a";
                req1.ContentLength = b2.Length + ico.Length + b3.Length;
                Stream wr1 = req1.GetRequestStream();
                wr1.Write(b2, 0, b2.Length);
                wr1.Write(ico, 0, ico.Length);
                wr1.Write(b3, 0, b3.Length);
                wr1.Close();

                HttpWebResponse resp1 = (HttpWebResponse)req1.GetResponse();
                Stream rd2 = resp1.GetResponseStream();
                StreamReader rd3 = new StreamReader(rd2, Encoding.UTF8, false);
                string text = rd3.ReadToEnd();
                rd3.Close();
                rd2.Close();
                resp1.Close();
            }
            catch (Exception e1)
            {
                WriteError(req1.RequestUri.AbsoluteUri, req1.Method, "icon size=" + ico.Length.ToString(), e1.Message);
            }
        }

        void SetAutoLoad(bool autoload)
        {
            RegistryKey key2 = Registry.CurrentUser.OpenSubKey(regPathAutoLoad, true);

            if (key2 != null)
            {
                if (autoload)
                {
                    key2.SetValue(regvalAutoLoad, Application.ExecutablePath);
                }
                else
                {
                    key2.DeleteValue(regvalAutoLoad);
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

        void WriteError(string url, string method, string post, string errorMessage)
        {
            string fn = "ProcNetClient-Errors.txt";

            lock (lWrError)
            {
                StreamWriter wr1 = new StreamWriter(fn, true, Encoding.UTF8);
                wr1.WriteLine("Time: " + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                wr1.WriteLine("Url: " + url);
                //wr1.WriteLine("Method: " + method);
                //wr1.WriteLine("Post: " + post);
                wr1.WriteLine("Error: " + errorMessage);
                wr1.WriteLine();
                wr1.Close();
            }
        }

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

        void SetBrowser(bool defBrowser, string pathOther)
        {
            RegistryKey key3 = Registry.CurrentUser.OpenSubKey(regProcNetClient, true);

            if (key3 != null)
            {
                if (File.Exists(pathOther))
                {
                    key3.SetValue(regvalDefaultBrowser, (defBrowser ? "1" : "0"));
                    key3.SetValue(regvalPathBrowser, pathOther);
                }
                else
                {
                    key3.SetValue(regvalDefaultBrowser, "1");
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
                RegistryKey key5 = Registry.CurrentUser.OpenSubKey(regProcNetClient, true);

                if (key5 != null)
                {
                    key5.SetValue(regvalShowNewUsers, (checkBoxShowNewUsers.Checked ? "1" : "0"));
                }
            }
        }

    }
}
