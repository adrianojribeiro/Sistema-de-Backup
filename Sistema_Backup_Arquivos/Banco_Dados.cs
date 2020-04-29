using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sistema_Backup_Arquivos
{
    public class Banco_Dados
    {

        public bool Existe_Rotina { get; set; }

        conexao conecta = new conexao();


        public bool Checar_Rotina(string nome_pc)
        {
        bool existe_rotina = true;
            try
            {

                string sql = "SELECT COUNT(id_computador) FROM rotinas INNER JOIN computadores ON rotinas.id_computador = computadores.id where nome_computador = '" + nome_pc + "' ";

                using (MySqlConnection con = new MySqlConnection(conecta.endereco))
                {
                    try
                    {

                        con.Open();

                        if (con.State.ToString() == "Open")
                        {
                            using (MySqlCommand cmd = new MySqlCommand(sql, con))
                            {
                                using (MySqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader != null)
                                    {
                                        while (reader.Read())
                                        {
                                            if (Convert.ToInt32(reader[0].ToString()) > 0)
                                            {
                                                Existe_Rotina = true;
                                            }
                                            else
                                            {
                                                Existe_Rotina = false;
                                            }
                                        }
                                    }

                                }

                            }
                        }
                        else
                        {
                            MessageBox.Show("Sem conexao com o Servidor");
                        }
                    }
                    catch (Exception erro)
                    {
                        MessageBox.Show(erro.ToString());
                    }
                }
            }
            catch (Exception erro)
            {
                MessageBox.Show(erro.Message);
            }
                return existe_rotina;
        }


        public List<string> Lista_Pastas(string nome_pc)
        {
            List<string> lista = new List<string>();

            string sql = "SELECT pasta_copia FROM rotinas INNER JOIN computadores ON rotinas.id_computador = computadores.id where nome_computador = '" + nome_pc + "' ";

            using (MySqlConnection con = new MySqlConnection(conecta.endereco))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader != null)
                        {
                            while (reader.Read())
                            {
                                lista.Add(reader[0].ToString()); 
                               
                            }
                        }

                    }

                }
            }
            return lista;
        }

        public void Executado(string pc, string pasta, string inicio, string fim,string status,string nome_pasta,string tamanho_backup)
        {
            string sql = "INSERT INTO executados (computador,pasta,inicio,fim,status,nome_pasta,tamanho_backup) VALUES (@computador,@pasta,@inicio,@fim,@status,@nome_pasta,@tamanho_backup)";

            using (MySqlConnection con = new MySqlConnection(conecta.endereco))
            {
                con.Open();
                using (MySqlCommand cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@computador", pc);
                    cmd.Parameters.AddWithValue("@pasta", pasta);
                    cmd.Parameters.AddWithValue("@inicio", inicio);
                    cmd.Parameters.AddWithValue("@fim", fim);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@nome_pasta", nome_pasta);
                    cmd.Parameters.AddWithValue("@tamanho_backup", tamanho_backup);

                    cmd.ExecuteNonQuery();                    

                }

            }
        }

    }
}
