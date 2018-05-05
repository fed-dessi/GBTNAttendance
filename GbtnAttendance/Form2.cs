using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GbtnAttendance
{
    public partial class Form2 : Form
    {
        SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=attendance.sqlite;Version= 3;");
        MainForm form1;

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                sqlite_conn.Open();

                SQLiteCommand sqlite_cmd1 = sqlite_conn.CreateCommand();
                sqlite_cmd1.CommandText = "SELECT * FROM settings WHERE id = 1";
                SQLiteDataReader sqlite_rdr = sqlite_cmd1.ExecuteReader();
                while (sqlite_rdr.Read())
                {
                    hourRateTextboxSettings.Text = Convert.ToString(sqlite_rdr.GetInt32(1));
                }
                sqlite_rdr.Close();
                sqlite_conn.Close();
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }

        public MainForm Form1
        {
            get { return form1; }
            set { form1 = value; }
        }

        private void button1_Click(object sender, EventArgs e)
        {

            try
            {
                sqlite_conn.Open();
                SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = "UPDATE settings set rate = '"+ hourRateTextboxSettings.Text +"' WHERE id ='1'";

                sqlite_cmd.ExecuteNonQuery();

                sqlite_conn.Close();

                Form1.hourRate = Convert.ToInt32(hourRateTextboxSettings.Text);
                Form1.populateTable();

                Form1.toolStripStatus.Text = DateTime.Now.ToString("hh:mm tt") + " - " + "Hour rate CHANGED!";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
            
        }
    }
}
