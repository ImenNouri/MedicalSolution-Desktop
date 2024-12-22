using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;

namespace MedicalSolution
{
    public partial class Form8 : Form
    {
        int code;
       long codes;
        private string connString = "Host=ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech;" +
                                   "Username=neondb_owner;" +
                                   "Password=R8IhSm2dFAKG;" +
                                   "Database=neondb;" +
                                   "Port=5432;" +
                                   "SSL Mode=Require;" +
                                   "Trust Server Certificate=true;";
        private PrintDocument printDocument = new PrintDocument();
        public Form8(int codem)
        {
            InitializeComponent();
            code = codem;
            codes = (long)code;
            printDocument.PrintPage += new PrintPageEventHandler(PrintDocument_PrintPage);
        }
        private void LoadAppointments()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    string query = @"
                SELECT p.id,a.id as  appointment_Id, p.nom AS patient_name, p.telephone, p.email, a.date, a.hour, d.nom AS doctor_name, d.specialite
                FROM appointment a
                JOIN patient p ON a.patient = p.id
                JOIN doctor d ON a.codem = d.codem
                WHERE a.codem = @codem 
                AND a.etat = 'confirme'
                AND a.date = @selectedDate";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        //command.Parameters.AddWithValue("@codem", codes);
                        command.Parameters.Add("@codem", NpgsqlTypes.NpgsqlDbType.Bigint).Value = codes;
                        command.Parameters.AddWithValue("@selectedDate", dateTimePicker1.Value.Date);

                        using (NpgsqlDataReader reader = command.ExecuteReader())
                        {
                            flowLayoutPanel1.Controls.Clear(); 

                            while (reader.Read())
                            {
                               
                                flowLayoutPanel1.Controls.Add(CreateAppointmentCard(reader));
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

        private Panel CreateAppointmentCard(IDataRecord row)
        {
            Panel cardPanel = new Panel
            {
                Size = new Size(720, 180), 
                Margin = new Padding(10),
                BackColor = Color.LightGray
            };

            
            int appointmentId = Convert.ToInt32(row["appointment_Id"]);
            string patientName = row["patient_name"].ToString();
            string patientPhone = row["telephone"].ToString();
            string patientEmail = row["email"].ToString();
            DateTime appointmentDate = Convert.ToDateTime(row["date"]);
            TimeSpan appointmentHour = (TimeSpan)row["hour"];
            string appointmentHourString = appointmentHour.ToString(@"hh\:mm");
            string doctorName = row["doctor_name"].ToString();
            string doctorSpecialite = row["specialite"].ToString();

           
            Label labelName = new Label { Text = $"{patientName}", Location = new Point(10, 10), AutoSize = true };
            Label labelPhone = new Label { Text = $"Téléphone: {patientPhone}", Location = new Point(10, 40), AutoSize = true };
            Label labelEmail = new Label { Text = $"Email: {patientEmail}", Location = new Point(10, 70), AutoSize = true };
            Label labelCurrentDate = new Label { Text = $"Date: {appointmentDate.ToShortDateString()}| Hour: {appointmentHourString}", Location = new Point(10, 100), AutoSize = true };

            
            Label labelDoctorName = new Label { Text = $"Docteur: {doctorName}", Location = new Point(10, 130), AutoSize = true };
            Label labelDoctorSpecialite = new Label { Text = $"Spécialité: {doctorSpecialite}", Location = new Point(10, 160), AutoSize = true };

            cardPanel.Controls.Add(labelName);
            cardPanel.Controls.Add(labelPhone);
            cardPanel.Controls.Add(labelEmail);
            cardPanel.Controls.Add(labelCurrentDate);
            cardPanel.Controls.Add(labelDoctorName);
            cardPanel.Controls.Add(labelDoctorSpecialite);

            
            Button buttonDecaler = new Button { Text = "Décaler", Location = new Point(600, 40) };
            buttonDecaler.Click += (sender, e) =>
            {
                ShowDatePickerForReschedule(appointmentId, cardPanel, labelCurrentDate, patientEmail, doctorName, doctorSpecialite, appointmentDate);
            };

            cardPanel.Controls.Add(buttonDecaler);
            return cardPanel;
        }

        private void ShowDatePickerForReschedule(
    int appointmentId, Panel cardPanel, Label labelCurrentDate,
    string patientEmail, string doctorName, string doctorSpecialite, DateTime currentDate)
        {
            foreach (Control control in cardPanel.Controls)
            {
                if (control is DateTimePicker || (control is Button button && button.Text == "Confirmer"))
                {
                    return; 
                }
            }

           
            DateTimePicker calendar = new DateTimePicker
            {
                Format = DateTimePickerFormat.Short,
                Location = new Point(530, 70),
                ShowUpDown = false,
                CustomFormat = "dd/MM/yyyy"
            };
            cardPanel.Controls.Add(calendar);

            
            Button confirmButton = new Button { Text = "Confirmer", Location = new Point(530, 100) };
            confirmButton.Click += (confirmSender, confirmEventArgs) =>
            {
                DateTime newDate = calendar.Value;

                if (newDate != currentDate)
                {
                    UpdateAppointmentDate(appointmentId, newDate);


                    labelCurrentDate.Text = $"Date: {newDate.ToShortDateString()}";


                    LoadAppointments();


                    SendEmailNotification(patientEmail, newDate, doctorName, doctorSpecialite);
                }

                cardPanel.Controls.Remove(calendar);
                cardPanel.Controls.Remove(confirmButton);
            };
            cardPanel.Controls.Add(confirmButton);
        }

        private void UpdateAppointmentDate(int appointmentId, DateTime newDate)
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    
                    string updateQuery = "UPDATE appointment SET date = @newDate WHERE id = @appointmentId";
                    using (var command = new NpgsqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@newDate", newDate);
                        command.Parameters.AddWithValue("@appointmentId", appointmentId);
                        command.ExecuteNonQuery();




                       
                        Console.WriteLine($"Executing query: {updateQuery}");
                        Console.WriteLine($"Parameters: AppointmentId = {appointmentId}, NewDate = {newDate}");

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("La date du rendez-vous a été mise à jour avec succès.");
                        }
                        else
                        {

                            MessageBox.Show("Aucune mise à jour effectuée.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la mise à jour de la date : {ex.Message}");
                }
            }
        }


        private void SendEmailNotification(string patientEmail, DateTime newDate, string doctorName, string doctorSpecialite)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress("imennouri82@gmail.com");
                    mail.To.Add(patientEmail);
                    mail.Subject = "Confirmation de votre rendez-vous";
                    mail.Body = $"Bonjour, votre rendez-vous avec le {doctorSpecialite} Docteur {doctorName} a été confirmé pour la date suivante : {newDate.ToShortDateString()}. Merci de votre confiance.";
                    mail.IsBodyHtml = false;

                    using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                    {
                        smtp.Credentials = new NetworkCredential("imennouri82@gmail.com", "ygiv enir xwca bjhm"); 
                        smtp.EnableSsl = true;

                        smtp.Send(mail); 
                    }
                }
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

        private void flowLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form6 form6 = new Form6(code);
            form6.Show();
            this.Hide();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form7 form7 = new Form7(code);
            form7.Show();
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Form8 form8 = new Form8(code);
            form8.Show();
            this.Hide();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1 form1 = new Form1();
            form1.Show();
            this.Hide();
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

        

        private void LoadDoctorDetails()
        {
            using (NpgsqlConnection connection = new NpgsqlConnection(connString))
            {
                try
                {
                    connection.Open();
                    string query = @"SELECT nom FROM doctor WHERE codem = @codem";

                    using (NpgsqlCommand command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.Add("@codem", NpgsqlTypes.NpgsqlDbType.Bigint).Value = codes;

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


        private void Form8_Load(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
            LoadAppointments();
            LoadDoctorDetails();
        }
    }
}