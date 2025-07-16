using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test_Burning.Classes;
using System.Xml;
using System.Xml.Linq;

namespace Test_Burning
{
    public static class Carros
    {
        private const byte MAX_CARROS = 24;
        public static Carro[] ListaCarros;


        public static void initCarros()
        {
            ListaCarros = new Carro[MAX_CARROS + 1];
            //for(int i = 0; i< MAX_CARROS; i++)
            //{
            //    ListaCarros[i] = new Carro();
            //}

            try
            {
                var doc = XDocument.Load(Xml.DEF_FILE);
                foreach (var register in doc.Root.Element("Carros").Elements("Carro"))
                {                    
                    byte bCarro = Convert.ToByte(register.Attribute("Num").Value);
                    ListaCarros[bCarro] = new Carro();                    
                    ListaCarros[bCarro].IP = register.Attribute("IP").Value;
                    ListaCarros[bCarro].Port = Convert.ToInt32(register.Attribute("Port").Value);
                    ListaCarros[bCarro].Tipo = (TipoCarro)Enum.Parse(typeof(TipoCarro), register.Attribute("Tipo").Value);
                }
            }
            catch(Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.ToString() + e.HelpLink , "ERROR DEL SISTEMA", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public class Carro
        {
            public TipoCarro Tipo;
            public string IP;
            public Int32 Port;

            public Carro()
            {

            }

        }
        
    }
}
