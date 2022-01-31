using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Data.Sqlite;

namespace pomidor
{
    public partial class Form1 : Form
    {
        readonly System.Windows.Forms.Timer timerx = new System.Windows.Forms.Timer();
        int year = DateTimeOffset.Now.Year;
        long startTime = 0;
        long stopTime = 0;
        long pauseMoment = 0;
        long focus_time = 25;
        long break_time = 5;
        long long_break_time = 15;
        int long_break_n = 4;
        string f1_sound = "";
        string f2_sound = "";
        string b_sound = "";
        string lb_sound = "";
        private bool _timer_work = false;
        private bool _timer_pause = false;
        private short _timer_type = 0;
        readonly WMPLib.WindowsMediaPlayer wplayer = new WMPLib.WindowsMediaPlayer();
        public Form1()
        {
            InitializeComponent();
            SetDoubleBuffered(tableLayoutPanel1);
            SetDoubleBuffered(tabPage1);
            SetDoubleBuffered(tabPage2);
            SetDoubleBuffered(panel1);
            SetDoubleBuffered(panel3);
            SetDoubleBuffered(groupBox2);
            SetDoubleBuffered(groupBox3);
            SetDoubleBuffered(groupBox4);
            timerx.Interval = 1000;

            tableLayoutPanel1.SuspendLayout();
            for (int i = 0; i < 53; i++)
            {
                for (int j = 0; j < 7; j++)
                {
                    PictureBox pB = new PictureBox
                    {
                        Size = MaximumSize,
                        Dock = DockStyle.Fill,
                        SizeMode = PictureBoxSizeMode.StretchImage
                    };
                    pB.BackColor = System.Drawing.Color.Transparent;
                    pB.Paint += pictureBox1_Paint_1;
                    pB.Margin = new Padding(0);
                    tableLayoutPanel1.Controls.Add(pB,i,j);
                }
            }
            tableLayoutPanel1.ResumeLayout();


            this.ShowInTaskbar = false;
            this.WindowState = FormWindowState.Minimized;



            this.notifyIcon1.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.notifyIcon1.ContextMenuStrip.Items.Add("Приостановить таймер", null, this.PauseClick);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Остановить таймер", null, this.StopClick);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Перезапустить...", null);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Запустить фокусировку", null, this.StartFocus);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Запустить перерыв", null, this.StartBreak);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Запустить длинный перерыв", null, this.StartLongBreak);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Настройки", null, this.OpenSettings);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Импорт данных", null, this.ImportDataBase);
            this.notifyIcon1.ContextMenuStrip.Items.Add("Выйти", null, this.ExitApp);
            (this.notifyIcon1.ContextMenuStrip.Items[2] as ToolStripMenuItem).DropDownItems.Add("Запустить фокусировку", null, this.StartFocus);
            (this.notifyIcon1.ContextMenuStrip.Items[2] as ToolStripMenuItem).DropDownItems.Add("Запустить перерыв", null, this.StartBreak);
            (this.notifyIcon1.ContextMenuStrip.Items[2] as ToolStripMenuItem).DropDownItems.Add("Запустить длинный перерыв", null, this.StartLongBreak);



            var f = Directory.GetFiles("sounds\\", "*.mp3");
            foreach(string f2 in f)
            {
                var temp = f2.Split('\\')[1].Split('.')[0];
                this.comboBox1.Items.Add(temp);
                this.comboBox2.Items.Add(temp);
                this.comboBox3.Items.Add(temp);
                this.comboBox4.Items.Add(temp);
            }



            TimersHide();


            if (!System.IO.File.Exists("userdata.db"))
            {
                using (var connection = new SqliteConnection("Data Source=userdata.db;Mode=ReadWriteCreate"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = connection
                    };
                    var SQL = "CREATE TABLE \"timers\" (\"date\"  INTEGER NOT NULL,\"type\"  INTEGER NOT NULL,\"id\"    INTEGER NOT NULL UNIQUE,\"length\"    INTEGER NOT NULL,\"start\" INTEGER NOT NULL,\"end\"   INTEGER NOT NULL,PRIMARY KEY(\"id\" AUTOINCREMENT))";
                    command.CommandText = SQL;
                    _ = command.ExecuteNonQuery();
                    SQL = "CREATE TABLE \"user_pref\" (\"id\"    INTEGER NOT NULL,\"focus_time\"    INTEGER NOT NULL DEFAULT 25,\"break_time\"    INTEGER NOT NULL DEFAULT 5,\"lbreak_time\"   INTEGER NOT NULL DEFAULT 15,\"before_lbreak\" INTEGER NOT NULL DEFAULT 4,\"f1_sound\"  TEXT DEFAULT \'нет\',\"f2_sound\"  TEXT DEFAULT \'нет\',\"b_sound\"   TEXT DEFAULT \'нет\',\"lb_sound\"  TEXT DEFAULT \'нет\')";
                    command.CommandText = SQL;
                    _ = command.ExecuteNonQuery();
                    SQL = "INSERT INTO user_pref(id) VALUES(0)";
                    command.CommandText = SQL;
                    _ = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT * FROM user_pref";
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())
                        {
                            object id = reader[0];
                            focus_time = (long)reader.GetValue(1);
                            break_time = (long)reader.GetValue(2);
                            long_break_time = (long)reader.GetValue(3);
                            long_break_n = Convert.ToInt32(reader.GetValue(4));
                            f1_sound = Convert.ToString(reader.GetValue(5));
                            f2_sound = Convert.ToString(reader.GetValue(6));
                            b_sound = Convert.ToString(reader.GetValue(7));
                            lb_sound = Convert.ToString(reader.GetValue(8));
                        }
                    }
                }
                connection.Close();
            }
                using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand
                {
                    Connection = connection
                };
                int temp_year = 0;
                command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE type = 1", connection);
                int current_n = 0;
                current_n = Convert.ToInt32(command.ExecuteScalar());
                if (current_n != 0)
                {
                    command.CommandText = "SELECT * FROM timers ORDER BY date LIMIT 1";
                    using (SqliteDataReader reader = command.ExecuteReader())
                    {

                        if (reader.HasRows) // если есть данные
                        {
                            while (reader.Read())
                            {
                                temp_year = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(reader.GetValue(0))).Year;
                            }
                        }
                    }
                    while (temp_year < Convert.ToInt32(DateTimeOffset.Now.Year))
                    {
                        _ = comboBox6.Items.Add(temp_year);
                        temp_year++;
                    }
                }
                connection.Close();
            }
            _ = comboBox6.Items.Add(DateTimeOffset.Now.Year);
            comboBox6.SelectedItem = DateTimeOffset.Now.Year;

        }

        private void ImportDataBase(object sender, EventArgs e)
        {
            OpenFileDialog theDialog = new OpenFileDialog();
            theDialog.Title = "Выберите SQLite базу данных";
            theDialog.Filter = "SQLite format|*.db";
            theDialog.InitialDirectory = @"C:\";
            if (theDialog.ShowDialog() == DialogResult.OK)
            {
                using (var connection = new SqliteConnection("Data Source=userdata.db"))
                {
                    string SQL = "ATTACH '" + theDialog.FileName + "' AS TOMERGE";
                SqliteCommand cmd = new SqliteCommand(SQL);
                cmd.Connection = connection;
                connection.Open();
                int retval = 0;
                try
                {
                    retval = cmd.ExecuteNonQuery();
                }
                catch (Exception Er)
                {
                    MessageBox.Show("An error occurred, your import was not completed.\n"+Er.Message);
                }
                finally
                {
                    cmd.Dispose();
                }

                SQL = "INSERT INTO timers(date,type,length,start,end) SELECT date,type,length,start,end FROM TOMERGE.timers";
                cmd = new SqliteCommand(SQL);
                cmd.Connection = connection;
                retval = 0;
                    try
                    {
                        retval = cmd.ExecuteNonQuery();
                    }
                    catch (Exception Er)
                    {
                        MessageBox.Show("An error occurred, your import was not completed.\n"+Er.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        
                    }
                    SQL = "DETACH TOMERGE";
                    cmd.Connection = connection;
                    connection.Open();
                    retval = 0;
                    try
                    {
                        retval = cmd.ExecuteNonQuery();
                    }
                    catch (Exception Er)
                    {
                        MessageBox.Show("An error occurred, your import was not completed.\n" + Er.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
                comboBox6.Items.Clear();
                using (var connection = new SqliteConnection("Data Source=userdata.db"))
                {
                    connection.Open();
                    SqliteCommand command = new SqliteCommand
                    {
                        Connection = connection
                    };
                    int temp_year = 0;
                    command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE type = 1", connection);
                    int current_n = 0;
                    current_n = Convert.ToInt32(command.ExecuteScalar());
                    if (current_n != 0)
                    {
                        command.CommandText = "SELECT * FROM timers ORDER BY date LIMIT 1";
                        using (SqliteDataReader reader = command.ExecuteReader())
                        {

                            if (reader.HasRows) // если есть данные
                            {
                                while (reader.Read())
                                {
                                    temp_year = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt32(reader.GetValue(0))).Year;
                                }
                            }
                        }
                        while (temp_year < Convert.ToInt32(DateTimeOffset.Now.Year))
                        {
                            _ = comboBox6.Items.Add(temp_year);
                            temp_year++;
                        }
                    }
                    connection.Close();
                }
                _ = comboBox6.Items.Add(DateTimeOffset.Now.Year);
                comboBox6.SelectedItem = DateTimeOffset.Now.Year;
            }
        }

        private void ExitApp(object sender, EventArgs e)
        {
            base.Close();
        }

        private void OpenSettings(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            if(long_break_n == -1)
            {
                this.comboBox5.SelectedIndex = 0;
            } else
            {
                this.comboBox5.SelectedIndex = long_break_n;
            }
            this.comboBox1.SelectedItem = f1_sound;
            this.comboBox2.SelectedItem = f2_sound;
            this.comboBox3.SelectedItem = b_sound;
            this.comboBox4.SelectedItem = lb_sound;
            this.textBox1.Text = Convert.ToString(focus_time);
            this.textBox2.Text = Convert.ToString(break_time);
            this.textBox3.Text = Convert.ToString(long_break_time);
            this.groupBox2.Visible = true;
            this.groupBox3.Visible = true;
            this.groupBox4.Visible = true;
            this.button3.BackColor = System.Drawing.Color.SeaGreen;
            this.button4.BackColor = System.Drawing.Color.MediumSeaGreen;
            this.Focus();
            this.WindowState = FormWindowState.Normal;
        }

        private void TimersHide()
        {
            this.notifyIcon1.ContextMenuStrip.Items[0].Visible = false;
            this.notifyIcon1.ContextMenuStrip.Items[1].Visible = false;
            this.notifyIcon1.ContextMenuStrip.Items[2].Visible = false;
            this.notifyIcon1.ContextMenuStrip.Items[3].Visible = true;
            this.notifyIcon1.ContextMenuStrip.Items[4].Visible = true;
            this.notifyIcon1.ContextMenuStrip.Items[5].Visible = true;
        }

        private void TimersShow()
        {
            this.notifyIcon1.ContextMenuStrip.Items[0].Visible = true;
            this.notifyIcon1.ContextMenuStrip.Items[1].Visible = true;
            this.notifyIcon1.ContextMenuStrip.Items[2].Visible = true;
            this.notifyIcon1.ContextMenuStrip.Items[3].Visible = false;
            this.notifyIcon1.ContextMenuStrip.Items[4].Visible = false;
            this.notifyIcon1.ContextMenuStrip.Items[5].Visible = false;
        }
        public void StartLongBreak(object sender, EventArgs e)
        {
            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            stopTime = DateTimeOffset.Now.AddMinutes(long_break_time).ToUnixTimeSeconds();
            timerx.Enabled = true;
            _timer_work = true;
            _timer_type = 3;
            timerx.Tick += new EventHandler(Timer_Tick);
            TimersShow();
            this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
            if (System.IO.File.Exists("images/delay.ico"))
            {
                this.notifyIcon1.Icon = new Icon("images/delay.ico");
            }
            else
            {
                this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
            }
        }

        public void StartBreak(object sender, EventArgs e)
        {
            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            stopTime = DateTimeOffset.Now.AddMinutes(break_time).ToUnixTimeSeconds();
            timerx.Enabled = true;
            _timer_work = true;
            _timer_type = 2;
            timerx.Tick += new EventHandler(Timer_Tick);
            TimersShow();
            this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
            if (System.IO.File.Exists("images/delay.ico"))
            {
                this.notifyIcon1.Icon = new Icon("images/delay.ico");
            }
            else
            {
                this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
            }
        }

        public void StartFocus(object sender, EventArgs e)
        {
            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            stopTime = DateTimeOffset.Now.AddMinutes(focus_time).ToUnixTimeSeconds();
            timerx.Enabled = true;
            _timer_work = true;
            _timer_type = 1;
            timerx.Tick += new EventHandler(Timer_Tick);
            TimersShow();
            this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
            if (System.IO.File.Exists("images/focus.ico"))
            {
                this.notifyIcon1.Icon = new Icon("images/focus.ico");
            } else
            {
                this.notifyIcon1.Icon = pomidor.Properties.Resources.focus;
            }
            if (f1_sound != "нет")
            {
                wplayer.URL = "sounds/" + f1_sound + ".mp3";
                wplayer.controls.play();
            }
        }

        public void PauseClick(object sender, EventArgs e)
        {
            if (!_timer_pause)
            {
                pauseMoment = DateTimeOffset.Now.ToUnixTimeSeconds();
                timerx.Stop();
                _timer_pause = true;
                this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Возобновить таймер";
                var date = DateTimeOffset.FromUnixTimeSeconds(stopTime - DateTimeOffset.Now.ToUnixTimeSeconds());
                switch (_timer_type)
                {
                    case 1:
                        notifyIcon1.Text = "Режим: Фокусировка";
                        break;
                    case 2:
                        notifyIcon1.Text = "Режим: Короткий перерыв";
                        break;
                    case 3:
                        notifyIcon1.Text = "Режим: Длинный перерыв";
                        break;
                }
                if (_timer_pause)
                {
                    notifyIcon1.Text += "\nСтатус: Пауза";
                }
                else
                {
                    notifyIcon1.Text += "\nСтатус: Активен";
                }
                notifyIcon1.Text += "\nОсталось: " + date.ToString("HH:mm:ss");
                if (System.IO.File.Exists("images/pause.ico"))
                {
                    this.notifyIcon1.Icon = new Icon("images/pause.ico");
                }
                else
                {
                    this.notifyIcon1.Icon = pomidor.Properties.Resources.pause;
                }
            }
            else
            {
                stopTime += (DateTimeOffset.Now.ToUnixTimeSeconds()-pauseMoment);
                timerx.Start();
                timerx.Enabled = true;
                _timer_pause = false;
                this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
                switch (_timer_type) {
                    case 1:
                        if (System.IO.File.Exists("images/focus.ico"))
                        {
                            this.notifyIcon1.Icon = new Icon("images/focus.ico");
                        }
                        else
                        {
                            this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
                        }
                        break;
                    case 2:
                        if (System.IO.File.Exists("images/delay.ico"))
                        {
                            this.notifyIcon1.Icon = new Icon("images/delay.ico");
                        }
                        else
                        {
                            this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
                        }
                        break;
                    case 3:
                        if (System.IO.File.Exists("images/delay.ico"))
                        {
                            this.notifyIcon1.Icon = new Icon("images/delay.ico");
                        }
                        else
                        {
                            this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
                        }
                        break;
                }
                
            }
        }
        public void StopClick(object sender, EventArgs e)
        {
            if (_timer_work)
            {
                startTime = -1;
                stopTime = -1;
                timerx.Enabled = false;
                _timer_work = false;
                _timer_pause = false;
                TimersHide();
                this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
                notifyIcon1.Text = "Нет активных таймеров.";
                if (System.IO.File.Exists("images/none.ico"))
                {
                    this.notifyIcon1.Icon = new Icon("images/none.ico");
                }
                else
                {
                    this.notifyIcon1.Icon = pomidor.Properties.Resources.none;
                }
            }

        }

        private void NotifyIcon1_MClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (_timer_work)
                {
                    PauseClick(sender, e);
                } else
                {
                    startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                    switch (_timer_type)
                    {
                        default:
                            stopTime = DateTimeOffset.Now.AddMinutes(focus_time).ToUnixTimeSeconds();
                            _timer_type = 1;
                            if (System.IO.File.Exists("images/focus.ico"))
                            {
                                this.notifyIcon1.Icon = new Icon("images/focus.ico");
                            }
                            else
                            {
                                this.notifyIcon1.Icon = pomidor.Properties.Resources.focus;
                            }
                            break;
                        case 2:
                            stopTime = DateTimeOffset.Now.AddMinutes(break_time).ToUnixTimeSeconds();
                            if (System.IO.File.Exists("images/delay.ico"))
                            {
                                this.notifyIcon1.Icon = new Icon("images/delay.ico");
                            }
                            else
                            {
                                this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
                            }
                            break;
                        case 3:
                            stopTime = DateTimeOffset.Now.AddMinutes(long_break_time).ToUnixTimeSeconds();
                            if (System.IO.File.Exists("images/delay.ico"))
                            {
                                this.notifyIcon1.Icon = new Icon("images/delay.ico");
                            }
                            else
                            {
                                this.notifyIcon1.Icon = pomidor.Properties.Resources.delay;
                            }
                            break;
                    }
                    timerx.Enabled = true;
                    _timer_work = true;
                    timerx.Tick += new EventHandler(Timer_Tick);
                    TimersShow();
                    this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
                    notifyIcon1.Text = "Нет активных таймеров.";
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (stopTime - DateTimeOffset.Now.ToUnixTimeSeconds() <= 0 && _timer_work)
            {
                _timer_work = false;
                timerx.Enabled=false;
                _timer_pause =false;
                timerx.Stop();
                timerx.Tick -= Timer_Tick;
                TimersHide();
                this.notifyIcon1.ContextMenuStrip.Items[0].Text = "Приостановить таймер";
                notifyIcon1.Text = "Нет активных таймеров.";
                if (System.IO.File.Exists("images/none.ico"))
                {
                    this.notifyIcon1.Icon = new Icon("images/none.ico");
                }
                else
                {
                    this.notifyIcon1.Icon = pomidor.Properties.Resources.none;
                }

                switch (_timer_type)
                {
                    case 1:
                        using (var connection = new SqliteConnection("Data Source=userdata.db"))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand("INSERT INTO timers(`date`, `start`, `end`, `length`,`type`) VALUES(" + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds()) + "," + Convert.ToString(startTime) + "," + Convert.ToString(stopTime) + "," + Convert.ToString(stopTime - startTime) + "," + Convert.ToString(_timer_type) + ")", connection);
                            command.ExecuteNonQuery();
                            command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE (date-(date%86400))/86400 = " + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds() / 86400) + " AND type = 2", connection);
                            int current_n = 0;
                            int current_lb = 0;
                            current_n = Convert.ToInt32(command.ExecuteScalar());
                            command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE (date-(date%86400))/86400 = " + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds() / 86400) + " AND type = 3", connection);
                            
                            current_lb = Convert.ToInt32(command.ExecuteScalar());
                            if (f2_sound != "нет")
                            {
                                wplayer.URL = "sounds/" + f2_sound + ".mp3";
                                wplayer.controls.play();
                            }
                            if (current_n/long_break_n == current_lb + 1)
                            {
                                _timer_type = 3;
                                Nortification("", 3);
                                
                            } else
                            {
                                _timer_type = 2;
                                Nortification("", 2);
                            }
                            StopClick(sender,e);
                            connection.Close();

                        }
                        break;
                    default:
                        using (var connection = new SqliteConnection("Data Source=userdata.db"))
                        {
                            connection.Open();
                            SqliteCommand command = new SqliteCommand("INSERT INTO timers(`date`, `start`, `end`, `length`,`type`) VALUES(" + Convert.ToString(DateTimeOffset.Now.ToUnixTimeSeconds()) + "," + Convert.ToString(startTime) + "," + Convert.ToString(stopTime) + "," + Convert.ToString(stopTime - startTime) + "," + Convert.ToString(_timer_type) + ")", connection);
                            command.ExecuteNonQuery();
                            connection.Close();
                        }
                        if (_timer_type == 2)
                        {
                            if (b_sound != "нет")
                            {
                                wplayer.URL = "sounds/" + b_sound + ".mp3";
                                wplayer.controls.play();

                            }
                        }
                        else if (_timer_type == 3)
                        {
                            if (lb_sound != "нет")
                            {
                                wplayer.URL = "sounds/" + lb_sound + ".mp3";
                                wplayer.controls.play();

                            }
                        }
                            _timer_type = 1;
                        
                        if (f1_sound != "нет")
                        {
                            wplayer.URL = "sounds/" + f1_sound + ".mp3";
                            wplayer.controls.play();
                        }
                        Nortification("", 1);
                        StopClick(sender, e);
                        break;
                }
                Task.Run(Calc_heatmap);
            } else if (_timer_work)
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(stopTime - DateTimeOffset.Now.ToUnixTimeSeconds());
                switch (_timer_type)
                {
                    case 1:
                        notifyIcon1.Text = "Режим: Фокусировка";
                        break;
                    case 2:
                        notifyIcon1.Text = "Режим: Короткий перерыв";
                        break;
                    case 3:
                        notifyIcon1.Text = "Режим: Длинный перерыв";
                        break;
                }
                if (_timer_pause)
                {
                    notifyIcon1.Text += "\nСтатус: Пауза";
                } else
                {
                    notifyIcon1.Text += "\nСтатус: Активен";
                }
                notifyIcon1.Text += "\nОсталось: " + date.ToString("HH:mm:ss");
            }
        }

        public void Nortification(string msg, int type)
        {
            Form_Nortification frm = new Form_Nortification(this);
            frm.showAlert(msg, type, long_break_n);
        }

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedState = comboBox5.SelectedIndex;
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET before_lbreak = ";
                if(selectedState == 0)
                {
                    selectedState = -1;
                }
                command.CommandText += Convert.ToString(selectedState)+ " WHERE id=0";
                command.ExecuteNonQuery();
                long_break_n = selectedState;

                connection.Close();


            }
        }

        private void TextBox1_TextChanged(object sender, EventArgs e)
        {
            string txt = this.textBox1.Text;
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET focus_time = ";
                command.CommandText += txt + " WHERE id=0";
                command.ExecuteNonQuery();
                focus_time = Convert.ToInt64(txt);

                connection.Close();


            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            string txt = this.textBox2.Text;
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET break_time = ";
                command.CommandText += txt + " WHERE id=0";
                command.ExecuteNonQuery();
                break_time = Convert.ToInt64(txt);

                connection.Close();


            }
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            string txt = this.textBox3.Text;
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET lbreak_time = ";
                command.CommandText += txt + " WHERE id=0";
                command.ExecuteNonQuery();
                long_break_time = Convert.ToInt64(txt);

                connection.Close();


            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string txt = this.comboBox1.SelectedItem.ToString();
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET f1_sound = '";
                command.CommandText += txt + "' WHERE id=0";
                command.ExecuteNonQuery();
                f1_sound = txt;

                connection.Close();


            }
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string txt = this.comboBox2.SelectedItem.ToString();
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET f2_sound = '";
                command.CommandText += txt + "' WHERE id=0";
                command.ExecuteNonQuery();
                f2_sound = txt;

                connection.Close();


            }
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            string txt = this.comboBox3.SelectedItem.ToString();
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET b_sound = '";
                command.CommandText += txt + "' WHERE id=0";
                command.ExecuteNonQuery();
                b_sound = txt;

                connection.Close();


            }
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            string txt = this.comboBox4.SelectedItem.ToString();
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;

                command.CommandText = "UPDATE user_pref SET lb_sound = '";
                command.CommandText += txt + "' WHERE id=0";
                command.ExecuteNonQuery();
                lb_sound = txt;

                connection.Close();


            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            panel1.Capture = false;
            Message m = Message.Create(Handle, 0xa1, new IntPtr(2), IntPtr.Zero);
            WndProc(ref m);
        }

        private void button4_MouseClick(object sender, MouseEventArgs e)
        {
            if (tabControl1.SelectedIndex == 0)
            {
                Task.Run(Calc_heatmap);
            }
            tabControl1.SelectTab(1);
            this.button4.BackColor = System.Drawing.Color.SeaGreen;
            this.button3.BackColor = System.Drawing.Color.MediumSeaGreen;
        }

        private void Calc_heatmap()
        {
            using (var connection = new SqliteConnection("Data Source=userdata.db"))
            {
                connection.Open();
                SqliteCommand command;
                command = new SqliteCommand("SELECT COUNT(*) FROM timers WHERE (date-(date%31556926))/31556926 = " + Convert.ToString(new DateTimeOffset(year, 1, 1, 0, 0, 0, 0, DateTimeOffset.Now.Offset).ToUnixTimeSeconds() / 31556926) + " AND type = 1", connection);
                int current_n = 0;
                current_n = Convert.ToInt32(command.ExecuteScalar());
                command = new SqliteCommand("SELECT * FROM timers WHERE (date-(date%31556926))/31556926 = " + Convert.ToString(new DateTimeOffset(year, 1, 1, 0, 0, 0, 0, DateTimeOffset.Now.Offset).ToUnixTimeSeconds() / 31556926) + " AND type = 1", connection);
                int[] days = new int[366];
                bool visokos = true;
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows) // если есть данные
                    {
                        while (reader.Read())
                        {
                            int date = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(reader.GetValue(0))).UtcDateTime.DayOfYear;
                            days[date - 1]++;
                        }
                    }
                }
                int maxValue = days.Max();
                int current_day = 0;
                tableLayoutPanel1.SuspendLayout();
                foreach (Control cell in tableLayoutPanel1.Controls)
                {
                    if (days[current_day] == 0)
                    {
                        cell.BackColor = System.Drawing.Color.White;
                    }
                    else
                    {
                        int percent = GetPercent(maxValue, days[current_day]);
                        if (percent <= 25)
                        {
                            cell.BackColor = System.Drawing.ColorTranslator.FromHtml("#c6e48b");
                        }
                        else if (percent <= 50 && percent > 25)
                        {
                            cell.BackColor = System.Drawing.ColorTranslator.FromHtml("#7bc96f");
                        }
                        else if (percent <= 75 && percent > 50)
                        {
                            cell.BackColor = System.Drawing.ColorTranslator.FromHtml("#239a3b");
                        }
                        else if (percent > 75)
                        {
                            cell.BackColor = System.Drawing.ColorTranslator.FromHtml("#196127");
                        }

                    }
                    cell.Name = days[current_day].ToString() + "n" + current_day.ToString() + "n" + cell.Location.X.ToString() + "n" + cell.Location.Y.ToString();
                    cell.MouseEnter += tt_Enter;
                    current_day++;
                    if ((visokos && current_day >= 365) || (!visokos && current_day >= 366))
                    {
                        break;
                    }


                }
                tableLayoutPanel1.ResumeLayout();

                connection.Close();
            }
        }
        private void tt_Leave(object sender, EventArgs e)
        {
            panel3.Visible = false;
        }

        public static Int32 GetPercent(Int32 b, Int32 a)
        {
            if (b == 0) return 0;

            return (Int32)(a / (b / 100M));
        }
        private void tt_Enter(object sender, EventArgs e)
        {
            this.SuspendLayout();
            tableLayoutPanel1.SuspendLayout();
            var pB = (PictureBox)sender;
            var args = pB.Name.Split('n');
            var temp_text = args[0];
            var offset = 0;
            if (int.Parse(args[2]) +tableLayoutPanel1.Location.X+panel3.Width-this.Width > 16)
            {
                offset = int.Parse(args[2]) + tableLayoutPanel1.Location.X + panel3.Width - this.Width + 16;
            }

            panel3.Location = new System.Drawing.Point(int.Parse(args[2]) + tableLayoutPanel1.Location.X-offset, int.Parse(args[3]) + tableLayoutPanel1.Location.Y-40);
            if (args[0][args[0].Length-1] == '1')
            {
                if (args[0].Length == 1)
                {
                    temp_text += " кукумбер, ";
                } else if (args[0][args[0].Length - 2] != '1')
                {
                    temp_text += " кукумбер, ";
                } else
                {
                    temp_text += " кукумберов, ";
                }
            }
            else if (args[0][args[0].Length - 1] == '2')
            {
                if (args[0].Length == 1)
                {
                    temp_text += " кукумбера, ";
                }
                else if (args[0][args[0].Length - 2] != '1')
                {
                    temp_text += " кукумбера, ";
                }
                else
                {
                    temp_text += " кукумберов, ";
                }
            }
            else if (args[0][args[0].Length - 1] == '3')
            {
                if (args[0].Length == 1)
                {
                    temp_text += " кукумбера, ";
                }
                else if (args[0][args[0].Length - 2] != '1')
                {
                    temp_text += " кукумбера, ";
                }
                else
                {
                    temp_text += " кукумберов, ";
                }
            }
            else if (args[0][args[0].Length - 1] == '4')
            {
                if (args[0].Length == 1)
                {
                    temp_text += " кукумбера, ";
                }
                else if (args[0][args[0].Length - 2] != '1')
                {
                    temp_text += " кукумбера, ";
                }
                else
                {
                    temp_text += " кукумберов, ";
                }
            }
            else
            {
                temp_text += " кукумберов, ";
            }
            var culture = CultureInfo.CreateSpecificCulture("ru-RU");
            DateTime date = new DateTime(year,1,1).AddDays(Convert.ToInt32(args[1]));
            temp_text+= date.ToString("dddd, d MMMM", culture);
            label13.Text = temp_text;
            panel3.Visible = true;
            tableLayoutPanel1.ResumeLayout();
            this.ResumeLayout();
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (year != int.Parse(comboBox6.SelectedItem.ToString()))
                {
                    year = int.Parse(comboBox6.SelectedItem.ToString());
                    Task.Run(Calc_heatmap);
                }
            }
            catch (Exception)
            {
            }
        }
        private void pictureBox1_Paint_1(object sender, PaintEventArgs e)
        {
            var pB = (PictureBox)sender;
            ControlPaint.DrawBorder(e.Graphics, pB.ClientRectangle, SystemColors.ControlDark, ButtonBorderStyle.Solid);
        }
        #region .. Double Buffered function ..
        public static void SetDoubleBuffered(System.Windows.Forms.Control c)
        {
            if (System.Windows.Forms.SystemInformation.TerminalServerSession)
                return;
            System.Reflection.PropertyInfo aProp = typeof(System.Windows.Forms.Control).GetProperty("DoubleBuffered",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);
            aProp.SetValue(c, true, null);
        }

        #endregion


        #region .. code for Flucuring ..

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }

        #endregion
    }
}
