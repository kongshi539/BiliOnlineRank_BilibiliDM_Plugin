using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BiliOnlineRank
{
    public partial class Form1 : Form
    {
        public event EventHandler<Userinfo> sendinfo;
        public Form1()
        {
            InitializeComponent();
       
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {  
            sendinfo += new EventHandler<Userinfo>(BliveOnline.loginconfig);
            sendinfo(this, new Userinfo(textBox1.Text, textBox2.Text));
            
        }
    }
}
