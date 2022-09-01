using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;
using Microsoft.VisualBasic;
using System.Speech.Recognition;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;
using FaceRecognition;
using System.Threading;

namespace SemWebBrowser
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            #region Win Key 
            ProcessModule objCurrentModule = Process.GetCurrentProcess().MainModule;
            objKeyboardProcess = new LowLevelKeyboardProc(captureKey);
            ptrHook = SetWindowsHookEx(13, objKeyboardProcess, GetModuleHandle(objCurrentModule.ModuleName), 0);
            #endregion            
            InitializeComponent();
        }
        #region Entities
        int sayac = 1;
        int timerSayac = 0;
        string urlText;
        string adress = "";
        string getUrl = "";
        SpeechRecognitionEngine recognitionEngine = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("en-US"));
        bool permission = true;
        ChromiumWebBrowser browser;
        string pcHostName;
        string pcHostAdres;
        public string getUserName;
        int counter = 2;
        FaceRec faceRec = new FaceRec();

        GlobalKeyboardHook globalKeyboardHook = new GlobalKeyboardHook();
        #endregion

        #region Windows Key Block Entities
        [StructLayout(LayoutKind.Sequential)]
        private struct KeyboardDLLStruct
        {
            public Keys key;
            public int scanCode;
            public int flags;
            public int time;
            public IntPtr extra;
        }
        #endregion

        #region Form Load Details
        private void Form1_Load(object sender, EventArgs e)
        {
            // Form Yapılandırması
            FormConfig();

            // Ip Adres Tespit
            FindIpAdres();

            // Api İstek 
            ApiGetUrl();

            // Adress Sınırlama / Sadece bu adrese gider.   
            //urlText = FileOperations();
            urlText = getUrl;

            //browser config
            browser = new ChromiumWebBrowser(urlText);
            browser.Load(urlText);
            panel1.Controls.Add(browser);

            // browser url adress değişikliği kontrol
            browser.AddressChanged += Browser_AddressChanged;

            //Kamera yüz takibi
            //OpenCamForCheck();
        }
        #endregion

        #region Set Start Message 
        void StartMessage()
        {
            // Başlangıç Uyarı Mesajı
            MessageBox.Show("• Program içerisinde herhangi bir farklı site açılmamaktatır.\n\n" +
                            "• Windows sekme geçiş işlemleri çalışmamaktadır. ( Alt + Tab )\n\n" +
                            "• Program farklı bir işlem yapmanızı engellemektedir.\n\n" +
                            "Sınavlarda Başarılar."
                            , "BİLGİLENDİRME");
        }
        #endregion

        #region Form Configuration
        void FormConfig()
        {
            // Form özellikleri
            //FormBorderStyle = FormBorderStyle.None;
            //WindowState = FormWindowState.Maximized;
            //TopMost = true;

            // Üst Bar rengi
            this.BackColor = Color.FromArgb(8, 124, 108);

            // url uzantısı göster / gizle
            txtBoxUrl.Visible = false;

            btnBack.Visible = false;
            btnForward.Visible = false;
            btnRefresh.Visible = false;

            // windows bar gizleme
            //Taskbar.Gizle();
        }
        #endregion        

        #region URL Trigger
        // Url adress değişikliğini tetikler
        private void Browser_AddressChanged(object sender, AddressChangedEventArgs e)
        {
            this.Invoke(new MethodInvoker(() =>
                {
                    //urlText = e.Address;
                    txtBoxUrl.Text = e.Address;

                    // Sınava girilince geri dön butonu pasif edilir. / Kullanıcı sınavı yapmak zorunda tutulur.
                    //if (txtBoxUrl.Text.Contains("quiz")) btnBack.Enabled = false;                    
                }));
        }
        #endregion

        #region Keyboard Press Code
        // Klavyede basılan her tuşun kontrol mekanizması
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == (Keys.F12 | Keys.Escape | Keys.F11 | Keys.F10))
            {
                UrlInputBox();
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        #endregion

        #region Url InputBox
        private void UrlInputBox()
        {
            this.SendToBack();
            adress = Interaction.InputBox("URL Adres Girişi", "Adres Giriniz.", "https://");

            if (adress != "" && adress != "https://")
            {
                panel1.Controls.Clear();
                browser = new ChromiumWebBrowser(adress);
                browser.Load(adress);
                panel1.Controls.Add(browser);
            }
            this.TopMost = true;
        }
        #endregion

        #region Exit Button
        // Çıkış buton
        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = new DialogResult();
            result = MessageBox.Show("Siteyi kapatmak istiyor musunuz?", "Çıkış", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Taskbar.Goster();
                Application.Exit();
            }
        }
        #endregion

        #region Refreash Button
        // Yenile Buton
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            browser.Refresh();
            browser.Reload();
        }
        #endregion

        #region GoBack Button
        // Geri Gel Buton
        private void btnBack_Click(object sender, EventArgs e)
        {
            if (browser.CanGoBack) browser.Back();
        }
        #endregion

        #region Forward Button
        // İleri Git Buton
        private void btnForward_Click(object sender, EventArgs e)
        {
            SpeechOptions();
            recognitionEngine.RecognizeAsync();
            permission = true;
            if (browser.CanGoForward) browser.Forward();
        }
        #endregion

        #region URL Text Changed
        // Url boyutuna göre şekil değiştiren txtbox
        private void txtBoxUrl_TextChanged(object sender, EventArgs e)
        {
            int charakter = txtBoxUrl.Text.Length;
            //MessageBox.Show(charakter.ToString());
            txtBoxUrl.Width = Convert.ToInt32(charakter * 5.5);
        }
        #endregion

        #region File Operations        
        static string FileOperations()
        {
            // Check edilecek dosya yolu 
            string path = @"C:\Program Files\semSystem.txt";

            bool check = File.Exists(path);
            if (!check)
            {
                FileStream fs = File.Create(path);
                fs.Close();
                StreamWriter sw = new StreamWriter(path);
                sw.WriteLine("https://sem.jsga.edu.tr");
                sw.Close();
            }

            StreamReader sr = new StreamReader(path);
            string Veri = sr.ReadToEnd();
            sr.Close();
            return Veri;

        }

        #endregion

        #region Timer Setup
        private void timer1_Tick(object sender, EventArgs e)
        {
            timerSayac++;
            if (timerSayac % 5 == 0) timer1.Stop();
        }
        #endregion

        #region Speech Options

        void SpeechOptions()
        {
            string[] words = { "Yes", "Hello", "One" };
            Choices choices = new Choices(words);
            Grammar grammar = new Grammar(new GrammarBuilder(choices));
            recognitionEngine.LoadGrammar(grammar);
            recognitionEngine.SetInputToDefaultAudioDevice();
            recognitionEngine.SpeechRecognized += RecognitionEngine_SpeechRecognized;
        }

        private void RecognitionEngine_SpeechRecognized(object sender, SpeechRecognizedEventArgs e)
        {
            if (permission == true)
            {
                if (e.Result.Text == "Yes") UrlInputBox();
                if (e.Result.Text == "Hello")
                {
                    Application.Exit();
                    Taskbar.Goster();
                }
                if (e.Result.Text == "One")
                {
                    this.SendToBack();
                    System.Diagnostics.Process.Start("C://Program Files//semSystem.txt");
                }
                permission = false;
            }
        }
        #endregion

        #region API Get Url
        public void ApiGetUrl()
        {
            WebClient webClient = new WebClient();
            webClient.Encoding = Encoding.UTF8;

            try
            {
                try
                {
                    string url = "https://localhost:44362/api/IpChecks/getbyip?ip=" + pcHostAdres;
                    string gelen = webClient.DownloadString(url);
                    JObject jObject = JObject.Parse(gelen);
                    if (Convert.ToBoolean(jObject["success"].ToString()))
                    {
                        var details = jObject["data"];
                        getUrl = details["urlAdres"].ToString();
                    } 
                }
                catch
                {
                    string url2 = "https://localhost:44362/api/urls/getall";
                    string gelen2 = webClient.DownloadString(url2);
                    JObject jObject2 = JObject.Parse(gelen2);
                    if (Convert.ToBoolean(jObject2["success"].ToString()))
                    {
                        JArray jArray2 = JArray.Parse(jObject2["data"].ToString());
                        foreach (var item2 in jArray2)
                        {
                            JObject jObject3 = JObject.Parse(item2.ToString());
                            getUrl = jObject3["adres"].ToString();
                        }
                        //MessageBox.Show(getUrl);
                    }
                }
            }
            catch (Exception)
            {
                MessageBox.Show("API yı kontrol ediniz.");
                getUrl = "https://sem.jsga.edu.tr";
            }
        }
        #endregion

        #region Find Ip Adress
        private void FindIpAdres()
        {
            pcHostName = Dns.GetHostName();
            pcHostAdres = Dns.GetHostByName(pcHostName).AddressList[0].ToString();
            //MessageBox.Show(pcHostAdres);
        }
        #endregion

        #region Blocked Win Key

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int id, LowLevelKeyboardProc callback, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hook);
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wp, IntPtr lp);
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern short GetAsyncKeyState(Keys key);

        private IntPtr ptrHook;
        private LowLevelKeyboardProc objKeyboardProcess;

        private IntPtr captureKey(int nCode, IntPtr wp, IntPtr lp)
        {
            if (nCode >= 0)
            {
                KeyboardDLLStruct objKeyInfo = (KeyboardDLLStruct)Marshal.PtrToStructure(lp, typeof(KeyboardDLLStruct));

                if (objKeyInfo.key == Keys.RWin 
                    || objKeyInfo.key == Keys.LWin 
                    || objKeyInfo.key == Keys.Tab && HasAltModifier(objKeyInfo.flags) 
                    || objKeyInfo.key == Keys.Escape && (ModifierKeys & Keys.Control) == Keys.Control) 
                {
                    return (IntPtr)1;
                }
            }
            return CallNextHookEx(ptrHook, nCode, wp, lp);
        }

        bool HasAltModifier(int flags)
        {
            return (flags & 0x20) == 0x20;
        }
        #endregion

        #region Listen To Keys and Operations
        public void ListenToKeys()
        {
            // Dinlemek istediğim tuşu tanımlıyorum
            globalKeyboardHook.HookedKeys.Add(Keys.Tab&Keys.Alt);
            globalKeyboardHook.HookedKeys.Add(Keys.Control);
            globalKeyboardHook.HookedKeys.Add(Keys.Delete);

            //tuşa basıldığı anda çalışır
            globalKeyboardHook.KeyDown += new KeyEventHandler(operation);

        }

        void operation(object sender, KeyEventArgs e)
        {
            //Yapılmasını istediğiniz kodlar burada yer alacak
            //Burası tuşa basıldığı an çalışır

            MessageBox.Show("");

            //Eğer buraya gelecek olan tuşa basıldığında
            //o tuşun normal işlevi yine çalışsın istiyorsanız
            //e.Handled değeri false olmalı
            //eğer ilgili tuşa basıldığında burada yakalansın
            // ve devamında tuş başka bir işlev gerçekleştirmesin
            //istiyorsanız bu değeri true yapmalısınız
            e.Handled = false;
        }
        #endregion

        #region Open Cam For Face Check
        void OpenCamForCheck()
        {            
            faceRec.openCamera(pictureBox1, pictureBox2);
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            //Thread.Sleep(5000);
            counter++;
            timer2.Interval =3000; // 5 Saniye
            faceRec.Save_IMAGE(getUserName + "_" + counter.ToString());
            faceRec.isTrained = true;
        }
        #endregion
    }
}
