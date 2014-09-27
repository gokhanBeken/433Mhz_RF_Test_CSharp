using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;

namespace _433Mhz_RF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        byte[] gelenVeri = new byte[100];
        int gelenSayac = 0;
        int durum = 0;
        bool veriAlmaAktifMi = false;

        void GorunumModeAlici(string mode)
        {
            if (mode == "alici")
            {
                groupBox2.Text = "Last data";
                veriAlmaAktifMi = true;
                button2.Visible = false;
            }
            if (mode == "verici")
            {
                groupBox2.Text = "Print";
                veriAlmaAktifMi = false;
                button2.Visible = true;
            }
        }

        bool seri_port_baglan(string port, string baudRate)
        {
            try
            {
                serialPort1.BaudRate = int.Parse(baudRate);
                serialPort1.DataBits = int.Parse("8"); // Veri bit ini de 8 bit olarak verdik
                serialPort1.StopBits = System.IO.Ports.StopBits.One; // Durma bitini tek sefer olarak verdik.
                serialPort1.Parity = Parity.None; // eşlik bit ini vermedik.
                serialPort1.PortName = port;
                serialPort1.Open(); // Bağlantıyı açıyoruz

                return true;
            }

            catch (Exception) // Herhangi bir hata anında alttaki hata mesajını alacağız..
            {
                return false;
            }
        }

        public string trDuzelt(string a) //Türkçe karakerleri, ingilizce karakterlere çevirmek için kullanıyoruz
        {
            a = a.Replace("İ", "I");
            a = a.Replace("ı", "i");

            a = a.Replace("Ü", "U");
            a = a.Replace("ü", "u");

            a = a.Replace("Ş", "S");
            a = a.Replace("ş", "s");

            a = a.Replace("Ç", "C");
            a = a.Replace("ç", "c");

            a = a.Replace("Ğ", "G");
            a = a.Replace("ğ", "g");
            
            a = a.Replace("Ö", "O");
            a = a.Replace("ö", "o");

            return a;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen) //bağlıysa kapat
            {
                serialPort1.Close();
                comboBox1.Enabled = true;
                comboBox2.Enabled = true;
                label1.Text = "Status: Disconnected";
                button1.Text = "Conntect";
            }
            else //bağlı değilse, bağlan
            {
                if (seri_port_baglan(comboBox1.Text, comboBox2.Text) == true)
                {
                    comboBox1.Enabled = false;
                    comboBox2.Enabled = false;
                    label1.Text = "Status: Connected";
                    button1.Text = "Disconnetect";
                }
                else
                {
                    comboBox1.Enabled = true;
                    comboBox2.Enabled = true;
                    label1.Text = "Status: Error";
                    button1.Text = "Conntect";
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (!serialPort1.IsOpen) 
            {
                MessageBox.Show("Hata: Bağlı değilsiniz!");
                return; 
            }
            
            //preamble + senkronizasyon + veri

            byte[] SYNC = new byte[10] { 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0xAA, 0x01, 0XFE };
            serialPort1.Write(SYNC, 0, 10);
            seri_port_data_gonder(trDuzelt(textBox1.Text)); // + (Char)13

            seri_port_data_gonder("~~~~"); // garanti olsun diye bir kaç kere tilda(~) gönderiyorum birini alamazsa birini alır

            textBox2.Text += trDuzelt(textBox1.Text)+"\r\n";
            textBox1.Text = "";

            textBox1.Focus();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string[] portlar = SerialPort.GetPortNames();  // portları dizi halinde aldık

            foreach (string port in portlar)
            {
                comboBox1.Items.Add(port.ToString()); // Portlarımızı combobox ın içine aldık.
            }

            comboBox2.Items.Add("300"); // BaudRate
            comboBox2.Items.Add("150"); // BaudRate
            comboBox2.Items.Add("110"); // BaudRate
            comboBox2.Items.Add("75"); // BaudRate
            comboBox2.Items.Add("600"); // BaudRate
            comboBox2.Items.Add("1200"); // BaudRate
            comboBox2.Items.Add("2400"); // BaudRate
            comboBox2.Items.Add("4800"); // BaudRate
            comboBox2.Items.Add("9600"); // BaudRate

            if (comboBox1.Items.Count > 0) 
            { 
                comboBox1.SelectedIndex = 0; 
            }
            
            comboBox2.SelectedIndex = 0;

            radioButton1.Checked = true;

            CheckForIllegalCrossThreadCalls = false;

            this.MaximumSize = this.Size; //formun boyutlarini engelleyelim, kendi değerinden daha büyük olamasın
            this.MinimumSize = this.Size; //formun boyutlarini engelleyelim, kendi değerinden daha küçük olamasın

            textBox2.ReadOnly = true;


            //textBox1.Enabled = false;

        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (veriAlmaAktifMi == true)
            {
                byte[] veri = new byte[1];
                serialPort1.Read(veri, 0, 1);

                if (durum == 2)
                {

                    gelenVeri[gelenSayac] = veri[0];
                    gelenSayac++;
                    if (gelenSayac > 30) //not: 30 karakteri geçerse zaman aşımına uğramış kabul edip, iptal ediyoruz
                    {
                        gelenSayac = 0;
                        durum = 0;
                        for (int i = 0; i < 30; i++) gelenVeri[i] = 0; //verileri sil
                    }
                    else if (veri[0] == '~') //enter geldiyse, veriyi onaylayalım
                    {
                        textBox1.Text = "";
                        for (int i = 0; i < gelenSayac - 1; i++)
                        {
                            //gelenVeri[i] = 66;
                            textBox1.Text += ((char)gelenVeri[i]).ToString();

                        }


                        textBox2.Text += textBox1.Text + "\r\n";
                        for (int i = 0; i < 30; i++) gelenVeri[i] = 0; //verileri sil
                        gelenSayac = 0;
                        durum = 0;
                    }


                }
                else
                {

                    //eğer program çalıştırılınca, alıcı moduna geçirilirse bu kısım çalışacak
                    //string indata = serialPort1.ReadExisting();

                    //MessageBox.Show(veri[0].ToString());

                    if (durum == 0)
                    {
                        if (veri[0] == 0x01)
                        {
                            durum++;
                        }
                        else
                        {
                            durum = 0;
                        }
                    }
                    else if (durum == 1)
                    {
                        if (veri[0] == 0xFE)
                        {
                            durum++;
                        }
                        else
                        {
                            durum = 0;
                        }
                    }

                }
            }
        }

        private void seri_port_data_gonder(string gonderilecek_veri)
        {
            try
            {
                if (serialPort1.IsOpen)
                {
                    serialPort1.Write(gonderilecek_veri);
                    System.Threading.Thread.Sleep(150);
                }
                else
                {
                    MessageBox.Show("No connected");
                }
            }

            catch (Exception)
            {
                MessageBox.Show("Sonuç: Başarısız !", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button3);
            }

        }

        private void comboBox1_KeyDown(object sender, KeyEventArgs e)
        {
            // comboBox1 is readonly
            e.SuppressKeyPress = true;
        }

        private void comboBox2_KeyDown(object sender, KeyEventArgs e)
        {
            // comboBox2 is readonly
            e.SuppressKeyPress = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            GorunumModeAlici("verici");
            textBox1.ReadOnly = false;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            GorunumModeAlici("alici");
            textBox1.ReadOnly = true;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                
                button2.PerformClick();
                e.SuppressKeyPress = true; //enterin gorunmesini engeller
                //e.Handled = true;
            }
        }


    }
}
