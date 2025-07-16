using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Test_Burning.Classes
{
    class Barcodes
    {
        public const string MARCA_REF = "(01)"; //Código Referencia
        public const string MARCA_REV = "(93)"; //Código Revisión
        public const string MARCA_SERIAL = "(21)"; //Código Numero de serie
        public const string MARCA_LOTE = "(10)"; //Código de Lote
        public const string MARCA_FAB = "(412)"; //Código de Fabricante
        public const string MARCA_REF_ELEC = "(240)"; //Código de referencia secundario, para electronicas en motorizaciones
        public const string MARCA_SERIAL_ELEC = "(250)"; //Código de Numero de serie secundario, para electronicas en motorizaciones


        public enum EBarcode
        {
            Unknown,
            Pieza,
            Layout,
            AñadirCarro,
            ExtraerCarro,
            Operario,
            Kill
        }

        public static EBarcode checkBarcode(string sBarcode)
        {
            if (sBarcode.StartsWith(MARCA_REF) && sBarcode.Contains(MARCA_REV) && sBarcode.Contains(MARCA_SERIAL))
            {
                string referencia = codigo(sBarcode, MARCA_REF, "(");
                Xml mXml = new Xml();
                foreach (var element in mXml.dReferencias)
                {
                    if (referencia == element.Key)
                        return EBarcode.Pieza;
                }
            }
            else if (sBarcode.StartsWith("OP") && sBarcode.Length > 2 && sBarcode.Length < 6)
                return EBarcode.Operario;
            else if (sBarcode.Equals("A") || sBarcode.Equals("B") || sBarcode.Equals("C") || sBarcode.Equals("D") || sBarcode.Equals("E") ||
                sBarcode.Equals("F") || sBarcode.Equals("G") || sBarcode.Equals("H") || sBarcode.Equals("I"))
                return EBarcode.Layout;
            else if (sBarcode.StartsWith("CARRO_"))
                return EBarcode.AñadirCarro;
            else if (sBarcode.StartsWith("EXTRAER_"))
                return EBarcode.ExtraerCarro;
            else if (sBarcode.ToUpper().StartsWith("KILL"))
                return EBarcode.Kill;

            return EBarcode.Unknown;
        }

        /// <summary>
        /// Devuelve el codigo de barras en mayusculas 
        /// </summary>
        /// <param name="text">Texto capturado por el lector</param>
        /// <returns>Texto modificado</returns>
        public static string BarcodeToUpper(string text)
        {
            string sBarcode = text.ToUpper();
            return sBarcode;
        }

        /// <summary>
        /// Devuelve el string comprendido entre el string de inicio y el string de final
        /// En caso de que no encuentre el string de inicio devolverá ""
        /// En caso de que no encuentre el string final, considera el final de la trama
        /// </summary>
        /// <param name="cadena">Cadena de texto original</param>
        /// <param name="inicio">String de inicio</param>
        /// <param name="final">String de final</param>
        /// <returns>Devuelve la cadena comprendida entre el string de inicio y el string de final</returns>
        public static string codigo(string cadena, string inicio, string final)
        {
            int firstLength = inicio.Length; //Cuenta la longitud del string de inicio
            int first = cadena.IndexOf(inicio); //Indica el inicio del string de inicio
            if (first == -1) return ""; //Si no existe el string de inicio no devuelve nada
            cadena = cadena.Remove(0, first + firstLength); //Borra de la cadena todo lo que sobra por detras
            int last = cadena.IndexOf(final); //Indica el inicio del string de final
            if (last == -1) last = cadena.Length; //Si no existe, asume que es el final de la trama
            cadena = cadena.Remove(last, cadena.Length - last); //Borra de la cadena todo lo que sobra por delante
            return cadena; //Devuelve la cadena
        }

        public static Dictionary<string, string> DecodificarQR(string cadenaCodificada)
        {
            //Claves válidas dentro de un código QR
            string[] claves = { "01", "93", "21", "95", "401", "10", "00", "98", "99", "37", "500", "501", "502", "503", "580", "581", "582", "583", "584", "585", "586" };
            Dictionary<string, string> diccionario = new Dictionary<string, string>();

            // Estra expresión regular obtiene directamente todos los pares código-valur (xx)xxxx(yy)yyyyy....
            Regex regex = new Regex(@"\((\d+)\)([^\(]+)");

            MatchCollection matches = regex.Matches(cadenaCodificada);

            foreach (Match match in matches)
            {
                string codigo = match.Groups[1].Value;
                string valor = match.Groups[2].Value.Trim();
                diccionario[codigo] = valor;
            }

            // Agregar en el diccionario todas las claves con valor vacío aunque no aparezcan en el código QR
            foreach (string s in claves)
            {
                if (!diccionario.ContainsKey(s))
                {
                    diccionario[s] = "";
                }
            }
            return diccionario;
        }
    }
}
