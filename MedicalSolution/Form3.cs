using System;
using System.Data;
using System.Windows.Forms;
using Npgsql;

namespace MedicalSolution
{

    public partial class Form3 : Form
    {
        private bool sidebarExpand;
        private NpgsqlConnection connection;
        string connString = "Host=ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech;" +
                              "Username=neondb_owner;" +
                              "Password=R8IhSm2dFAKG;" +
                              "Database=neondb;" +
                              "Port=5432;" +
                              "SSL Mode=Require;" +
                              "Trust Server Certificate=true;";

        public Form3()
        {
            InitializeComponent();
            if (!DesignMode)
            {
                sidebar1.Width = sidebar1.MaximumSize.Width;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            MessageBox.Show("Logging out...");
        }

        private void menuButton_Click(object sender, EventArgs e)
        {
            
           
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void sidebarTimer_Tick(object sender, EventArgs e)
        {
            if (DesignMode) return; 

            if (sidebarExpand)
            {
                sidebar1.Width += 10;
                if (sidebar1.Width >= sidebar1.MaximumSize.Width)
                {
                    sidebar1.Width = sidebar1.MaximumSize.Width;
                    sidebarExpand = false;
                    sidebarTimer.Stop();
                }
            }
            else
            {
                sidebar1.Width -= 10; 
                if (sidebar1.Width <= sidebar1.MinimumSize.Width)
                {
                    sidebar1.Width = sidebar1.MinimumSize.Width;
                    sidebarExpand = true;
                    sidebarTimer.Stop();
                }
            }
        }
        
        private void sidebar_Paint(object sender, PaintEventArgs e)
        {
        }

        
        private void Form3_Load_1(object sender, EventArgs e)
        {
            LoadDoctors();
        }
        private void LoadDoctors()
        {
            using (connection = new NpgsqlConnection(connString))
            {
                try
                {
                    
                    string connectionString = "Host=ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech;Username=neondb_owner;Password=R8IhSm2dFAKG;Database=neondb;SslMode=Require";
                    using (var connection = new NpgsqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "SELECT codem, nom, specialite, telephone, email FROM doctor";
                        using (var dataAdapter = new NpgsqlDataAdapter(query, connection))
                        {
                            DataTable dataTable = new DataTable();
                            dataAdapter.Fill(dataTable);

                            
                            flowLayoutPanel1.Controls.Clear();

                            if (dataTable.Rows.Count > 0)
                            {
                                
                                foreach (DataRow row in dataTable.Rows)
                                {
                                    
                                    Panel doctorCard = new Panel
                                    {
                                        Width = 300,
                                        Height = 170,
                                        BorderStyle = BorderStyle.FixedSingle,
                                        Margin = new Padding(10, 20, 10, 20), 
                                        BackColor = System.Drawing.Color.White
                                    };

                                   
                                    Label doctorNameLabel = new Label
                                    {
                                        Text = $"Nom: {row["nom"]}",
                                        Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold),
                                        Location = new System.Drawing.Point(10, 10),
                                        AutoSize = true
                                    };

                                    
                                    Label doctorSpecialtyLabel = new Label
                                    {
                                        Text = $"Specialty: {row["specialite"]}",
                                        Location = new System.Drawing.Point(10, 40),
                                        AutoSize = true
                                    };

                                    
                                    Label doctorContactLabel = new Label
                                    {
                                        Text = $"📞 {row["telephone"]}\n📧 {row["email"]}",
                                        Location = new System.Drawing.Point(10, 70),
                                        AutoSize = true
                                    };

                                   
                                    Button checkCalendarButton = new Button
                                    {
                                        Text = "Check Calendar",
                                        Location = new System.Drawing.Point(10, 120),
                                        Width = 280,
                                        Height=40,
                                        BackColor = System.Drawing.Color.MidnightBlue,
                                        ForeColor = System.Drawing.Color.White,
                                        FlatStyle = FlatStyle.Flat
                                    };
                                    int codem = Convert.ToInt32(row["codem"]);


                                   
                                    checkCalendarButton.Click += (sender, e) =>
                                    {
                                       
                                        //int codem = (int)row["codem"];
                                       
                                        Form5 form5 = new Form5(codem);
                                        form5.Show();
                                        this.Hide();
                                    };

                                    
                                    doctorCard.Controls.Add(doctorNameLabel);
                                    doctorCard.Controls.Add(doctorSpecialtyLabel);
                                    doctorCard.Controls.Add(doctorContactLabel);
                                    doctorCard.Controls.Add(checkCalendarButton);

                                   
                                    flowLayoutPanel1.Controls.Add(doctorCard);
                                }
                            }
                            else
                            {
                                MessageBox.Show("No doctors found.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {

        }

        private void button8_Click(object sender, EventArgs e)
        {
          
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            
            Form4 form4 = new Form4();
            form4.Show();
            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();

        }

        private void button5_Click(object sender, EventArgs e)
        {
           
            Form3 form3 = new Form3();
            form3.Show();
            this.Hide();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            sidebarTimer.Start();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
