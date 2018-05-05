using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GbtnAttendance
{
    public partial class MainForm : Form
    {
        SQLiteConnection sqlite_conn = new SQLiteConnection("Data Source=attendance.sqlite;Version= 3;");
        SQLiteDataAdapter sqlite_adapter;
        DataTable sqlite_table;
        public int hourRate;

        public MainForm()
        {
            InitializeComponent();

        }
        
        /* Initialization and populating the table.
         * If no database is found then a new one will be created
         */
         
        private void Form1_Load(object sender, EventArgs e)
        {
            String directory = System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);

            directory += "\\attendance.sqlite";


            if (!File.Exists(directory))
            {
                try
                {
                    sqlite_conn.Open();

                    SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                    sqlite_cmd.CommandText = "CREATE TABLE attendance (id integer primary key autoincrement, projectName varchar(30), date varchar(50), hours varchar(30), paid varchar(10));";
                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_cmd.CommandText = "CREATE TABLE settings (id integer primary key autoincrement, rate integer);";
                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_cmd.CommandText = "INSERT INTO settings (rate) Values (@Rate)";
                    sqlite_cmd.Parameters.AddWithValue("@Rate", "11");
                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_conn.Close();

                    MessageBox.Show("No database found, a new one was created", "Database Created!");

                }

                catch (SQLiteException ex)
                {
                    MessageBox.Show("Database Error", "Database error");
                    Debug.WriteLine(ex.StackTrace);
                }
            }

            sqlite_conn.Open();

            SQLiteCommand sqlite_cmd1 = sqlite_conn.CreateCommand();
            sqlite_cmd1.CommandText = "SELECT * FROM settings WHERE id = 1";
            SQLiteDataReader sqlite_rdr = sqlite_cmd1.ExecuteReader();

            while (sqlite_rdr.Read())
            {
                hourRate = sqlite_rdr.GetInt32(1);
            }

            sqlite_rdr.Close();
            sqlite_conn.Close();

            populateTable();

            projectDatePicker.Value = DateTime.Now;
            
        }

        public void populateTable()
        {
            try
            {

                sqlite_adapter = new SQLiteDataAdapter("SELECT id, projectName, date, hours, paid FROM [attendance]", sqlite_conn);
                sqlite_table = new DataTable();
                sqlite_adapter.Fill(sqlite_table);

                attendanceTable.DataSource = sqlite_table;
                attendanceTable.Columns[0].Visible = false;
                int aDue = 0;
                int aPaid = 0;

                for(int i = 0; i < attendanceTable.Rows.Count; i++)
                {
                    //This colors the paid cell: Red for N, Green for Y
                    if (attendanceTable.Rows[i].Cells[4].Value.Equals("N")){
                        attendanceTable.Rows[i].Cells[4].Style.Font = new Font("Microsoft Sans Serif", (float)8.25, FontStyle.Bold);
                        attendanceTable.Rows[i].Cells[4].Style.ForeColor = Color.Red;

                        //Set the Amount due label
                        aDue += (Convert.ToInt32(attendanceTable.Rows[i].Cells[3].Value)) * hourRate;                       
                    }
                    else
                    {
                        attendanceTable.Rows[i].Cells[4].Style.Font = new Font("Microsoft Sans Serif", (float)8.25, FontStyle.Bold);
                        attendanceTable.Rows[i].Cells[4].Style.ForeColor = Color.Green;

                        //Set the Amount paid label
                        //We assume that if the value for the cell is not N it must be Y
                        aPaid += (Convert.ToInt32(attendanceTable.Rows[i].Cells[3].Value)) * hourRate;                        
                    }                                       
                }

                //We update the values outside the for loop so that even if they are 0 they will be updated
                amountPaid.Text = Convert.ToString(aPaid);
                amountDue.Text = Convert.ToString(aDue);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.StackTrace);
            }
        }


        private void reset()
        {
            projectNameTextbox.ResetText();
            hoursTestedTextbox.ResetText();
            projectDatePicker.ResetText();
        }

        private void insertButton_Click(object sender, EventArgs e)
        {
            try
            { 
                sqlite_conn.Open();

                SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                sqlite_cmd.CommandText = "INSERT INTO attendance (projectName, date, hours, paid) Values (@ProjectName, @Date, @Hours, @Paid)";

                sqlite_cmd.Parameters.AddWithValue("@ProjectName", projectNameTextbox.Text);
                sqlite_cmd.Parameters.AddWithValue("@Hours", hoursTestedTextbox.Text);
                sqlite_cmd.Parameters.AddWithValue("@Date", projectDatePicker.Text);
                sqlite_cmd.Parameters.AddWithValue("@Paid", "N");

                sqlite_cmd.ExecuteNonQuery();

                sqlite_conn.Close();
                
                toolStripStatus.Text = DateTime.Now.ToString("hh:mm tt") + " - " + "Project " + projectNameTextbox.Text + " added!";

                reset();
                populateTable();
            }
            catch(Exception)
            {
                MessageBox.Show("insertButton Error!", "Error");
            }
        }

        private void modifyButton_Click(object sender, EventArgs e)
        {
            int selectedrowindex = attendanceTable.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = attendanceTable.Rows[selectedrowindex];

            if (selectedRow.Cells[4].Value.Equals("N"))
            {
                try
                {
                    sqlite_conn.Open();
                    SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                    sqlite_cmd.CommandText = "UPDATE attendance set paid = 'Y' WHERE id ='" + selectedRow.Cells[0].Value +"'";

                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_conn.Close();

                    toolStripStatus.Text = DateTime.Now.ToString("hh:mm tt") + " - " + "Project " + selectedRow.Cells[1].Value + " PAID!";

                    populateTable();
                }
                catch(Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
            }
            else
            {
                try
                {
                    sqlite_conn.Open();
                    SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                    sqlite_cmd.CommandText = "UPDATE attendance set paid = 'N' WHERE id ='" + selectedRow.Cells[0].Value + "'";

                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_conn.Close();

                    toolStripStatus.Text = DateTime.Now.ToString("hh:mm tt") + " - " + "Project " + selectedRow.Cells[1].Value + " modified!";

                    populateTable();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.StackTrace);
                }
            }
        }

        private void deleteButton_Click(object sender, EventArgs e)
        {
            int selectedrowindex = attendanceTable.SelectedCells[0].RowIndex;
            DataGridViewRow selectedRow = attendanceTable.Rows[selectedrowindex];

            DialogResult dialogResult = MessageBox.Show("Delete the project?", "Delete project", MessageBoxButtons.YesNo);

            if (dialogResult == DialogResult.Yes)
            { 
                    try
                    {
                    sqlite_conn.Open();
                    SQLiteCommand sqlite_cmd = sqlite_conn.CreateCommand();

                    sqlite_cmd.CommandText = "DELETE FROM attendance WHERE id ='" + selectedRow.Cells[0].Value + "'";

                    sqlite_cmd.ExecuteNonQuery();

                    sqlite_conn.Close();

                    toolStripStatus.Text = DateTime.Now.ToString("hh:mm tt") + " - " + "Project " + selectedRow.Cells[1].Value + " deleted!";

                    populateTable();

                    
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.StackTrace);
                    }
            }
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Form2 form2 = new Form2();
            form2.Form1 = this;
            form2.Show();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
    }
}
