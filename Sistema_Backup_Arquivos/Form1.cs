using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Sistema_Backup_Arquivos
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string data_hora_inicio;
        string data_hora_fim;             
        string pastaOrigem;


        string nome_pc;

        Banco_Dados dados = new Banco_Dados();
        List<string> lista = new List<string>();

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox.Visible = false;     

            nome_pc = SystemInformation.ComputerName;
            dados.Checar_Rotina(nome_pc);    
                     
            if (dados.Existe_Rotina == true)
            {
                lblhora.Text = DateTime.Now.ToString();
                pictureBox.Visible = true;
                lista = dados.Lista_Pastas(nome_pc);
                backgroundWorker.RunWorkerAsync();                      
            }

            if (dados.Existe_Rotina == false)
            {
                this.Close();
            }        

        }       

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {      
            
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            //if (!dir.Exists)
            //{
            //    throw new DirectoryNotFoundException(
            //        "Source directory does not exist or could not be found: "
            //        + sourceDirName);
            //}

            //If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {            
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);      
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);                   
                }
            }          
        }              

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // var raiz_destino = @"\\192.168.2.12\Backups\";            

            var raiz_destino = @"\\Servidor\d\arquivos_teste\";

          
            int tamanho = lista.Count;
  

            try
            {

                int passo = 0;              

                foreach (var caminho in lista)
                {

                    Thread.Sleep(1000);

                    data_hora_inicio = Convert.ToString(DateTime.Now); //converte para string a Data Atual         

                    var nome_pasta = data_hora_inicio.Replace(":", "").Replace("/", "").Replace(" ", "");
                    var usuario = SystemInformation.ComputerName;

                    pastaOrigem = caminho; //busca de uma lista localizada no banco de dados

                    var pastaDestino = raiz_destino + usuario + " - " + nome_pasta;

                    Directory.CreateDirectory(pastaDestino);

                    DirectoryCopy(pastaOrigem, pastaDestino, true);

                    data_hora_fim = Convert.ToString(DateTime.Now);

                    dados.Executado(usuario, caminho, data_hora_inicio, data_hora_fim, "Ok", pastaDestino);

                    passo += 1;

                    //   for (var passo = 1; passo <= tamanho; passo++)

                    backgroundWorker.ReportProgress(passo, tamanho);
                }
            }
            catch (Exception erro)
            {
                MessageBox.Show(erro.ToString());
            }
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Thread.Sleep(1000);
            this.Close();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {    
            progressBar.Maximum = Convert.ToInt32(e.UserState);
            lbltotal.Text = Convert.ToString(e.UserState);


            progressBar.Value = e.ProgressPercentage;
            lblatual.Text = Convert.ToString(e.ProgressPercentage);
        }
    }

}


