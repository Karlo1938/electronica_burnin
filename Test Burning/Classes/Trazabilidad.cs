using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test;
using System.IO;
using System.Collections;
using DatabaseConnector;
using System.Data;
using EmbalajePrimario.DatabaseConnector;
using System.Diagnostics;
using System.Windows.Forms;


namespace Test_Burning.Classes
{
    class Trazabilidad
    {
        //private string FILE_INI;

        //public Trazabilidad(string FileIni)
        //{
        //    this.FILE_INI = FileIni;
        //}

        /// <summary>
        /// Este metodo crea un archivo de trazabilidad y le crea una cabecera con informacion de la pieza
        /// </summary>
        /// <param name="nPieza">Pieza en cuestión</param>
        /// <param name="sOperario">nº de operario que realiza la operación</param>
        /// <param name="testName">Nombre actual del test donde se muestra la version del mismo</param>
        public void cabeceraTrazabilidad(Pieza nPieza, string sOperario, string testName)
        {
            //Crea un objeto de consulta a archivo .INI
            //IniManager m_iniManager = new IniManager(FILE_INI);
            //Prepara el Path del archivo de trazabilidad
            string sSerial = nPieza.Serial;
            string sRefElectronica = "";
            string sSerialElectronica = "";
            if (sSerial.Contains("(240)"))
                sRefElectronica = " - P/N ELECTRONICA:" + Barcodes.codigo(sSerial, "(240)", "(");
            if (sSerial.Contains("(250)"))
                sSerialElectronica = " - S/N ELECTRONICA:" + Barcodes.codigo(sSerial, "(250)", "(");

            //Crea el archivo de trazabilidad
            FileStream fs = new FileStream(nPieza.Path, FileMode.Create, FileAccess.Write);
            fs.Close();

            //Añade las siguientes lineas al archivo de trazabilidad
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(nPieza.Path, true))
            {
                file.WriteLine("************************ Test Burning ************************");
                file.WriteLine("Testeado con: " + testName);
                file.WriteLine("Pieza: P/N: " + Barcodes.codigo(sSerial, "(01)", "(") +
                    " - S/N:" + Barcodes.codigo(sSerial, "(21)", "(") +
                    " - REV:" + Barcodes.codigo(sSerial, "(93)", "(") +
                    sRefElectronica + sSerialElectronica);
                file.WriteLine("Fecha Inicio: " + nPieza.FechaInicio.Insert(2, "/").Insert(5, "/"));
                file.WriteLine("Fecha Fin:");
                file.WriteLine("Hora Inicio: " + nPieza.HoraInicio.Insert(2, ":").Insert(5, ":"));
                file.WriteLine("Hora Fin:");
                file.WriteLine("Operario: " + sOperario);
                file.WriteLine("Posición: " + nPieza.Posicion);
                file.WriteLine("Resultado Final: No_finalizado...");
                file.WriteLine("==============================================================");
                file.WriteLine("");
                file.WriteLine("******************** Resultados del Test *********************");
            }
        }

        /// <summary>
        /// Este metodo modifica el contenido de un archivo pasado como argumento 
        /// </summary>
        /// <param name="fichero">Fichero en cuestion</param>
        /// <param name="textRemplazo">Texto que se ha de dejar y que esta antes del texto a remplazar</param>
        /// <param name="nuevoTexto">Texto que se quiere eliminar en el remplazo</param>
        /// <param name="extension">Extension del archivo por el que va a ser sustituido la extension .log</param>
        public void modificaResultadoArchivo(string fichero, string textRemplazo, string nuevoTexto, string extension)
        {
            //Si el nombre del fichero no está vacío
            if (fichero != (""))
            {
                int Lineas = 1;

                StreamReader sr = new StreamReader(fichero);

                while (sr.Peek() != -1)
                {
                    sr.ReadLine();
                    Lineas++;
                }

                sr.Close();

                //Definicion de variamoes temporales
                string[] cadena = new string[Lineas]; //matriz de cadenas de caracteres para recoger todo el contenido del txt.
                string cadena2 = ""; //cadena de texto donde almacena de forma temporal cada posicion de la matriz.
                int lon = 0; //variable que indica la posicion de la matriz.
                int lon1; //variable que indica la posicion de la matriz de escritura al nuevo archivo.


                //Lee todo el archivo y lo almacena en la matriz "cadena".
                StreamReader objReader = new StreamReader(fichero);
                string sLine = "";
                ArrayList arrText = new ArrayList();
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                    {
                        cadena[lon] = sLine;
                        if (cadena[lon].Contains(textRemplazo))
                            cadena[lon] = cadena[lon].Replace(textRemplazo, nuevoTexto);
                        lon++;
                    }
                }
                objReader.Close();

                //se carga la variable con valor 0 
                lon = 0;

                string NuevoFichero = fichero.Replace(".log", extension);

                //Se pasa cada linea del archivo Original y lo añade al archivo de trazabilidad.
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(NuevoFichero, true))
                {
                    lon = 0;
                    for (lon1 = 0; lon1 < cadena.Length; lon1++)
                    {
                        cadena2 = cadena[lon];
                        if (cadena2 != "\n")
                            file.WriteLine(cadena2);
                        lon++;
                    }
                }

                //Borra el fichero original
                File.Delete(fichero);
            }
        }

        /// <summary>
        /// Este metodo modifica el contenido de una linea de un archivo pasado como argumento 
        /// </summary>
        /// <param name="fichero">Fichero en cuestion</param>
        /// <param name="textRemplazo">Texto que se ha de dejar y que esta antes del texto a remplazar</param>
        /// <param name="nuevoTexto">Texto que se quiere eliminar en el remplazo</param>
        public void modificaArchivo(string fichero, string textRemplazo, string nuevoTexto)
        {
            //Si el nombre del fichero no está vacío
            if (fichero != (""))
            {
                int Lineas = 1;

                StreamReader sr = new StreamReader(fichero);

                while (sr.Peek() != -1)
                {
                    sr.ReadLine();
                    Lineas++;
                }

                sr.Close();

                //Definicion de variamoes temporales
                string[] cadena = new string[Lineas]; //matriz de cadenas de caracteres para recoger todo el contenido del txt.
                string cadena2 = ""; //cadena de texto donde almacena de forma temporal cada posicion de la matriz.
                int lon = 0; //variable que indica la posicion de la matriz.
                int lon1; //variable que indica la posicion de la matriz de escritura al nuevo archivo.


                //Lee todo el archivo y lo almacena en la matriz "cadena".
                StreamReader objReader = new StreamReader(fichero);
                string sLine = "";
                ArrayList arrText = new ArrayList();
                while (sLine != null)
                {
                    sLine = objReader.ReadLine();
                    if (sLine != null)
                    {
                        cadena[lon] = sLine;
                        if (cadena[lon].Contains(textRemplazo))
                            cadena[lon] = cadena[lon].Replace(textRemplazo, nuevoTexto);
                        lon++;
                    }
                }
                objReader.Close();

                //se carga la variable con valor 0 
                lon = 0;

                //Se pasa cada linea del archivo Original y lo añade al archivo de trazabilidad.
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fichero, false))
                {
                    lon = 0;
                    for (lon1 = 0; lon1 < cadena.Length; lon1++)
                    {
                        cadena2 = cadena[lon];
                        if (cadena2 != "\n")
                            file.WriteLine(cadena2);
                        lon++;
                    }
                }

            }
        }

        /// <summary>
        /// Añade una linea en un archivo 
        /// </summary>
        /// <param name="archivo">Path del archivo</param>
        /// <param name="linea"></param>
        public void añadeLinea(string archivo, string linea)
        {
            int Lineas = 1;

            StreamReader sr = new StreamReader(archivo);
            while (sr.Peek() != -1)
            {
                sr.ReadLine();
                Lineas++;
            }
            sr.Close();

            //Definicion de variamoes temporales
            string[] cadena = new string[Lineas]; //matriz de cadenas de caracteres para recoger todo el contenido del txt.
            string cadena2 = ""; //cadena de texto donde almacena de forma temporal cada posicion de la matriz.
            int lon = 0; //variable que indica la posicion de la matriz.
            int lon1; //variable que indica la posicion de la matriz de escritura al nuevo archivo.


            //Lee todo el archivo y lo almacena en la matriz "cadena".
            StreamReader objReader = new StreamReader(archivo);
            string sLine = "";
            ArrayList arrText = new ArrayList();
            while (sLine != null)
            {
                sLine = objReader.ReadLine();
                if (sLine != null)
                {
                    cadena[lon] = sLine;
                    lon++;
                }
            }
            objReader.Close();

            //se carga la variable con valor 0 
            lon = 0;

            //Se pasa cada linea del archivo Original y lo añade al archivo de trazabilidad.
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(archivo, false))
            {
                lon = 0;
                for (lon1 = 0; lon1 < cadena.Length; lon1++)
                {
                    cadena2 = cadena[lon];
                    if (cadena2 != "\n")
                        file.WriteLine(cadena2);
                    lon++;
                }
                file.WriteLine(linea);
            }
        }
        /// <summary>
        /// Añade una linea en un archivo 
        /// </summary>
        /// <param name="archivo">Path del archivo</param>
        /// <param name="linea"></param>
        public void añadeLineaExcel(string archivo, string linea)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(archivo, true))
            {
                file.WriteLine(linea);
            }
        }

        public bool InsertTestInSql(Pieza piezaLote = null, string horaFin = null, string fechaFin = null)
        {
            try
            {
                var dbConnector = ConnectDB.getBDConnector();
                if (dbConnector == null)
                    throw new Exception("No se pudo obtener una conexión válida a la base de datos.");

                if (piezaLote == null)
                {
                    piezaLote = new Pieza
                    {
                        Serial = "DUMMY_SERIAL",
                        FechaInicio = fechaFin ?? DateTime.Now.ToString("yyyy-MM-dd"),
                        HoraInicio = DateTime.Now.ToString("HH:mm:ss"),
                        Ciclos = 0,
                        eResultado = Resultado.OK,
                        eAlarma = 0
                    };
                }

                var data = Barcodes.DecodificarQR(piezaLote.Serial);
                if (data == null)
                    data = new Dictionary<string, string>();

                string testError;
                if ((int)piezaLote.eAlarma < 100)
                {
                    testError = "La electrónica detectó una alarma interna, Error " + piezaLote.eAlarma;
                }
                else if (Pieza.mensajesAlarma != null && Pieza.mensajesAlarma.ContainsKey(piezaLote.eAlarma))
                {
                    testError = Pieza.mensajesAlarma[piezaLote.eAlarma];
                }
                else
                {
                    testError = null;
                }

                string sql = @"
                    INSERT INTO test_electronica_visio_plus_burning (
                        referencia, descripcion, serial, machine_name, usuario, 
                        fecha, hora_inicio, hora_final,
                        numero_ciclos, lote, codigo_revision, codigo_fabricante, codigo_referencia_secundaria, serial_secundario, test_result, test_error
                    )
                    VALUES (
                        @referencia, @descripcion, @serial, @machine_name, @usuario,
                        @fecha, @hora_inicio, @hora_final,
                        @numero_ciclos, @lote, @codigo_revision, @codigo_fabricante, @codigo_referencia_secundaria, @serial_secundario, @test_result, @test_error
                    );";

                var fecha = DateTime.ParseExact(piezaLote.FechaInicio, "yyyy-MM-dd", null);
                var horaInicio = DateTime.ParseExact(piezaLote.HoraInicio, "HH:mm:ss", null);
                var horaFinal = string.IsNullOrWhiteSpace(horaFin)
                    ? DateTime.Now
                    : DateTime.ParseExact(horaFin, "HH:mm:ss", null);

                int res = dbConnector.ExecuteNonQuery(sql, DatabaseConnectorManager.Params(
                    "@referencia", data.ContainsKey("01") ? data["01"] : "DUMMY_REF",
                    "@descripcion", "Dummy pieza para test",
                    "@serial", piezaLote.Serial ?? "DUMMY_SERIAL",
                    "@machine_name", Environment.MachineName,
                    "@usuario", Environment.UserName,
                    "@fecha", fecha,
                    "@hora_inicio", horaInicio,
                    "@hora_final", horaFinal,
                    "@numero_ciclos", (int)piezaLote.Ciclos,
                    "@lote", data.ContainsKey("10") ? data["10"] : "DUMMY_LOTE",
                    "@codigo_revision", data.ContainsKey("93") ? data["93"] : "DUMMY_REV",
                    "@codigo_fabricante", data.ContainsKey("412") ? data["412"] : "DUMMY_FAB",
                    "@codigo_referencia_secundaria", data.ContainsKey("240") ? data["240"] : "DUMMY_REF2",
                    "@serial_secundario", data.ContainsKey("250") ? data["250"] : "DUMMY_SERIAL2",
                    "@test_result", piezaLote.eResultado == Resultado.OK ? "OK" :
                                   piezaLote.eResultado == Resultado.NOK ? "NOK" : "Unknown",
                    "@test_error", testError
                ));

                if (res <= 0)
                {
                    MessageBox.Show("Advertencia: la inserción no afectó filas.", "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al insertar en SQL: " + ex.Message, "DB Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }




    }
}
