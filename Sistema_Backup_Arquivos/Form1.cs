﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

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

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                var raiz_destino = @"\\192.168.2.12\Backups\";                                      
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
                                               
                        Compactar_Pasta(pastaOrigem,pastaDestino + ".zip");
                        data_hora_fim = Convert.ToString(DateTime.Now);
                        passo += 1;
                        backgroundWorker.ReportProgress(passo, tamanho);

                        Thread.Sleep(1000);

                        DirectoryInfo pasta_atual = new DirectoryInfo(caminho);
                        long tamanho_pasta_long = TamanhoTotalDiretorio(pasta_atual, true);
                        string tamanho_pasta = FormataExibicaoTamanhoArquivo(tamanho_pasta_long);
                        dados.Executado(usuario, caminho, data_hora_inicio, data_hora_fim, "Ok", pastaDestino + ".zip", tamanho_pasta);
                    
                    }
                }
                catch (Exception erro)
                {
                    MessageBox.Show(erro.ToString());
                }
            }
            catch (Exception erro)
            {
                MessageBox.Show(erro.Message);
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
            string tamanho_atual_convertido = FormataExibicaoTamanhoArquivo(tamanho_ja_copiado);

            lblstatustamanho.Text = tamanho_atual_convertido + " de " + tamanho_total + " copiados" + " - " + Convert.ToString(e.ProgressPercentage) + "/" + Convert.ToString(e.UserState) + " (Pastas)"; ;

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

        public void Compactar_Pasta(string origem,string destino)
        {           

            ZipFile.CreateFromDirectory(origem, destino, CompressionLevel.NoCompression, true);


        }
                     


    }
    

}


