using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static pomidor.Form1;
using System.Windows;

namespace pomidor
{
    public partial class Form_Nortification : Form
    {
        public delegate void StopClick(object sender, EventArgs e);
        public delegate void StartFocus(object sender, EventArgs e);
        public delegate void StartBreak(object sender, EventArgs e);
        public delegate void StartLongBreak(object sender, EventArgs e);
        public event StopClick stopTimer;
        public event StartFocus startFocus;
        public event StartBreak startBreak;
        public event StartLongBreak startLong;
        private int _timer_type = 0;
        public Form_Nortification(Form1 f1)
        {
            InitializeComponent();
            stopTimer += f1.StopClick;
            startFocus += f1.StartFocus;
            startBreak += f1.StartBreak;
            startLong += f1.StartLongBreak;
            this.TopMost = true;
        }
        public enum enmAction
        {
            wait,
            start,
            close
        }
        private Form_Nortification.enmAction action;
        private int x, y;
        public void showAlert(string msg, int type, int long_break_n)
        {
            base.BackColor = this.button1.BackColor = this.button2.BackColor = System.Drawing.Color.SeaGreen;
            _timer_type = type;
            switch (type)
            {
                case 1:
                    this.button1.Text = "Начать фокусировку";
                    base.BackColor = this.button1.BackColor = this.button2.BackColor = System.Drawing.Color.Brown;
                    break;
                case 2:
                    this.button1.Text = "Начать перерыв";
                    break;
                case 3:
                    this.button1.Text = "Начать длинный перерыв";
                    break;
            }
            this.Opacity = 0.0;
            this.StartPosition = FormStartPosition.Manual;
            string fname;

            for (int i = 1; i < 10; i++)
            {
                fname = "alert" + i.ToString();
                Form_Nortification frm = (Form_Nortification)System.Windows.Forms.Application.OpenForms[fname];

                if (frm == null)
                {
                    this.Name = fname;
                    this.x = Screen.PrimaryScreen.WorkingArea.Width - this.Width + 15;
                    this.y = Screen.PrimaryScreen.WorkingArea.Height - this.Height * i - 5 * i;
                    this.Location = new System.Drawing.Point(this.x, this.y);
                    break;

                }

            }
            this.x = Screen.PrimaryScreen.WorkingArea.Width - base.Width - 5;

            
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE (date-(date%86400))/86400 = " + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds() / 86400) + " AND type = 2", connection);
                int current_n = 0;
                int current_lb = 0;
                int current_t = 0;
                current_n = Convert.ToInt32(command.ExecuteScalar());
                command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE (date-(date%86400))/86400 = " + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds() / 86400) + " AND type = 3", connection);

                current_lb = Convert.ToInt32(command.ExecuteScalar());
                command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE (date-(date%86400))/86400 = " + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds() / 86400) + " AND type = 1", connection);

                current_t = Convert.ToInt32(command.ExecuteScalar());
                this.label1.Text = Convert.ToString(current_t)+" Кукумберов за сегодня";
                //System.Windows.Forms.MessageBox.Show(Convert.ToString(current_n)+' '+Convert.ToString(current_lb)+' ' +Convert.ToString(long_break_n)+ ' ' + Convert.ToString((current_n + current_lb) % long_break_n));
                if ((current_n + current_lb) % long_break_n != long_break_n - 1)
                {
                    this.label1.Text += "\n" + Convert.ToString(Math.Abs(long_break_n-(current_n + current_lb) % long_break_n)-1) + " до длинного перерыва";
                } else
                {
                    this.label1.Text += "\nСледующий перерыв будет длинным";
                }
            }

            this.Show();
            this.action = enmAction.start;
            this.timer1.Interval = 1;
            this.timer1.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            switch (this.action)
            {
                case enmAction.wait:
                    timer1.Interval = 360000000;
                    action = enmAction.close;
                    break;
                case Form_Nortification.enmAction.start:
                    this.timer1.Interval = 1;
                    this.Opacity += 0.1;
                    if (this.x < this.Location.X)
                    {
                        this.Left--;
                    }
                    else
                    {
                        if (this.Opacity == 1.0)
                        {
                            action = Form_Nortification.enmAction.wait;
                        }
                    }
                    break;
                case enmAction.close:
                    timer1.Interval = 1;
                    this.Opacity -= 0.1;

                    this.Left -= 3;
                    if (base.Opacity == 0.0)
                    {
                        base.Close();
                    }
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            switch (_timer_type)
            {
                case 1:
                    startFocus(sender, e);
                    break;
                case 2:
                    startBreak(sender, e);
                    break;
                case 3:
                    startLong(sender, e);
                    break;
            }
            timer1.Interval = 1;
            this.action = enmAction.close;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stopTimer(sender, e);
            timer1.Interval = 1;
            this.action = enmAction.close;
        }

    }

}
