using System;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Mail;
using System.Windows.Forms;
using Npgsql;

namespace MedicalSolution
{
    public partial class Form4 : Form
    {
        private string connString = "Host=ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech;" +
                                    "Username=neondb_owner;" +
                                    "Password=R8IhSm2dFAKG;" +
                                    "Database=neondb;" +
                                    "Port=5432;" +
                                    "SSL Mode=Require;" +
                                    "Trust Server Certificate=true;";

        public Form4()
        {
            InitializeComponent();
        }

        private void Form4_Load(object sender, EventArgs e)
        {
            LoadAppointmentCards();
        }

        private void LoadAppointmentCards()
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                        SELECT a.id, p.nom AS patientName, a.date, a.hour, p.email AS patientEmail, a.codem , d.nom, d.specialite
                        FROM appointment a
                        JOIN patient p ON a.patient = p.id
                        JOIN doctor d using(codem)
                        WHERE a.etat = 'encours'";

                    using (var command = new NpgsqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        flowLayoutPanel1.Controls.Clear(); 

                        while (reader.Read())
                        {
                            int appointmentId = reader.GetInt32(reader.GetOrdinal("id"));
                            string patientName = reader.GetString(reader.GetOrdinal("patientName"));
                            DateTime appointmentDate = reader.GetDateTime(reader.GetOrdinal("date"));
                            string appointmentDateString = appointmentDate.ToString("yyyy-MM-dd");
                            TimeSpan appointmentHour = reader.GetTimeSpan(reader.GetOrdinal("hour"));
                            string appointmentHourString = appointmentHour.ToString(@"hh\:mm");
                            string patientEmail = reader.GetString(reader.GetOrdinal("patientEmail"));
                            int doctorId = reader.GetInt32(reader.GetOrdinal("codem"));
                            string doctorName = reader.GetString(reader.GetOrdinal("nom"));
                            string doctorSpecialite = reader.GetString(reader.GetOrdinal("specialite"));

                            
                            Panel appointmentCard = new Panel
                            {
                                Size = new Size(300, 200), 
                                BorderStyle = BorderStyle.FixedSingle,
                                BackColor = Color.LightGray,
                                Margin = new Padding(10)
                            };

                            
                            Label nameLabel = new Label
                            {
                                Text = "Patient: " + patientName,
                                Location = new Point(10, 10),
                                AutoSize = true,
                                Font = new Font("Arial", 10, FontStyle.Bold)
                            };

                            Label dateLabel = new Label
                            {
                                Text = "Date: " + appointmentDateString,
                                Location = new Point(10, 40),
                                AutoSize = true
                            };

                            Label timeLabel = new Label
                            {
                                Text = "Hour: " + appointmentHourString,
                                Location = new Point(10, 70),
                                AutoSize = true
                            };

                            Label namedLabel = new Label
                            {
                                Text = "Doctor: " + doctorName,
                                Location = new Point(10, 100),
                                AutoSize = true,
                                Font = new Font("Arial", 10, FontStyle.Bold)
                            };

                            Label specialiteLabel = new Label
                            {
                                Text = "Specialité: " + doctorSpecialite,
                                Location = new Point(10, 130), 
                                AutoSize = true,
                                Font = new Font("Arial", 10, FontStyle.Bold)
                            };

                           
                            Button acceptButton = new Button
                            {
                                Text = "Accept",
                                Size = new Size(280, 30),
                                Location = new Point(10, 160),
                                BackColor = System.Drawing.Color.MidnightBlue,
                                ForeColor = System.Drawing.Color.White,
                                FlatStyle = FlatStyle.Flat

                            };
                            acceptButton.Click += (s, e) => AcceptAppointment(appointmentId, patientEmail, appointmentDateString, doctorName, doctorSpecialite);

                           

                           
                            appointmentCard.Controls.Add(nameLabel);
                            appointmentCard.Controls.Add(dateLabel);
                            appointmentCard.Controls.Add(timeLabel);
                            appointmentCard.Controls.Add(namedLabel);
                            appointmentCard.Controls.Add(specialiteLabel);
                            appointmentCard.Controls.Add(acceptButton);

                            
                            flowLayoutPanel1.Controls.Add(appointmentCard);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors du chargement des rendez-vous : " + ex.Message);
                }
            }
        }
        private void SendConfirmationEmail(string recipientEmail, string appointmentDate,string doctorName,String doctorSpecialite)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Preparing to send email to: {recipientEmail} for date: {appointmentDate}");

                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("imennouri82@gmail.com");
                    mail.To.Add(recipientEmail);
                    mail.Subject = "Confirmation de votre rendez-vous";
                    mail.Body = $"Bonjour, votre rendez-vous avec le {doctorSpecialite} Docteur {doctorName} a été confirmé pour la date suivante : {appointmentDate}. Merci de votre confiance.";
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("imennouri82@gmail.com", "ygiv enir xwca bjhm"); 
                        smtp.EnableSsl = true;

                        smtp.Send(mail); 
                    }
                }
               // MessageBox.Show("Confirmation email sent successfully.");
            }
            catch (SmtpException smtpEx)
            {
                MessageBox.Show($"SMTP Error: {smtpEx.Message}\n{smtpEx.StackTrace}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"General Error: {ex.Message}\n{ex.StackTrace}");
            }
        }


        private void AcceptAppointment(int appointmentId, string patientEmail, string appointmentDate, string doctorName, string doctorSpecialite)
        {
            using (var connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    string updateQuery = "UPDATE appointment SET etat = 'confirme' WHERE id = @appointmentId";

                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@appointmentId", appointmentId);
                        command.ExecuteNonQuery();
                    }

                    
                    SendConfirmationEmail(patientEmail, appointmentDate, doctorName, doctorSpecialite);
                    //MessageBox.Show("Rendez-vous accepté et e-mail de confirmation envoyé !");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erreur lors de l'acceptation du rendez-vous : " + ex.Message);
                }
            }

           
            LoadAppointmentCards();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
           
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
            Form2 form2 = new Form2();
            form2.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            
            Form3 form3 = new Form3();
            form3.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
           
            Form4 form4 = new Form4();
            form4.Show();
            this.Hide();

        }

        private void button4_Click(object sender, EventArgs e)
        {
            
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
        }
    }
}
