using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Windows.Forms;
using Npgsql;

namespace MedicalSolution
{
    public partial class Form5 : Form
    {
        private int codem;
        private string connectionString = "Host=ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech;" +
                                          "Username=neondb_owner;" +
                                          "Password=R8IhSm2dFAKG;" +
                                          "Database=neondb;" +
                                          "Port=5432;" +
                                          "SSL Mode=Require;" +
                                          "Trust Server Certificate=true;";
        private PrintDocument printDocument = new PrintDocument();

        public Form5(int codem)
        {
            InitializeComponent();
            this.codem = codem;
            printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
        }

        private void Form5_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
            LoadAppointments();
            LoadDoctorDetails();
        }

       

        private void LoadAppointments()
        {
           
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                SELECT 
                    p.nom AS patient_name, 
                    p.telephone, 
                    a.date, 
                    a.hour 
                FROM appointment a
                JOIN patient p ON a.patient= p.id
                WHERE a.codem = @codem 
                  AND a.etat = 'confirme'
                  AND a.date = @selectedDate";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@codem", codem);
                        command.Parameters.AddWithValue("@selectedDate", dateTimePicker1.Value.Date);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            flowLayoutPanel1.Controls.Clear(); 
                            while (reader.Read())
                            {
                               

                               
                                Panel card = new Panel
                                {
                                    Width = flowLayoutPanel1.Width - 20, 
                                    Height = 150,
                                    Margin = new Padding(10),
                                    BackColor = Color.White,
                                    BorderStyle = BorderStyle.FixedSingle,
                                };

                                
                                Label nameLabel = new Label
                                {
                                    Text = $"Nom: {reader["patient_name"]}",
                                    Font = new Font("Arial", 12, FontStyle.Bold),
                                    Location = new Point(10, 10),
                                    AutoSize = true
                                };

                               
                                Label phoneLabel = new Label
                                {
                                    Text = $"Téléphone: {reader["telephone"]}",
                                    Font = new Font("Arial", 11),
                                    Location = new Point(10, 50),
                                    AutoSize = true
                                };

                                
                                Label dateLabel = new Label
                                {
                                    Text = $"Date: {Convert.ToDateTime(reader["date"]).ToString("yyyy-MM-dd")}",
                                    Font = new Font("Arial", 11),
                                    Location = new Point(10, 90),
                                    AutoSize = true
                                };

                               
                                Label hourLabel = new Label
                                {
                                    Text = $"Heure: {reader["hour"]}",
                                    Font = new Font("Arial", 11),
                                    Location = new Point(10, 120),
                                    AutoSize = true
                                };

                                
                                card.Controls.Add(nameLabel);
                                card.Controls.Add(phoneLabel);
                                card.Controls.Add(dateLabel);
                                card.Controls.Add(hourLabel);

                                
                                flowLayoutPanel1.Controls.Add(card);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading appointments: {ex.Message}");
                }
            }
        }

        private void LoadDoctorDetails()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT nom FROM doctor WHERE codem = @codem";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@codem", codem);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                label3.Text = $"Docteur: {reader["nom"]}";
                            }
                            else
                            {
                                MessageBox.Show("Détails du médecin introuvables.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading doctor details: {ex.Message}");
                }
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font titleFont = new Font("Arial", 16, FontStyle.Bold);
            Font font = new Font("Arial", 12);
            float pageWidth = e.PageBounds.Width;
            float xMargin = 50;
            float yPos = 50;

            
            string doctorName = label3.Text;
            float doctorNameX = (pageWidth - e.Graphics.MeasureString(doctorName, titleFont).Width) / 2;
            e.Graphics.DrawString(doctorName, titleFont, Brushes.Black, doctorNameX, yPos);
            yPos += 50;

            
            string dateText = $"Date: {dateTimePicker1.Value.ToString("yyyy-MM-dd")}";
            float dateX = pageWidth - e.Graphics.MeasureString(dateText, font).Width - xMargin;
            e.Graphics.DrawString(dateText, font, Brushes.Black, dateX, yPos);
            yPos += 50;

           
            e.Graphics.DrawString("Confirmed Appointments:", font, Brushes.Black, xMargin, yPos);
            yPos += 30;

           
            foreach (Control card in flowLayoutPanel1.Controls)
            {
                foreach (Label label in card.Controls.OfType<Label>())
                {
                    e.Graphics.DrawString(label.Text, font, Brushes.Black, xMargin, yPos);
                    yPos += 20;
                }

                yPos += 10; 
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog
            {
                Document = printDocument
            };

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    printDocument.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Printing error: {ex.Message}");
                }
            }
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            LoadAppointments();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            Form4 form4 = new Form4();
            form4.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
           
            Form3 form3 = new Form3();
            form3.Show();
             this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
