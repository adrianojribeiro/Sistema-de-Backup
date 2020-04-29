using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sistema_Backup_Arquivos
{
    public class conexao
    {

        public string endereco;       

        public conexao()
        {
            endereco = "server=192.168.2.12; port=3306;User Id=root;database=backups; password=root;convert zero datetime=True";
        //  endereco = "server=192.168.1.199; port=3306;User Id=adriano;database=backups; password=g5a7b9t3;convert zero datetime=True";

        //   endereco = "server=localhost; port=3306;User Id=root;database=backups; password=root;convert zero datetime=True";


        }
    }
}
