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

        private void Form1_Load(object sender, EventArgs e)
        {
            lblhora.Text = DateTime.Now.ToString();
            pictureBox.Visible = true;
            backgroundWorker.RunWorkerAsync();
        }         
      
       
        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {           

            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }
            
            // If the destination directory doesn't exist, create it.
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
            var pastaOrigem = Directory.GetCurrentDirectory();
            var data_hora_atual = Convert.ToString(DateTime.Now); //converte para string a Data Atual
            var nome_pasta = data_hora_atual.Replace(":", "").Replace("/", "").Replace(" ", "");
            var usuario = SystemInformation.ComputerName;
            var pastaDestino = @"\\192.168.2.12\Email\" + usuario + " - " + nome_pasta;
            Directory.CreateDirectory(pastaDestino);
            DirectoryCopy(pastaOrigem, pastaDestino, true);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
           // this.progressBar.Value = e.ProgressPercentage;
        }
    }

}


