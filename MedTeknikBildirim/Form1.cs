using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Oracle.ManagedDataAccess.Client;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using System.Net;
using System.Net.Sockets;
using System.Web.Hosting;

namespace MedTeknikBildirim
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // Değişkenler -- bot API = 5257546499:AAGewTVbheD-sw9RrXk4mLE3eHOkJoCOwEE
        static TelegramBotClient teleBot = new TelegramBotClient("5257546499:AAGewTVbheD-sw9RrXk4mLE3eHOkJoCOwEE"); // HBYS Bilgi İşlem
        static TelegramBotClient teleBotTumu = new TelegramBotClient("5203574119:AAF2Eamdp_LpaN2I8BYAclYUx04yQv0LWXk"); // Tümü

        string grupCode = "-1001710519491";// HBYS Bilgi işlem ID
        string grupCodeTumu = "-1001651664333"; // Tümü ID
        string grupGalen = "-1001561260599"; // Tümü ID
        string grupTinaz = "-1001513964197"; // Tümü ID
        string grupBtm = "-1001561260599"; // Tümü ID

        string gunlukRaporIcerik;


        string telegramMessage;
        string teleMe = "724301168";
        string telegramMessageTumu;
        int arizaSayisi = 0;

        void AyarlariKaydet()
        {
            if (txtDB.Text == "" && txtKullAdi.Text == "" && txtSifre.Text == "")
            {
                MessageBox.Show("Veritabanı bilgileri boş. Lütfen alanları doldurunuz", "DB Bilgileri Hatalı");
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Ayarlar kayıt edilsin mi ?", "Ayarları Kaydet", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    try
                    {
                        const string sPath = "MedTeknikBildirim.ini";

                        System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(sPath);

                        SaveFile.Write("DB=" + txtDB.Text + ";");
                        SaveFile.Write("KullAdi=" + txtKullAdi.Text + ";");
                        SaveFile.Write("KullSifre=" + txtSifre.Text);

                        SaveFile.Close();

                        dbAdres = txtDB.Text;
                        dbKullAdi = txtKullAdi.Text;
                        dbSifre = txtSifre.Text;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Hata : " + ex);
                    }
                }
            }
        }
        public static string dbAdres, dbKullAdi, dbSifre, baglanti;
        public void AyarlariOku() // bağlantı ayarları

        {
            string dosya_dizini = AppDomain.CurrentDomain.BaseDirectory.ToString() + "MedTeknikBildirim.ini";
            if (File.Exists(dosya_dizini) == false) // dizindeki dosya var mı ?
            {
                const string sPath = "MedTeknikBildirim.ini";
                System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(sPath);
                SaveFile.Write("DB=" + "172.0.0.1:1521/orcl" + ";");
                SaveFile.Write("KullAdi=" + "HASTANE" + ";");
                SaveFile.Write("KullSifre=" + "HASTANE");
                SaveFile.Close();
                txtDB.Text = dbAdres;
                txtKullAdi.Text = dbKullAdi;
                txtSifre.Text = dbSifre;
                MessageBox.Show("Bağlantı ayarları bulunamadı. Yeni dosya oluşturuluyor.");

            }
            else
            {

                string veri = "";
                using (StreamReader sr = new StreamReader("MedTeknikBildirim.ini"))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        veri = line;
                    }
                    sr.Close();
                }

                dbAdres = veri.Substring(3, veri.IndexOf(";") - 3);
                dbSifre = veri.Substring(veri.IndexOf("KullSifre") + 10, veri.Length - (veri.IndexOf("KullSifre") + 10));
                dbKullAdi = veri.Substring(veri.IndexOf("KullAdi=") + 8, (veri.LastIndexOf(";")) - (veri.IndexOf("KullAdi=") + 8));
                txtDB.Text = dbAdres;
                txtKullAdi.Text = dbKullAdi;
                txtSifre.Text = dbSifre;
            }
        }

        
        private void Form1_Load(object sender, EventArgs e)
        {
            LogKlasoru();
            AyarlariOku();
            timer1.Start();

        }
        void LogKlasoru()
        {
            if (!Directory.Exists(@"log"))
                Directory.CreateDirectory("log");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            AyarlariKaydet();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            ArizaSorgula();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UpdateEt();
        }

        private void btnDurdur_Click(object sender, EventArgs e)
        {
            if (btnDurdur.Text == "Durdur")
            {
                timer1.Stop();
                btnDurdur.Text = "Başlat";
            }
            else
            {
                timer1.Start();
                btnDurdur.Text = "Durdur";
            }
            
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            ArizaSorgula();
           //GunlukRaporGonder();
        }

        void TeleBildirim(string hastaneAdi, string birim, string bolum, string arizaAciklama, string arizaAcan, string tel)
        {
            string telegramMessage = @"HASTANE : " + hastaneAdi + "\nBIRIM : " + birim + "\nBOLUM : " + bolum + "\nSORUNU : " + arizaAciklama + "\nBILDIREN : " + arizaAcan + "\nTEL : " + tel;

            teleBot.SendTextMessageAsync(grupCode, telegramMessage);
        }
        void TeleBildirimTumu(string hastaneAdi, string birim, string bolum, string arizaAciklama, string arizaAcan, string tel)
        {
            string telegramMessageTumu = @"HASTANE : " + hastaneAdi + "\nBIRIM : " + birim + "\nBOLUM : " + bolum + "\nSORUNU : " + arizaAciklama + "\nBILDIREN : " + arizaAcan + "\nTEL : " + tel;

            teleBotTumu.SendTextMessageAsync(grupCodeTumu, telegramMessageTumu);
        }

        void TeleBildirimGalen(string hastaneAdi, string birim, string bolum, string arizaAciklama, string arizaAcan, string tel)
        {
            string telegramMessageGalen = @"HASTANE : " + hastaneAdi + "\nBIRIM : " + birim + "\nBOLUM : " + bolum + "\nSORUNU : " + arizaAciklama + "\nBILDIREN : " + arizaAcan + "\nTEL : " + tel;

            teleBotTumu.SendTextMessageAsync(grupGalen, telegramMessageGalen);
        }

        void TeleBildirimTinaz(string hastaneAdi, string birim, string bolum, string arizaAciklama, string arizaAcan, string tel)
        {
            string telegramMessageTinaz = @"HASTANE : " + hastaneAdi + "\nBIRIM : " + birim + "\nBOLUM : " + bolum + "\nSORUNU : " + arizaAciklama + "\nBILDIREN : " + arizaAcan + "\nTEL : " + tel;

            teleBotTumu.SendTextMessageAsync(grupTinaz, telegramMessageTinaz);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            GunlukRaporGonder();
        }

        private void GunlukRaporGonder()
        {
            string suan = DateTime.Now.ToShortTimeString();
           
            if (suan.Contains("10:"))
            {
                GunlukRaporSorgula();
                label3.Text = "Son Rapor Zamanı : " + DateTime.Now.ToShortDateString() + " / " + DateTime.Now.ToShortTimeString();
                label3.Visible = true;
            }
        }
        private void GunlukRaporSorgula()
        {
            string connstr = "data source=" + dbAdres + ";user id=" + dbKullAdi + ";password=" + dbSifre + ";";

            string cmdtxt =
                  @"SELECT 
                    DECODE(BAKIMYERI,'4','HBYS','1','BILGI ISLEM','2','BIYOMEDIKAL','0','TEKNIK SERVIS')BIRIM,
                    DECODE(NVL(DURUMU,10),'10','BEKLIYOR','2','TAMİRİ BİTTİ','5','TAMAMLANDI','4','GERİ GÖNDERİLDİ') DURUM,
                    COUNT(DURUMU) ADET
                    FROM TEK_ARIZA
                    WHERE 
                    TARIH BETWEEN TO_DATE(SYSDATE-4,'DD/MM/YYYY') AND TO_DATE(SYSDATE-1,'DD/MM/YYYY')
                    and SORUNU NOT IN ('Bakım İsteği') AND BAKIMYERI IN (1,2,4,0) AND ( DURUMU IN (2,5,4) OR DURUMU IS NULL )
                    GROUP BY BAKIMYERI, DURUMU
                    ORDER BY BIRIM";
            try
            {

                using (OracleConnection conn = new OracleConnection(connstr))
                using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                {
                    conn.Open();
                    using (OracleDataReader reader = cmd.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        dgwList.DataSource = dataTable;
                    }
                }

                String[] baslik = new string[4];
                String[] govde = new string[100];
                string baslikTest = "";
                string govdeTest = "";
                string govdeTestt = "";

                for (int i = 1; i < dgwList.Columns.Count + 1; i++)
                {
                    baslikTest += dgwList.Columns[i - 1].HeaderText + " | ";
                    //baslik[i] = dgwList.Columns[i - 1].HeaderText;
                }

                for (int i = 0; i < dgwList.Rows.Count; i++)
                {
                    for (int j = 0; j < dgwList.Columns.Count; j++)
                    {
                        govdeTest += dgwList.Rows[i].Cells[j].Value.ToString() + " | ";
                        //govde[j] = dgwList.Rows[i].Cells[j].Value.ToString();
                    }
                    govdeTestt += govdeTest + "\n";
                    govdeTest = "";
                }
                gunlukRaporIcerik = " 📅 SON 3 GÜNÜN RAPORU 📅 \n" + baslikTest + "\n" + govdeTestt;
               // teleBotTumu.SendTextMessageAsync(grupCodeTumu, gunlukRaporIcerik);
                  teleBot.SendTextMessageAsync(txtChatID.Text, gunlukRaporIcerik);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Select Hatası " + ex.Message);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (txtMesaj.Text != "" && txtChatID.Text != "")
            {
                teleBot.SendTextMessageAsync(txtChatID.Text, txtMesaj.Text);
                txtMesaj.Text = "";
            }
            else MessageBox.Show("Chat ID veya Mesaj alanı boş!");
        }

        private void ArizaSorgula()
        {
            string hastaneAdi = "";
            string birim = "";
            string bakimYeri = "";
            string bolum = "";
            string aciklama = "";
            string acanKullanici = "";
            string tel = "";
            string arizaNo = "";

            string connstr = "data source=" + dbAdres + ";user id=" + dbKullAdi + ";password=" + dbSifre + ";";

            string cmdtxt =
                  @"select 'TINAZTEPE' HASTANE_ADI,
                    DECODE(BAKIMYERI,'4','HBYS','1','BILGI ISLEM','2','BIYOMEDIKAL','0','TEKNIK SERVIS')BIRIM,
                    BOLUMADI,SORUNU,ACANKULLANICI,TELEFON,ARIZA_NO,BAKIMYERI FROM hastane.tek_ariza where TELEBILDIRIM = 'F' AND BAKIMYERI <> 6
                    UNION
                    select 'GALEN' HASTANE_ADI,
                    DECODE(BAKIMYERI,'4','HBYS','1','BILGI ISLEM','2','BIYOMEDIKAL','0','TEKNIK SERVIS')BIRIM,
                    BOLUMADI,SORUNU,ACANKULLANICI,TELEFON,ARIZA_NO,BAKIMYERI FROM hastane.tek_ariza@GALEN where TELEBILDIRIM = 'F' AND BAKIMYERI <> 6
                    UNION
                    select 'BUCATIP' HASTANE_ADI,
                    DECODE(BAKIMYERI,'4','HBYS','1','BILGI ISLEM','2','BIYOMEDIKAL','0','TEKNIK SERVIS')BIRIM,
                    BOLUMADI,SORUNU,ACANKULLANICI,TELEFON,ARIZA_NO,BAKIMYERI FROM hastane.tek_ariza@BUCATIP where TELEBILDIRIM = 'F' AND BAKIMYERI <> 6 ";
            try
            {
                using (OracleConnection conn = new OracleConnection(connstr))
                using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                {
                    conn.Open();

                    // reader is IDisposable and should be closed
                    using (OracleDataReader dr = cmd.ExecuteReader())
                    {
                        //List<String> items = new List<String>();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                //items.Add(String.Format("{0}", dr.GetValue(0)));
                                hastaneAdi = dr[0].ToString();
                                birim = dr[1].ToString();
                                bolum = dr[2].ToString();
                                aciklama = dr[3].ToString();
                                acanKullanici = dr[4].ToString();
                                tel = dr[5].ToString();
                                arizaNo = dr[6].ToString();
                                bakimYeri = dr[7].ToString();

                                //LogOlustur(arizaNo, hastaneAdi, birim, bolum, aciklama, acanKullanici, tel);

                                if (hastaneAdi == "GALEN")
                                {
                                    TeleBildirimGalen(hastaneAdi, birim, bolum, aciklama, acanKullanici, tel);
                                }
                                if (hastaneAdi == "TINAZTEPE")
                                {
                                    TeleBildirimTinaz(hastaneAdi, birim, bolum, aciklama, acanKullanici, tel);
                                }

                                if (bakimYeri == "4" || bakimYeri == "1")
                                {
                                    TeleBildirim(hastaneAdi, birim, bolum, aciklama, acanKullanici, tel);
                                }
                                TeleBildirimTumu(hastaneAdi, birim, bolum, aciklama, acanKullanici, tel);
                                arizaSayisi++;
                                lblArizaSayisi.Text = "Bildirilen Arıza Sayısı : " + arizaSayisi;
                                //dgwList.Rows.Add(hastaneAdi, arizaNo, birim);
                            }
                            UpdateEt();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Select Hatası " + ex.Message);
            }
        }

        void UpdateEt()
        {
            string connstr = "data source=" + dbAdres + ";user id=" + dbKullAdi + ";password=" + dbSifre + ";";

            string cmdtxt =
                @"update hastane.tek_ariza set telebildirim = :teleBildirim where telebildirim = 'F'";
            try
            {
                OracleDataAdapter da = new OracleDataAdapter();
                using (OracleConnection conn = new OracleConnection(connstr))
                using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                {
                    conn.Open();

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = cmdtxt;
                    cmd.Parameters.Add(":teleBildirim", "T");

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                }

                cmdtxt =
                @"update hastane.tek_ariza@GALEN set telebildirim = :teleBildirim where telebildirim = 'F'";
                using (OracleConnection conn = new OracleConnection(connstr))
                using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                {
                    conn.Open();

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = cmdtxt;
                    cmd.Parameters.Add(":teleBildirim", "T");

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                }

                cmdtxt =
                @"update hastane.tek_ariza@BUCATIP set telebildirim = :teleBildirim where telebildirim = 'F'";
                using (OracleConnection conn = new OracleConnection(connstr))
                using (OracleCommand cmd = new OracleCommand(cmdtxt, conn))
                {
                    conn.Open();

                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = cmdtxt;
                    cmd.Parameters.Add(":teleBildirim", "T");

                    cmd.ExecuteNonQuery();
                    cmd.Dispose();
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Update Hatası : " + ex.Message);
            }

        }

        void LogOlustur(string arizaNo, string hastaneAdi, string birim, string bolum, string arizaAciklama, string arizaAcan, string tel)
        {

            string time = DateTime.Now.ToShortDateString();
            string sPath = "MedTeknik_" + time + ".log";
            string str_Path = HostingEnvironment.ApplicationPhysicalPath + ("Log") + "\\" + sPath;
            string log = hastaneAdi + ";" + arizaNo + ";" + birim + ";" + bolum + ";" + arizaAciklama + ";" + arizaAcan + ";" + tel;

            try
            {
                if (!File.Exists(str_Path))
                {
                    File.Create(str_Path).Close();
                    File.AppendAllText(str_Path, log + Environment.NewLine);
                }
                else if (File.Exists(str_Path))
                {
                    File.AppendAllText(str_Path, log + Environment.NewLine);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Log yazarken hata : " + ex);
            }


        }

    }
}
