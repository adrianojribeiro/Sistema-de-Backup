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

        long tamanho_total_backup = 0;
        long tamanho_ja_copiado = 0;

        string caminho_para_pasta = "";
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
                foreach (var caminho in lista)
                {
                    DirectoryInfo infoDiretorio = new DirectoryInfo(caminho);
                    tamanho_total_backup += TamanhoTotalDiretorio(infoDiretorio, true);
                }
            }

            catch (Exception erro)
            {
                MessageBox.Show(erro.Message);
            }
            
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
                    caminho_para_pasta = caminho; // linha reponsavel pela atualizacao do tamanho ja executado

                   var pastaDestino = raiz_destino + usuario + " - " + nome_pasta;

                    Directory.CreateDirectory(pastaDestino);
                    DirectoryCopy(pastaOrigem, pastaDestino, true);
                    data_hora_fim = Convert.ToString(DateTime.Now);
                    dados.Executado(usuario, caminho, data_hora_inicio, data_hora_fim, "Ok", pastaDestino);

                    passo += 1;
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
            progressBar.Value = e.ProgressPercentage;                       

            DirectoryInfo infoDiretorio = new DirectoryInfo(caminho_para_pasta);
            tamanho_ja_copiado += TamanhoTotalDiretorio(infoDiretorio, true);

            string tamanho_total = FormataExibicaoTamanhoArquivo(tamanho_total_backup);
            string tamanho_atual = FormataExibicaoTamanhoArquivo(tamanho_ja_copiado);

            lblstatustamanho.Text = tamanho_atual + " de " + tamanho_total + " copiados" + " - "+ Convert.ToString(e.ProgressPercentage) + "/" + Convert.ToString(e.UserState) + " (Pastas)"; ;

        }


        private long TamanhoTotalDiretorio(DirectoryInfo dInfo, bool includeSubDir)
        {
            //percorre os arquivos da pasta e calcula o tamanho somando o tamanho de cada arquivo
            long tamanhoTotal = dInfo.EnumerateFiles().Sum(file => file.Length);
            if (includeSubDir)
            {
                tamanhoTotal += dInfo.EnumerateDirectories().Sum(dir => TamanhoTotalDiretorio(dir, true));
            }
            return tamanhoTotal;
        }

        // Retorna o tamanho do arquivo para um tamanho
        // O formato padrão é "0.### XB", Ex: "4.2 KB" ou "1.434 GB"
        public string FormataExibicaoTamanhoArquivo(long i)
        {
            // Obtém o valor absoluto
            long i_absoluto = (i < 0 ? -i : i);
            // Determina o sufixo e o valor
            string sufixo;
            double leitura;
            if (i_absoluto >= 0x1000000000000000) // Exabyte
            {
                sufixo = "EB";
                leitura = (i >> 50);
            }
            else if (i_absoluto >= 0x4000000000000) // Petabyte
            {
                sufixo = "PB";
                leitura = (i >> 40);
            }
            else if (i_absoluto >= 0x10000000000) // Terabyte
            {
                sufixo = "TB";
                leitura = (i >> 30);
            }
            else if (i_absoluto >= 0x40000000) // Gigabyte
            {
                sufixo = "GB";
                leitura = (i >> 20);
            }
            else if (i_absoluto >= 0x100000) // Megabyte
            {
                sufixo = "MB";
                leitura = (i >> 10);
            }
            else if (i_absoluto >= 0x400) // Kilobyte
            {
                sufixo = "KB";
                leitura = i;
            }
            else
            {
                return i.ToString("0 bytes"); // Byte
            }
            // Divide por 1024 para obter o valor fracionário
            leitura = (leitura / 1024);
            // retorna o número formatado com sufixo
            return leitura.ToString("0.### ") + sufixo;
        }
    }

}


