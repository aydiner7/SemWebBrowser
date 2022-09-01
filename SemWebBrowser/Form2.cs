using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FaceRecognition;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SemWebBrowser
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
            InputBox();
            FormConfig();
        }

        #region Entities

        public string userName="Hasan";
        FaceRec faceRec = new FaceRec();
        int counter=0;

        #endregion

        #region Form Config Details
        void FormConfig()
        {
            try
            {
                timer1.Interval = 1000;
                button1.Enabled = false;
                FormBorderStyle = FormBorderStyle.None;
                //TopMost = true;

                faceRec.openCamera(pictureBox1, pictureBox2);
                //Thread.Sleep(3000);
                //ProfilCheck();
                timer1.Start();
                button1.Enabled = true;
                Thread.Sleep(3000);
                faceRec.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Bilgisayarında kamera bağlantısı bulunamadı.");
                this.SendToBack();
                this.Hide();
                Form1 frm1 = new Form1();
                frm1.Show();
            }                      
        }

        #endregion

        #region Get Name InputBox
        void InputBox()
        {
            
            string nameCheck = Interaction.InputBox("Ad Soyad Giriniz.", "Kullanıcı Bilgi Girişi");
            userName = nameCheck;
        }
        #endregion

        #region Timer Control
        private void timer1_Tick(object sender, EventArgs e)
        {
            counter++;
            timer1.Interval = 5000; // 5 Saniye
            faceRec.Save_IMAGE(userName + "_" + counter.ToString());
            faceRec.isTrained = true;

            if (counter >= 1 && faceRec.isTrained==true) button1.Enabled = true;

            //using (WebClient client = new WebClient())
            //{
            //    ImageTracking imageTracking = new ImageTracking()
            //    {
            //        name = userName,
            //        picture = pictureBox2.Image.ToString()
            //    };

            //    try
            //    {
            //        client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
            //        var JSONData = imageTracking;
            //        var dataString = JsonConvert.SerializeObject(JSONData);
            //        string gelen = client.UploadString("https://localhost:44362/api/urls/update", "POST", dataString);
            //        JObject jObject = JObject.Parse(gelen);
            //        if (Convert.ToBoolean(jObject["success"].ToString()))
            //        {
            //            var details = jObject["data"];
            //            MessageBox.Show("Url Başarıyla Değiştirildi.");
            //        }
            //    }

            //    catch (Exception)
            //    {
            //        MessageBox.Show("Geçersiz Kullanıcı Bilgileri");
            //        //throw;
            //    }
            //}
        }
        #endregion

        #region Button Config
        private void button1_Click(object sender, EventArgs e)
        {
            //this.SendToBack();
            
            Form1 frm1 = new Form1();
            frm1.getUserName = userName;
            this.Hide();
            frm1.Show();         
        }
        #endregion

        #region Face Picture Save
        void ProfilCheck()
        {
            faceRec.Save_IMAGE(userName);
            faceRec.isTrained = true;
        }
        #endregion

    }
}
