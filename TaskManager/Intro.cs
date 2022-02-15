using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TaskManager
{
    public partial class Intro : MetroFramework.Forms.MetroForm
    {
        public static bool isTheOpenMainPage { get; set; } = false;
        private int counter { get; set; }
        public Intro()
        {
            InitializeComponent();
        }
        
        private void Intro_Load(object sender, EventArgs e)
        {
            mediaPlayer.URL = "Task Manager2.mp4";
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            counter++;

            if (counter == 10)
            {
                MainPage mainPage = new MainPage();
                mainPage.Show();
                this.Hide();
            }
        }
    }
}
