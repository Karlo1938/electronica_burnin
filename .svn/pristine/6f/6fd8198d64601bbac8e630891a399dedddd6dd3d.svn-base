using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;


namespace Test_Burning.Classes
{
    class Xml
    {
        public const string DEF_FILE = Form1.PATH + "Parametros.xml";

        public Dictionary<string, string> dReferencias;
        public Dictionary<string, string> dParametros;

        public Xml()
        {
            dReferencias = new Dictionary<string, string>();
            readfromXml();            
        }

        public Xml(string referencia)
        {
            dParametros = new Dictionary<string, string>();
            readfromXml(referencia);
        }

        public void readfromXml()
        {
            try
            {
                var doc = XDocument.Load(DEF_FILE);
                foreach (var register in doc.Root.Element("Referencias").Elements("Referencia"))
                {
                    dReferencias.Add(register.Attribute("Name").Value, register.Attribute("Voltaje").Value);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("NO SE HA ENCONTRADO EL ARCHIVO DE CONFIGURACION\nCONTACTE CON EL ADMINISTRADOR Y CIERRE LA APLICACION", "ERROR DEL SISTEMA", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        public void readfromXml(string referencia)
        {
            try
            {
                var doc = XDocument.Load(DEF_FILE);
                foreach (var register in doc.Root.Element("Referencias").Elements("Referencia"))
                {
                    if (register.Attribute("Name").Value == referencia)
                    {
                        dParametros.Add("Referencia", register.Attribute("Name").Value);
                        dParametros.Add("Voltaje", register.Attribute("Voltaje").Value);
                        //dParametros.Add("Cfg_cel", register.Attribute("Cfg_cel").Value);
                        //dParametros.Add("Cfg_pany", register.Attribute("Cfg_pany").Value);
                        dParametros.Add("Cfg_ant", register.Attribute("Cfg_ant").Value);
                        //dParametros.Add("Cfg_alr", register.Attribute("Cfg_alr").Value);
                        dParametros.Add("N_marques", register.Attribute("N_marques").Value);
                        dParametros.Add("Familia", register.Attribute("Familia").Value);
                        dParametros.Add("Antipanico", register.Attribute("Antipanico").Value);
                    }
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("NO SE HA ENCONTRADO EL ARCHIVO DE CONFIGURACION\nCONTACTE CON EL ADMINISTRADOR Y CIERRE LA APLICACION", "ERROR DEL SISTEMA", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }
                
        public static string xmlAtributo(string archivo, string grupo, string elemento, string atributo)
        {
            try
            {
                XmlDocument xDoc = new XmlDocument();
                xDoc.Load(archivo);
                XmlNodeList nCab = xDoc.GetElementsByTagName(grupo);
                XmlNodeList nDato = ((XmlElement)nCab[0]).GetElementsByTagName(elemento);
                return nDato[0].Attributes[atributo].Value;
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("NO SE HA ENCONTRADO EL ARCHIVO DE CONFIGURACION\nCONTACTE CON EL ADMINISTRADOR Y CIERRE LA APLICACION", "ERROR DEL SISTEMA", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                return "";
            }
        }

        /// <summary>
        /// Busca el valor de un atributo buscado a traves de otro atributo que  conocemos
        /// </summary>
        /// <param name="archivo">Archivo XML</param>
        /// <param name="grupo">Grupo de elementos</param>
        /// <param name="elemento">Tipo de elemento en el que queremos realizar la busqueda</param>
        /// <param name="atributo">Atributo cuyo valor conocemos</param>
        /// <param name="elemetBuscado">Valor del atributo que conocemos</param>
        /// <param name="atributoABuscar">Atributo que queremos buscar</param>
        /// <returns>Devuelve el valor del atributo que buscamos</returns>
        public static string xmlAtributo(string archivo, string grupo, string elemento, string atributo, string elemetBuscado, string atributoABuscar)
        {
            try
            { 
                var doc = XDocument.Load(archivo);
                foreach (var register in doc.Root.Element(grupo).Elements(elemento))
                {
                    if (register.Attribute(atributo).Value == elemetBuscado)
                    {
                        return register.Attribute(atributoABuscar).Value;
                    }
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("NO SE HA ENCONTRADO EL ARCHIVO DE CONFIGURACION\nCONTACTE CON EL ADMINISTRADOR Y CIERRE LA APLICACION", "ERROR DEL SISTEMA", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return "";
        }


        public static Dictionary <string, Int16> getList (string archivo, string raiz, string grupo, string elementos, string name, string value)
        {
            Dictionary<string, Int16> tmpList = new Dictionary<string, Int16>();
            try
            {
                var doc = XDocument.Load(archivo);
                foreach (var register in doc.Root.Element(archivo).Element(raiz).Elements(elementos))
                {
                    tmpList.Add(register.Attribute(name).Value, short.Parse(register.Attribute(value).Value));                    
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("NO SE HA ENCONTRADO EL ARCHIVO DE CONFIGURACION\nCONTACTE CON EL ADMINISTRADOR Y CIERRE LA APLICACION", "ERROR DEL SISTEMA", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
            return tmpList;
        }
    }
}
