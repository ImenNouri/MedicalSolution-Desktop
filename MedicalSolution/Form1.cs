using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Npgsql;

namespace MedicalSolution
{
    public partial class Form1 : Form
    {
        private NpgsqlConnection connection;
        public Form1()
        {
            InitializeComponent();
            string connString = "Host=ep-nameless-mountain-a5lkhw43.us-east-2.aws.neon.tech;" +
                               "Username=neondb_owner;" +
                               "Password=R8IhSm2dFAKG;" +
                               "Database=neondb;" +
                               "Port=5432;" +
                               "SSL Mode=Require;" +
                               "Trust Server Certificate=true;";

            try
            {
                connection = new NpgsqlConnection(connString);

                
                connection.Open();
                //MessageBox.Show("Connexion réussie à la base de données PostgreSQL !");
            }
            catch (Exception ex)
            {
                
                MessageBox.Show($"Erreur de connexion : {ex.Message}");
            }
            finally
            {
                
                if (connection != null && connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
       

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textBox1.Text.Trim();
            string password = textBox2.Text.Trim();

            
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                //MessageBox.Show("Veuillez entrer vos informations !");
                return;
            }

            try
            {
                connection.Open();

               
                string query = "SELECT * FROM personnel WHERE email=@username AND pwd=@password";
                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string role = reader["role"].ToString();
                            long codemLong = reader.GetInt64(reader.GetOrdinal("id"));
                            int idc = Convert.ToInt32(codemLong);

                            if (role == "AGENT")
                            {
                                Form2 form2 = new Form2();
                                form2.Show();
                                this.Hide();
                            }
                            else if (role == "SECRETARY")
                            {
                                reader.Close();
                                string query1 = @"
                                    SELECT d.codem
                                    FROM doctor d
                                    JOIN personnel p ON p.id = d.secretary_id
                                    WHERE d.secretary_id = @idc";

                                try
                                {
                                    using (NpgsqlCommand command1 = new NpgsqlCommand(query1, connection))
                                    {
                                        command1.Parameters.AddWithValue("@idc", idc);

                                        using (NpgsqlDataReader reader1 = command1.ExecuteReader())
                                        {
                                            if (reader1.Read())
                                            {
                                               
                                                int codem = Convert.ToInt32(reader1["codem"]);

                                                
                                                Form6 form6 = new Form6(codem);
                                                form6.Show();
                                                this.Hide();
                                            }
                                            else
                                            {
                                                MessageBox.Show("Aucun médecin trouvé pour cette secrétaire.");
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show($"Erreur : {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Identifiants incorrects.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur : {ex.Message}");
            }
            finally
            {
                connection.Close();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                
                textBox2.PasswordChar = '\0';
            }
            else
            {
                
                textBox2.PasswordChar = '*';
            }
        }


        private void label6_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
