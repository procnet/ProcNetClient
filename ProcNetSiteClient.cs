using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.AccessControl;

namespace ProcNetClient
{
    class ProcNetSiteClient
    {
        /// <summary>
        /// Версия программы в формате: 1.4.555.8888
        /// </summary>
        public string VersionFull = "";


        string VersionForCheckUpdates = "1.4";


        string guid = null;

        ushort codeBase = 0x4c9a; // 01001100 10011010

        string tempHash = "";




        SHA1 sha = new SHA1Managed();

        const string serverBaseUrl = "http://procnet.ru/";

        object lWrError = new object();

        SortedDictionary<string, string> filenamesIconHashes = new SortedDictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        bool sendAllProcesses = false;

        int usersOnline = 0;

        string downloadUrlNewVersion = null;


        /// <summary>
        /// "true" - программа была закрыта
        /// </summary>
        public bool Closed = false;


        /// <summary>
        /// Временный ID
        /// </summary>
        public string TempHash
        {
            get
            {
                return tempHash;
            }
        }


        /// <summary>
        /// Корневой адрес сайта
        /// </summary>
        public string ServerBaseUrl
        {
            get
            {
                return serverBaseUrl;
            }
        }


        /// <summary>
        /// "false" - не все сообщения о процессах были отправлены на сайт
        /// </summary>
        public bool SendAllProcesses
        {
            get
            {
                return sendAllProcesses;
            }
            set
            {
                sendAllProcesses = value;
            }
        }


        /// <summary>
        /// Кол-во пользователей онлайн
        /// </summary>
        public int UsersOnline
        {
            get
            {
                return usersOnline;
            }
        }


        /// <summary>
        /// Авторизация
        /// </summary>
        /// <param name="regPathProcNetClient">Путь в реестре к ID</param>
        /// <param name="regNameClientId">Имя параметра в реестре</param>
        /// <returns>"true" - если авторизация прошла успешно</returns>
        public bool Login(string regPathProcNetClient, string regNameClientId)
        {
            guid = null;

            RegistryKey key1 = Registry.CurrentUser.OpenSubKey(regPathProcNetClient, true);

            if (key1 == null)
            {
                key1 = Registry.CurrentUser.CreateSubKey(regPathProcNetClient, RegistryKeyPermissionCheck.Default);
            }

            object id = key1.GetValue(regNameClientId);
            key1.Close();


            if (id == null || id.GetType() != typeof(string) || ((string)id).Length != 40)
            {
                if (!Registration(regPathProcNetClient, regNameClientId))
                {
                    MessageBox.Show("Ошибка регистрации", "Ошибка");
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


        /// <summary>
        /// Регистрация в системе (получение ID)
        /// </summary>
        /// <param name="regPathProcNetClient">Путь в реестре к ID</param>
        /// <param name="regNameClientId">Имя параметра в реестре</param>
        /// <returns>"true" - если регистрация прошла успешно</returns>
        bool Registration(string regPathProcNetClient, string regNameClientId)
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
                    RegistryKey key1 = Registry.CurrentUser.OpenSubKey(regPathProcNetClient, true);

                    if (key1 == null)
                    {
                        key1 = Registry.CurrentUser.CreateSubKey(regPathProcNetClient);
                        key1.Close();

                        key1 = Registry.CurrentUser.OpenSubKey(regPathProcNetClient, true);
                    }

                    key1.SetValue(regNameClientId, guid);
                    key1.Close();


                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Выход из системы
        /// </summary>
        public void Logout()
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


        /// <summary>
        /// Создание запроса к сайту
        /// </summary>
        /// <param name="relPageUrl">Относительный адрес</param>
        /// <param name="method">Метод: "GET" или "POST"</param>
        /// <returns>Запрос</returns>
        HttpWebRequest CreateWebRequest2(string relPageUrl, string method)
        {
            string url = serverBaseUrl + relPageUrl;

            HttpWebRequest req3 = (HttpWebRequest)WebRequest.Create(url);
            req3.Method = method;
            req3.UserAgent = "ProcNetClient-" + VersionFull;

            return req3;
        }


        /// <summary>
        /// Запись при POST запросе к сайту
        /// </summary>
        /// <param name="req1">Запрос</param>
        /// <param name="contentType">Тип содержимого</param>
        /// <param name="body">Данных</param>
        /// <param name="coding">"true" - добавить конрольную сумму</param>
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


        /// <summary>
        /// Проверка наличия новой версии
        /// </summary>
        /// <returns>"true" - есть новая версия</returns>
        public bool CheckVersion()
        {
            HttpWebRequest req1 = CreateWebRequest2("client/checkversion/", WebRequestMethods.Http.Get);
            try
            {
                HttpWebResponse resp1 = (HttpWebResponse)req1.GetResponse();
                Stream rd1 = resp1.GetResponseStream();
                StreamReader rd2 = new StreamReader(rd1, Encoding.Default);
                string ver2 = rd2.ReadLine();
                downloadUrlNewVersion = rd2.ReadLine();
                rd2.Close();
                rd1.Close();
                resp1.Close();

                if (ver2 != VersionForCheckUpdates)
                {
                    if (MessageBox.Show("Доступна новая версия " + ver2 + ". Обновить?", "Обновление", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        UpdateVersion(downloadUrlNewVersion);
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


        /// <summary>
        /// Отправка иконки
        /// </summary>
        /// <param name="hash">Хеш иконки</param>
        /// <param name="ico">Данные</param>
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


        /// <summary>
        /// Отправка информации об новых/закрытых процессах
        /// </summary>
        /// <param name="updates3">Процессы</param>
        /// <param name="countNewMessages">[out] Кол-во личных сообщений на сайте</param>
        /// <returns>"true" - успешная отправка</returns>
        public bool SendUpdatesProcNetClient(List<OneUpdate> updates3, out int countNewMessages)
        {
            StringBuilder str = new StringBuilder();

            str.Append("temphash=" + tempHash + "&");

            SortedDictionary<string, byte[]> hashAndIconsData = new SortedDictionary<string, byte[]>();

            List<string> addIconHashes = new List<string>();

            countNewMessages = 0;


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
                if (Closed)
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

                        Int32.TryParse(val2, out countNewMessages);
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


        /// <summary>
        /// Запись в журнал ошибок
        /// </summary>
        /// <param name="url">Адрес проблемного запроса</param>
        /// <param name="method">Метод запроса</param>
        /// <param name="post">Тело, если метод POST</param>
        /// <param name="errorMessage">Сообщение об ошибке</param>
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


        /// <summary>
        /// Преобразование массива байтов в шестнадцатеричную строку
        /// </summary>
        /// <param name="bs">Массив байтов</param>
        /// <returns>Шестнадцатеричная строка</returns>
        string BytesToHexString(byte[] bs)
        {
            StringBuilder str = new StringBuilder();
            foreach (byte b1 in bs)
            {
                str.Append(b1.ToString("x2"));
            }
            return str.ToString();
        }


        /// <summary>
        /// Получение Sid Windows (напрямую он не используется)
        /// </summary>
        /// <returns>Sid</returns>
        string GetComputerSid()
        {
            WindowsIdentity iden = WindowsIdentity.GetCurrent();
            return iden.User.AccountDomainSid.ToString();
        }


        /// <summary>
        /// Обновление версии программы
        /// </summary>
        /// <param name="exeTemp">Имя временного файла</param>
        /// <returns>"true" - если обновление прошло успешно</returns>
        public bool UpdateVersion(string exeTemp)
        {
            HttpWebRequest req2 = CreateWebRequest2(downloadUrlNewVersion, WebRequestMethods.Http.Get);
            try
            {
                HttpWebResponse resp1 = (HttpWebResponse)req2.GetResponse();
                byte[] buff = new byte[2048];
                Stream rd1 = resp1.GetResponseStream();

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
            }
            catch (Exception e1)
            {
                WriteError(req2.RequestUri.AbsoluteUri, req2.Method, "", e1.Message);

                MessageBox.Show("Обновление не удалось!\r\n\r\nСкачайте программу с сайта вручную", "Ошибка");

                return false;
            }

            return true;
        }

    }
}
