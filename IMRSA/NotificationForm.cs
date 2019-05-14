using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IMRSA
{
    public partial class NotificationForm : Form
    {
        public NotificationForm()
        {
            InitializeComponent();
        }

        private void NotificationForm_Load(object sender, EventArgs e)
        {

        }
        string name, IP;
        public NotificationForm(string name, string IP)
        {
            InitializeComponent();
            this.name = name;
            this.IP = IP;
        }

        private void notificationTempLabel_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {
            notificationTempLabel.Text = "File sending to " + IP + " " + name + "...";
        }
    }
}

