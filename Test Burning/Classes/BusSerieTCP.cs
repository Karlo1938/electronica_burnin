using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Test
{
    public class BusSerieTCP
    {
        public const string FILE_INI = "TestBoard.ini";
        public const byte NUM_DISPOSITIVOS = 255; //Se añade una mas de lo necesario para evitar que tenga un numero diferente a la id
        public const byte STX = 0x02;         //Byte de inicio de trama
        public byte ultimoEnvio;

        public List<byte[]> lMensajes;
        public List<bool> lEntregado;
        public List<bool> lRecibido;
        public List<bool> lFallido;
        public List<byte[]> lRespuesta;

        private byte[] TBReceive = new byte[255];   //Variable que almacena lo recibido por canal serie
        private IniManager m_inimanager = new IniManager(FILE_INI); //Archivo de configuracion .ini

        private const int TIMELAPSE = 100;
        private int MAX_MILIS = 200;
        private Socket _socket;
        private Thread _thread;
        private IPAddress _ipAddress;
        IPEndPoint _remoteEP; 
        //String _sIP = "192.168.10.86";
        //int _iPort = 9002; 
        public String _sIP;
        public int _iPort;
        StateObject so = new StateObject();

        public BusSerieTCP(string IP = "192.168.11.111", int puerto = 8899)
        {
            _sIP = IP;
            _iPort = puerto;

            lMensajes = new List<byte[]>();
            lEntregado = new List<bool>();
            lRecibido = new List<bool>();
            lFallido = new List<bool>();
            lRespuesta = new List<byte[]>();

            for (int i = 0; i < NUM_DISPOSITIVOS; i++)
            {
                lMensajes.Add(new byte[50]);
                lEntregado.Add(false);
                lRecibido.Add(false);
                lFallido.Add(false);
                lRespuesta.Add(new byte[50]);
            }

            connectSocket();
            _thread = new Thread(run);
            _thread.Start();
        }

      
        private void run()
        {
            while (true)
            {
                try
                {
                    for (byte i = 0; i < NUM_DISPOSITIVOS; i++)
                    {
                        if (lEntregado[i])
                        {
                            for (byte ii = 0; ii < 2; ii++)
                            {
                                envioTrama(lMensajes[i]);
                              //  Thread.Sleep(200);
                                if (waitResponse())
                                {
                                    ii = 10;
                                    analizaTrama(i);
                                    lRecibido[i] = true;
                                }
                                else if (ii == 1)
                                {
                                    Debug.WriteLine("Mensaje de Operador " + i.ToString() + " sin respuesta!");
                                    lFallido[i] = true;
                                }
                            }
                            lMensajes[i] = null;
                            lEntregado[i] = false;

                        }
                    }
                }catch
                {

                }
            }
        }

        public void close()
        {
            disconnectSocket();
        }
        public void setMaxTimeResponse(int milis = 500) { MAX_MILIS = milis; }

        /// <summary>
        /// Metodo que añade la trama a la lista para que la envíe cuando le toque
        /// </summary>
        /// <param name="trama"></param>
        /// <returns></returns>
        public bool addTrama(byte[] trama)
        {
            lock (this)
            {
                byte id;
                try
                {
                    id = (byte)(trama[1]);
                    if (lEntregado[id])
                        return false;
                    //  while (lEntregado[0] || lEntregado[1]) ; //Evita choques
                }
                catch
                {
                    Debug.WriteLine("Error al introducir la trama");
                    return false;
                }

                lMensajes[id] = trama;
                lEntregado[id] = true;
                lFallido[id] = false;
                lRecibido[id] = false;
                return true;
            }
        }

        public void reConnect()
        {
            disconnectSocket();
            try
            {
                _socket = null;
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Thread.Sleep(500);
            connectSocket();
        }


     #region métodos privados

        private void analizaTrama(byte id)
        {
            switch (lMensajes[id][3])
            {
                case (byte)EnumTB.Instruccion.LeerByte:
                    if (tramaByteOk(id))
                        mueveTrama(id);
                    else
                        lFallido[id] = true;
                    break;
                case (byte)EnumTB.Instruccion.LeerWord:
                    if (tramaWordOk(id))
                        mueveTrama(id);
                    else
                        lFallido[id] = true;
                    break;
                case (byte)EnumTB.Instruccion.LeerBuffer:
                    if (tramaBufferOk(id))
                        mueveTrama(id);
                    else
                        lFallido[id] = true;
                    break;
                case (byte)EnumTB.Instruccion.EscribirByte:
                    if (tramaByteOk(id))
                        mueveTrama(id);
                    else
                        lFallido[id] = true;
                    break;
                case (byte)EnumTB.Instruccion.EscribirWord:
                    if (tramaWordOk(id))
                        mueveTrama(id);
                    else
                        lFallido[id] = true;
                    break;
                case (byte)EnumTB.Instruccion.EnviarBucle:
                    if (tramaOperadorOk(id))
                        mueveTrama(id);
                    else
                        lFallido[id] = true;
                    break;
            }
        }
        private void mueveTrama(byte id)
        {
            for (int i = 0; i < 50; i++)
            {
                lRespuesta[id][i] = TBReceive[i];
            }
        }
        private bool waitResponse()
        {
            bool response = false;
            so._rcv_txt = "";
            int i = 0;
            ClearMsg();
            Thread.Sleep(20);
            getTextoPorTCPClient();

            while (so._rcv_txt == "" && i < 10)
            {
                Thread.Sleep(20);
                i++;
            }
            Thread.Sleep(100);
           
            
            if (i < 10) {
                TBReceive = so._rcv_msg; response = true;
            }
            else
            {

            }
            
            return response;

            ////EndReceive = true;
            //int i = 0;
            //while (PendingResponse && i < 500) //Espera a que se apague el semaforo o a el timeout 250,  150 antes 100 ultimo valor 350
            //{
            //    Thread.Sleep(1);
            //    i++;
            //}
            //if (!PendingResponse) //Si se ha apagado el semaforo devuelve true y en caso contrario false
            //    return true;
            //else
            //{
            //    Debug.WriteLine("Timeout en waitResponse");
            //    PendingResponse = false;
            //    return false;
            //}
        }

        /// <summary>
        /// Envio de trama de byte, en caso de fallo abre el puerto y lo reintenta
        /// </summary>
        /// <param name="trama"></param>
        private void envioTrama(byte[] trama)
        {
            ClearBytes();
            try
            {
                so._msg = trama;
                sendTextPorTCPClient();              

            } //Envia la trama pasada como argumento
            catch { } //Si falla lo ignora porque ya se dara cuenta de que no ha contestado
            Debug.WriteLine("Enviando ....> " + BitConverter.ToString(trama));
        }

        /// <summary>
        /// Comprueba que la trama de recepcion de un byte sea correcta
        /// </summary>
        /// <param name="id">Id del circuito que nos interesa</param>
        /// <returns>Devuelve si la trama es correcta o no</returns>
        private bool tramaByteOk(byte id)
        {
            if (TBReceive[0] == STX && TBReceive[1] == id && TBReceive[2] == 5 && tramaLRC()) //Comprueba si la trama es correcta
                return true;
            else return false;
        }
        
        /// <summary>
        /// Comprueba que la trama de recepcion de un word sea correcta
        /// </summary>
        /// <param name="id">Id del circuito que nos interesa</param>
        /// <returns>Devuelve si la trama es correcta o no</returns>
        private bool tramaWordOk(byte id)
        {
            if (TBReceive[0] == STX && TBReceive[1] == id && TBReceive[2] == 6 && tramaLRC()) //Comprueba si la trama es correcta
                return true;
            else return false;
        }
        
        /// <summary>
        /// Comprueba que la trama de recepcion de un buffer sea correcta
        /// </summary>
        /// <param name="id">Id del circuito que nos interesa</param>
        /// <returns>Devuelve si la trama es correcta o no</returns>
        private bool tramaBufferOk(byte id)
        {
            if (TBReceive[0] == STX && TBReceive[1] == id && TBReceive[2] == 21 && tramaLRC()) //Comprueba si la trama es correcta
                return true;
            else return false;
        }
       
        /// <summary>
        /// Comprueba que la trama de recepcion de un buffer sea correcta
        /// </summary>
        /// <param name="id">Id del circuito que nos interesa</param>
        /// <returns>Devuelve si la trama es correcta o no</returns>
        private bool tramaOperadorOk(byte id)
        {
            if (TBReceive[0] == STX && TBReceive[4] == lMensajes[id][8] && tramaLRC()) //Comprueba si la trama es correcta
                return true;
            else return false;
        }
        
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
        /// Comprueba si la trama recibida es correcta comprobando su checksum
        /// </summary>
        /// <returns>Devuelve el resultado true o false</returns>
        private bool tramaLRC()
        {
            byte[] b = new byte[TBReceive[2] + 3]; //Se crea un byte del tamaño de la trama
            int ii = 0;

            if (TBReceive[0] == STX) //Si la trama comienza por el byte STX es correcta en principio
            {
                foreach (int i in b) //Almacena la trama en un byte de tamaño ajustado
                {
                    b[ii] = TBReceive[ii];
                    ii++;
                }
                if (LRC(b) == b[b.Length - 1])//Comprueba el byte del LCR y devuelve el resultado
                    return true;
                else return false;
            }
            return false;
        }
        
        /// <summary>
        /// Limpia los bytes de la anterior recepcion por puerto serie
        /// </summary>
        private void ClearBytes()
        {
            for (int i = 0; i < 20; i++)
                TBReceive[i] = 0;

        }

        /// <summary>
        /// Limpia los bytes de la anterior recepcion por puerto serie
        /// </summary>
        private void ClearMsg()
        {
            for (int i = 0; i < 200; i++)
                so._rcv_msg[i] = 0;

        }

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndConnect(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            so = (StateObject)ar.AsyncState;

            int bytesReceived;
            try
            {
                bytesReceived = _socket.EndReceive(ar);
                if (bytesReceived > 0)
                {
                    so._rcv_txt = Encoding.ASCII.GetString(so._rcv_msg, 0, bytesReceived);
                    Debug.WriteLine("Recibiendo <.... " + BitConverter.ToString(so._rcv_msg));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //connect();
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                _socket.EndSend(ar);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void sendTextPorTCPClient()
        {
            try
            {
                _socket.BeginSend(so._msg, 0, so._msg.Length, SocketFlags.None, new AsyncCallback(SendCallback), so);

            }
            catch (Exception ex)
            {
                Debug.WriteLine("de sendCommand-->" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        private void getTextoPorTCPClient()
        {
            try
            {
                _socket.BeginReceive(so._rcv_msg, 0, so._rcv_msg.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), so);


            }
            catch (Exception ex)
            {
                Debug.WriteLine("de sendCommand-->" + ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);

}
        }
        private void connectSocket()
        {
            _ipAddress = IPAddress.Parse(_sIP);
            _remoteEP = new IPEndPoint(_ipAddress, _iPort);

            if (_socket == null)
            {
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }
            try
            {
                _socket.BeginConnect(_ipAddress, _iPort, new AsyncCallback(ConnectCallback), null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void disconnectSocket()
        {
            try
            {
                _socket.Close();
                _socket = null;
                _thread.Abort();
            }
            catch (Exception ex) { }
        }



     #endregion

    }
}
