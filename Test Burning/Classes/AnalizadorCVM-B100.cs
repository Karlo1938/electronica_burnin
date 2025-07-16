using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;
using Test;
using System.Windows.Forms;

namespace Test_Burning.Classes
{
    class AnalizadorCVM_B100
    {
        public const string FILE_INI = "Analizador.ini";
        public const byte ID_ANALIZADOR = 0x01;
        public const byte FUNCION_LECTURA = 0X04;
        public const byte FUNCION_ESCRITURA_RELE = 0x05;
        public const byte FUNCION_ESCRITURA = 0x10;
        public const byte RESPUESTA_INT = 0X04;
        public const byte RESPUESTA_ALARMAS = 26;
        public const byte RESPUESTA_ENTRADAS = 8;

        public bool AlarmaCorteSuministro {get; set;}
        public bool CaidaProtectorSobretensiones { get; set; }
        public UInt32 CorrienteTrifasica { get; set; }      //A x1000 = mA
        public UInt32 CorrienteL1 { get; set; }             //A x1000 = mA
        public UInt32 CorrienteL2 { get; set; }             //A x1000 = mA
        public UInt32 CorrienteL3 { get; set; }             //A x1000 = mA
        public UInt32 CorrienteNeutro { get; set; }         //A x1000 = mA
        public UInt32 TensionFaseTrifasica { get; set; }    //V x100
        public UInt32 TensionFaseL1 { get; set; }           //V x100
        public UInt32 TensionFaseL2 { get; set; }           //V x100
        public UInt32 TensionFaseL3 { get; set; }           //V x100
        public UInt32 TensionNeutro { get; set; }           //V x100
        public UInt32 PotenciaActiva { get; set; }          //W
        public UInt32 PotenciaReactiva { get; set; }        //kVAr
        public UInt32 EnergiaActiva { get; set; }           //kWh
        public UInt32 EnergiaReactiva { get; set; }         //kVAr
        public UInt32 CosPhi { get; set; }                  //x 100
        public bool ErrorConexion { get; set; }             //Error en alguna trama
        private int reintentos;


        private bool EndReceive;
        private byte[] TBReceive = new byte[255];   //Variable que almacena lo recibido por canal serie

        SerialPort serialPort1;    //Puerto serie
        
        public AnalizadorCVM_B100(string COM)
        {
            serialPort1 = new SerialPort(COM);
            serialPort1.PortName = COM;
            serialPort1.BaudRate = 19200;
            serialPort1.Parity = Parity.None;
            serialPort1.StopBits = StopBits.One;
            serialPort1.DataBits = 8;
            serialPort1.Handshake = Handshake.None;
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            //consutaParametros();
            serialPort1Open();
        }

        ~AnalizadorCVM_B100()
        {
            serialPort1Close();
        }

        public void consutaParametros()
        {
            //ErrorConexion = false;
            
            getAlarmaCorteSuministro();
            getCaidaProtectorSobretensiones();
            //getCorrienteNeutro();
            //getCorrienteTrifasica();
            //getCorrienteL1();
            //getCorrienteL2();
            //getCorrienteL3();
            //getTensionFaseTrifasica();
            //getTensionFaseL1();
            //getTensionFaseL2();
            //getTensionFaseL3();
            //getTensionNeutro();
            //getPotenciaActiva();
            //getPotenciaReactiva();
            getEnergiaActiva();
            getEnergiaReactiva();
            //getCosPhi();
            //habilitaConfiguracion();
            //cambiaEstadoRele2(true);
           // serialPort1Close();
            
            ////Trama habilita la configuracion de registros
            //trama[0] = 0x01;
            //trama[1] = 0x10;
            //trama[2] = 0x2A;
            //trama[3] = 0x97;
            //trama[4] = 0x00;
            //trama[5] = 0x01;
            //trama[6] = 0xB8;
            //trama[7] = 0x3D;

            //Trama consulta configuracion modo conexion de medida debe ser 0x04 Trifasico con neutro
            //trama[0] = 0x01;
            //trama[1] = 0x04;
            //trama[2] = 0x2A;
            //trama[3] = 0x9D;
            //trama[4] = 0x00;
            //trama[5] = 0x01;
            //trama[6] = 0xA8;
            //trama[7] = 0x3C;

            ////Trama consulta configuracion relacion de transformacion
            //trama[0] = ID_ANALIZADOR;
            //trama[1] = FUNCION_LECUTRA;
            //trama[2] = 0x27;
            //trama[3] = 0x10;
            //trama[4] = 0x00;
            //trama[5] = 0x0E;
            //trama[6] = 0x7A;
            //trama[7] = 0xBF;
        }

        public void resetAlarmaRL1()
        {
        }


        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getCorrienteNeutro()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;
            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x32;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            CorrienteNeutro = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>i
        private void getCorrienteL1()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x02;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            CorrienteL1 = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getCorrienteL2()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x12;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            CorrienteL2 = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getCorrienteL3()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x22;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            CorrienteL3 = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getCorrienteTrifasica()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x40;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            CorrienteTrifasica = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getTensionNeutro()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x30;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            TensionNeutro = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getTensionFaseL1()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x00;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            TensionFaseL1 = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getTensionFaseL2()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x10;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            TensionFaseL2 = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getTensionFaseL3()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x20;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            TensionFaseL3 = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getTensionFaseTrifasica()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x3E;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            TensionFaseTrifasica = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getPotenciaActiva()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x42;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            PotenciaActiva = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getPotenciaReactiva()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x64;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            PotenciaReactiva = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getEnergiaActiva()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x06;
            trama[3] = 0x85;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            EnergiaActiva = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getEnergiaReactiva()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x06;
            trama[3] = 0xA9;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            EnergiaReactiva = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getCosPhi()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            UInt32 uValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x00;
            trama[3] = 0x4C;
            trama[4] = 0x00;
            trama[5] = 0x02;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerDWord(out uValor))
                    Thread.Sleep(1);//ErrorConexion = true;
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;
            CosPhi = uValor;
        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getAlarmaCorteSuministro()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            Byte bValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x75;
            trama[3] = 0x58;
            trama[4] = 0x00;
            trama[5] = 0x0d;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerAlarmas(out bValor))
                    falloDeConexion(true);
                else
                {
                    falloDeConexion(false);
                    if (bValor == 0)
                        AlarmaCorteSuministro = false;
                    else
                        AlarmaCorteSuministro = true;
                }
            }
            else
                falloDeConexion(true);

        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void getCaidaProtectorSobretensiones()
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            Byte bValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_LECTURA;
            trama[2] = 0x59;
            trama[3] = 0xD8;
            trama[4] = 0x00;
            trama[5] = 0x04;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerValorEntrada1(out bValor))
                    Thread.Sleep(1);//ErrorConexion = true;
                else
                {
                    if (bValor == 0)
                        CaidaProtectorSobretensiones = true;
                    else
                        CaidaProtectorSobretensiones = false;
                }
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;

        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void habilitaConfiguracion() //No funciona
        {
            byte[] trama = new byte[8];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            Byte bValor = 0;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_ESCRITURA;
            trama[2] = 0x2A;
            trama[3] = 0x97;
            trama[4] = 0x00;
            trama[5] = 0x01;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[6] = Crc[0];
            trama[7] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerValorEntrada1(out bValor))
                    Thread.Sleep(1);//ErrorConexion = true;
                else
                {
                    if (bValor == 0)
                        CaidaProtectorSobretensiones = true;
                    else
                        CaidaProtectorSobretensiones = false;
                }
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;

        }
        /// <summary>
        /// Envia la trama de lectura de la variable solicitada y actualiza la variable si la recepcion ha sido buena
        /// en caso de no recibir respuesta por parte del analizador, actualiza la variable ErrorConexión = true;
        /// </summary>
        private void cambiaEstadoRele2(bool activada) //No funciona
        {
            byte[] trama = new byte[10];
            byte[] Crc = new byte[2];
            bool recibido;
            UInt16 uCrc;
            Byte bActivada;
            Byte bValor = 0;

            if (activada)
                bActivada = 0x01;
            else
                bActivada = 0x00;

            trama[0] = ID_ANALIZADOR;
            trama[1] = FUNCION_ESCRITURA;
            trama[2] = 0x4F;
            trama[3] = 0x34;
            trama[4] = 0x00;
            trama[5] = 0x01;
            trama[6] = 0x00;
            trama[7] = bActivada;
            uCrc = modRTU_CRC(trama, trama.Length - 2); //Calcula el crc-16 (modbus)
            Crc = BitConverter.GetBytes(uCrc);
            trama[8] = Crc[0];
            trama[9] = Crc[1];
            clearBytes();
            envioTrama(trama);

            recibido = (waitResponse());  //Espera a que responda y actualiza la variable
            if (recibido)
            {
                if (!extraerValorEntrada1(out bValor))
                    Thread.Sleep(1);//ErrorConexion = true;
                else
                {
                    if (bValor == 0)
                        CaidaProtectorSobretensiones = true;
                    else
                        CaidaProtectorSobretensiones = false;
                }
            }
            else
                Thread.Sleep(1);//ErrorConexion = true;

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="buf"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        private UInt16 modRTU_CRC(byte[] buf, int len)
        {
            UInt16 crc = 0xFFFF;
            try
                {
            for (int pos = 0; pos < len; pos++)
            {
                crc ^= (UInt16)buf[pos];          // XOR byte into least sig. byte of crc

                for (int i = 8; i != 0; i--)
                {    // Loop over each bit
                    if ((crc & 0x0001) != 0)
                    {      // If the LSB is set
                        crc >>= 1;                    // Shift right and XOR 0xA001
                        crc ^= 0xA001;
                    }
                    else                            // Else LSB is not set
                        crc >>= 1;                    // Just shift right
                }
            }
                }
                catch{ }
            // Note, this number has low and high bytes swapped, so use it accordingly (or swap bytes)
            return crc;
        }
        /// <summary>
        /// Comprueba si ha llegado el mensaje de respuesta del Analizador
        /// </summary>
        /// <returns></returns>
        private bool waitResponse()
        {
            EndReceive = true;
            int i = 0;
            while (EndReceive & i < 100) //Espera a que se apague el semaforo o a el timeout
            {
                Thread.Sleep(1);
                i++;
            }
            if (!EndReceive) //Si se ha apagado el semaforo devuelve true y en caso contrario false
                return true;
            else
            {
                EndReceive = false;
                return false;
            }
        }    
        /// <summary>
        /// 
        /// </summary>
        /// <param name="trama"></param>
        private void envioTrama(byte[] trama)
        {
            try { serialPort1.Write(trama, 0, trama.Length); } //Envia la trama pasada como argumento
            catch { } //Si falla lo ignora porque ya se dara cuenta de que no ha contestado

        }
        /// <summary>
        /// Limpia los bytes de la anterior recepcion por puerto serie
        /// </summary>
        private void clearBytes()
        {
            for (int i = 0; i < TBReceive.Length ; i++)
                TBReceive[i] = 0;
        }
        /// <summary>
        /// Extrae el valor del word que ha recibido en la trama
        /// </summary>
        /// <param name="id">Direccion del analizador</param>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerDWord(out UInt32 resultado)
        {
            byte[] Crc = new byte[2];
            UInt16 uCrc = modRTU_CRC(TBReceive, TBReceive[2] +3);
            Crc = BitConverter.GetBytes(uCrc);

            if (TBReceive[0] == ID_ANALIZADOR && TBReceive[1] == FUNCION_LECTURA && TBReceive[2] == RESPUESTA_INT && TBReceive[7] == Crc[0] && TBReceive[8] == Crc[1]) //Comprueba que la trama sea correcta Falta cambiar por la variable correcta
            {

                byte[] datoCovertir = new byte[4];//Crea una matriz de 2 bytes y les invierte el orden para sacar el valor entero                
                datoCovertir[0] = TBReceive[6];
                datoCovertir[1] = TBReceive[5];
                datoCovertir[2] = TBReceive[4];
                datoCovertir[3] = TBReceive[3];

                resultado = BitConverter.ToUInt32(datoCovertir, 0); //Convierte los dos byte en un valor entero
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                resultado = 0; //Devuelve un 0 y un false
                return false;
            }
        }
        /// <summary>
        /// Extrae el valor del word que ha recibido en la trama
        /// </summary>
        /// <param name="id">Direccion del analizador</param>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerAlarmas(out Byte resultado)
        {
            byte[] Crc = new byte[2];
            UInt16 uCrc = modRTU_CRC(TBReceive, TBReceive[2] + 3);
            Crc = BitConverter.GetBytes(uCrc);

            if (TBReceive[0] == ID_ANALIZADOR && TBReceive[1] == FUNCION_LECTURA && TBReceive[2] == RESPUESTA_ALARMAS && TBReceive[29] == Crc[0] && TBReceive[30] == Crc[1]) //Comprueba que la trama sea correcta Falta cambiar por la variable correcta
            {
                resultado = TBReceive[28];
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                resultado = 0; //Devuelve un 0 y un false
                return false;
            }
        }
        /// <summary>
        /// Extrae el valor del word que ha recibido en la trama
        /// </summary>
        /// <param name="id">Direccion del analizador</param>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerValorEntrada1(out Byte resultado)
        {
            byte[] Crc = new byte[2];
            UInt16 uCrc = modRTU_CRC(TBReceive, TBReceive[2] + 3);
            Crc = BitConverter.GetBytes(uCrc);

            if (TBReceive[0] == ID_ANALIZADOR && TBReceive[1] == FUNCION_LECTURA && TBReceive[2] == RESPUESTA_ENTRADAS && TBReceive[11] == Crc[0] && TBReceive[12] == Crc[1]) //Comprueba que la trama sea correcta Falta cambiar por la variable correcta
            {
                resultado = TBReceive[10];
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                resultado = 0; //Devuelve un 0 y un false
                return false;
            }
        }

        private void falloDeConexion(bool falloLaConexion)
        {
            if (falloLaConexion)
                reintentos++;
            else
            {
                reintentos = 0;
                this.ErrorConexion = false;
            }

            if (reintentos > 10)
                this.ErrorConexion = true;

        }


        /// <summary>
        /// Evento de recepción de datos por canal serie
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            byte[] recibido = new byte[255];
            Thread.Sleep(35); //Espera para recibir toda la trama antes de configurar buffer
            try
            {
                serialPort1.Read(recibido, 0, serialPort1.BytesToRead); //Almacena la trama en una matriz de bytes
            }
            catch
            {
                try
                {
                    serialPort1.DiscardInBuffer(); //Si algo ha ido mal descarta el buffer para que no vuelva a pasar
                }
                catch { };
            }
            TBReceive = recibido;
            EndReceive = false; //Apaga el semaforo
        }
        /// <summary>
        /// Método para abrir el puerto serie
        /// </summary>
        public void serialPort1Open()
        {
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Open();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }
        /// <summary>
        /// Método para cerrar el puerto serie
        /// </summary>
        public void serialPort1Close()
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }        

    }

}
