using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Windows.Forms;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Threading;

namespace IMRSA
{
    public partial class Form1 : Form
    {
        private Stopwatch stops = null;
        Bitmap citraenkripsi = new Bitmap(256, 256);
        Bitmap citraen = new Bitmap(256, 256);
        double[,] asli = new double[256, 256];
        double[,] enkripted = new double[256, 256];
        double[,] decripted = new double[256, 256];
        BigInteger[,] enkripsi = new BigInteger[256, 256];
        byte[] txt;
        string E;
        byte[] textE;
        byte[] txtdec;
        int stride, progres;
        bool close = true;
      
        public Form1()
        {
            InitializeComponent();
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.WorkerReportsProgress = true;

            loading1.Minimum = 0;
            loading1.Maximum = 256;

            loading2.Minimum = 0;
            loading2.Maximum = 256;
        }

        private string IP = "127.0.0.1";
        TcpListener listener;
        TcpClient client;
        Socket socketForClient;
        private Thread serverThread;
        private Thread findPC;
        private Thread notification;
        int flag = 0;
        string fileName = "";
        string Message = "";
        private bool serverRunning = false;
        private bool isConnected = false;

        int x = 9;
        int y = 308;
        int fileReceived = 0;
        string savePath;
        string senderIP;
        string senderMachineName;
        string targetIP;
        string targetName;
        NotificationForm f2;
        Bitmap citra, citraencrip;


        void showNotification()
        {
            f2 = new NotificationForm(targetName, targetIP);
            f2.ShowDialog();
        }

        void startServer()
        {
            try
            {
                serverRunning = true;
                listener = new TcpListener(IPAddress.Parse(IP), 11000);
                listener.Start();
                serverThread = new Thread(new ThreadStart(serverTasks));
                serverThread.Start();
                while (!serverThread.IsAlive) ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void startServerMessage()
        {
            try
            {
                serverRunning = true;
                listener = new TcpListener(IPAddress.Parse(IP), 11000);
                listener.Start();
                serverThread = new Thread(new ThreadStart(serverTasksMessage));
                serverThread.Start();
                while (!serverThread.IsAlive) ;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        //thread: waiting for client request and receiving data two times and resets.

        void serverTasks()
        {
            try
            {
                while (true)
                {
                    if (fileReceived == 1)
                    {
                        if (MessageBox.Show("Save File?", "File received", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                        {
                            File.Delete(savePath);
                            fileReceived = 0;
                        }
                        else
                        {
                            fileReceived = 0;
                        }
                    }

                    client = listener.AcceptTcpClient();
                    Invoke((MethodInvoker)delegate
                    {
                        notificationPanel.Visible = true;
                        notificationTempLabel.Text = "File coming..." + "\n" + fileName + "\n" + "From: " + senderIP + " " + senderMachineName;
                        fileNotificationLabel.Text = "File Coming from " + senderIP + " " + senderMachineName;
                    });
                    isConnected = true;
                    NetworkStream stream = client.GetStream();
                    if (flag == 1 && isConnected)
                    {
                        savePath = savePathLabel.Text + "\\" + fileName;
                        using (var output = File.Create(savePath))
                        {
                            // read the file divided by 1KB
                            var buffer = new byte[1024];
                            int bytesRead;
                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                output.Write(buffer, 0, bytesRead);
                            }
                            //MessageBox.Show("ok");
                            flag = 0;
                            client.Close();
                            isConnected = false;
                            fileName = "";
                            Invoke((MethodInvoker)delegate
                            {
                                notificationTempLabel.Text = "";
                                notificationPanel.Visible = false;
                                fileNotificationLabel.Text = "";
                            });
                            fileReceived = 1;
                        }
                    }
                    else if (flag == 0 && isConnected)
                    {
                        Byte[] bytes = new Byte[256];
                        String data = null;
                        int i;
                        // Loop to receive all the data sent by the client.
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        }
                        string[] msg = data.Split('@');
                        fileName = msg[0];
                        senderIP = msg[1];
                        senderMachineName = msg[2];
                        client.Close();
                        isConnected= false;
                        flag = 1;
                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
                flag = 0;
                isConnected = false;
                if (client != null)
                    client.Close();
            }
        }

        void serverTasksMessage()
        {
            try
            {
                while (true)
                {
                    if (fileReceived == 1)
                    {
                        
                            fileReceived = 0;
                    
                    }

                    client = listener.AcceptTcpClient();
                    Invoke((MethodInvoker)delegate
                    {
                        if (!(Message == ""))
                        { listBox1.Items.Add("[" + senderIP + "] : " + Message); ; }
                    });
                    
                    isConnected = true;
                    NetworkStream stream = client.GetStream();
                    if (flag == 1 && isConnected)
                    {
                       
                            flag = 0;
                            client.Close();
                            isConnected = false;
                            Message = "";
                            fileReceived = 1;
                    
                    }
                    else if (flag == 0 && isConnected)
                    {
                        Byte[] bytes = new Byte[256];
                        Byte[] txtconvert;
                        String data = null;
                        int i;
                        // Loop to receive all the data sent by the client.
                        
                        while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                        {
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        }
                        string[] convert= data.Split('!');
                        //MessageBox.Show(convert[0]);
                        if (keyPublicMsg.Text == "" || keyPrivatemsg.Text == "")
                        {
                            MessageBox.Show("Pesan tidak dapat masuk. Harap input Public key dan Private key ");
                        }
                        else
                        {
                            decripMSG(convert);
                        }
                        string[] msg = E.Split('@');
                        if (msg.Length < 2)
                        {
                            Message = msg[0];
                            senderIP = "wrong key";
                        }
                        else
                        {
                            Message = msg[0];
                            senderIP = msg[1];
                        }
                        client.Close();
                        isConnected = false;
                        flag = 1;

                    }
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(ex.Message);
                flag = 0;
                isConnected = false;
                if (client != null)
                    client.Close();
            }
        }

        private void saveenkrip()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Encripted file (*.rsa*)|*.rsa*";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                string file_name = sfd.FileName + ".rsa";
                using (FileStream stream = File.Create(file_name))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    bf.Serialize(stream, enkripted);

                }
                MessageBox.Show("Tersimpan");

            }
        }

        public void RSA()
        {

            BigInteger red, green, blue, r, g, b;
            int e = int.Parse(InputE.Text);
            int n = int.Parse(publickey.Text);
            for (int i = 0; i < citra.Height; i++)
            {
                for (int j = 0; j < citra.Width; j++)
                {
                    Color c = citra.GetPixel(i, j);
                    red = c.R;
                    green = c.G;
                    blue = c.B;
                    int grey = (int)(red + green + blue) / 3;
                    asli[i, j] = grey;
                    red = BigInteger.Pow(grey, e);
                    red = (BigInteger)(red % n);
                    enkripted[i, j] = (double)red;
                    int warna = (int)enkripted[i, j];
                    if (warna < 0) { warna = 0; }
                    if (warna > 255) { warna = 255; }
                    Color enkrip = Color.FromArgb(warna, warna, warna);
                    citraenkripsi.SetPixel(i, j, enkrip);

                }

               backgroundWorker1.ReportProgress(i);

            }

        }

        public void RSATEXT(byte[] text)
        {
            E = "";
            BigInteger encripted;
            int e = int.Parse(keyEmsg.Text);
            int n = int.Parse(keyNmsg.Text);

            for (int j = 0; j < text.Length; j++)
            {
                int textenkrip = text[j];
                encripted = BigInteger.Pow(textenkrip, e);
                encripted = encripted % n;
                E = E+encripted.ToString()+"!";
            }
            textE = Encoding.Default.GetBytes(E);
        }

        private void opencitra()
        {
            Boolean cekk = true;
            openFileDialog1.Filter = "Image Files(*.jpg;*.bmp;*.tif)|*.jpg; *.bmp; *.tif";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                citra = new Bitmap(openFileDialog1.FileName);
                if (citra.Height != 256 && citra.Width != 256) { MessageBox.Show("Citra tidak sesuai"); }
                else
                {
                    for (int i = 0; i < citra.Width; i++)
                    {
                        for (int j = 0; j < citra.Height; j++)
                        {
                            int r = int.Parse(citra.GetPixel(i, j).R.ToString());
                            int g = int.Parse(citra.GetPixel(i, j).G.ToString());
                            int b = int.Parse(citra.GetPixel(i, j).B.ToString());
                            if (r == g && r == b && g == b)
                            { cekk = true; }
                            else { cekk = false; break; }
                        }
                    }
                    if (cekk)
                    {
                        btn_enkrip.Enabled = true;
                        pictureBox1.Image = citra;
                        width.Text = (citra.Width).ToString();
                        height.Text = (citra.Height).ToString();
                    }
                    else { MessageBox.Show("Citra hasrus greyscale"); citra = null; btn_enkrip.Enabled = false; }
                }
            }
        }

        public void keyy()
        {
            Boolean cek = true;
            Boolean cek1 = false;
            Boolean prima1 = true, prima2 = true;
            double p, q, e, N, No, b, ec;
            double d;
            d = 0;
            if (InputP.Text == "" || InputQ.Text == "" || InputE.Text == "")
            { MessageBox.Show("Harap inputkan nilai P,Q dan e dengan lengkap"); }
            else if (InputP.Text == InputQ.Text) { MessageBox.Show("P tidak boleh sama dengan Q"); InputP.Text = ""; InputQ.Text = ""; InputE.Text = ""; publickey.Text = ""; privatekey.Text = ""; }
            else
            {
                p = int.Parse(InputP.Text);
                double jumb = 0, sisa, x;
                for (x = 1; x <= p; x++)
                {
                    sisa = p % x;
                    if (sisa == 0) { jumb = jumb + 1; }
                    else { jumb = jumb; }
                }
                if (jumb > 2) { prima1 = false; MessageBox.Show("Inputan P bukan bilangan prima"); }
                q = int.Parse(InputQ.Text);
                jumb = 0;
                for (x = 1; x <= q; x++)
                {
                    sisa = q % x;
                    if (sisa == 0) { jumb = jumb + 1; }
                    else { jumb = jumb; }
                }
                if (jumb > 2) { prima2 = false; MessageBox.Show("Inputan Q bukan bilangan prima"); }
                e = int.Parse(InputE.Text);
                if (prima1 && prima2)
                {
                    N = p * q;
                    No = (p - 1) * (q - 1);
                    ec = e;
                    double p1, q1;
                    p1 = e;
                    q1 = No;

                    while (cek)
                    {
                        ec = q1 % p1;
                        if (ec == 0)
                        { cek = false; }
                        else { q1 = p1; p1 = ec; if (ec == 1) { cek1 = true; } }
                    }
                    int i = 1;
                    while (cek1)
                    {
                        d = (1 + (i * No)) / e;
                        if ((d % 2 != 0) && (d % 2 != 1) && (d % 2 != -1))
                        { }
                        else
                        {
                            d = d;
                            break;
                        }

                        i++;
                    }

                    //InputE.Text = Convert.ToString(e);
                    if (cek1 == true)
                    {
                        publickey.Text = Convert.ToString(N);
                        privatekey.Text = Convert.ToString(d);
                    }
                    else { MessageBox.Show("Masukan bilangan prima 'e' yang sesuai dengan syarat perhitungan"); }
                }
            }

        }

        public void keyyMsg()
        {
            Boolean cek = true;
            Boolean cek1 = false;
            Boolean prima1 = true, prima2 = true;
            double p, q, e, N, No, b, ec;
            double d;
            d = 0;
            if (keyPmsg.Text == "" || keyQmsg.Text == "" || keyEmsg.Text == "")
            { MessageBox.Show("Harap inputkan nilai P,Q dan e dengan lengkap"); }
            else if (keyPmsg.Text == keyQmsg.Text) { MessageBox.Show("P tidak boleh sama dengan Q"); keyPmsg.Text = ""; keyQmsg.Text = ""; keyEmsg.Text = ""; keyNmsg.Text = ""; keyDmsg.Text = ""; }
            else
            {
                p = int.Parse(keyPmsg.Text);
                double jumb = 0, sisa, x;
                for (x = 1; x <= p; x++)
                {
                    sisa = p % x;
                    if (sisa == 0) { jumb = jumb + 1; }
                    else { jumb = jumb; }
                }
                if (jumb > 2) { prima1 = false; MessageBox.Show("Inputan P bukan bilangan prima"); }
                q = int.Parse(keyQmsg.Text);
                jumb = 0;
                for (x = 1; x <= q; x++)
                {
                    sisa = q % x;
                    if (sisa == 0) { jumb = jumb + 1; }
                    else { jumb = jumb; }
                }
                if (jumb > 2) { prima2 = false; MessageBox.Show("Inputan Q bukan bilangan prima"); }
                e = int.Parse(keyEmsg.Text);
                if (prima1 && prima2)
                {
                    N = p * q;
                    No = (p - 1) * (q - 1);
                    ec = e;
                    double p1, q1;
                    p1 = e;
                    q1 = No;

                    while (cek)
                    {
                        ec = q1 % p1;
                        if (ec == 0)
                        { cek = false; }
                        else { q1 = p1; p1 = ec; if (ec == 1) { cek1 = true; } }
                    }
                    int i = 1;
                    while (cek1)
                    {
                        d = (1 + (i * No)) / e;
                        if ((d % 2 != 0) && (d % 2 != 1) && (d % 2 != -1))
                        { }
                        else
                        {
                            d = d;
                            break;
                        }

                        i++;
                    }

                    //InputE.Text = Convert.ToString(e);
                    if (cek1 == true)
                    {
                        keyNmsg.Text = Convert.ToString(N);
                        keyDmsg.Text = Convert.ToString(d);
                    }
                    else { MessageBox.Show("Masukan bilangan prima 'e' yang sesuai dengan syarat perhitungan"); }
                }
            }

        }

        private void generateKey()
        {
            int a, i, k;
            i = 0;
            k = 0;
            a = 0;
            int[] prima = new int[48];
            Random rand = new Random();
            i = 0;
            k = 200;
            for (i = i; i <= k; i++)
            {
                bool isPrime = true;
                for (int j = 2; j <= Math.Sqrt(i); j++)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    prima[a] = i;
                    a++;
                }
            }
            int randomindex = rand.Next(4, 24);
            int randomindex1 = rand.Next(12, 28);
            int randomindex2 = rand.Next(25, 48);
            int randomnumer = prima[randomindex];
            int randomnumer1 = prima[randomindex1];
            int randomnumber2 = prima[randomindex2];
            if (randomnumber2 == randomnumer1 || randomnumber2 == randomnumer)
            { InputP.Text = ""; InputQ.Text = ""; InputE.Text = ""; publickey.Text = ""; privatekey.Text = ""; MessageBox.Show("Coba generate key lagi untuk angka yang optimal"); }
            else
            {
                InputP.Text = Convert.ToString(randomnumer);
                InputQ.Text = Convert.ToString(randomnumer1);
                InputE.Text = Convert.ToString(randomnumber2);
                keyy();
            }
        }

        private void generateKeyMsg()
        {
            int a, i, k;
            i = 0;
            k = 0;
            a = 0;
            int[] prima = new int[48];
            Random rand = new Random();
            i = 0;
            k = 200;
            for (i = i; i <= k; i++)
            {
                bool isPrime = true;
                for (int j = 2; j <= Math.Sqrt(i); j++)
                {
                    if (i % j == 0)
                    {
                        isPrime = false;
                        break;
                    }
                }
                if (isPrime)
                {
                    prima[a] = i;
                    a++;
                }
            }
            int randomindex = rand.Next(4, 24);
            int randomindex1 = rand.Next(12, 28);
            int randomindex2 = rand.Next(25, 48);
            int randomnumer = prima[randomindex];
            int randomnumer1 = prima[randomindex1];
            int randomnumber2 = prima[randomindex2];
            if (randomnumber2 == randomnumer1 || randomnumber2 == randomnumer)
            { InputP.Text = ""; InputQ.Text = ""; InputE.Text = ""; publickey.Text = ""; privatekey.Text = ""; MessageBox.Show("Coba generate key lagi untuk angka yang optimal"); }
            else
            {
                keyPmsg.Text = Convert.ToString(randomnumer);
                keyQmsg.Text = Convert.ToString(randomnumer1);
                keyEmsg.Text = Convert.ToString(randomnumber2);
                keyyMsg();
            }
        }

        private void searchPC()
        {
            bool isNetworkUp = NetworkInterface.GetIsNetworkAvailable();
            if (isNetworkUp)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        this.IP = ip.ToString();
                    }
                }
                Invoke((MethodInvoker)delegate
                {
                    infoLabel.Text = "This Computer: " + this.IP;
                });
                string[] ipRange = IP.Split('.');
                for (int i = 100; i < 255; i++)
                {
                    Ping ping = new Ping();
                    //string testIP = "192.168.1.67";
                    string testIP = ipRange[0] + '.' + ipRange[1] + '.' + ipRange[2] + '.' + i.ToString();
                    if (testIP != this.IP)
                    {
                        ping.PingCompleted += new PingCompletedEventHandler(pingCompletedEvent);
                        ping.SendAsync(testIP, 100, testIP);
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Green;
                    notificationLabel.Text = "Application is Online";
                });
                //Starting this program as a server.
                if (!serverRunning)
                    startServer();
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Red;
                    notificationLabel.Text = "Application is Offline";
                });
                MessageBox.Show("Not connected to LAN");
            }
        }

        private void searchPCMessage()
        {
            bool isNetworkUp = NetworkInterface.GetIsNetworkAvailable();
            if (isNetworkUp)
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        this.IP = ip.ToString();
                    }
                }
                Invoke((MethodInvoker)delegate
                {
                    infoLabel.Text = "This Computer: " + this.IP;
                });
                string[] ipRange = IP.Split('.');
                for (int i = 100; i < 255; i++)
                {
                    Ping ping = new Ping();
                    //string testIP = "192.168.1.67";
                    string testIP = ipRange[0] + '.' + ipRange[1] + '.' + ipRange[2] + '.' + i.ToString();
                    if (testIP != this.IP)
                    {
                        ping.PingCompleted += new PingCompletedEventHandler(pingCompletedEvent);
                        ping.SendAsync(testIP, 100, testIP);
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Green;
                    notificationLabel.Text = "Application is Online";
                });
                //Starting this program as a server.
                if (!serverRunning)
                    startServerMessage();
            }
            else
            {
                Invoke((MethodInvoker)delegate
                {
                    notificationLabel.ForeColor = Color.Red;
                    notificationLabel.Text = "Application is Offline";
                });
                MessageBox.Show("Not connected to LAN");
            }
        }
        private void opencitra2()
        {
            openFileDialog2.Filter = "Encripted Files (*.rsa*)|*.rsa*";
            string[,] file = new string[256, 256];

            if (openFileDialog2.ShowDialog() == DialogResult.OK)
            {
                Bitmap citradecripsi = new Bitmap(256, 256);
                string file_name = openFileDialog2.FileName;
                MemoryStream ms = new MemoryStream();
                using (FileStream stream = File.Open(file_name, FileMode.Open))
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    decripted = (Double[,])bf.Deserialize(stream);
                }
                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        enkripsi[i, j] = (BigInteger)(decripted[i, j]);
                    }
                }
                for (int i = 0; i < 256; i++)
                {
                    for (int j = 0; j < 256; j++)
                    {
                        int warna = (int)decripted[i, j];
                        if (warna < 0) { warna = 0; }
                        if (warna > 255) { warna = 255; }
                        Color c = Color.FromArgb(warna, warna, warna);
                        citradecripsi.SetPixel(i, j, c);
                    }
                }
                textBox2.Text = citradecripsi.Width.ToString();
                textBox1.Text = citradecripsi.Height.ToString();
                pictureBox4.Image = citradecripsi;

                //MessageBox.Show("Tersimpan");
            }


        }

        void pingCompletedEvent(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply.Status == IPStatus.Success)
            {
                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                }
                catch (SocketException ex)
                {
                    name = ex.Message;
                }
                Invoke((MethodInvoker)delegate
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = ip;
                    item.SubItems.Add(name);
                    onlinePCList.Items.Add(item);
                });
            }
        }

        void pingCompletedEvent2(object sender, PingCompletedEventArgs e)
        {
            string ip = (string)e.UserState;
            if (e.Reply.Status == IPStatus.Success)
            {
                string name;
                try
                {
                    IPHostEntry hostEntry = Dns.GetHostEntry(ip);
                    name = hostEntry.HostName;
                }
                catch (SocketException ex)
                {
                    name = ex.Message;
                }
                Invoke((MethodInvoker)delegate
                {
   
                });
            }
        }

        public void decrip()
        {
            
            double[,] reds = new double[256, 256];
            BigInteger red, green, blue, r, g, b;
            int e = int.Parse(privatekeydekrip.Text);
            int n = int.Parse(publickeydekrip.Text);

            for (int i = 0; i < 256; i++)
            {
                for (int j = 0; j < 256; j++)
                {

                    BigInteger grey = (BigInteger)decripted[i, j];
                    red = BigInteger.Pow(grey, e);
                    red = red % n;
                    if (red < 0) { red = 0; }
                    if (red > 255) { red = 255; }
                    Color c = Color.FromArgb((int)red, (int)red, (int)red);
                    citraen.SetPixel(i, j, c);
                }
                int prog = i;
                backgroundWorker2.ReportProgress(i);
            }
            //savedecrip();

        }

        public void decripMSG(String[] txtdecrip)
        {

            BigInteger decriptext;
            int e = int.Parse(keyPrivatemsg.Text);
            int n = int.Parse(keyPublicMsg.Text);
            string character = "";
            for (int i = 0; i < txtdecrip.Length-1; i++)
            {
                    BigInteger txtdecripted = BigInteger.Parse(txtdecrip[i]);
                    decriptext = BigInteger.Pow(txtdecripted, e);
                    decriptext = decriptext % n;
                    if (decriptext < 0 || decriptext > 255)
                    { decriptext = 42; }
                    character = character+(Char.ConvertFromUtf32((int)decriptext));
            }
            E = character;
            //savedecrip();

        }

        private void btn_enkrip_Click(object sender, EventArgs e)
        {
            if (InputP.Text == "" && InputQ.Text == "" && InputE.Text == "" && publickey.Text == "" && privatekey.Text == "")
            { MessageBox.Show("Kunci Enkripsi belum lengkap"); }
            else if (pictureBox1.Image == null) { MessageBox.Show("Citra yang akan dienkripsi belum ada"); }
            else
            {
                stops = new Stopwatch();
                timer1.Enabled = true;
                stops.Start();
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            RSA();
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            loading1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Proses Dibatalkan");

            }
            else
            {
                pictureBox3.Image = citraenkripsi;
                MessageBox.Show("Proses Selesai");
            }
        }

        private void btn_save_Click(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null)
            { MessageBox.Show("Citra yang akan disimpan belum ada"); }
            else
            {
                saveenkrip();
            }
        }

        private void btn_key_Click_1(object sender, EventArgs e)
        {
            generateKey();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ipBox.Text = "";
            onlinePCList.Items.Clear();
            notificationLabel.ForeColor = Color.Green;
            notificationLabel.Text = "Finding...";
            //searchPC();
            try
            {
                findPC = new Thread(new ThreadStart(searchPC));
                findPC.Start();
                button6.Enabled = false;
                button5.Enabled = false;

                while (!findPC.IsAlive) ;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            notificationLabel.ForeColor = Color.Red;
            notificationLabel.Text = "Application is offline";
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serverRunning)
            {
                serverRunning = false;
                onlinePCList.Items.Clear();
                if (listener != null)
                    listener.Stop();
                if (serverThread != null)
                {
                    serverThread.Abort();
                    serverThread.Join();
                }

                notificationLabel.ForeColor = Color.Red;
                notificationLabel.Text = "Application is Offline";
                button1.Enabled = true;
                button6.Enabled = true;
                button5.Enabled = true;
                sendFileButton.Enabled = true;
                fileNameLabel.Text = ".";
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browse = new FolderBrowserDialog();
            if (browse.ShowDialog() == DialogResult.OK)
            {
                string savePath = browse.SelectedPath;
                savePathLabel.Text = savePath;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "All Files|*.*";
            openFileDialog1.Title = "Select a File";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileNameLabel.Text = openFileDialog1.FileName;  //file path
                fileNameLabel.Tag = openFileDialog1.SafeFileName; //file name only.
            }
            timer3.Start();
        }

        private void sendFileButton_Click(object sender, EventArgs e)
        {
            targetIP = null;
            targetName = null;
            if ((onlinePCList.SelectedIndices.Count > 0 || ipBox.Text != "") && serverRunning && fileNameLabel.Text != ".")
            {
                if (ipBox.Text != "")
                {
                    targetIP = ipBox.Text;
                    targetName = "";
                }
                else
                {
                    targetIP = onlinePCList.SelectedItems[0].Text;
                    targetName = onlinePCList.SelectedItems[0].SubItems[1].Text;
                }
                try
                {
                    Ping p = new Ping();
                    PingReply r;
                    r = p.Send(targetIP);
                    if (!(r.Status == IPStatus.Success))
                    {
                        MessageBox.Show("Target computer is not available.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        notification = new Thread(new ThreadStart(showNotification));
                        notification.Start();
                        //notificationPanel.Visible = true;
                        //notificationTempLabel.Text = "File sending to " + targetIP + " " + targetName + "...";
                        fileNotificationLabel.Text = "Please don't do other tasks. File sending to " + targetIP + " " + targetName + "...";
                        //closing the server
                        listener.Stop();
                        serverThread.Abort();
                        serverThread.Join();
                        serverRunning = false;
                        //now making this program a client
                        socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                        string fileName = fileNameLabel.Tag.ToString();
                        //long fileSize = new FileInfo(fileNameLabel.Text).Length;
                        byte[] fileNameData = Encoding.Default.GetBytes(fileName + "@" + this.IP + "@" + Environment.MachineName);
                        socketForClient.Send(fileNameData);
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                        socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                        socketForClient.SendFile(fileNameLabel.Text);
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                        //notification.Abort();
                        //notification.Join();
                        //notificationTempLabel.Text = "";
                        //notificationPanel.Visible = false;
                        Invoke((MethodInvoker)delegate
                        {
                            f2.Dispose();
                        });
                        MessageBox.Show("File sent to " + targetIP + " " + targetName, "Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (socketForClient != null)
                    {
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                    }
                }
                finally
                {
                    for (int i = 0; i < onlinePCList.SelectedIndices.Count; i++)
                    {
                        onlinePCList.Items[this.onlinePCList.SelectedIndices[i]].Selected = false;
                    }
                    fileNotificationLabel.Text = ".";
                    //again making this program a server
                    startServer();
                }
            }
        }

        private void timer3_Tick(object sender, EventArgs e)
        {
        
        }

        private void button8_Click(object sender, EventArgs e)
        {
            opencitra2();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (publickeydekrip.Text == "" && privatekeydekrip.Text == "")
            { MessageBox.Show("Kunci untuk dekripsi belum lengkap"); }
            else if (pictureBox4.Image == null) { MessageBox.Show("Citra yang akan didekripsi belum ada"); }
            else
            { 
                if (!backgroundWorker2.IsBusy)
                {
                    backgroundWorker2.RunWorkerAsync();
                }
            }

        }


        private void btn_open_Click_1(object sender, EventArgs e)
        {
            opencitra();
        }

        private void btn_enkrip_Click_1(object sender, EventArgs e)
        {
            if (InputP.Text == "" && InputQ.Text == "" && InputE.Text == "" && publickey.Text == "" && privatekey.Text == "")
            { MessageBox.Show("Kunci Enkripsi belum lengkap"); }
            else if (pictureBox1.Image == null) { MessageBox.Show("Citra yang akan dienkripsi belum ada"); }
            else
            {
                
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
            }
        }

        private void btn_save_Click_1(object sender, EventArgs e)
        {
            if (pictureBox3.Image == null)
            { MessageBox.Show("Citra yang akan disimpan belum ada"); }
            else
            {
                saveenkrip();
            }
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            decrip();
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            loading2.Value = e.ProgressPercentage;
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Proses Dibatalkan");
            }
            else
            {
                pictureBox2.Image = citraen;
                MessageBox.Show("Proses Selesai");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void button5_Click_1(object sender, EventArgs e)
        {
            targetIP = null;
            targetName = null;
            if ((onlinePCList.SelectedIndices.Count > 0 || ipBox.Text != "" || keyPmsg.Text!="" || keyQmsg.Text != "" || keyEmsg.Text != "" || keyNmsg.Text != "" || keyDmsg.Text != "") && serverRunning)
            {
                if (ipBox.Text != "")
                {
                    targetIP = ipBox.Text;
                    targetName = "";
                }
                else
                {
                    targetIP = onlinePCList.SelectedItems[0].Text;
                    targetName = onlinePCList.SelectedItems[0].SubItems[1].Text;
                }
                try
                {
                    Ping p = new Ping();
                    PingReply r;
                    r = p.Send(targetIP);
                    if (!(r.Status == IPStatus.Success))
                    {
                        MessageBox.Show("Target computer is not available.", "", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {

                        //notificationPanel.Visible = true;
                        //notificationTempLabel.Text = "File sending to " + targetIP + " " + targetName + "...";
                        fileNotificationLabel.Text = "Please don't do other tasks. File sending to " + targetIP + " " + targetName + "...";
                        //closing the server
                        listener.Stop();
                        serverThread.Abort();
                        serverThread.Join();
                        serverRunning = false;
                        //now making this program a client
                        socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                        string Message = messagetext.Text + "@" + this.IP;
                        String MessageSend = messagetext.Text;
                        //long fileSize = new FileInfo(fileNameLabel.Text).Length;
                        byte[] msgData = Encoding.Default.GetBytes(Message);
                        RSATEXT(msgData);
                        socketForClient.Send(textE);
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                        socketForClient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        socketForClient.Connect(new IPEndPoint(IPAddress.Parse(targetIP), 11000));
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                        //notification.Abort();
                        //notification.Join();
                        //notificationTempLabel.Text = "";
                        //notificationPanel.Visible = false;
                        listBox1.Items.Add("[" + this.IP + "] : " + MessageSend);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    if (socketForClient != null)
                    {
                        socketForClient.Shutdown(SocketShutdown.Both);
                        socketForClient.Close();
                    }
                }
                finally
                {
                    for (int i = 0; i < onlinePCList.SelectedIndices.Count; i++)
                    {
                        onlinePCList.Items[this.onlinePCList.SelectedIndices[i]].Selected = false;
                    }
                    fileNotificationLabel.Text = ".";
                    //again making this program a server
                    startServerMessage();
                }
            }
            else
            {
                MessageBox.Show("IP tujuan dan key harap diisi harap diisi");
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ipBox.Text = "";
            onlinePCList.Items.Clear();
            notificationLabel.ForeColor = Color.Green;
            notificationLabel.Text = "Finding...";
            //searchPC();
            try
            {
                findPC = new Thread(new ThreadStart(searchPCMessage));
                findPC.Start();
                button1.Enabled = false;
                sendFileButton.Enabled = false;
                while (!findPC.IsAlive) ;
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serverRunning)
            {
                listener.Stop();
                serverThread.Abort();
            }
                
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            generateKeyMsg();
        }

        private void clearButton_Click(object sender, EventArgs e)
        {
            listBox1.Text = "";
        }

        private void btn_open_Click(object sender, EventArgs e)
        {
            opencitra();
        }

        private void btn_key_Click(object sender, EventArgs e)
        {
            generateKey();
        }
    }
}
