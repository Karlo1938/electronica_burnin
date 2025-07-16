using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    public class EnumTB
    {
        public enum Id
        {
            Master, Slave1, Slave2, Slave3, Slave4, Slave5, Slave6, Slave7, Slave8, Slave9, Slave10, Slave11, Slave12, Slave13, Slave14, Slave15, Slave16, Slave17, Slave18, Slave19, Slave20
        }
        public enum Instruccion
        {
            EscribirByte = 0xb5,
            LeerByte = 0x5b,
            EscribirWord = 0x93,
            LeerWord = 0x39,
            LeerBuffer = 0x11,
            EnviarBucle = 0x30
        }
        public enum Input
        {
            Free, In1, In2,
            In3 = 4,
            In4 = 8,
            In5 = 16,
            In6 = 32,
            In7 = 64
        }
        public enum Output
        {
            Free,
            Out1,
            Out2,
            Out3 = 4,
            Out4 = 8,
            Out5 = 16,
            Out6 = 32,
            Out7 = 64,
            Led = 128,
        }
        public enum Rele
        {
            Free,
            RL1,
            RL2,
            RL3 = 4,
            RL4 = 8,
            ALL = 15
        }

        #region //Enumeraciones conexión bucle de corriente
        //Mapa de variables según documento "D00192 Mapas de variables ADII según operador.docx"
        public enum ParametroADII //Enumeración de las variables ADII
        {
            f_alarma,
            mode,
            cfg_cel,
            cfg_pany,
            cfg_alr,
            cfg_ant,
            apertura,
            t_pausa,
            port6,
            port7,
            direc,
            estat_cal,
            p_tancar,
            n_marques,
            v_tancar,
            v_obrir = 16,
            retard_t = 18,
            versio = 20,
            c_cicles = 22,
            c_cicst = 26,
            data_inst = 30,
            data_st = 32,
            tbam = 34,
            t_clau = 36,
            c_aper = 38,
            sens_ripwm,
            sens_rxpwm,
            nivell_ri,
            nivel_Bateria, //Antes nivell_rx
            dec_sens,
            Comp_pas, //Solo en Visio, en activa es Porte
            Aper_pro = 48, //Solo en Visio
            ResetControl = 50, //Solo en Visio desde 360
            ResetDSP = 52, //Solo en Visio desde 360
            versioDSP = 54,
            Test = 56, //Solo en Visio desde 360
        }
        public enum Funcion //Enumeración de la función
        {
            Lectura = 6,
            Escritura,
            TestSelectorOptima = 0x10,
            TestSelectorOptimaModoTest = 0x11
        }
        public enum Modo //Enumeracion de los modos
        {
            Close,
            Open,
            Automatic,
            OnlyExit,
            Farmacia
        }
        public enum InputPort6
        {
            Reposo,
            Foto1 = 1,
            Foto2 = 2,
            E_Bat = 4,
            Llave = 8,
            X_Pany = 32,
            X_Obert = 64,
            X_Tancat = 128
        }
        public enum InputPort7
        {
            Reposo,
            SelectorB = 1,
            Pany = 2,
            Sos = 4,
            Encoder1 = 8,
            SelectorA = 16,
            Encoder0 = 32,
            RadarExt = 64,
            RadarInt = 128
        }
        public enum AnomaliaVisio
        {
            SinAnomalia,
            Sobrecorriente,
            Entrada_SOS,
            Bloqueo_Cerrar,
            Entrada_Llave,
            Memoria_Parametros,
            Bloqueo_Al_Cerrar,
            Fotocelula_1_o_2 = 8,
            Incendio,
            Bateria = 11,
            Radar_Interior,
            Radar_Exterior,
            Fotocelula_3,
            Comunicacion_DSP,
            Secur
        }
        #endregion
    }

    public class TestBoardTCP
    {
        #region Constantes, variables y objetos
        //Constantes        
        //public const string FILE_INI = Form1.PATH + "TestBoard.ini"; //Tiene la dependencia de public string PATH en From1

        public const byte STX = 0x02;       //Byte de inicio de trama
        public const byte BYTE_H = 0xE8;    //Byte alto de la dirección, común para todas las direcciones que nos interesan. 
        public const byte AN0_L = 0x48;
        public const byte AN1_L = 0x4A;
        public const byte AN2_L = 0x4C;
        public const byte AN3_L = 0x4E;
        public const byte AN4_L = 0x50;
        public const byte AN5_L = 0x52;
        public const byte INPUTS_L = 0x54;
        public const byte OUTPUTS_L = 0x55;
        public const byte RELES_L = 0x56;
        public const byte SET_OUT_L = 0x57;
        public const byte RSET_OUT_L = 0x58;
        public const byte SET_RELES_L = 0x59;
        public const byte RSET_RELES_L = 0x5A;
        public const byte AD_HOLD_L = 0x5B;
        public const byte POSICION_L = 0x5C;
        public const byte HOLD_AN0_L = 0x5E;
        public const byte HOLD_AN1_L = 0x60;
        public const byte HOLD_AN2_L = 0x62;
        public const byte HOLD_AN3_L = 0x64;
        public const byte HOLD_AN4_L = 0x66;
        public const byte HOLD_AN5_L = 0x68;
        public const byte TECLA_IR = 0x6A;  //Código 1 -> Apertura red      Código 2 -> Cerrado     Código 3 -> Sólo Salir
                                            //Código 4 -> Abierto           Código 5 -> Un ciclo    Código 6 -> Automático

        private const int REINTENTOS = 3;
        //Variables
        // byte[] TBReceive = new byte[255];   //Variable que almacena lo recibido por canal serie
        // public bool EndReceive = false;    //Indica cuando termina la recepción.
        // bool StopReceive = false;   //Detiene la recepcion de tramas para poder leer la variable.
        private BusSerieTCP busSerie;
        //Objetos
       // private Semaforo semaforo;

        #endregion


        #region Constructores
        public TestBoardTCP(BusSerieTCP busSerie)
        {
            this.busSerie = busSerie;

        }
        #endregion


        #region Metodos publicos
        /// <summary>
        /// Comprueba si una entrada determinada está activa
        /// </summary>
        /// <param name="id">direccion del circuito Testboard utilizando el enumerado</param>
        /// <param name="inp">Input que queremos filtrar utilizando el enumerado</param>
        /// <param name="ItsOn">Parametro de salida que indica si está o no activa</param>
        /// <returns>devuelve true si consiguió comunicación</returns>
        public bool inputOn(EnumTB.Id id, EnumTB.Input inp, out bool ItsOn)
        {
            byte resultado = 0;
            if (leerByte((byte)id, (byte)INPUTS_L, out resultado)) //Lee el valor del byte de la entradas
            {
                resultado &= (byte)inp;     //Realiza un and sobre su valor y la entrada que queremos filtrar
                if (resultado == (byte)inp)  //Si estaba activa la entrada devuelve true al argumento de salida
                    ItsOn = true;
                else ItsOn = false;
                return true;                //Devuelve true porque ha conseguido comunicar
            }
            else ItsOn = false;             //Si no consigue comunicar devuelve false
            return false;
        }
        /// <summary>
        /// Realiza Set sobre las salidas del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar</param>
        /// <param name="setout">Valor de la variable para el set(ver enumerado Outputs)</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool set(byte id, byte setout)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = SET_OUT_L;
            trama[6] = setout;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, SET_OUT_L));
        }
        /// <summary>
        /// Realiza Set sobre las salidas del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <param name="setout">Salida que queremos activar</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool set(EnumTB.Id id, EnumTB.Output setout)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = SET_OUT_L;
            trama[6] = (byte)setout;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, SET_OUT_L));
        }
        /// <summary>
        /// Realiza Rset sobre las salidas del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar</param>
        /// <param name="setout">Valor de la variable para el reset(ver enumerado Outputs)</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool rSet(byte id, byte rsetout)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = RSET_OUT_L;
            trama[6] = rsetout;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, RSET_OUT_L));
        }
        /// <summary>
        /// Realiza Rset sobre las salidas del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <param name="setout">Salida que queremos desactivar</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool rSet(EnumTB.Id id, EnumTB.Output rsetout)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = RSET_OUT_L;
            trama[6] = (byte)rsetout;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, RSET_OUT_L));
        }
        /// <summary>
        /// Realiza Set sobre los reles del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar</param>
        /// <param name="setout">Valor de la variable para el set(ver enumerado Rele)</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool setRele(byte id, byte setrele)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = SET_RELES_L;
            trama[6] = setrele;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, SET_RELES_L));
        }
        /// <summary>
        /// Realiza Set sobre los reles del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <param name="setout">Rele que queremos activar</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool setRele(EnumTB.Id id, EnumTB.Rele setrele)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = SET_RELES_L;
            trama[6] = (byte)setrele;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, SET_RELES_L));
        }
        /// <summary>
        /// Realiza Rset sobre los reles del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar</param>
        /// <param name="setout">Valor de la variable para el reset(ver enumerado Rele)</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool rSetRele(byte id, byte rsetrele)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = RSET_RELES_L;
            trama[6] = rsetrele;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, RSET_RELES_L));
        }
        /// <summary>
        /// Realiza Rset sobre los reles del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <param name="setout">Salida que queremos desactivar</param>
        /// <returns>Es true si ha contestado el circuito</returns>
        public bool rSetRele(EnumTB.Id id, EnumTB.Rele rsetrele)
        {
            //Se genera la trama y se envia por serie al circuito seleccionado
            byte[] trama = new byte[8];
            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x05;
            trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
            trama[4] = BYTE_H;
            trama[5] = RSET_RELES_L;
            trama[6] = (byte)rsetrele;
            trama[7] = LRC(trama);
            envioTrama(trama);
            return (waitResponse(id, RSET_RELES_L));
        }
        /// <summary>
        /// Resetea todas las salidas del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <returns>Devuelve true si ha contestado el circuito</returns>
        public bool rSetAll(EnumTB.Id id)
        {
            if (!escribirByte((byte)id, RSET_RELES_L, 0xFF))
                return false;
            if (!escribirByte((byte)id, RSET_OUT_L, 0xFF))
                return false;
            return true;
        }
        /// <summary>
        /// Resetea todos los reles del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <returns>Devuelve true si ha contestado el circuito</returns>
        public bool rSetAllReles(EnumTB.Id id)
        {
            if (!escribirByte((byte)id, RSET_RELES_L, 0xFF))
                return false;
            return true;
        }
        /// <summary>
        /// Resetea todas las salidas del TestBoard
        /// </summary>
        /// <param name="id">Id del dispositivo al que queremos comunicar utilizando el enumerado</param>
        /// <returns>Devuelve true si ha contestado el circuito</returns>
        public bool rSetAllOut(EnumTB.Id id)
        {
            if (!escribirByte((byte)id, RSET_OUT_L, 0xFF))
                return false;
            return true;
        }
        /// <summary>
        /// Solicita la lectura de una variable cuyo resultado es un byte
        /// </summary>
        /// <param name="id">Numero del dispositivo</param>
        /// <param name="variable_L">Variable que queremos leer</param>
        /// <param name="valor">Variable de salida que contiene el valor leido</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerByte(byte id, byte variable_L, out byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[7];
            valor = 0;
            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x04;
            trama[3] = (byte)EnumTB.Instruccion.LeerByte;
            trama[4] = BYTE_H;
            trama[5] = variable_L;
            trama[6] = LRC(trama);
            envioTrama(trama);
            recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable
            if (recibido)
                recibido = extraerByte(id, out valor);     //Extrae el valor recibido en la trama
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// Solicita la lectura de una variable cuyo resultado es un byte
        /// </summary>
        /// <param name="id">Numero del dispositivo</param>
        /// <param name="variable_L">Variable que queremos leer</param>
        /// <param name="valor">Variable de salida que contiene el valor leido</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerWord(byte id, byte variable_L, out UInt16 valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[7];

            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x04;
            trama[3] = (byte)EnumTB.Instruccion.LeerWord;
            trama[4] = BYTE_H;
            trama[5] = variable_L;
            trama[6] = LRC(trama);
            envioTrama(trama);
            recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable
            extraerWord(id, out valor);     //Extrae el valor recibido en la trama
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// Solicita la lectura de un buffer de 16 bytes desde el byte de inicio.
        /// </summary>
        /// <param name="id">Numero del dispositivo</param>
        /// <param name="inicio">direccion baja de la primera variable que queremos leer</param>
        /// <param name="Buffer">matriz de bytes de salida</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerBuffer(byte id, byte inicio_L, out byte[] Buffer)
        {
            bool recibido = false;          //Indica si ha contestado el circuito
            byte[] trama = new byte[8];
            byte[] bufferRecibido = new byte[16];
            int reintentos = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x05;
                trama[3] = (byte)EnumTB.Instruccion.LeerBuffer;
                trama[4] = BYTE_H;
                trama[5] = inicio_L;
                trama[6] = 16;                  //Numero de bytes que quiero recibir
                trama[7] = LRC(trama);
                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, inicio_L));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerBuffer(id, inicio_L, out bufferRecibido)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
               
            }

            Buffer = bufferRecibido;
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// Escribe un valor en la variable seleccionada
        /// </summary>
        /// <param name="id">Numero del dispositivo</param>
        /// <param name="variable_L">Variable que queremos escribir</param>
        /// <param name="valor">Valor que queremos cargar a la variable</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>        
        public bool escribirByte(byte id, byte variable_L, byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[8];
            int reintentos = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x05;
                trama[3] = (byte)EnumTB.Instruccion.EscribirByte;
                trama[4] = BYTE_H;
                trama[5] = variable_L;
                trama[6] = valor;
                trama[7] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda
                reintentos++;
            }
            return recibido;                //Deuelve true si se ha recibido respuesta
        }
        /// <summary>
        /// Escribe un valor en la variable seleccionada
        /// </summary>
        /// <param name="id">Numero del dispositivo</param>
        /// <param name="variable_L">Variable que queremos escribir</param>
        /// <param name="valor">Valor que queremos cargar a la variable</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>        
        public bool escribirWord(byte id, byte variable_L, UInt16 valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[9];
            int reintentos = 0;
            byte[] bvalor = new byte[2];
            bvalor = BitConverter.GetBytes(valor);

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x06;
                trama[3] = (byte)EnumTB.Instruccion.EscribirWord;
                trama[4] = BYTE_H;
                trama[5] = variable_L;
                trama[6] = bvalor[1];
                trama[7] = bvalor[0];
                trama[8] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda
                reintentos++;
            }
            return recibido;                //Deuelve true si se ha recibido respuesta
        }
        /// <summary>
        /// lee un byte por bucle de corriente a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="variable_H">byte alto de la variable</param>
        /// <param name="variable_L">byte bajo de la variable</param>
        /// <param name="valor">byte que devuelve por bucle</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerByteBucle(byte id, byte idOperador, byte variable_H, byte variable_L, out byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[12];
            int reintentos = 0;
            byte temp = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x09;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x04;
                trama[7] = (byte)EnumTB.Instruccion.LeerByte;
                trama[8] = variable_H;
                trama[9] = variable_L;
                trama[10] = LRC(trama, 10);
                trama[11] = LRC(trama);

                ClearBytes(id);   //Limpia el contenido del mensaje anterior
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerByte(idOperador, out temp)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// lee un byte por bucle de corriente a traves del circuito de test con un numero de intentos predeterminado
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="variable_H">byte alto de la variable</param>
        /// <param name="variable_L">byte bajo de la variable</param>
        /// <param name="valor">byte que devuelve por bucle</param>
        /// <param name="intentos">numero de intentos que queremos que realice en la comunicacion hasta que responda</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerByteBucle(byte id, byte idOperador, byte variable_H, byte variable_L, out byte valor, int intentos)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[12];
            int reintentos = 0;
            byte temp = 0;

            while (!recibido && reintentos < intentos)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x09;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x04;
                trama[7] = (byte)EnumTB.Instruccion.LeerByte;
                trama[8] = variable_H;
                trama[9] = variable_L;
                trama[10] = LRC(trama, 10);
                trama[11] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerByte(idOperador, out temp)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// lee un word por bucle de corriente a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="variable_H">byte alto de la variable</param>
        /// <param name="variable_L">byte bajo de la variable</param>
        /// <param name="valor">word que devuelve por bucle</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerWordBucle(byte id, byte idOperador, byte variable_H, byte variable_L, out UInt16 valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[12];
            int reintentos = 0;
            UInt16 temp = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x09;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x04;
                trama[7] = (byte)EnumTB.Instruccion.LeerWord;
                trama[8] = variable_H;
                trama[9] = variable_L;
                trama[10] = LRC(trama, 10);
                trama[11] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerWord(idOperador, out temp)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
            }
            valor = temp;
            return recibido;    //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// escribe un byte por bucle de corriente a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="variable_H">byte alto de la variable</param>
        /// <param name="variable_L">byte bajo de la variable</param>
        /// <param name="valor">byte que queremos escribir en la variable</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool escribirByteBucle(byte id, byte idOperador, byte variable_H, byte variable_L, byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[13];
            int reintentos = 0;
            byte temp = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 10;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x05;
                trama[7] = (byte)EnumTB.Instruccion.EscribirByte;
                trama[8] = variable_H;
                trama[9] = variable_L;
                trama[10] = valor;
                trama[11] = LRC(trama, 11);
                trama[12] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable               
                reintentos++;
            }
            valor = temp;
            return recibido;    //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// escribe un word por bucle de corriente a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="variable_H">byte alto de la variable</param>
        /// <param name="variable_L">byte bajo de la variable</param>
        /// <param name="valor">word que queremos escribir en la variable</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool escribirWordBucle(byte id, byte idOperador, byte variable_H, byte variable_L, UInt16 valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[14];
            int reintentos = 0;
            byte temp = 0;
            byte[] bvalor = new byte[2];

            bvalor = BitConverter.GetBytes(valor);

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 11;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x06;
                trama[7] = (byte)EnumTB.Instruccion.EscribirWord;
                trama[8] = variable_H;
                trama[9] = variable_L;
                trama[10] = bvalor[1];
                trama[11] = bvalor[0];
                trama[12] = LRC(trama, 12);
                trama[13] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponse(id, variable_L));    //Espera a que responda y almacena el resultado en la variable               
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// leer un byte de algun parametro de la ADII a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="param">parametro de la ADII que queremos leer</param>
        /// <param name="valor">valor de la variable leida</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerByteADIIBucle(byte id, byte idOperador, EnumTB.ParametroADII param, out byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[11];
            int reintentos = 0;
            byte temp = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x08;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x03;
                trama[7] = (byte)EnumTB.Funcion.Lectura;
                trama[8] = Convert.ToByte(param); //Parametro a leer
                trama[9] = LRC(trama, 9);
                trama[10] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponseBucle(id, Convert.ToByte(param)));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerByteADIISinId(id, out temp)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }

        /// <summary>
        /// leer un byte de algun parametro especial del selector optima
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="posicion">parametro que queremos leer</param>
        /// <param name="valor">valor de la variable leida</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerByteOptimaBucle(byte id, byte idOperador, byte posicion, out byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[11];
            int reintentos = 0;
            byte temp = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x08;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x03;
                trama[7] = (byte)EnumTB.Funcion.TestSelectorOptima;
                trama[8] = posicion; //Parametro a leer
                trama[9] = LRC(trama, 9);
                trama[10] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponseBucle(id, posicion));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerByteADIISinId(id, out temp)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }

        public bool leerIROptima(byte id, out byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[11];
            byte temp = 0;

            trama[0] = STX;
            trama[1] = (byte)id;
            trama[2] = 0x08;
            trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
            trama[4] = STX;
            trama[5] = 0x87;
            trama[6] = 0x03;
            trama[7] = (byte)EnumTB.Funcion.TestSelectorOptima;
            trama[8] = 0; //Parametro a leer
            trama[9] = LRC(trama, 9);
            trama[10] = LRC(trama);

            ClearBytes(id);
            envioTrama(trama);
            recibido = (waitResponseBucle(id, 0));    //Espera a que responda y almacena el resultado en la variable
            if (!extraerByteADIISinId(id, out temp)) //Extrae el valor recibido en la trama
                recibido = false;

            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }

        /// <summary>
        /// leer un byte de algun parametro especial del selector optima
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="posicion">parametro que queremos leer</param>
        /// <param name="valor">valor de la variable leida</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerByteOptimaBucleModoTest(byte id, byte idOperador, byte posicion, out byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[11];
            int reintentos = 0;
            byte temp = 0;

            while (reintentos < 2)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x08;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x03;
                trama[7] = (byte)EnumTB.Funcion.TestSelectorOptimaModoTest;
                trama[8] = posicion; //Parametro a leer
                trama[9] = LRC(trama, 9);
                trama[10] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }

      
        /// <summary>
        /// leer un word de algun parametro de la ADII a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="param">parametro de la ADII que queremos leer</param>
        /// <param name="valor">valor de la variable leida</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool leerWordADIIBucle(byte id, byte idOperador, EnumTB.ParametroADII param, out UInt16 valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[11];
            int reintentos = 0;
            UInt16 temp = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x08;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x03;
                trama[7] = (byte)EnumTB.Funcion.Lectura;
                trama[8] = Convert.ToByte(param); //Parametro a leer
                trama[9] = LRC(trama, 9);
                trama[10] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponseBucle(id, Convert.ToByte(param)));    //Espera a que responda y almacena el resultado en la variable
                if (!extraerWordADIISinId(id, out temp)) //Extrae el valor recibido en la trama
                    recibido = false;
                reintentos++;
            }
            valor = temp;
            return recibido;                //Deuelve true si se ha recibido el valor
        }
        /// <summary>
        /// escribir un byte de algun parametro de la ADII a traves del circuito de test
        /// </summary>
        /// <param name="id">Id del circuito de test al que queremos comunicar</param>
        /// <param name="idOperador">Id del operador al que queremos comunicar</param>
        /// <param name="param">parametro de la ADII que queremos escribir</param>
        /// <param name="valor">valor que queremos introducir en la variable</param>
        /// <returns>Devuelve true si ha recibido la trama del circuito</returns>
        public bool escribirByteADIIBucle(byte id, byte idOperador, EnumTB.ParametroADII param, byte valor)
        {
            bool recibido = false;      //Indica si ha contestado el circuito
            byte[] trama = new byte[12];
            int reintentos = 0;

            while (!recibido && reintentos < REINTENTOS)
            {
                trama[0] = STX;
                trama[1] = (byte)id;
                trama[2] = 0x09;
                trama[3] = (byte)EnumTB.Instruccion.EnviarBucle;
                trama[4] = STX;
                trama[5] = idOperador;
                trama[6] = 0x04;
                trama[7] = (byte)EnumTB.Funcion.Escritura;
                trama[8] = Convert.ToByte(param); //Parametro a leer
                trama[9] = valor;
                trama[10] = LRC(trama, 10);
                trama[11] = LRC(trama);

                ClearBytes(id);
                envioTrama(trama);
                recibido = (waitResponseBucle(id, Convert.ToByte(param)));    //Espera a que responda y almacena el resultado en la variable                
                reintentos++;
            }
            return recibido;                //Deuelve true si se ha recibido el valor
        }

        #endregion


        #region Metodos privados
        /// <summary>
        /// Calcula el LRC para la trama y lo devuelve en un byte
        /// </summary>
        /// <param name="bytesToSend">Matriz de bytes de la trama</param>
        /// <returns>Byte checksun</returns>
        private byte LRC(byte[] bytesToSend)
        {
            int iLRC;
            //version particular de Manusa para la función LRC. Lo normal sería hacer sólo:
            //foreach (byte b in bytesToSend)
            //	byteLRC ^= b;
            iLRC = 0;
            for (int i = 0; i < bytesToSend.Length - 1; i++)
                iLRC += bytesToSend[i];

            iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
            iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
            return Convert.ToByte(iLRC);
        }
        /// <summary>
        /// Calcula el LRC para la traba hasta el byte indicado desde el byte 4
        /// Se utiliza para poder enviarle al operador una trama completa a traves
        /// del circuito del test
        /// </summary>
        /// <param name="bytesToSend">trama que se va a enviar</param>
        /// <param name="Maximo">byte hasta donde se tiene que calcular el LRC desde el byte 4</param>
        /// <returns>Devuelve el byte checksum</returns>
        private byte LRC(byte[] bytesToSend, int Maximo)
        {
            int iLRC;
            //version particular de Manusa para la función LRC. Lo normal sería hacer sólo:
            //foreach (byte b in bytesToSend)
            //	byteLRC ^= b;
            iLRC = 0;
            for (int i = 4; i < Maximo; i++)
                iLRC += bytesToSend[i];

            iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
            iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
            return Convert.ToByte(iLRC);
        }
        /// <summary>
        /// Limpia los bytes de la anterior recepcion por puerto serie
        /// </summary>
        private void ClearBytes(byte id)
        {
            for (int i = 0; i < 20; i++)
                busSerie.lRespuesta[id][i] = 0;
        }


        private void envioTrama(byte[] trama)
        {
            lock (this)
            {
                busSerie.addTrama(trama);
            }
        }
        private bool waitResponse(EnumTB.Id id, byte direccion)
        {
            return waitResponse((byte)id, direccion);
        }

        private bool waitResponse(byte id, byte direccion)
        {
            for (int i = 0; i < 300; i++)
            {
                if (busSerie.lRecibido[id])
                {
                    if (busSerie.lRespuesta[id][5] == direccion)
                        return true;
                    else
                        return false;
                }
                else if (busSerie.lFallido[id])
                    return false;
                else
                    Thread.Sleep(10);
            }
            Debug.Write("timeout comunicacion");
            return false;

        }

        private bool waitResponseBucle(byte id, byte direccion)
        {
            for (int i = 0; i < 300; i++)
            {
                if (busSerie.lRecibido[id])
                {
                    if (busSerie.lRespuesta[id][4] == direccion)
                        return true;
                    else
                        return false;
                }
                else if (busSerie.lFallido[id])
                    return false;
                else
                    Thread.Sleep(10);
            }
            Debug.Write("timeout comunicacion");
            return false;
        }

        /// <summary>
        /// Extrae el valor del byte que ha recibido en la trama
        /// </summary>
        /// <param name="id">Direccion del circuito en cuestion</param>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerByte(byte id, out byte resultado)
        {
            //if (TBReceive[0] == STX & TBReceive[1] == id & TBReceive[2] == 5 & tramaByteOk(id)) //Comprueba que la trama sea correcta
            if (busSerie.lRespuesta[id][0] == STX && busSerie.lRespuesta[id][2] == 5) //Comprueba que la trama sea correcta
            {
                resultado = busSerie.lRespuesta[id][6]; //Devuelve el resultado y 
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                resultado = 0; //Devuelve un 0 y un false
                return false;
            }
        }
        /// <summary>
        /// Extrae el valor del byte que ha recibido en la trama pero no comprueba que se trate
        /// de la direccion a la que la ha enviado (Para que funcione con Visio)
        /// </summary>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerByteADIISinId(byte id, out byte resultado)
        {
            //if (TBReceive[0] == STX & TBReceive[1] == id & TBReceive[2] == 5 & tramaByteOk(id)) //Comprueba que la trama sea correcta
            if (busSerie.lRespuesta[id][0] == STX && busSerie.lRespuesta[id][2] == 4) //Comprueba que la trama sea correcta
            {
                resultado = busSerie.lRespuesta[id][5]; //Devuelve el resultado y 
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
        /// <param name="id">Direccion del circuito en cuestion</param>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerWord(byte id, out UInt16 resultado)
        {
            if (busSerie.lRespuesta[id][0] == STX && busSerie.lRespuesta[id][1] == id && busSerie.lRespuesta[id][2] == 6) //Comprueba que la trama sea correcta
            {
                byte[] datoCovertir = new byte[2];//Crea una matriz de 2 bytes y les invierte el orden para sacar el valor entero                
                datoCovertir[0] = busSerie.lRespuesta[id][7];
                datoCovertir[1] = busSerie.lRespuesta[id][6];

                resultado = BitConverter.ToUInt16(datoCovertir, 0); //Convierte los dos byte en un valor entero
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                resultado = 0; //Devuelve un 0 y un false
                return false;
            }
        }

        //private bool extraerBuffer(byte id, out byte[] resultado)
        //{
        //    byte[] bufferRecibido = new byte[16];
        //    StopReceive = true; //Detiene la recepcion para que no se modifique la variable
        //    if (TBReceive[0] == STX && TBReceive[1] == id && TBReceive[2] == 21 && tramaBufferOk(id)) //Comprueba que la trama sea correcta
        //    {
        //        for (int i = 0; i < 16; i++)
        //        {
        //            bufferRecibido[i] = TBReceive[i + 7];
        //        }              

        //        resultado = bufferRecibido;
        //        StopReceive = false; //Vuelve a dejar libre la comunicación
        //        return true; //Devuelve true si la trama es correcta
        //    }
        //    else
        //    {
        //        for (int i = 0; i < 16; i++)
        //        {
        //            bufferRecibido[i] = 0;
        //        }
        //        resultado = bufferRecibido;
        //        StopReceive = false; //Vuelve a dejar libre la comunicación
        //        return false;
        //    }
        //}
        private bool extraerBuffer(byte id, byte direccion, out byte[] resultado)
        {
            byte[] bufferRecibido = new byte[16];
            if (busSerie.lRespuesta[id][0] == STX && busSerie.lRespuesta[id][1] == id && busSerie.lRespuesta[id][2] == 21 && busSerie.lRespuesta[id][5] == direccion) //Comprueba que la trama sea correcta
            {
                for (int i = 0; i < 16; i++)
                {
                    bufferRecibido[i] = busSerie.lRespuesta[id][i + 7];
                }

                resultado = bufferRecibido;
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    bufferRecibido[i] = 0;
                }
                resultado = bufferRecibido;
                return false;
            }
        }

        /// <summary>
        /// Extrae el valor del word que ha recibido en la trama  pero no comprueba que se trate
        /// de la direccion a la que la ha enviado (Para que funcione con Visio)
        /// </summary>
        /// <param name="resultado">Devuelve el resultado como argumento de salida</param>
        /// <returns>Devuelve true si la trama ha sido correcta</returns>
        private bool extraerWordADIISinId(byte id, out UInt16 resultado)
        {
            if (busSerie.lRespuesta[id][0] == STX & busSerie.lRespuesta[id][2] == 5) //Comprueba que la trama sea correcta
            {
                byte[] datoCovertir = new byte[2];//Crea una matriz de 2 bytes y les invierte el orden para sacar el valor entero                
                datoCovertir[0] = busSerie.lRespuesta[id][6];
                datoCovertir[1] = busSerie.lRespuesta[id][5];

                resultado = BitConverter.ToUInt16(datoCovertir, 0); //Convierte los dos byte en un valor entero
                return true; //Devuelve true si la trama es correcta
            }
            else
            {
                resultado = 0; //Devuelve un 0 y un false
                return false;
            }
        }




        #endregion          
    }
}
