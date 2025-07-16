using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Test_Burning.Classes;
using Test;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Xaml;
using System.Windows.Forms.Integration;
using System.Timers;
using System.Diagnostics;

//using System.Runtime.Serialization.Json;
//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;

namespace Test_Burning
{
    public partial class Form1 : Form
    {
        public const string PATH = "C:\\Users\\Public\\Manusa\\Burnin\\";
        public const string FILE_INI = PATH + "Test.ini";
        public const string PATH_TRACER = PATH + "Resultados.xls"; //Path del archivo .xls de trazabilidad
        public const int SIZE_LAYOUT = 1570;
        public const int POSICIONES = 9;
        private const int MINUTOS_MIN = 600;//1200
        private const int CICLOS_MIN = 1500;//3000
        private const int NUM_MUESTRAS_BATERIA = 100;// 100;
        private const int DIVISOR_MUESTRAS = MINUTOS_MIN / NUM_MUESTRAS_BATERIA;
        private const int DIFERENCIA_MAX_MINUTOS = 5;
        private const int TABLA1 = 0xA0;
        private const int TABLA2 = 0xB0;
        private const int TABLA3 = 0xC0;
        private const int TABLA4 = 0xD0;
        private const int TIMER_CLEAN = 600000; //ms

        private int iBarcode = 0;
        private char cBarcode = '0';
        public static char cLayoutSeleccionado = '0';
        public static bool eventoFCarroMini = false;
        
        private delegate void UpdateLabelText(Label label, string texto);
        private delegate void UpdateLabelColor(Label label, Color color); //Delegado de actualizacion de color de etiquetas
        private delegate void UpdateVisibleLabel(Label label, bool visible);
        private delegate void UpdateBorderStyle3D(Label label, bool estile3d);
        private delegate void UpdateRichTextBox(string texto);
        private delegate void UpdateLayour();

        private Burning mBurn;
        private Burning mInicialBurn;
        //private TestBoard mTB;
        private GestorConexiones mGS;
        private Thread tLoopUpdate;
        private Trazabilidad mTracer;
        private System.Timers.Timer aTimer;
        private System.Timers.Timer clearTimer;
        private Semaforo LoopSemap;
        private AnalizadorCVM_B100 mAnalizador;
        private FormDisplay mDisplay;
        private FCarroMini mCarro1;
        private FCarroMini mCarro2;
        private FCarroMini mCarro3;
        private FCarroMini mCarro4;
        private FCarroMini mCarro5;
        private FCarroMini mCarro6;
        private FCarroMini mCarro7;
        private FCarroMini mCarro8;
        private FCarroMini mCarro9;
        //private Carros mCarros;
        //public FCarro fCarroElecTabla;
        private readonly object updateTableLock = new object();
        
        public Form1()
        {
            Process[] proces = Process.GetProcessesByName("TestBurning");
            try
            {
                if (proces[1] != null)
                    proces[1].Kill();
            }
            catch { }
            try
            {
                proces[0].PriorityClass = ProcessPriorityClass.BelowNormal;
            }
            catch { }
            //Inicializacion de objetos
            InitializeComponent();
            activarClearTimer();
            Carros.initCarros();
                    
            IniManager m_iniManager = new IniManager(FILE_INI);
            mDisplay = new FormDisplay();
            mBurn = new Burning();
            LoopSemap = new Semaforo();
            mInicialBurn = new Burning();

            inicializaFCarroMini();

            mGS = new GestorConexiones();
            // mTB = new TestBoard(m_iniManager.ReadValue("GEN", "TestBoard", "COM3"));
            mAnalizador = new AnalizadorCVM_B100(m_iniManager.ReadValue("GEN", "Analizador", "COM4"));
            mTracer = new Trazabilidad();

            mDisplay.Show();
            if(SystemInformation.MonitorCount == 2)
                mDisplay.Location = new Point(SystemInformation.PrimaryMonitorSize.Width, 0);
            //else
                //this.Location = new Point(-1200, 0);
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized; //Se maximiza la interfaz
            mDisplay.WindowState = System.Windows.Forms.FormWindowState.Maximized; //Se maximiza la interfaz
                        
            tOP.Text = m_iniManager.ReadValue("GEN", "Operario", "0");//Muestra el número de Operario que está entrado
            mensajeInformacion("Test Iniciado el día " + Date_Time.GetFechaFormat() + " , a las " + Date_Time.GetHoraFormat());
            LoopSemap= Semaforo.Verde;
            //mTB.serialPort1Open();
            mBurn.recuperarTest();
            mBurn.recuperarCarros();
            seleccionarNuevoLayout();
            mDisplay.actualizaLayout(mBurn.dDistribucionCarros);
            mGS.anadirCarros(mBurn);


            tLoopUpdate = new Thread(loopUpdate);
            tLoopUpdate.Start();
            aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 2500;
            aTimer.Enabled = true;
            tBarcode.Focus();
        }

        private void loopUpdate()
        {
            Thread.Sleep(2000); //Tiempo de inicialización
           
            if (mBurn.listLote == null) //Si es igual a null es porque no hay maquinas recuperadas
                mBurn = new Burning();
            else //De las que acaban de arrancar busca las que son OK o NOK para actualizar la interfaz ya que despues no actualizarán
            {
                Thread.Sleep(300); //Tengo que dar tiempo a que se cree la interfaz para poder utilizar los delegados
                foreach (Pieza element in mBurn.listLote)
                {
                    if (element.eResultado != Resultado.Unknown)
                    {
                        actualizaPoscicion(element);
                    }
                    mDisplay.actualizaPieza(element);
                }
            }
            eventoFCarroMini = true; //Dispara el evento para actualizar la tabla de minicarros
            while (true)
            {
                byte[] recibido = new byte[16];
                byte[] wordTemp = new byte[2];

                while(mInicialBurn.listLote != null && mInicialBurn.listLote.Count != 0 && LoopSemap == Semaforo.Rojo) //Si hay que añadir alguna maquina
                {
                    if (!añadirPieza(mInicialBurn.listLote[0]))
                        muestraIconoWarning(mInicialBurn.listLote[0]);
                    mInicialBurn.listLote.RemoveAt(0);
                    if (mInicialBurn.listLote.Count == 0)
                    {
                        LoopSemap = Semaforo.Verde;
                    }
                }                
                if (mBurn.listLote != null)
                {
                    if (LoopSemap == Semaforo.Verde) //Esta contolado por este semaforo para que detenga la comunicacion cuando sea necesario
                    {
                        try
                        {
                            foreach (Pieza element in mBurn.listLote)
                            {
                                if (LoopSemap == Semaforo.Verde) //Esta contolado por este semaforo para que detenga la comunicacion cuando sea necesario
                                {
                                    if (element.eResultado == Resultado.Unknown || element.eEstado < EstadosTestBoard.Prestop) //Solo actualiza en el caso de no tener un resultado.
                                    {
                                        if (element.eEstadoTest == EstadosTest.Cargando_Param)//Comprueba si le faltan los parametros                                    
                                            introducirParametros(element);

                                        if (!leerPosicion(element, out recibido))
                                        {
                                            element.Reintentos++;
                                            Thread.Sleep(300);
                                        }
                                        else if (lrcOk(recibido))
                                        {
                                            element.Reintentos = 0;
                                            wordTemp[0] = recibido[1]; wordTemp[1] = recibido[0];
                                            element.Version = BitConverter.ToUInt16(wordTemp, 0);
                                            wordTemp[0] = recibido[3]; wordTemp[1] = recibido[2];
                                            element.Ciclos = BitConverter.ToUInt16(wordTemp, 0);
                                            wordTemp[0] = recibido[5]; wordTemp[1] = recibido[4];
                                            element.Minutos = BitConverter.ToUInt16(wordTemp, 0);
                                            element.eEstado = (EstadosTestBoard)recibido[6];
                                            element.eAlarma = (Alarma)recibido[7];
                                            element.eResultado = (Resultado)recibido[8];
                                            element.eMaxReintentos = recibido[11];
                                            element.eNivelBateria[(Byte)(element.Minutos / DIVISOR_MUESTRAS)] = recibido[12];
                                            ActualizaEstado(element);
                                            actualizaPoscicion(element);
                                        }
                                    }
                                    else if (element.eEstadoTest == EstadosTest.VolcadoMemoria)
                                        leerMemoria(element);
                                   // Thread.Sleep(10000);//Oxigeno CPU
                                }
                                labelWarning(element);
                            }
                        }catch (System.InvalidOperationException){}

                        if (LoopSemap == Semaforo.Amarillo) //Si se ha mandado la parada cuando sale del foreach lo indica poniendolo a rojo
                        {
                            LoopSemap = Semaforo.Rojo;
                        }
                    }
                }
                if (LoopSemap == Semaforo.Amarillo) //Si se ha mandado la parada cuando sale del foreach lo indica poniendolo a rojo
                {
                    LoopSemap = Semaforo.Rojo;
                }                
                consultaAnalizador();
                mBurn.grabarEstado();
            }
        }

        private bool leerPosicion (Pieza element, out Byte[] respuesta)
        {
            byte[] recibido = new byte[16];
            respuesta = null;

            TestBoardTCP mTB = mGS.retTestBoard(element.Posicion);
            if (!mTB.leerBuffer(Convert.ToByte(element.Posicion), 0x80, out recibido))
                return false;
            else
            {
                respuesta = recibido;
                return true;
            }
        }
        
        private void consultaAnalizador()
        {
            try
            {
                if (mBurn.listLote.Count > 0)
                {
                    if (mAnalizador.ErrorConexion)
                    {
                        if (mAnalizador.AlarmaCorteSuministro)
                            gestionaCorteSuministro();
                        else
                            rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeError), "No hay comunicación con el analizador de redes");
                    }
                    else
                    {
                        if (mAnalizador.AlarmaCorteSuministro)
                        {
                            rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeError), "Advertencia!!, Corte de suministro detectado...");
                            for (int i = 0; i < 5000; i++) ; //Enreda un poco para que el delegado tenga tiempo de mostrar el mensaje                        
                            gestionaCorteSuministro();
                        }
                    }
                }
                if (mAnalizador.CaidaProtectorSobretensiones)
                {
                    lWarning.BeginInvoke(new UpdateVisibleLabel(visibleLabel), lWarning, true);
                }
                else { lWarning.BeginInvoke(new UpdateVisibleLabel(visibleLabel), lWarning, false); }
            }
            catch (Exception)
            {
                
              
            }
           

        }

        private void gestionaCorteSuministro()
        {
            if (mAnalizador.AlarmaCorteSuministro)
            {
                foreach (Pieza element in mBurn.listLote)
                {
                    if(element.eResultado == Resultado.Unknown)
                        mTracer.añadeLinea(element.Path, "Corte de suminitro detectado el día " + Date_Time.GetFechaFormat() + " a las " + Date_Time.GetHoraFormat());
                }
                do
                {
                    Thread.Sleep(1000);
                    mAnalizador.consutaParametros();                    
                }
                while (mAnalizador.AlarmaCorteSuministro);
                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeInformacionSinSalto), "Restableciendo suministro");
                for (int i = 0; i < 3; i++)
                {
                    rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeInformacionSinSalto), "..." + Convert.ToString(3 - i));
                    Thread.Sleep(1000);
                }

                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeInformacion), "\nRecuperando el test..");
                
                mBurn.recuperarTest();
                mBurn.recuperarCarros();

                foreach (Pieza element in mBurn.listLote)
                {
                    if (element.eResultado == Resultado.Unknown) //Solo si la pieza no tiene ya un resultado.
                    {
                        mTracer.añadeLinea(element.Path, "La alimentación eléctrica se ha restrablecido el día " + Date_Time.GetFechaFormat() + " a las " + Date_Time.GetHoraFormat());
                        rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeInformacion), "Restableciendo posición: " + element.Posicion + "    Nº Serie: " + element.Serial);
                        restablecerPieza(element);
                    }
                    else if (element.eResultado == Resultado.OK)
                    {
                        rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeInformacion), "Realimentando posición: " + element.Posicion + "    Nº Serie: " + element.Serial);
                        realimentarPieza(element);
                    }
                }
                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeInformacion), "Test recuperado");
            }
        }

        private void inicializaFCarroMini()
        {

            try
            {
                if (Carros.ListaCarros[1].Tipo == TipoCarro.Elec)
                    mCarro1 = new FCarroElecMini();
                else if (Carros.ListaCarros[1].Tipo == TipoCarro.Plus)
                    mCarro1 = new FCarroElecPlusMini();
                else
                    mCarro1 = new FCarroMini();

                if (Carros.ListaCarros[2].Tipo == TipoCarro.Elec)
                    mCarro2 = new FCarroElecMini();
                else if (Carros.ListaCarros[2].Tipo == TipoCarro.Plus)
                    mCarro2 = new FCarroElecPlusMini();
                else
                    mCarro2 = new FCarroMini();

                if (Carros.ListaCarros[3].Tipo == TipoCarro.Elec)
                    mCarro3 = new FCarroElecMini();
                else if (Carros.ListaCarros[3].Tipo == TipoCarro.Plus)
                    mCarro3 = new FCarroElecPlusMini();
                else
                    mCarro3 = new FCarroMini();

                if (Carros.ListaCarros[4].Tipo == TipoCarro.Elec)
                    mCarro4 = new FCarroElecMini();
                else if (Carros.ListaCarros[4].Tipo == TipoCarro.Plus)
                    mCarro4 = new FCarroElecPlusMini();
                else
                    mCarro4 = new FCarroMini();

                if (Carros.ListaCarros[5].Tipo == TipoCarro.Elec)
                    mCarro5 = new FCarroElecMini();
                else if (Carros.ListaCarros[5].Tipo == TipoCarro.Plus)
                    mCarro5 = new FCarroElecPlusMini();
                else
                    mCarro5 = new FCarroMini();

                if (Carros.ListaCarros[6].Tipo == TipoCarro.Elec)
                    mCarro6 = new FCarroElecMini();
                else if (Carros.ListaCarros[6].Tipo == TipoCarro.Plus)
                    mCarro6 = new FCarroElecPlusMini();
                else
                    mCarro6 = new FCarroMini();

                if (Carros.ListaCarros[7].Tipo == TipoCarro.Elec)
                    mCarro7 = new FCarroElecMini();
                else if (Carros.ListaCarros[7].Tipo == TipoCarro.Plus)
                    mCarro7 = new FCarroElecPlusMini();
                else
                    mCarro7 = new FCarroMini();

                if (Carros.ListaCarros[8].Tipo == TipoCarro.Elec)
                    mCarro8 = new FCarroElecMini();
                else if (Carros.ListaCarros[8].Tipo == TipoCarro.Plus)
                    mCarro8 = new FCarroElecPlusMini();
                else
                    mCarro8 = new FCarroMini();

                if (Carros.ListaCarros[9].Tipo == TipoCarro.Elec)
                    mCarro9 = new FCarroElecMini();
                else if (Carros.ListaCarros[9].Tipo == TipoCarro.Plus)
                    mCarro9 = new FCarroElecPlusMini();
                else
                    mCarro9 = new FCarroMini();
            }

            catch (FormatException ex)
            {
                Console.WriteLine("Error de formato: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error general: " + ex.Message);
            }

        }

        private void refrescaTablaConLayoutSeleccionado()
        {
            limpiaTabla();

            for (int i = 0; i < mBurn.dDistribucionCarros.Count; i++)
            {
                if (mBurn.dDistribucionCarros.ElementAt(i).Key == cLayoutSeleccionado)
                {
                    carroToObject(mBurn.dDistribucionCarros.ElementAt(i).Value).BackColor = Color.SkyBlue;
                    carroToObject(mBurn.dDistribucionCarros.ElementAt(i).Value).BorderStyle = BorderStyle.Fixed3D;

                    string sNumCarro = Convert.ToString(mBurn.dDistribucionCarros.ElementAt(i).Value);

                    fCarroElecTabla.lNumCarro.Text = sNumCarro;
                    fCarroElecTabla.lPosCarro.Text = Convert.ToString(mBurn.dDistribucionCarros.ElementAt(i).Key);
                    fCarroElecPlusTabla.lNumCarro.Text = sNumCarro;
                    fCarroElecPlusTabla.lPosCarro.Text = Convert.ToString(mBurn.dDistribucionCarros.ElementAt(i).Key);
                    fCarroTabla.lNumCarro.Text = sNumCarro;
                    fCarroTabla.lPosCarro.Text = Convert.ToString(mBurn.dDistribucionCarros.ElementAt(i).Key);

                    if (Carros.ListaCarros[Convert.ToByte(sNumCarro)].Tipo == TipoCarro.Elec)
                    {
                        fCarroElecTabla.Visible = true;
                        fCarroTabla.Visible = false;
                        fCarroElecPlusTabla.Visible = false;
                    }
                    else if (Carros.ListaCarros[Convert.ToByte(sNumCarro)].Tipo == TipoCarro.Moto)
                    {
                        fCarroElecTabla.Visible = false;
                        fCarroTabla.Visible = true;
                        fCarroElecPlusTabla.Visible = false;
                    }
                    else
                    {
                        fCarroElecTabla.Visible = false;
                        fCarroTabla.Visible = false;
                        fCarroElecPlusTabla.Visible = true;
                    }

                }
                else
                {
                    if (mBurn.dDistribucionCarros.ElementAt(i).Value != 0)
                    {
                        carroToObject(mBurn.dDistribucionCarros.ElementAt(i).Value).BackColor = SystemColors.ActiveBorder;
                        carroToObject(mBurn.dDistribucionCarros.ElementAt(i).Value).BorderStyle = BorderStyle.None;
                    }
                }                
            }
            actualizaTodasPosicionesTabla();
        }

        private void actualizaTodasPosicionesTabla()
        {           
            byte bNumCarro;
            try
            {
                bNumCarro = Convert.ToByte(fCarroTabla.lNumCarro.Text);
                if(fCarroTabla.Visible == true)
                    bNumCarro = Convert.ToByte(fCarroTabla.lNumCarro.Text);
                else if(fCarroElecTabla.Visible == true)
                    bNumCarro = Convert.ToByte(fCarroElecTabla.lNumCarro.Text);
                else
                    bNumCarro = Convert.ToByte(fCarroElecPlusTabla.lNumCarro.Text);

            }
            catch { return; }
            
            foreach (Pieza element in mBurn.listLote)
            {
                actualizaPoscicion(element);
            }
        }

        private void limpiaTabla()
        {
            const byte MAX_PIEZAS_CARRO = 20;
            List<Label> listLab = new List<Label>();

            for (int i = 1; i <= MAX_PIEZAS_CARRO; i++)
            {
                listLab = etiquetasPosicionTabla((byte)i);
                for (int ii = 0; ii < listLab.Count; ii++)
                {
                    listLab.ElementAt(ii).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(ii), "-");
                }
            }            

            for (int i = 1; i <= MAX_PIEZAS_CARRO; i++)
            {
                Label LPos = labelPosicion((byte)i, fCarroTabla);
                LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, false); //LPos.Visible = true;
                LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, false); //LPos.BorderStyle = BorderStyle.Fixed3D;
                LPos.BeginInvoke(new UpdateLabelColor(labelBackColor), LPos, Color.Transparent);
                Label LTiempo = labelTiempo((byte)i, fCarroTabla);
                LTiempo.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LTiempo, false); //LTiempo.Visible = true;
                LTiempo.BeginInvoke(new UpdateLabelColor(labelBackColor), LTiempo, Color.Transparent); //LTiempo.BackColor = color;
                LTiempo.BeginInvoke(new UpdateLabelText(actualizaLabel), LTiempo, ""); // LTiempo.Text = minutos + "'";
                Label LCiclos = labelCiclos((byte)i, fCarroTabla);
                LCiclos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LCiclos, false); //LCiclos.Visible = true;
                LCiclos.BeginInvoke(new UpdateLabelColor(labelBackColor), LCiclos, Color.Transparent); //LCiclos.BackColor = color;
                LCiclos.BeginInvoke(new UpdateLabelText(actualizaLabel), LCiclos, ""); //LCiclos.Text = ciclos;
            }

            if (cLayoutSeleccionado <= '0')
            {
                fCarroTabla.lNumCarro.BeginInvoke(new UpdateLabelText(actualizaLabel), fCarroTabla.lNumCarro, "-");
                fCarroTabla.lPosCarro.BeginInvoke(new UpdateLabelText(actualizaLabel), fCarroTabla.lPosCarro, "-");
            }

            limpiaCarroElecTabla();
            limpiaCarroElecPlusTabla();
        }

        private void limpiaCarroElecTabla()
        {
            const byte MAX_PIEZAS_CARRO = 20;
            List<Label> listLab = new List<Label>();

            for (int i = 1; i <= MAX_PIEZAS_CARRO; i++)
            {
                listLab = etiquetasPosicionTabla((byte)i);
                for (int ii = 0; ii < listLab.Count; ii++)
                {
                    listLab.ElementAt(ii).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(ii), "-");
                }
            }

            for (int i = 1; i <= MAX_PIEZAS_CARRO; i++)
            {
                Label LPos = labelPosicion((byte)i, fCarroElecTabla);
                LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, false); //LPos.Visible = true;
                LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, false); //LPos.BorderStyle = BorderStyle.Fixed3D;
                LPos.BeginInvoke(new UpdateLabelColor(labelBackColor), LPos, Color.Transparent);
                Label LTiempo = labelTiempo((byte)i, fCarroElecTabla);
                LTiempo.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LTiempo, false); //LTiempo.Visible = true;
                LTiempo.BeginInvoke(new UpdateLabelColor(labelBackColor), LTiempo, Color.Transparent); //LTiempo.BackColor = color;
                LTiempo.BeginInvoke(new UpdateLabelText(actualizaLabel), LTiempo, ""); // LTiempo.Text = minutos + "'";
                Label LCiclos = labelCiclos((byte)i, fCarroElecTabla);
                LCiclos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LCiclos, false); //LCiclos.Visible = true;
                LCiclos.BeginInvoke(new UpdateLabelColor(labelBackColor), LCiclos, Color.Transparent); //LCiclos.BackColor = color;
                LCiclos.BeginInvoke(new UpdateLabelText(actualizaLabel), LCiclos, ""); //LCiclos.Text = ciclos;
            }

            if (cLayoutSeleccionado <= '0')
            {
                fCarroElecTabla.lNumCarro.BeginInvoke(new UpdateLabelText(actualizaLabel), fCarroElecTabla.lNumCarro, "-");
                fCarroElecTabla.lPosCarro.BeginInvoke(new UpdateLabelText(actualizaLabel), fCarroElecTabla.lPosCarro, "-");
            }
        }

        private void limpiaCarroElecPlusTabla()
        {
            const byte MAX_PIEZAS_CARRO = 20;
            List<Label> listLab = new List<Label>();

            for (int i = 1; i <= MAX_PIEZAS_CARRO; i++)
            {
                listLab = etiquetasPosicionTabla((byte)i);
                for (int ii = 0; ii < listLab.Count; ii++)
                {
                    listLab.ElementAt(ii).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(ii), "-");
                }
            }

            for (int i = 1; i <= MAX_PIEZAS_CARRO; i++)
            {
                Label LPos = labelPosicion((byte)i, fCarroElecPlusTabla);
                LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, false); //LPos.Visible = true;
                LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, false); //LPos.BorderStyle = BorderStyle.Fixed3D;
                LPos.BeginInvoke(new UpdateLabelColor(labelBackColor), LPos, Color.Transparent);
                Label LTiempo = labelTiempo((byte)i, fCarroElecPlusTabla);
                LTiempo.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LTiempo, false); //LTiempo.Visible = true;
                LTiempo.BeginInvoke(new UpdateLabelColor(labelBackColor), LTiempo, Color.Transparent); //LTiempo.BackColor = color;
                LTiempo.BeginInvoke(new UpdateLabelText(actualizaLabel), LTiempo, ""); // LTiempo.Text = minutos + "'";
                Label LCiclos = labelCiclos((byte)i, fCarroElecPlusTabla);
                LCiclos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LCiclos, false); //LCiclos.Visible = true;
                LCiclos.BeginInvoke(new UpdateLabelColor(labelBackColor), LCiclos, Color.Transparent); //LCiclos.BackColor = color;
                LCiclos.BeginInvoke(new UpdateLabelText(actualizaLabel), LCiclos, ""); //LCiclos.Text = ciclos;
            }

            if (cLayoutSeleccionado <= '0')
            {
                fCarroElecPlusTabla.lNumCarro.BeginInvoke(new UpdateLabelText(actualizaLabel), fCarroElecPlusTabla.lNumCarro, "-");
                fCarroElecPlusTabla.lPosCarro.BeginInvoke(new UpdateLabelText(actualizaLabel), fCarroElecPlusTabla.lPosCarro, "-");
            }
        }

        private void actualizaPosicionTabla(Pieza element)
        {
            Color color = new Color();
            color = devuelveColorPieza(element);
            Label LPos, LTiempo, LCiclos;
            

            //Actualización del carro
            byte carro = devuelveNumCarro(element.Posicion);
            if(Carros.ListaCarros[carro].Tipo == TipoCarro.Elec)
            {
                LPos = labelPosicion(element.Posicion, fCarroElecTabla);
                LTiempo = labelTiempo(element.Posicion, fCarroElecTabla);
                LCiclos = labelCiclos(element.Posicion, fCarroElecTabla);
            }
            else if (Carros.ListaCarros[carro].Tipo == TipoCarro.Plus)
            {
                LPos = labelPosicion(element.Posicion, fCarroElecPlusTabla);
                LTiempo = labelTiempo(element.Posicion, fCarroElecPlusTabla);
                LCiclos = labelCiclos(element.Posicion, fCarroElecPlusTabla);
            }
            else
            {
                LPos = labelPosicion(element.Posicion, fCarroTabla);
                LTiempo = labelTiempo(element.Posicion, fCarroTabla);
                LCiclos = labelCiclos(element.Posicion, fCarroTabla);
            }

            //if (LPos.IsDisposed)
            //{
                LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, true); //LPos.Visible = true;
                LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, true); //LPos.BorderStyle = BorderStyle.Fixed3D;
                LPos.BeginInvoke(new UpdateLabelColor(labelBackColor), LPos, color);

                LTiempo.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LTiempo, true); //LTiempo.Visible = true;
                LTiempo.BeginInvoke(new UpdateLabelColor(labelBackColor), LTiempo, color); //LTiempo.BackColor = color;
                LTiempo.BeginInvoke(new UpdateLabelText(actualizaLabel), LTiempo, Convert.ToString(element.Minutos) + "'"); // LTiempo.Text = minutos + "'";

                LCiclos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LCiclos, true); //LCiclos.Visible = true;
                LCiclos.BeginInvoke(new UpdateLabelColor(labelBackColor), LCiclos, color); //LCiclos.BackColor = color;
                LCiclos.BeginInvoke(new UpdateLabelText(actualizaLabel), LCiclos, Convert.ToString(element.Ciclos)); //LCiclos.Text = ciclos;
            //}

            int iMinutosRestantes = MINUTOS_MIN - Convert.ToInt32(element.Minutos);
            if (iMinutosRestantes < 0)
                iMinutosRestantes = 0;

            //Actualización de la tabla
            List<Label> listLab = new List<Label>();
            listLab = etiquetasPosicionTabla(element.Posicion);
            listLab.ElementAt(0).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(0), Convert.ToString(element.Posicion));
            listLab.ElementAt(1).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(1), Convert.ToString(iMinutosRestantes.ToString() + "'"));
            listLab.ElementAt(2).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(2), Convert.ToString(element.eEstadoTest));
            mensajeInfo(element, listLab.ElementAt(3));
            listLab.ElementAt(4).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(4), Convert.ToString(element.Serial));
        }

        // Traduce el enum de alarma a mensajes.
        private void mensajeInfo(Pieza element, Label lab)
        {
            if(element.eResultado == Resultado.OK)
                lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab,"Pieza finalizada con éxito");
            else if (element.eResultado == Resultado.NOK)
            {
                if (element.eAlarma < (Alarma)100)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "La electrónica detectó una alarma interna, Error " + element.eAlarma);
                else if (element.eAlarma == Alarma.Comunicacion)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "La pieza ha superado el maximo de reintentos de comunicación");
                else if (element.eAlarma == Alarma.Bloqueo)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "La pieza no ha completado la maniobra, posible bloqueo");
                else if (element.eAlarma == Alarma.Antipanico)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "La pieza no ha completado la apertura de antipánico");
                else if (element.eAlarma == Alarma.Parametros)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "La pieza no ha grabado los parametros introducidos");
                else if (element.eAlarma == Alarma.Tasa_Ciclos_Minuto)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "La pieza ha hecho menos ciclos de los esperados");
                else if (element.eAlarma == Alarma.TBAM)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Valor incorrecto en parametro TBAM");
                else if (element.eAlarma == Alarma.Desconocido)
                    lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Error desconocido, contacte con el responsable");
            }
            else if (element.eResultado == Resultado.Unknown)
            {
                switch (element.eEstadoTest)
                {
                    case EstadosTest.Vacio:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "-");
                        break;
                    case EstadosTest.Iniciando:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Inicializando pieza…");
                        break;
                    case EstadosTest.Cargando_Param:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Introduciendo parámetros de test");
                        break;
                    case EstadosTest.Testeando:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Testeando…");
                        break;
                    case EstadosTest.Espera:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Esperando ATENCION!");
                        break;
                    case EstadosTest.Finalizando:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Finalizando con test antipánico…");
                        break;
                    case EstadosTest.VolcadoMemoria:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Cargando parametros y comprobando…");
                        break;
                    case EstadosTest.OK:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Pieza finalizada con éxito."); //Este mensaje es temporal hasta que el test gestiona el resultado
                        break;
                    case EstadosTest.NOK:
                        lab.BeginInvoke(new UpdateLabelText(actualizaLabel), lab, "Pieza finalizada NO conforme."); //Este mensaje es temporal hasta que el test gestiona el resultado
                        break;
                }
            }
        }

        

        private void seleccionarNuevoLayout()
        {
            if (cLayoutSeleccionado <= '0' || mBurn.dDistribucionCarros.ElementAt(cLayoutSeleccionado - 65).Value == 0)
            {
                for (int i = 0; i < mBurn.dDistribucionCarros.Count; i++)
                {
                    if (mBurn.dDistribucionCarros.ElementAt(i).Value > 0 )
                    {
                        cLayoutSeleccionado = mBurn.dDistribucionCarros.ElementAt(i).Key;
                        eventoFCarroMini = true; //Para que actualice el carro
                        actualizaLayout();
                        return;
                    }                   
                }
                cLayoutSeleccionado = '0';
                eventoFCarroMini = true; //Para que actualice el carro
                //actualizaLayout();
            }
            actualizaLayout();
        }             
 
        private void ActualizaEstado(Pieza PiezaLote)
        {
            if (mAnalizador.AlarmaCorteSuministro)
                return;
            if (PiezaLote.eEstadoTest == EstadosTest.Iniciando)
            {
                if (PiezaLote.eEstado == EstadosTestBoard.Init)
                    PiezaLote.eEstadoTest = EstadosTest.Cargando_Param;
                else if(PiezaLote.eEstado == EstadosTestBoard.Burn)
                    PiezaLote.eEstadoTest = EstadosTest.Cargando_Param;
                else if (PiezaLote.eEstado == EstadosTestBoard.End)
                    PiezaLote.eEstadoTest = EstadosTest.Cargando_Param;
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Stop) //NEW
                {
                    if (Date_Time.segTranscurridos(PiezaLote) > 160.0)
                    testNOK(PiezaLote);
                }
            }
            else if (PiezaLote.eEstadoTest == EstadosTest.Cargando_Param)
            {
                if (PiezaLote.eEstado == EstadosTestBoard.Prestop)
                {
                    if (Date_Time.segTranscurridos(PiezaLote) > 150.0)
                        testNOK(PiezaLote);
                }
                else if (PiezaLote.eEstado == EstadosTestBoard.Stop)
                {
                    if (Date_Time.segTranscurridos(PiezaLote) > 150.0)
                        testNOK(PiezaLote);
                }
                else if (PiezaLote.eEstado == EstadosTestBoard.Init)
                {
                    if (Date_Time.segTranscurridos(PiezaLote) > 210.0)
                        testNOK(PiezaLote);
                }
            }
            else if (PiezaLote.eEstadoTest == EstadosTest.Testeando)
            {
                //if (PiezaLote.eEstado == EstadosTestBoard.End && PiezaLote.eFamilia == Familia.Bravo)
                //    PiezaLote.eEstadoTest = EstadosTest.Espera;
                if(PiezaLote.eEstado == EstadosTestBoard.End)
                    PiezaLote.eEstadoTest = EstadosTest.Testeando;
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop && PiezaLote.eResultado == Resultado.OK)
                    PiezaLote.eEstadoTest = EstadosTest.VolcadoMemoria;
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop && PiezaLote.eResultado == Resultado.NOK)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Stop)
                    testNOK(PiezaLote); //Aqui se tendra que añadir codigo cuando se produzca un apagon
            }
            else if (PiezaLote.eEstadoTest == EstadosTest.Espera)
            {
                if (PiezaLote.eEstado == EstadosTestBoard.Stop)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Burn)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop && PiezaLote.eResultado == Resultado.OK)
                    PiezaLote.eEstadoTest = EstadosTest.VolcadoMemoria;
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop && PiezaLote.eResultado == Resultado.NOK)
                    testNOK(PiezaLote);

            }
            else if ((PiezaLote.eEstadoTest == EstadosTest.Finalizando))
            {
                if (PiezaLote.eEstado == EstadosTestBoard.Stop)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop && PiezaLote.eResultado == Resultado.OK)
                    PiezaLote.eEstadoTest = EstadosTest.VolcadoMemoria;
                else if (PiezaLote.eEstado == EstadosTestBoard.Prestop && PiezaLote.eResultado == Resultado.NOK)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Init)
                    testNOK(PiezaLote);
                else if (PiezaLote.eEstado == EstadosTestBoard.Burn)
                    testNOK(PiezaLote);
            }
        }

        private void leerMemoria(Pieza PiezaLote)
        {
            byte[] bTabla1 = new byte[16];
            byte[] bTabla2 = new byte[16];
            byte[] bTabla3 = new byte[16];
            byte[] bTabla4 = new byte[16];
            byte[] wordTemp = new byte[2];
            byte[] longTemp = new byte[4];
            TestBoardTCP mTB = mGS.retTestBoard(PiezaLote.Posicion);

            //Tabla1
            if (!mTB.leerBuffer(PiezaLote.Posicion, TABLA1, out bTabla1))
            {
                PiezaLote.Reintentos++;
                return;
            }
            if (!mTB.leerBuffer(PiezaLote.Posicion, TABLA2, out bTabla2))
            {
                PiezaLote.Reintentos++;
                return;
            }
            if (!mTB.leerBuffer(PiezaLote.Posicion, TABLA3, out bTabla3))
            {
                PiezaLote.Reintentos++;
                return;
            }
            if (!mTB.leerBuffer(PiezaLote.Posicion, TABLA4, out bTabla4))
            {
                PiezaLote.Reintentos++;
                return;
            }
            if (!checksumBuffer(bTabla1)) //Falta programar en testboard el refresco de las variables
                return;
            if (!checksumBuffer(bTabla2))
                return;
            if (!checksumBuffer(bTabla3))
                return;
            if (!checksumBuffer(bTabla4))
                return;

            wordTemp[0] = bTabla2[7]; wordTemp[1] = bTabla2[6]; //Hay que darle la vuelta a los bytes
            UInt16 version =  BitConverter.ToUInt16(wordTemp, 0);
            if (version != PiezaLote.Version)                
                return;
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(PiezaLote.Path, true))            
            {
                file.WriteLine("Volcado de memoria ADII:");
                file.WriteLine("0: " + bTabla1[0]);
                file.WriteLine("1: " + bTabla1[1]);
                file.WriteLine("2: " + bTabla1[2]);
                file.WriteLine("3: " + bTabla1[3]);
                file.WriteLine("4: " + bTabla1[4]);
                file.WriteLine("5: " + bTabla1[5]);
                file.WriteLine("6: " + bTabla1[6]);
                file.WriteLine("7: " + bTabla1[7]);
                file.WriteLine("8: " + bTabla1[8]);
                file.WriteLine("9: " + bTabla1[9]);
                file.WriteLine("10: " + bTabla1[10]);
                file.WriteLine("11: " + bTabla1[11]);
                file.WriteLine("12: " + bTabla1[12]);
                file.WriteLine("13: " + bTabla1[13]);
                wordTemp[0] = bTabla2[1]; wordTemp[1] = bTabla2[0];
                file.WriteLine("14: " + BitConverter.ToUInt16(wordTemp, 0));
                wordTemp[0] = bTabla2[3]; wordTemp[1] = bTabla2[2];
                file.WriteLine("16: " + BitConverter.ToUInt16(wordTemp, 0));
                wordTemp[0] = bTabla2[5]; wordTemp[1] = bTabla2[4];
                file.WriteLine("18: " + BitConverter.ToUInt16(wordTemp, 0));
                wordTemp[0] = bTabla2[7]; wordTemp[1] = bTabla2[6];
                file.WriteLine("20: " + BitConverter.ToUInt16(wordTemp, 0));
                longTemp[0] = bTabla2[11]; longTemp[1] = bTabla2[10]; longTemp[2] = bTabla2[9]; longTemp[3] = bTabla2[8];
                file.WriteLine("22: " + BitConverter.ToUInt32(longTemp, 0));
                longTemp[0] = bTabla3[3]; longTemp[1] = bTabla3[2]; longTemp[2] = bTabla3[1]; longTemp[3] = bTabla3[0];
                file.WriteLine("26: " + BitConverter.ToUInt32(longTemp, 0));
                wordTemp[0] = bTabla3[5]; wordTemp[1] = bTabla3[4];
                file.WriteLine("30: " + BitConverter.ToUInt16(wordTemp, 0));
                wordTemp[0] = bTabla3[7]; wordTemp[1] = bTabla3[6]; 
                file.WriteLine("32: " + BitConverter.ToUInt16(wordTemp, 0));
                wordTemp[0] = bTabla3[9]; wordTemp[1] = bTabla3[8];
                file.WriteLine("34: " + BitConverter.ToUInt16(wordTemp, 0));
                //if (BitConverter.ToUInt16(wordTemp, 0) != 0x4000) //Comprueba la variable TBAM, para que salga con el valor correcto
                //    PiezaLote.eAlarma = Alarma.TBAM;
                wordTemp[0] = bTabla3[11]; wordTemp[1] = bTabla3[10]; //Hay que darle la vuelta a los bytes
                file.WriteLine("36: " + BitConverter.ToUInt16(wordTemp, 0));
                file.WriteLine("38: " + bTabla3[12]);
                file.WriteLine("39: " + bTabla3[13]);
                file.WriteLine("40: " + bTabla3[14]);
                file.WriteLine("41: " + bTabla4[0]);
                file.WriteLine("42: " + bTabla4[1]);
                file.WriteLine("43: " + bTabla4[2]);
                longTemp[0] = bTabla4[6]; longTemp[1] = bTabla4[5]; longTemp[2] = bTabla4[4]; longTemp[3] = bTabla4[3]; //Hay que darle la vuelta a los bytes
                file.WriteLine("44: " + BitConverter.ToUInt32(longTemp, 0));
                wordTemp[0] = bTabla4[8]; wordTemp[1] = bTabla4[7]; //Hay que darle la vuelta a los bytes
                file.WriteLine("48: " + BitConverter.ToUInt16(wordTemp, 0));
            }            
            comprobacionFinal(PiezaLote);            
        }

        private bool checksumBuffer(byte[] buffer)
        {
            int iLRC = 0;
            for (int i = 0; i < 15; i++)
            {
                iLRC += buffer[i];
            }
        
            iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
            iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
            if (Convert.ToByte(iLRC) == buffer[15])
                return true;
            else
                return false;
        }

        /// <summary>
        /// En este metodo se realizan comprobaciones que son necesarias y que no tiene en cuenta el circuito de test,
        /// por ejemplo que garantice un minimo de maniobras
        /// </summary>
        /// <param name="PiezaLote"></param>
        private void comprobacionFinal(Pieza PiezaLote) 
        { 
            double ciclosMinuto = (double)PiezaLote.Ciclos / (double)PiezaLote.Minutos;

            if (PiezaLote.eAlarma == Alarma.TBAM)
            {
                PiezaLote.eResultado = Resultado.NOK;
                testNOK(PiezaLote);
            }
            else if (ciclosMinuto < 4.167 && PiezaLote.Minutos > 100) //Solo lo comprueba en test reales (las pruebas que hago no superan los 100 minutos)
            {
                PiezaLote.eResultado = Resultado.NOK;
                PiezaLote.eAlarma = Alarma.Tasa_Ciclos_Minuto;
                testNOK(PiezaLote);
            }
            else
                testOK(PiezaLote);
                
        }

        private void testOK(Pieza PiezaLote)
        {
            if (PiezaLote.eEstadoTest.Equals(EstadosTest.OK))
                return;
            PiezaLote.eEstadoTest = EstadosTest.OK;
            string sSerial = PiezaLote.Serial;
            string sFecha = PiezaLote.FechaInicio.Insert(2, "/").Insert(5, "/");
            string sHora = PiezaLote.HoraInicio.Insert(2, ":").Insert(5, ":");
            string sFechaFin = Date_Time.GetFecha().Insert(2, "/").Insert(5, "/");
            string sHoraFin = Date_Time.GetHora().Insert(2, ":").Insert(5, ":");
          
            
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(PiezaLote.Path, true))
            {
                file.WriteLine("");
                file.WriteLine("Test finalizado con exito");
                file.WriteLine("Familia de la Pieza: " + PiezaLote.eFamilia);
                file.WriteLine("Versión de la electrónica: " + PiezaLote.Version);
                file.WriteLine("Duración del test: " + PiezaLote.Minutos + " minutos");
                file.WriteLine("Ciclos realizados: " + PiezaLote.Ciclos);
                file.WriteLine("Nivel inicial de batería: " + PiezaLote.eNivelBateria[0]);
                file.WriteLine("Nivel de batería antes de Antipánico: " + PiezaLote.eNivelBateria[NUM_MUESTRAS_BATERIA-2]);
                file.WriteLine("Nivel final de batería: " + PiezaLote.eNivelBateria[NUM_MUESTRAS_BATERIA]);
                file.WriteLine("");
            }
            mTracer.modificaArchivo(PiezaLote.Path, "Fecha Fin:", "Fecha Fin: " + sFechaFin);
            mTracer.modificaArchivo(PiezaLote.Path, "Hora Fin:", "Hora Fin: " + sHoraFin);
            mTracer.modificaResultadoArchivo(PiezaLote.Path, "No_finalizado...", "OK", ".OK"); //Actualiza el resultado en la trazabilidad y modifica la extension del archivo de trazabilidad
            actualizaPoscicion(PiezaLote);

            string tab = "\t";
            string LineaTraza = sSerial + tab + this.Text + tab + PiezaLote.eEstadoTest + tab + sFecha + tab + sHora + tab +
              sFechaFin + tab + sHoraFin + tab + PiezaLote.Posicion + tab + PiezaLote.Minutos + tab + PiezaLote.Ciclos + tab +
              PiezaLote.eFamilia + tab + PiezaLote.eAlarma + tab + PiezaLote.eMaxReintentos + tab + PiezaLote.Path + tab;
            
            LineaTraza += String.Join("\t", PiezaLote.eNivelBateria);

            mTracer.añadeLineaExcel(PATH_TRACER, LineaTraza);
            mTracer.InsertTestInSql(PiezaLote, sHoraFin, sFechaFin);

            mDisplay.actualizaPieza(PiezaLote);
        }               

        private void testNOK(Pieza PiezaLote)
        {
            rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeError), Date_Time.GetFechaFormat() + ", " + Date_Time.GetHoraFormat() + ", Posición: " + PiezaLote.Posicion + " = pieza finalizada con resultado NOK");            
            PiezaLote.eEstadoTest = EstadosTest.NOK;
            //Crea un objeto de consulta a archivo .INI
            IniManager m_iniManager = new IniManager(FILE_INI);
            string sSerial = PiezaLote.Serial;
            string sFecha = PiezaLote.FechaInicio.Insert(2, "/").Insert(5, "/");
            string sHora = PiezaLote.HoraInicio.Insert(2, ":").Insert(5, ":");
            string sFechaFin = Date_Time.GetFecha().Insert(2, "/").Insert(5, "/");
            string sHoraFin = Date_Time.GetHora().Insert(2, ":").Insert(5, ":");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(PiezaLote.Path, true))
            {
                file.WriteLine("");
                file.WriteLine("Test finalizado NOK");

                file.WriteLine("Familia de la Pieza: " + PiezaLote.eFamilia);
                file.WriteLine("Versión de la electrónica: " + PiezaLote.Version);
                file.WriteLine("Duración del test: " + PiezaLote.Minutos + " minutos");
                file.WriteLine("Ciclos realizados: " + PiezaLote.Ciclos);
                file.WriteLine("Error encontrado: " +  PiezaLote.eAlarma);
                file.WriteLine("");
            }

            mTracer.modificaArchivo(PiezaLote.Path, "Fecha Fin:", "Fecha Fin: " + sFechaFin);
            mTracer.modificaArchivo(PiezaLote.Path, "Hora Fin:", "Hora Fin: " + sHoraFin);
            mTracer.modificaResultadoArchivo(PiezaLote.Path, "No_finalizado...", "NOK", ".NOK"); //Actualiza el resultado en la trazabilidad y modifica la extension del archivo de trazabilidad
            
            string tab = "\t";
            string LineaTraza = sSerial + tab + this.Text + tab + PiezaLote.eEstadoTest + tab + sFecha + tab + sHora + tab +
              sFechaFin + tab + sHoraFin + tab + PiezaLote.Posicion + tab + PiezaLote.Minutos + tab + PiezaLote.Ciclos + tab +
              PiezaLote.eFamilia + tab + PiezaLote.eAlarma + tab + PiezaLote.eMaxReintentos + tab + PiezaLote.Path + tab;

            LineaTraza += String.Join("\t", PiezaLote.eNivelBateria);

            mTracer.añadeLineaExcel(PATH_TRACER, LineaTraza);
            mTracer.InsertTestInSql(PiezaLote, sHoraFin, sFechaFin);

            mDisplay.actualizaPieza(PiezaLote);
        }        


        private void muestraIconoWarning(Pieza element)
        {
            FCarroMini mFCarro = devuelveCarro(element.Posicion);
            Label LPos = labelPosicion(element.Posicion, mFCarro);
            //Para mostrar la emergencia        
            LPos.Image = Test_Burning.Properties.Resources.warning_icon_18px;
            //MethodInvoker delegado = new MethodInvoker(LPos.BringToFront);
            //LPos.BeginInvoke(delegado);
            try
            {
                if (devuelveNumCarro(element.Posicion) == Convert.ToByte(fCarroTabla.lNumCarro.Text))
                {
                    Label Ltemp = labelPosicion(element.Posicion, fCarroTabla);
                    Ltemp.Image = Test_Burning.Properties.Resources.warning_icon;
                    MethodInvoker delegado = new MethodInvoker(Ltemp.BringToFront);
                    Ltemp.BeginInvoke(delegado);
                    Ltemp.BeginInvoke(new UpdateVisibleLabel(visibleLabel), Ltemp, true); //LPos.Visible = true;
                }
            }
            catch { }
        }

        private void labelWarning(Pieza PiezaLote)
        {
           
            if (PiezaLote.Reintentos > 2)
            {
                muestraIconoWarning(PiezaLote);
            }
            else if (PiezaLote.Reintentos == 0)
            {
                FCarroMini mFCarro = devuelveCarro(PiezaLote.Posicion);
                Label LPos = labelPosicion(PiezaLote.Posicion, mFCarro);
                Label Ltemp = labelPosicion(PiezaLote.Posicion, fCarroTabla);
                if (LPos.Image != null || Ltemp.Image != null)
                {
                    LPos.Image = null;
                    //MethodInvoker delegado = new MethodInvoker(LPos.SendToBack);
                    //LPos.BeginInvoke(delegado);

                    if (devuelveNumCarro(PiezaLote.Posicion) == Convert.ToByte(fCarroTabla.lNumCarro.Text))
                    {
                        
                        Ltemp.Image = null;
                        MethodInvoker delegado2 = new MethodInvoker(Ltemp.SendToBack);
                        Ltemp.BeginInvoke(delegado2);
                        Ltemp.BeginInvoke(new UpdateVisibleLabel(visibleLabel), Ltemp, true); //LPos.Visible = true;
                    }
                }
            }
        }


        private bool lrcOk(byte[] recibido)
        {
            int iLRC;
            //version particular de Manusa para la función LRC. Lo normal sería hacer sólo:
            //foreach (byte b in bytesToSend)
            //	byteLRC ^= b;
            iLRC = 0;
            for (int i = 0; i < 15; i++)
                iLRC += recibido[i];

            iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
            iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
            if (recibido[15] == iLRC)
                return true;
            else
                return false;
        }
        
        private void actualizaLayoutConPiezaImplicada(byte pieza)
        {
            actualizaLayoutConCarroImplicado(devuelveNumCarro(pieza));           
        }

        private void actualizaLayoutConCarroImplicado(byte carro)
        {
            foreach (char Layout in mBurn.dDistribucionCarros.Keys)
            {
                if (mBurn.dDistribucionCarros[Layout] == carro)
                {
                    cLayoutSeleccionado = Layout;
                    eventoFCarroMini = true;
                    actualizaLayout();
                }
            }
        }

        private void eliminaCarro(byte carro)
        {
            foreach (char Layout in mBurn.dDistribucionCarros.Keys)
            {
                if (mBurn.dDistribucionCarros[Layout] == carro)
                {
                    mBurn.dDistribucionCarros[Layout] = 0;
                    limpiaTabla();
                    seleccionarNuevoLayout();              
                    actualizaTodasPosicionesTabla();
                    return;
                }
            }
        }

        private void crearNuevaPieza(string sText)
        {
            LoopSemap = Semaforo.Amarillo; //Pone el semaforo amarillo para que pare de comunicar
            Pieza nuevaPieza = new Pieza();
            IniManager m_iniManager = new IniManager(FILE_INI);
            nuevaPieza.Posicion = Convert.ToByte(iBarcode);
            nuevaPieza.Serial = sText;
            nuevaPieza.FechaInicio = Date_Time.GetFecha();
            nuevaPieza.HoraInicio = Date_Time.GetHora();
            nuevaPieza.Minutos = 0;
            nuevaPieza.Ciclos = 0;
            nuevaPieza.Reintentos = 0;
            nuevaPieza.eMaxReintentos = 0;
            string sPath = m_iniManager.ReadValue("GEN", "PathTracer", "C:\\Trazabilidad\\Burning\\") + sText + "_" + Date_Time.GetFecha() + "_" + Date_Time.GetHora() + ".log";
            nuevaPieza.Path = sPath;
            mInicialBurn.listLote.Add(nuevaPieza);
        }        
    
        private bool añadirPieza(Pieza nuevaPieza)
        {
            bool comunicacionFallida = false;
            Xml mXml = new Xml(Barcodes.codigo(nuevaPieza.Serial, "(01)", "("));
            string voltaje = mXml.dParametros["Voltaje"];
            string familia = mXml.dParametros["Familia"];
            nuevaPieza.eFamilia = (Familia)Enum.Parse(typeof(Familia), familia);
            byte bVoltaje;
            TestBoardTCP mTB = mGS.retTestBoard(nuevaPieza.Posicion);

            if (voltaje == "AL230V")
                bVoltaje = 1;
            else if (voltaje == "AL115V")
                bVoltaje = 2;
            else bVoltaje = 0;

            for (int i = 0; i < 6; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                byte temp;
                if (!mTB.leerByte(nuevaPieza.Posicion, TestBoardTCP.OUTPUTS_L,out temp)) //Lee algo para ver si comunica
                {
                    if (i == 2)
                        return false;
                }
                else i = 10;
            }
                        
            for (int i = 0; i < 3; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                if (!mTB.escribirByte(nuevaPieza.Posicion, 0x70, bVoltaje))
                    comunicacionFallida = true;
                if (!mTB.escribirByte(nuevaPieza.Posicion, 0x71, Convert.ToByte(EstadosTestBoard.Init)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(nuevaPieza.Posicion, 0x72, Convert.ToUInt16(0)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(nuevaPieza.Posicion, 0x74, Convert.ToUInt16(0)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(nuevaPieza.Posicion, 0x78, Convert.ToUInt16(MINUTOS_MIN)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(nuevaPieza.Posicion, 0x7A, Convert.ToUInt16(CICLOS_MIN)))
                    comunicacionFallida = true;
                if (!mTB.escribirByte(nuevaPieza.Posicion, 0x76, Convert.ToByte(ControlTestBoard.InicioNormal)))
                    comunicacionFallida = true;

                int iLRC = 0;
                byte[] temp = new byte[2];
                temp = BitConverter.GetBytes(Convert.ToUInt16(0));
                iLRC += temp[0] + temp[1];
                temp = BitConverter.GetBytes(Convert.ToUInt16(0));
                iLRC += temp[0] + temp[1];
                temp = BitConverter.GetBytes(Convert.ToUInt16(CICLOS_MIN));
                iLRC += temp[0] + temp[1];
                temp = BitConverter.GetBytes(Convert.ToUInt16(MINUTOS_MIN));
                iLRC += temp[0] + temp[1];
                iLRC += bVoltaje + Convert.ToByte(EstadosTestBoard.Init) + Convert.ToByte(ControlTestBoard.InicioNormal);
                //Calculo de crc
                iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
                iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
                if (!mTB.escribirByte(nuevaPieza.Posicion, 0x7F, Convert.ToByte(iLRC)))
                    comunicacionFallida = true;


                if (!comunicacionFallida) //Si la comunicacion no ha fallado
                {
                    nuevaPieza.eEstadoTest = EstadosTest.Iniciando;
                    mBurn.listLote.Add(nuevaPieza); //Añade la pieza al burning
                    actualizaPoscicion(nuevaPieza);
                    mTracer.cabeceraTrazabilidad(nuevaPieza, tOP.Text, this.Text);
                    mDisplay.actualizaPieza(nuevaPieza);
                    return true;
                }
                else if (i < 2)
                    comunicacionFallida = false;
                else
                {
                    nuevaPieza.Reintentos++;
                    return false;
                }
            }
            return false;
        }

        private bool restablecerPieza(Pieza element)
        {
            bool comunicacionFallida = false;
            Xml mXml = new Xml(Barcodes.codigo(element.Serial, "(01)", "("));
            string voltaje = mXml.dParametros["Voltaje"];
            byte bVoltaje;
            TestBoardTCP mTB = mGS.retTestBoard(element.Posicion);

            if (voltaje == "AL230V")
                bVoltaje = 1;
            else if (voltaje == "AL115V")
                bVoltaje = 2;
            else bVoltaje = 0;

            for (int i = 0; i < 6; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                byte temp;
                if (!mTB.leerByte(element.Posicion, TestBoardTCP.OUTPUTS_L, out temp)) //Lee algo para ver si comunica
                {
                    if (i == 2)
                        return false;
                }
                else i = 10;
            }

            for (int i = 0; i < 3; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                if (!mTB.escribirByte(element.Posicion, 0x70, bVoltaje))
                    comunicacionFallida = true;
                if (!mTB.escribirByte(element.Posicion, 0x71, Convert.ToByte(EstadosTestBoard.Init)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(element.Posicion, 0x72, Convert.ToUInt16(element.Ciclos)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(element.Posicion, 0x74, Convert.ToUInt16(element.Minutos)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(element.Posicion, 0x78, Convert.ToUInt16(MINUTOS_MIN)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(element.Posicion, 0x7A, Convert.ToUInt16(CICLOS_MIN)))
                    comunicacionFallida = true;
                if (!mTB.escribirByte(element.Posicion, 0x76, Convert.ToByte(ControlTestBoard.Reinicio)))
                    comunicacionFallida = true;

                int iLRC = 0;
                byte[] temp = new byte[2];
                temp = BitConverter.GetBytes(Convert.ToUInt16(element.Ciclos));
                iLRC += temp[0] + temp[1];
                temp = BitConverter.GetBytes(Convert.ToUInt16(element.Minutos));
                iLRC += temp[0] + temp[1];
                temp = BitConverter.GetBytes(Convert.ToUInt16(CICLOS_MIN));
                iLRC += temp[0] + temp[1];
                temp = BitConverter.GetBytes(Convert.ToUInt16(MINUTOS_MIN));
                iLRC += temp[0] + temp[1];
                iLRC += bVoltaje + Convert.ToByte(EstadosTestBoard.Init) + Convert.ToByte(ControlTestBoard.Reinicio);
                //Calculo de crc
                iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
                iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
                if (!mTB.escribirByte(element.Posicion, 0x7F, Convert.ToByte(iLRC)))
                    comunicacionFallida = true;


                if (!comunicacionFallida) //Si la comunicacion no ha fallado
                {
                    element.eEstadoTest = EstadosTest.Iniciando;
                    actualizaPoscicion(element);
                    mDisplay.actualizaPieza(element);
                    return true;
                }
                else if (i < 2)
                    comunicacionFallida = false;
                else
                {
                    element.Reintentos++;
                    return false;
                }
            }
            return false;
        }

        private bool realimentarPieza(Pieza element)
        {
            bool comunicacionFallida = false;
            Xml mXml = new Xml(Barcodes.codigo(element.Serial, "(01)", "("));
            string voltaje = mXml.dParametros["Voltaje"];
            byte bVoltaje;
            TestBoardTCP mTB = mGS.retTestBoard(element.Posicion);

            if (voltaje == "AL230V")
                bVoltaje = 1;
            else if (voltaje == "AL115V")
                bVoltaje = 2;
            else bVoltaje = 0;

            for (int i = 0; i < 6; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                byte temp;
                if (!mTB.leerByte(element.Posicion, TestBoardTCP.OUTPUTS_L, out temp)) //Lee algo para ver si comunica
                {
                    if (i == 2)
                        return false;
                }
                else i = 10;
            }

            for (int i = 0; i < 3; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                if (!mTB.escribirByte(element.Posicion, 0x70, bVoltaje))
                    comunicacionFallida = true;
                if (!mTB.escribirByte(element.Posicion, 0x71, Convert.ToByte(EstadosTestBoard.Alim)))
                    comunicacionFallida = true;                
                if (!mTB.escribirByte(element.Posicion, 0x76, Convert.ToByte(ControlTestBoard.InicioNormal)))
                    comunicacionFallida = true;

                int iLRC = 0;
                iLRC += bVoltaje + Convert.ToByte(EstadosTestBoard.Alim) + Convert.ToByte(ControlTestBoard.InicioNormal);
                //Calculo de crc
                iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
                iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
                if (!mTB.escribirByte(element.Posicion, 0x7F, Convert.ToByte(iLRC)))
                    comunicacionFallida = true;
                
                if (!comunicacionFallida) //Si la comunicacion no ha fallado
                {
                    return true;
                }
                else if (i < 2)
                    comunicacionFallida = false;
                else
                {
                    element.Reintentos++;
                    return false;
                }
            }
            return false;
        }
        
        private void introducirParametros(Pieza nPieza)
        {
            bool comunicacionFallida = false;
            string sAntipanico = "";
            Xml mXml = new Xml(Barcodes.codigo(nPieza.Serial, "(01)", "("));
            byte Cfg_cel = 0;   //Convert.ToByte(mXml.dParametros["Cfg_cel"]);
            byte Cfg_pany = 3;  //Convert.ToByte(mXml.dParametros["Cfg_pany"]);
            byte Cfg_alr = 0;   //Convert.ToByte(mXml.dParametros["Cfg_alr"]);
            byte Cfg_ant = Convert.ToByte(mXml.dParametros["Cfg_ant"]);
            byte N_marques = Convert.ToByte(mXml.dParametros["N_marques"]);
            byte ParamControl = (byte)ControlParametros.CargarParametros; //Para que cargue los parametros, 
            TestBoardTCP mTB = mGS.retTestBoard(nPieza.Posicion);

            sAntipanico = mXml.dParametros["Antipanico"];

            if(sAntipanico == "false")
                ParamControl += (byte)ControlParametros.SinAntipanico;
            else
                ParamControl += (byte)ControlParametros.AntipanicoAuto;
            
            //if (nPieza.eFamilia != Familia.Bravo) //Si es Bravo el antipanico es manual y tiene que activarlo el operario
            
            

            if (!mTB.escribirByte(nPieza.Posicion, 0x90, Cfg_cel))
                comunicacionFallida = true;
            if (!mTB.escribirByte(nPieza.Posicion, 0x91, Cfg_pany))
                comunicacionFallida = true;
            if (!mTB.escribirByte(nPieza.Posicion, 0x92, Cfg_alr))
                comunicacionFallida = true;
            if (!mTB.escribirByte(nPieza.Posicion, 0x93, Cfg_ant))
                comunicacionFallida = true;
            if (!mTB.escribirByte(nPieza.Posicion, 0x94, N_marques))
                comunicacionFallida = true;
            if (!mTB.escribirByte(nPieza.Posicion, 0x9C, ParamControl))
                comunicacionFallida = true;

            int iLRC = Convert.ToByte(Cfg_cel) + Convert.ToByte(Cfg_pany) + Convert.ToByte(Cfg_alr) + Convert.ToByte(Cfg_ant) + Convert.ToByte(N_marques) + ParamControl;
            //Calculo de crc
            iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
            iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
            byte bLRC = Convert.ToByte(iLRC);
            if (!mTB.escribirByte(nPieza.Posicion, 0x9F, bLRC))
                comunicacionFallida = true;

            byte[] bBuffer = new byte[16];
            bBuffer[0] = Cfg_cel;
            bBuffer[1] = Cfg_pany;
            bBuffer[2] = Cfg_alr;
            bBuffer[3] = Cfg_ant;
            bBuffer[4] = N_marques;
            bBuffer[0x0C] = ParamControl;
            bBuffer[0x0F] = bLRC;

            if (!comunicacionFallida) //Si la comunicacion no ha fallado
            {
                if(compruebaParametros(nPieza, bBuffer))
                    nPieza.eEstadoTest = EstadosTest.Testeando;
                return;
            }
            else
                nPieza.Reintentos++;
        }

        private bool compruebaParametros(Pieza nPieza, byte[] buffer)
        {
            byte[] bBuffer = new byte[16];
            buffer[10] = 64; //TBAM
            TestBoardTCP mTB = mGS.retTestBoard(nPieza.Posicion);

            if (!mTB.leerBuffer(nPieza.Posicion, 0x90, out bBuffer))
                return false;
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    if (buffer[i] != bBuffer[i])
                        return false;
                }
                return true;
            }
        }

        //private void antipanicoBravo(Pieza nPieza)
        //{
        //    bool comunicacionFallida = false;
        //    Xml mXml = new Xml(Barcodes.codigo(nPieza.Serial, "(01)", "("));
        //    byte Cfg_cel = Convert.ToByte(mXml.dParametros["Cfg_cel"]);
        //    byte Cfg_pany = Convert.ToByte(mXml.dParametros["Cfg_pany"]);
        //    byte Cfg_alr = Convert.ToByte(mXml.dParametros["Cfg_alr"]);
        //    byte Cfg_ant = Convert.ToByte(mXml.dParametros["Cfg_ant"]);
        //    byte N_marques = Convert.ToByte(mXml.dParametros["N_marques"]);
        //    byte ParamControl = (byte)ControlParametros.CargarParametros + (byte)ControlParametros.AntipanicoManual; //Para que cargue los parametros

        //    int iLRC = Cfg_cel + Cfg_pany + Cfg_alr + Cfg_ant + N_marques + ParamControl;
        //    //Calculo de crc
        //    iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
        //    iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
        //    byte bLRC = Convert.ToByte(iLRC);
        //    byte[] bParam = new byte[16];
        //    bParam[0] = Cfg_cel;
        //    bParam[1] = Cfg_pany;
        //    bParam[2] = Cfg_alr;
        //    bParam[3] = Cfg_ant;
        //    bParam[4] = N_marques;
        //    bParam[0x0C] = ParamControl;
        //    bParam[0x0F] = bLRC;

        //    LoopSemap = Semaforo.Amarillo;
        //    for (int ii = 0; ii < 2000; ii++)
        //    {
        //        if (LoopSemap == Semaforo.Rojo)
        //            ii = 2000;
        //        Thread.Sleep(1);
        //    }

        //    for (int i = 0; i < 4; i++)
        //    {
        //        comunicacionFallida = false;
        //        if (!mTB.escribirByte(nPieza.Posicion, 0x9C, ParamControl))
        //            comunicacionFallida = true;

        //        if (!mTB.escribirByte(nPieza.Posicion, 0x9F, Convert.ToByte(iLRC)))
        //            comunicacionFallida = true;

        //        //if (!compruebaParametros(nPieza, bParam))
        //         //   comunicacionFallida = true;

        //        if (!comunicacionFallida) //Si no ha fallado la trama sale del bucle
        //            i = 10;
        //    }

        //    //for (int i = 0; i < 2; i++)
        //    //{
        //    //    comunicacionFallida = false;
        //    //    if (!compruebaParametros(nPieza, bParam))
        //    //        comunicacionFallida = true;

        //    //    if (!comunicacionFallida) //Si no ha fallado la trama sale del bucle
        //    //        i = 10;
        //    //}

        //    LoopSemap = Semaforo.Verde;
        //    if (comunicacionFallida)
        //    {
        //        nPieza.Reintentos = nPieza.Reintentos + 1;
        //        return;
        //    }
        //    else //Si la comunicacion no ha fallado
        //        nPieza.eEstadoTest = EstadosTest.Finalizando;
        //}

        //private void antipanicoBravoOLD(Pieza nPieza, ControlParametros resultado)
        //{
        //    bool comunicacionFallida = false;
        //    Xml mXml = new Xml(Barcodes.codigo(nPieza.Serial, "(01)", "("));
        //    byte Cfg_cel = Convert.ToByte(mXml.dParametros["Cfg_cel"]);
        //    byte Cfg_pany = Convert.ToByte(mXml.dParametros["Cfg_pany"]);
        //    byte Cfg_alr = Convert.ToByte(mXml.dParametros["Cfg_alr"]);
        //    byte Cfg_ant = Convert.ToByte(mXml.dParametros["Cfg_ant"]);
        //    byte N_marques = Convert.ToByte(mXml.dParametros["N_marques"]);
        //    byte ParamControl = Convert.ToByte((byte)ControlParametros.CargarParametros + (byte)ControlParametros.AntipanicoManual + (byte)resultado); //Para que cargue los parametros, 

        //    if (!mTB.escribirByte(nPieza.Posicion, 0x9C, ParamControl))
        //        comunicacionFallida = true;

        //    int iLRC = Cfg_cel + Cfg_pany + Cfg_alr + Cfg_ant + N_marques + ParamControl;
        //    //Calculo de crc
        //    iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
        //    iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
        //    if (!mTB.escribirByte(nPieza.Posicion, 0x9F, Convert.ToByte(iLRC)))
        //        comunicacionFallida = true;

        //    byte bLRC = Convert.ToByte(iLRC);
        //    byte[] bParam = new byte[16];
        //    bParam[0] = Cfg_cel;
        //    bParam[1] = Cfg_pany;
        //    bParam[2] = Cfg_alr;
        //    bParam[3] = Cfg_ant;
        //    bParam[4] = N_marques;
        //    bParam[0x0C] = ParamControl;
        //    bParam[0x0F] = bLRC;
            
        //    if (!compruebaParametros(nPieza, bParam))
        //        comunicacionFallida = true;
            
        //    if (!comunicacionFallida) //Si la comunicacion no ha fallado
        //    {
        //        return;
        //    }
        //    else
        //        nPieza.Reintentos = nPieza.Reintentos + 1;
        //}
        
        //private void actualizaPoscicion(byte posicion, int minutos, int ciclos, Color color)
        //{
        //    FCarro carro = mDisplay.encuentraCarro(posicion);
        //    if (carro == null)
        //        return;            
        //    labelPosicion(posicion, carro).BackColor = color;
        //    Label LPos = labelPosicion(posicion, carro);
        //    LPos.Visible = true;
        //    LPos.BorderStyle = BorderStyle.Fixed3D;
        //    Label LTiempo = labelTiempo(posicion, carro);
        //    LTiempo.Visible = true;
        //    LTiempo.BackColor = color;             
        //    LTiempo.Text = Convert.ToString(minutos) + "'";
        //    Label LCiclos = labelCiclos(posicion, carro);
        //    LCiclos.Visible = true;
        //    LCiclos.BackColor = color;
        //    LCiclos.Text = Convert.ToString(ciclos);            
        //}

        //private void actualizaPoscicion (byte posicion, string minutos, string ciclos, Color color){
        //    FCarro carro = mDisplay.encuentraCarro(posicion);
        //    if (carro == null)
        //        return;
        //    labelPosicion(posicion, carro).BackColor = color;
        //    Label LPos = labelPosicion(posicion, carro);
        //    LPos.Visible = true;
        //    LPos.BorderStyle = BorderStyle.Fixed3D;
        //    Label LTiempo = labelTiempo(posicion, carro);
        //    LTiempo.Visible = true;
        //    LTiempo.BackColor = color;
        //    LTiempo.Text = minutos + "'";
        //    Label LCiclos = labelCiclos(posicion, carro);
        //    LCiclos.Visible = true;
        //    LCiclos.BackColor = color;
        //    LCiclos.Text = ciclos;            
        //}

        //private void actualizaPoscicion(byte posicion, UInt16 minutos, UInt16 ciclos, EstadosTest estado, Resultado resultado)
        //{
        //    FCarro carro = mDisplay.encuentraCarro(posicion);
        //    Color color = new Color();
        //    if (carro == null)
        //        return;
        //    if (estado == EstadosTest.Reposo)           
        //        color = Color.Transparent;            
        //    else if (estado == EstadosTest.EsperaInicio)
        //        color = Color.LemonChiffon;
        //    else if (estado == EstadosTest.TesteandoSinParam)
        //        color = Color.LemonChiffon;
        //    else if (estado == EstadosTest.Testeando)
        //        color = Color.LemonChiffon;
        //    else if (estado == EstadosTest.VolcadoMemoria)
        //        color = Color.LemonChiffon;
        //    else if (estado == EstadosTest.Espera)
        //        color = Color.Yellow;
        //    else if (estado == EstadosTest.Finalizando)
        //        color = Color.LemonChiffon;
        //    else if (estado == EstadosTest.OK)
        //        color = Color.Lime;
        //    else if (estado == EstadosTest.NOK)
        //        color = Color.Red;
            
            
        //    labelPosicion(posicion, carro).BackColor = color;
        //    labelPosicion(posicion, fCarroTabla).BackColor = color;
        //    Label LPosmini = labelPosicion(posicion, carro);
        //    LPosmini.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPosmini, true); //LPos.Visible = true;
        //    LPosmini.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPosmini, true); //LPos.BorderStyle = BorderStyle.Fixed3D;
        //    Label LPos = labelPosicion(posicion, fCarroTabla);
        //    LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, true); //LPos.Visible = true;
        //    LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, true); //LPos.BorderStyle = BorderStyle.Fixed3D;
        //    Label LTiempo = labelTiempo(posicion, fCarroTabla);
        //    LTiempo.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LTiempo, true); //LTiempo.Visible = true;
        //    LTiempo.BeginInvoke(new UpdateLabelColor(labelBackColor), LTiempo, color); //LTiempo.BackColor = color;
        //    LTiempo.BeginInvoke(new UpdateLabelText(actualizaLabel), LTiempo, Convert.ToString(minutos) + "'"); // LTiempo.Text = minutos + "'";
        //    Label LCiclos = labelCiclos(posicion, fCarroTabla);
        //    LCiclos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LCiclos, true); //LCiclos.Visible = true;
        //    LCiclos.BeginInvoke(new UpdateLabelColor(labelBackColor), LCiclos, color); //LCiclos.BackColor = color;
        //    LCiclos.BeginInvoke(new UpdateLabelText(actualizaLabel), LCiclos, Convert.ToString(ciclos)); //LCiclos.Text = ciclos;
        //}

        private void actualizaPoscicion(Pieza element)
        {
            lock (updateTableLock)
            {
                try
                {
                    // if (mAnalizador.AlarmaCorteSuministro)
                    //    return;
                    FCarroMini carro = devuelveCarro(element.Posicion);
                    Color color = new Color();
                    if (element == null)
                        return;
                    color = devuelveColorPieza(element);

                    labelPosicion(element.Posicion, carro).BackColor = color;
                    //labelPosicion(element.Posicion, fCarroTabla).BackColor = color;
                    Label LPosmini = labelPosicion(element.Posicion, carro);
                    LPosmini.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPosmini, true); //LPos.Visible = true;
                    LPosmini.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPosmini, true); //LPos.BorderStyle = BorderStyle.Fixed3D;

                    //if (devuelveNumCarro(element.Posicion) == Convert.ToByte(fCarroTabla.lNumCarro.Text))
                    if (devuelveNumCarro(element.Posicion) == mBurn.dDistribucionCarros[cLayoutSeleccionado])
                        actualizaPosicionTabla(element);
                }
                catch (Exception)
                {
                    
                   
                }
                
            }
        }

        private Color devuelveColorPieza(Pieza element)
        {
            Color color = new Color();
            if (element.eEstadoTest == EstadosTest.Vacio)
                color = Color.Transparent;
            else if (element.eEstadoTest == EstadosTest.Iniciando)
                color = Color.LemonChiffon;
            else if (element.eEstadoTest == EstadosTest.Cargando_Param)
                color = Color.LemonChiffon;
            else if (element.eEstadoTest == EstadosTest.Testeando)
                color = Color.LemonChiffon;
            else if (element.eEstadoTest == EstadosTest.VolcadoMemoria)
                color = Color.LemonChiffon;
            else if (element.eEstadoTest == EstadosTest.Espera)
                color = Color.Yellow;
            else if (element.eEstadoTest == EstadosTest.Finalizando)
                color = Color.LemonChiffon;
            else if (element.eEstadoTest == EstadosTest.OK)
                color = Color.Lime;
            else if (element.eEstadoTest == EstadosTest.NOK)
                color = Color.Red;
            return color;
        }

        private void limpiaPosicion(byte posicion)
        {            
            FCarroMini carro = devuelveCarro(posicion);
            Label LPos = labelPosicion(posicion, carro);

            LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, false);            
            LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, false);

            if (devuelveNumCarro(posicion) == mBurn.dDistribucionCarros[cLayoutSeleccionado])
                limpiaPosicionCarroTabla(posicion);
        }

        private bool desconectaTestBoard(byte posicion)
        {
            bool comunicacionFallida = false;
            TestBoardTCP mTB = mGS.retTestBoard(posicion);

            for (int i = 0; i < 3; i++) //Realiza hasta 3 reintentos de comunicar con la maquina
            {
                if (!mTB.escribirByte(posicion, 0x70, 0)) //Alimentacion
                    comunicacionFallida = true;
                if (!mTB.escribirByte(posicion, 0x71, Convert.ToByte(EstadosTestBoard.Stop)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(posicion, 0x72, Convert.ToUInt16(0)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(posicion, 0x74, Convert.ToUInt16(0)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(posicion, 0x78, Convert.ToUInt16(0)))
                    comunicacionFallida = true;
                if (!mTB.escribirWord(posicion, 0x7A, Convert.ToUInt16(0)))
                    comunicacionFallida = true;
                if (!mTB.escribirByte(posicion, 0x76, Convert.ToByte(ControlTestBoard.InicioNormal)))
                    comunicacionFallida = true;

                int iLRC = 0;
                iLRC = Convert.ToByte(ControlTestBoard.InicioNormal);
                //Calculo de crc
                iLRC = iLRC & 0xFF;					//formatea a byte (0-255). Equivale a iLRC = (iLRC % 256)
                iLRC = ((iLRC ^ 0xFF) + 1) & 0xFF;	//equivale a iLRC = (-iLRC + 256) & 0xFF
                if (!mTB.escribirByte(posicion, 0x7F, Convert.ToByte(iLRC)))
                    comunicacionFallida = true;

                if (!comunicacionFallida) //Si la comunicacion no ha fallado
                    return true;
                else if (i < 2)
                    comunicacionFallida = false;                
            }
            return false;
        }

        private void limpiaPosicionCarroTabla(byte posicion)
        {
            List<Label> listLab = new List<Label>();
            Label LPos = labelPosicion(posicion, fCarroTabla);
            Label LTiempo = labelTiempo(posicion, fCarroTabla);
            Label LCiclos = labelCiclos(posicion, fCarroTabla);

            LPos.BeginInvoke(new UpdateBorderStyle3D(borderStyle3D), LPos, false);
            LTiempo.BeginInvoke(new UpdateLabelColor(labelBackColor), LTiempo, Color.Transparent);
            LTiempo.BeginInvoke(new UpdateLabelText(actualizaLabel), LTiempo, "");
            LTiempo.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LTiempo, false);
            LCiclos.BeginInvoke(new UpdateLabelColor(labelBackColor), LCiclos, Color.Transparent);
            LCiclos.BeginInvoke(new UpdateLabelText(actualizaLabel), LCiclos, "");
            LCiclos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LCiclos, false);
            LPos.BeginInvoke(new UpdateVisibleLabel(visibleLabel), LPos, false);

            listLab = etiquetasPosicionTabla(posicion);
            for (int ii = 0; ii < listLab.Count; ii++)
            {
                listLab.ElementAt(ii).BeginInvoke(new UpdateLabelText(actualizaLabel), listLab.ElementAt(ii), "-");
            }
        }

        private FCarroMini devuelveCarro(byte Posicion)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte((Posicion + 19) / 20);

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    return mCarro1;
                case 2:
                    return mCarro2;
                case 3:
                    return mCarro3;
                case 4:
                    return mCarro4;
                case 5:
                    return mCarro5;
                case 6:
                    return mCarro6;
                case 7:
                    return mCarro7;
                case 8:
                    return mCarro8;
                case 9:
                    return mCarro9;
                default:
                    return null;
            }
        }

        private byte devuelveNumCarro(byte Posicion)
        {
            byte bNumCarro;
            bNumCarro = Convert.ToByte((Posicion + 19) / 20);
            return bNumCarro;          
        }

        private byte devuelePosicionRelativa(byte posicion)
        {
            int iPosRelativa=0;
            byte bNumCarro = devuelveNumCarro(posicion);
            
            iPosRelativa = posicion + 20 - (bNumCarro * 20);
            return Convert.ToByte(iPosRelativa);
        }
        
        private Label labelPosicion(byte Posicion, FCarro Carro)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) /20) * 20);

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    return Carro.p1;
                case 2:
                    return Carro.p2;
                case 3:
                    return Carro.p3;
                case 4:
                    return Carro.p4;
                case 5:
                    return Carro.p5;
                case 6:
                    return Carro.p6;
                case 7:
                    return Carro.p7;
                case 8:
                    return Carro.p8;
                case 9:
                    return Carro.p9;
                case 10:
                    return Carro.p10;
                case 11:
                    return Carro.p11;
                case 12:
                    return Carro.p12;
                case 13:
                    return Carro.p13;
                case 14:
                    return Carro.p14;
                case 15:
                    return Carro.p15;
                case 16:
                    return Carro.p16;
                case 17:
                    return Carro.p17;
                case 18:
                    return Carro.p18;
                case 19:
                    return Carro.p19;
                case 20:
                    return Carro.p20;                
                default:
                    return null;
            }
        }

        private Label labelPosicion(byte Posicion, FCarroMini Carro)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) / 20) * 20);

            try
            {
                switch (bPosPiezaSelec)
                {
                    case 0:
                        return null;
                    case 1:
                        return Carro.p1;
                    case 2:
                        return Carro.p2;
                    case 3:
                        return Carro.p3;
                    case 4:
                        return Carro.p4;
                    case 5:
                        return Carro.p5;
                    case 6:
                        return Carro.p6;
                    case 7:
                        return Carro.p7;
                    case 8:
                        return Carro.p8;
                    case 9:
                        return Carro.p9;
                    case 10:
                        return Carro.p10;
                    case 11:
                        return Carro.p11;
                    case 12:
                        return Carro.p12;
                    case 13:
                        return Carro.p13;
                    case 14:
                        return Carro.p14;
                    case 15:
                        return Carro.p15;
                    case 16:
                        return Carro.p16;
                    case 17:
                        return Carro.p17;
                    case 18:
                        return Carro.p18;
                    case 19:
                        return Carro.p19;
                    case 20:
                        return Carro.p20;
                    default:
                        return null;
                }
            }
            catch(Exception ex)
            {
                
            }

            return null;
        }
       
        private Label labelTiempo(byte Posicion, FCarro Carro)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) / 20) * 20);

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    return Carro.lM1;
                case 2:
                    return Carro.lM2;
                case 3:
                    return Carro.lM3;
                case 4:
                    return Carro.lM4;
                case 5:
                    return Carro.lM5;
                case 6:
                    return Carro.lM6;
                case 7:
                    return Carro.lM7;
                case 8:
                    return Carro.lM8;
                case 9:
                    return Carro.lM9;
                case 10:
                    return Carro.lM10;
                case 11:
                    return Carro.lM11;
                case 12:
                    return Carro.lM12;
                case 13:
                    return Carro.lM13;
                case 14:
                    return Carro.lM14;
                case 15:
                    return Carro.lM15;
                case 16:
                    return Carro.lM16;
                case 17:
                    return Carro.lM17;
                case 18:
                    return Carro.lM18;
                case 19:
                    return Carro.lM19;
                case 20:
                    return Carro.lM20;
                default:
                    return null;
            }
        }

        private Label labelCiclos(byte Posicion, FCarro Carro)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) /20) * 20);

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    return Carro.lC1;
                case 2:
                    return Carro.lC2;
                case 3:
                    return Carro.lC3;
                case 4:
                    return Carro.lC4;
                case 5:
                    return Carro.lC5;
                case 6:
                    return Carro.lC6;
                case 7:
                    return Carro.lC7;
                case 8:
                    return Carro.lC8;
                case 9:
                    return Carro.lC9;
                case 10:
                    return Carro.lC10;
                case 11:
                    return Carro.lC11;
                case 12:
                    return Carro.lC12;
                case 13:
                    return Carro.lC13;
                case 14:
                    return Carro.lC14;
                case 15:
                    return Carro.lC15;
                case 16:
                    return Carro.lC16;
                case 17:
                    return Carro.lC17;
                case 18:
                    return Carro.lC18;
                case 19:
                    return Carro.lC19;
                case 20:
                    return Carro.lC20;                
                default:
                    return null;
            }
        }

        private List<Label> etiquetasPosicionTabla(byte Posicion)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) / 20) * 20);

            List<Label> listLab = new List<Label>();
            Label lN = new Label();
            Label lRestante = new Label();
            Label lEstado = new Label();
            Label lInfo = new Label();
            Label lNSerie = new Label();

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    lN = lN1;
                    lRestante = lRestante1;
                    lEstado = lEstado1;
                    lInfo = lInfo1;
                    lNSerie = lNSerie1;
                    break;
                case 2:
                    lN = lN2;
                    lRestante = lRestante2;
                    lEstado = lEstado2;
                    lInfo = lInfo2;
                    lNSerie = lNSerie2;
                    break;
                case 3:
                    lN = lN3;
                    lRestante = lRestante3;
                    lEstado = lEstado3;
                    lInfo = lInfo3;
                    lNSerie = lNSerie3;
                    break;
                case 4:
                    lN = lN4;
                    lRestante = lRestante4;
                    lEstado = lEstado4;
                    lInfo = lInfo4;
                    lNSerie = lNSerie4;
                    break;
                case 5:
                    lN = lN5;
                    lRestante = lRestante5;
                    lEstado = lEstado5;
                    lInfo = lInfo5;
                    lNSerie = lNSerie5;
                    break;
                case 6:
                    lN = lN6;
                    lRestante = lRestante6;
                    lEstado = lEstado6;
                    lInfo = lInfo6;
                    lNSerie = lNSerie6;
                    break;
                case 7:
                    lN = lN7;
                    lRestante = lRestante7;
                    lEstado = lEstado7;
                    lInfo = lInfo7;
                    lNSerie = lNSerie7;
                    break;
                case 8:
                    lN = lN8;
                    lRestante = lRestante8;
                    lEstado = lEstado8;
                    lInfo = lInfo8;
                    lNSerie = lNSerie8;
                    break;
                case 9:
                    lN = lN9;
                    lRestante = lRestante9;
                    lEstado = lEstado9;
                    lInfo = lInfo9;
                    lNSerie = lNSerie9;
                    break;
                case 10:
                    lN = lN10;
                    lRestante = lRestante10;
                    lEstado = lEstado10;
                    lInfo = lInfo10;
                    lNSerie = lNSerie10;
                    break;
                case 11:
                    lN = lN11;
                    lRestante = lRestante11;
                    lEstado = lEstado11;
                    lInfo = lInfo11;
                    lNSerie = lNSerie11;
                    break;
                case 12:
                    lN = lN12;
                    lRestante = lRestante12;
                    lEstado = lEstado12;
                    lInfo = lInfo12;
                    lNSerie = lNSerie12;
                    break;
                case 13:
                    lN = lN13;
                    lRestante = lRestante13;
                    lEstado = lEstado13;
                    lInfo = lInfo13;
                    lNSerie = lNSerie13;
                    break;
                case 14:
                    lN = lN14;
                    lRestante = lRestante14;
                    lEstado = lEstado14;
                    lInfo = lInfo14;
                    lNSerie = lNSerie14;
                    break;
                case 15:
                    lN = lN15;
                    lRestante = lRestante15;
                    lEstado = lEstado15;
                    lInfo = lInfo15;
                    lNSerie = lNSerie15;
                    break;
                case 16:
                    lN = lN16;
                    lRestante = lRestante16;
                    lEstado = lEstado16;
                    lInfo = lInfo16;
                    lNSerie = lNSerie16;
                    break;
                case 17:
                    lN = lN17;
                    lRestante = lRestante17;
                    lEstado = lEstado17;
                    lInfo = lInfo17;
                    lNSerie = lNSerie17;
                    break;
                case 18:
                    lN = lN18;
                    lRestante = lRestante18;
                    lEstado = lEstado18;
                    lInfo = lInfo18;
                    lNSerie = lNSerie18;
                    break;
                case 19:
                    lN = lN19;
                    lRestante = lRestante19;
                    lEstado = lEstado19;
                    lInfo = lInfo19;
                    lNSerie = lNSerie19;
                    break;
                case 20:
                    lN = lN20;
                    lRestante = lRestante20;
                    lEstado = lEstado20;
                    lInfo = lInfo20;
                    lNSerie = lNSerie20;
                    break;
                default:
                    return null;
            }
            listLab.Add(lN);
            listLab.Add(lRestante);
            listLab.Add(lEstado);
            listLab.Add(lInfo);
            listLab.Add(lNSerie);

            return listLab;
        }

        private void activarClearTimer()
        {
            try
            {
                clearTimer.Enabled = false;
            }
            catch { }
            clearTimer = new System.Timers.Timer();
            clearTimer.Elapsed += new ElapsedEventHandler(ClearTimerEvent);
            clearTimer.Interval = TIMER_CLEAN;
            clearTimer.AutoReset = false;
            clearTimer.Enabled = true;            
            //clearTimer.BeginInit();
        }

        private void clear_rBitacora()
        {
            string mensajeInicial = rBitacora.Lines[0].ToString();
            rBitacora.Clear();
            //clearTimer.Enabled = false;
            //clearTimer.Stop();
            //clearTimer.Enabled = false;

            rBitacora.ScrollToCaret();
            rBitacora.SelectionColor = Color.DarkBlue;
            rBitacora.SelectionFont = new Font("Verdana", 12);
            rBitacora.SelectedText = mensajeInicial + "\n";
            rBitacora.ScrollToCaret();
        }

        private void mensajeError(string mensaje)
        {
            activarClearTimer();
            
            //rBitacora.ScrollToCaret();
            rBitacora.SelectionFont = new Font("Verdana", 12,FontStyle.Bold);
            rBitacora.SelectionColor = Color.Red;
            rBitacora.SelectedText = mensaje + "\n";
            rBitacora.ScrollToCaret();
        }

        private void mensajeInformacionSinSalto(string mensaje)
        {
            activarClearTimer();

            rBitacora.ScrollToCaret();
            rBitacora.SelectionColor = Color.DarkBlue;
            rBitacora.SelectionFont = new Font("Verdana", 12);
            rBitacora.SelectedText = mensaje;
            rBitacora.ScrollToCaret();
            //rBitacora.BulletIndent = 10;
            //rBitacora.SelectionFont = new Font("Georgia", 16, FontStyle.Bold);
            //rBitacora.SelectedText = "Mindcracker Network \n";
            //rBitacora.SelectionFont = new Font("Verdana", 12);
            //rBitacora.SelectionBullet = true;
            //rBitacora.SelectionColor = Color.DarkBlue;
            //rBitacora.SelectedText = "C# Corner" + "\n";
            //rBitacora.SelectionFont = new Font("Verdana", 12);
            //rBitacora.SelectionColor = Color.Orange;
            //rBitacora.SelectedText = "VB.NET Heaven" + "\n";
            //rBitacora.SelectionFont = new Font("Verdana", 12);
            //rBitacora.SelectionColor = Color.Green;
            //rBitacora.SelectedText = ".Longhorn Corner" + "\n";
            //rBitacora.SelectionColor = Color.Red;
            //rBitacora.SelectedText = ".NET Heaven" + "\n";
            //rBitacora.SelectionBullet = false;
            //rBitacora.SelectionFont = new Font("Tahoma", 10);
            //rBitacora.SelectionColor = Color.Black;
            //rBitacora.SelectedText = "This is a list of Mindcracker Network websites.\n";
            //rBitacora.ScrollToCaret();            
        }

        private void mensajeAdvertencia(string mensaje)
        {
            activarClearTimer();

            rBitacora.ScrollToCaret();
            rBitacora.SelectionFont = new Font("Verdana", 12, FontStyle.Regular);
            rBitacora.SelectionColor = Color.OrangeRed;
            rBitacora.SelectedText = mensaje + "\n";
            rBitacora.ScrollToCaret();
        }

        private void mensajeInformacion(string mensaje)
        {
            activarClearTimer();

            rBitacora.ScrollToCaret();
            rBitacora.SelectionColor = Color.DarkBlue;
            rBitacora.SelectionFont = new Font("Verdana", 12);
            rBitacora.SelectedText = mensaje + ".\n";
            rBitacora.ScrollToCaret();
            //rBitacora.BulletIndent = 10;
            //rBitacora.SelectionFont = new Font("Georgia", 16, FontStyle.Bold);
            //rBitacora.SelectedText = "Mindcracker Network \n";
            //rBitacora.SelectionFont = new Font("Verdana", 12);
            //rBitacora.SelectionBullet = true;
            //rBitacora.SelectionColor = Color.DarkBlue;
            //rBitacora.SelectedText = "C# Corner" + "\n";
            //rBitacora.SelectionFont = new Font("Verdana", 12);
            //rBitacora.SelectionColor = Color.Orange;
            //rBitacora.SelectedText = "VB.NET Heaven" + "\n";
            //rBitacora.SelectionFont = new Font("Verdana", 12);
            //rBitacora.SelectionColor = Color.Green;
            //rBitacora.SelectedText = ".Longhorn Corner" + "\n";
            //rBitacora.SelectionColor = Color.Red;
            //rBitacora.SelectedText = ".NET Heaven" + "\n";
            //rBitacora.SelectionBullet = false;
            //rBitacora.SelectionFont = new Font("Tahoma", 10);
            //rBitacora.SelectionColor = Color.Black;
            //rBitacora.SelectedText = "This is a list of Mindcracker Network websites.\n";
            //rBitacora.ScrollToCaret();            
        }

        private void actualizaLayout()
        {
            Label l;
            for (int i = tableLayoutPanel1.ColumnCount; i > 0; i--)
            {
                try
                {
                    tableLayoutPanel1.Controls.RemoveAt(i - 1);
                }
                catch { }
            }
            for (int i = mBurn.dDistribucionCarros.Count; i != 0; i--)
            {
                byte posLayout = Convert.ToByte(mBurn.dDistribucionCarros.Count - i);
                if (mBurn.dDistribucionCarros.ElementAt(i - 1).Value == 0)
                {
                    l = new Label();
                    l.AutoSize = false;
                    l.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    l.Size = new System.Drawing.Size(160, 184);
                    l.ForeColor = Color.Black;
                    l.Text = mBurn.dDistribucionCarros.ElementAt(i - 1).Key.ToString();
                    l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    tableLayoutPanel1.Controls.Add(l, posLayout, 0);
                }
                else
                {
                    tableLayoutPanel1.Controls.Add(carroToObject(mBurn.dDistribucionCarros.ElementAt(i - 1).Value), posLayout, 0);
                    carroToObject(mBurn.dDistribucionCarros.ElementAt(i - 1).Value).lMiniPos.Text = Convert.ToString(mBurn.dDistribucionCarros.ElementAt(i - 1).Key);
                    carroToObject(mBurn.dDistribucionCarros.ElementAt(i - 1).Value).lMiniNum.Text = Convert.ToString(mBurn.dDistribucionCarros.ElementAt(i - 1).Value);
                }
            }
            //mDisplay.actualizaLayout(mBurn.dDistribucionCarros);
        }

        private FCarroMini carroToObject(int carro)
        {
            switch (carro)
            {
                case 1:
                    return mCarro1;
                case 2:
                    return mCarro2;
                case 3:
                    return mCarro3;
                case 4:
                    return mCarro4;
                case 5:
                    return mCarro5;
                case 6:
                    return mCarro6;
                case 7:
                    return mCarro7;
                case 8:
                    return mCarro8;
                case 9:
                    return mCarro9;
                default:
                    return null;
            }
        }

        private void focus()
        {
            try
            {
                tBarcode.Focus();
            }
            catch { }
        }

        /// <summary>
        /// Actualiza el contenido en una etiqueta orientada a mostrar texto
        /// </summary>
        /// <param name="etiqueta">Etiqueta a modificar</param>
        /// <param name="texto">Texto que ha de mostrar la etiqueta</param>
        private void actualizaLabel(Label etiqueta, string texto)
        {
            etiqueta.Text = texto;
        }
        /// <summary>
        /// Cambia el color de la etiqueta pasada como argumento
        /// </summary>
        /// <param name="label">Etiqueta que se ha de modificar</param>
        /// <param name="color">Color por el que se ha de cambiar</param>
        private void labelForeColor(Label label, Color color)
        {
            label.ForeColor = color;
        }
        /// <summary>
        /// Cambia el color de fondo de la etiqueta pasada como argumento
        /// </summary>
        /// <param name="label">Etiqueta que se ha de modificar</param>
        /// <param name="color">Color por el que se ha de cambiar</param>
        private void labelBackColor(Label label, Color color)
        {
            label.BackColor = color;
        }
        /// <summary>
        /// Cambia el color del groupbox pasado como argumento
        /// </summary>
        /// <param name="groupbox">Groupbox que se ha de modificar</param>
        /// <param name="color">Color por el que se ha de cambiar</param>
        private void cambioBackColor(GroupBox groupbox, Color color)
        {
            groupbox.BackColor = color;
        }
        /// <summary>
        /// Cambia el color del groupbox pasado como argumento
        /// </summary>
        /// <param name="groupbox">Groupbox que se ha de modificar</param>
        /// <param name="color">Color por el que se ha de cambiar</param>
        private void cambioBackSystemColorControl(GroupBox groupbox)
        {
            groupbox.BackColor = SystemColors.Control;
        }
        /// <summary>
        /// Cambia el parametro visible en una etiqueta
        /// </summary>
        /// <param name="label">Label a la que queremos modificar la visibilidad</param>
        /// <param name="visible">Visible = true y No_Visible = false</param>
        private void visibleLabel(Label label, bool visible)
        {
            //if (label.IsDisposed)
            label.Visible = visible;
        }

        private void borderStyle3D(Label label, bool estile)
        {
            if (estile)
                label.BorderStyle = BorderStyle.Fixed3D;
            else
                label.BorderStyle = BorderStyle.None;
        }

        public static void layoutMarcado(char cPosicion)
        {
            cLayoutSeleccionado = cPosicion;
        }



        #region Eventos

        private void label1_Click(object sender, EventArgs e)
        {
            tBarcode.Text = "(01)GIEL01";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //fCarroMini4.lMiniNum.Text = "4";
            //actualizaLayout();
            //for (int i = tableLayoutPanel1.ColumnCount; i > 0; i--)
            //{
            //    try
            //    {
            //        tableLayoutPanel1.Controls.RemoveAt(i - 1);
            //    }
            //    catch { }
            //}

            //Label l = new Label();
            //l.AutoSize = false;
            //l.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            //l.Size = new System.Drawing.Size(128, 184);
            //l.Text = "A";
            //l.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            //tableLayoutPanel1.Controls.Add(l);
            //tableLayoutPanel1.Controls.Add(l);
            //tableLayoutPanel1.Controls.Add(l);
            //tableLayoutPanel1.Controls.Add(l);
            //tableLayoutPanel1.Controls.Add(l);
            //tableLayoutPanel1.Controls.Add(l);

            //mCarro7 = new FCarroMini();
            //mCarro7.lMiniNum.Text = "1";
            //mCarro7.lMiniPos.Text = "C";
            //tableLayoutPanel1.Controls.Add(mCarro7);

            //l.Text = "A";


            //mCarro7.p1.BackColor = SystemColors.WindowText;

            // l.Image = Test_Burning.Properties.Resources.cancel_icon;

            //UInt32 Corriente;
            //UInt32 Tension;
            //bool alarma;
            //bool caidaTermico;
            ////double hola = segTranscurridos(mBurn.lLote[0]);
            ////DateTime inicio = new DateTime(2014, 07, 15, 13, 0, 0);
            ////DateTime fin = new DateTime(2014, 07, 16, 14,12 , 24);
            ////TimeSpan ts = fin - inicio;
            ////double horas = ts.TotalHours;
            ////Para mostrar la emergencia
            ////fCarro1.p1.Image = null;
            ////fCarro1.p1.Image = Test_Burning.Properties.Resources.barcode_icon;

            //////MethodInvoker delegado = new MethodInvoker(LPos.BringToFront);
            //////LPos.BeginInvoke(delegado);
            //////MethodInvoker delegado2 = new MethodInvoker(LPos.SendToBack);
            //////LPos.BeginInvoke(delegado2);

            ////fCarro1.p1.BringToFront();
            ////fCarro1.p1.SendToBack();          

            //AnalizadorCVM_B100 analizer = new AnalizadorCVM_B100("COM5");
            //Corriente = analizer.CorrienteNeutro;
            //Tension = analizer.TensionFaseTrifasica;
            //alarma = analizer.AlarmaCorteSuministro;
            //caidaTermico = analizer.CaidaProtectorSobretensiones;

            //if (analizer.consutaParametros())
            //{
            //    Corriente = analizer.CorrienteNeutro;
            //    Tension = analizer.TensionFaseTrifasica;
            //    alarma = analizer.AlarmaCorteSuministro;
            //    caidaTermico = analizer.CaidaProtectorSobretensiones;
            //}
        }

        private void fCarroMini_Click(object sender, EventArgs e)
        {

        }

        private void rBitacora_Click(object sender, EventArgs e)
        {
            tBarcode.Focus();
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Point coordenadas = new Point();
                coordenadas = Form1.MousePosition;
                lXY.Text = coordenadas.X + " , " + coordenadas.Y;
                Point cero = new Point();
                cero = Form1.ActiveForm.PointToClient(coordenadas);
                label69.Text = cero.X + " , " + cero.Y; //Posicion de la pantalla
            }
            catch { }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //label70.Text = Convert.ToString(bPieza);
            ////if (bPieza == 0)
            //    //gInfo.Visible = false;
            //if (mBurn != null && mBurn.lLote != null && bPieza != 0)
            //{
            //    foreach (Pieza element in mBurn.lLote)
            //    {
            //        if (Convert.ToByte(element.Posicion) == bPieza)
            //        {
            //            byte bCarro = Convert.ToByte((bPieza + 19) / 20);
            //            byte bFila = Convert.ToByte((bPieza + 3 + 20 - (bCarro * 20)) / 4);
            //            byte bPosicion = Convert.ToByte(bPieza + 20 - (bCarro * 20) - ((bFila - 1) * 4));
            //            int iCiclosRestantes = CICLOS_MIN - Convert.ToInt32(element.Ciclos);
            //            if (iCiclosRestantes < 0)
            //                iCiclosRestantes = 0;
            //            int iMinutosRestantes = MINUTOS_MIN - Convert.ToInt32(element.Minutos);
            //            if (iMinutosRestantes < 0)
            //                iMinutosRestantes = 0;

            //            //tInfoCiclos.Text = "Ciclos Res " + Convert.ToString(iCiclosRestantes);
            //            //tInfoMin.Text = "Minutos Res " + Convert.ToString(iMinutosRestantes);
            //            //tInfoSerial.Text = "S/N: " + element.Serial;
            //            //tInfoCarro.Text = "C " + bCarro;
            //            //tInfoFila.Text = "F " + bFila;
            //            //tInfoPos.Text = "P " + bPosicion;
            //            //tInfoEstado.Text = Convert.ToString(element.eEstadoTest);
            //            //gInfo.Visible = true;
            //            //return;

            //            mensajeError(GetFechaFormat() + ", " + GetHoraFormat() + ", Posición: " + element.Posicion + " = pieza finalizada con resultado NOK");

            //            rBitacora.ScrollToCaret();
            //            rBitacora.SelectionFont = new Font("Verdana", 12);
            //            rBitacora.SelectionColor = Color.DarkBlue;
            //            rBitacora.SelectedText = "Carro " + bCarro + ", Fila " + bFila +
            //                ", Posición " + bPosicion +  ", Serial " + element.Serial;
            //            rBitacora.SelectedText = bPieza.ToString() + "\n";
            //            //rBitacora.ScrollToCaret();
            //            bPieza = 0;
            //            return;
            //        }
            //    }
            //    //tInfoCiclos.Text = "Ciclos -";
            //    //tInfoMin.Text = "Minutos -";
            //    //tInfoSerial.Text = "S/N: -";
            //    //tInfoCarro.Text = "Carro -";
            //    //tInfoFila.Text = "Fila -";
            //    //tInfoPos.Text = "Pos -";
            //    //gInfo.Visible= false;
            //    //return;
            //}
            tBarcode.Focus();
        }

        private void tRefreshForm_Tick(object sender, EventArgs e)
        {

        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {            
            if (eventoFCarroMini)
            {
                tableLayoutPanel1.BeginInvoke(new UpdateLayour(refrescaTablaConLayoutSeleccionado));
                //MethodInvoker delegado1 = new MethodInvoker(refrescaTablaConLayoutSeleccionado);
                //tableLayoutPanel1.BeginInvoke(delegado1);
                eventoFCarroMini = false;
            }
        }

        private void ClearTimerEvent(object source, ElapsedEventArgs e)
        {
            MethodInvoker delegadoClearTimer = new MethodInvoker(clear_rBitacora);
            tBarcode.BeginInvoke(delegadoClearTimer);            
        }        

        private void tBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            byte bPiezaSelec = 0;
            byte[] bPosPiezaSelec = new byte[2];

            if (e.KeyCode == Keys.Return) //Cuando detecta el intro            
            {
                string sText = tBarcode.Text; ;
                tBarcode.Text = "";

                if (Barcodes.checkBarcode(sText.ToUpper()) == Barcodes.EBarcode.Operario)
                {
                    //Se crean variables
                    int Operario;
                    string OperarioString = sText.Remove(0, 2); //Se elimina las letras OP
                    IniManager m_iniManager = new IniManager(FILE_INI);

                    //Se capturan las excepciones, para que en el caso de que contenga caracteres despues de OP no modifique su valor
                    try
                    {
                        Operario = Convert.ToInt32(OperarioString); //Convierte en Int32 el string
                    }
                    catch
                    {
                        //Si dispara la excepción lee el valor que tenía para volverlo a poner
                        Operario = Convert.ToInt32(m_iniManager.ReadValue("GEN", "Operario", "0"));
                    }

                    //Escribe en el archivo de configuración el número de Operario
                    m_iniManager.WriteValue("GEN", "Operario", Convert.ToString(Operario));
                    //Muestra el Nºde operario en pantalla
                    tOP.Text = Convert.ToString(Operario);
                }
                else if (Barcodes.checkBarcode(sText) == Barcodes.EBarcode.Layout)
                {
                    cBarcode = Convert.ToChar(sText);
                }

                else if (Barcodes.checkBarcode(sText) == Barcodes.EBarcode.AñadirCarro)
                {
                    if (cBarcode != '0')
                    {
                        try
                        {
                            if (!mBurn.añadirCarroLayout(cBarcode, Convert.ToByte(sText.Replace("CARRO_", ""))))
                            {
                                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Error, el Carro o la posición ya están ocupadas.");
                            }
                            else
                            {
                                mGS.anadirCarro(Convert.ToByte(sText.Replace("CARRO_", "")));
                                cLayoutSeleccionado = cBarcode;
                                eventoFCarroMini = true;
                                actualizaLayout();
                                mDisplay.actualizaLayout(mBurn.dDistribucionCarros);
                                tBarcode.Focus();
                            }
                        }
                        catch
                        {
                            rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Error en la operacion deseada con el Carro.");
                        }
                        cBarcode = '0';
                    }
                }
                else if (Barcodes.checkBarcode(sText) == Barcodes.EBarcode.ExtraerCarro)
                {
                    byte bCarro = Convert.ToByte(sText.Replace("EXTRAER_", ""));
                    //Primero comprueba que no tenga el carro ninguna pieza en test
                    foreach (Pieza element in mBurn.listLote) //Si la pieza ya existe en el lote no la vuelve a introducir
                    {
                        if (element.Posicion > bCarro * 20 - 20 && element.Posicion <= bCarro * 20)
                        {
                            iBarcode = 0;
                            return;
                        }
                    }
                    mGS.eliminarCarro(Convert.ToByte(sText.Replace("EXTRAER_", "")));
                    eliminaCarro(bCarro);
                    mDisplay.actualizaLayout(mBurn.dDistribucionCarros);
                    tBarcode.Focus();
                }
                else if (Barcodes.checkBarcode(sText) == Barcodes.EBarcode.Kill)
                {
                    byte KillPosition;
                    try
                    {
                        KillPosition = Convert.ToByte(sText.Remove(0, 4).Replace(" ","").Replace("-", "").Replace("_", ""));
                    }
                    catch
                    {
                        return;
                    }
                    for (int i = 0; i < mBurn.listLote.Count; i++)
                    {
                        Pieza element = mBurn.listLote[i];
                        if (Convert.ToByte(element.Posicion) == KillPosition)
                        {
                            LoopSemap = Semaforo.Amarillo;
                            for (int ii = 0; ii < 2000; ii++)
                            {
                                if (LoopSemap == Semaforo.Rojo)
                                    ii = 2000;
                                Thread.Sleep(1);
                            }
                            //desconectaTestBoard(element.Posicion);
                            mBurn.listLote.RemoveAt(i);
                            limpiaPosicion(element.Posicion);
                            actualizaLayoutConPiezaImplicada(element.Posicion);
                            mDisplay.eliminaPieza(element.Posicion);
                            LoopSemap = Semaforo.Verde;
                            return;
                        }
                    }
                }
                else if (iBarcode == 0)
                {
                    try
                    {
                        bPiezaSelec = Convert.ToByte(sText);
                    }
                    catch { }

                    if (bPiezaSelec != 0 && mBurn.listLote != null)
                    {
                        //Comprueba que no exista esa posicion en el lote
                        for (int i = 0; i < mBurn.listLote.Count; i++)
                        {
                            Pieza element = mBurn.listLote[i];
                            if (Convert.ToByte(element.Posicion) == bPiezaSelec)
                            {
                                //if (element.eEstadoTest == EstadosTest.Espera && element.eFamilia == Familia.Bravo)
                                //{
                                //    antipanicoBravo(element);
                                //    iBarcode = 0;
                                //    return;
                                //}
                                if (element.Posicion == bPiezaSelec && (element.eEstadoTest == EstadosTest.OK || element.eEstadoTest == EstadosTest.NOK))
                                {                                   
                                    LoopSemap = Semaforo.Amarillo;
                                    for (int ii = 0; ii < 2000; ii++)
                                    {
                                        if (LoopSemap == Semaforo.Rojo)
                                            ii = 2000;
                                        Thread.Sleep(1);
                                    }
                                    if (!desconectaTestBoard(element.Posicion))
                                    {
                                        LoopSemap = Semaforo.Verde;
                                        return;
                                    }
                                    mBurn.listLote.RemoveAt(i);
                                    limpiaPosicion(element.Posicion);
                                    actualizaLayoutConPiezaImplicada(element.Posicion);
                                    mDisplay.eliminaPieza(element.Posicion);
                                    LoopSemap = Semaforo.Verde;
                                    return;
                                }
                            }
                        }
                    }

                    if (mBurn.CompruebaPresenciaCarro(bPiezaSelec)) //Si el carro de la pieza está presente 
                        iBarcode = bPiezaSelec;
                    else
                        rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Carro no existente, por favor introduzca primero el carro en su posición.");
                }
                else if (iBarcode > 0 && iBarcode < 254)
                {
                    if (Barcodes.checkBarcode(sText) == Barcodes.EBarcode.Pieza)
                    {
                        foreach (Pieza element in mBurn.listLote) //Si la pieza ya existe en el lote no la vuelve a introducir
                        {
                            if (element.Posicion == iBarcode)
                            {
                                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Error. Esta posición ya está ocupada.");
                                iBarcode = 0;
                                return;
                            }
                            if (element.Serial == sText)
                            {
                                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Error. Este serial está siendo testeado en la posición " + element.Posicion.ToString());
                                iBarcode = 0;
                                return;
                            }
                        }
                        foreach (Pieza element in mInicialBurn.listLote) //Si la pieza ya existe en el lote no la vuelve a introducir
                        {
                            if (element.Posicion == iBarcode)
                            {
                                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Error. Esta posición ya está ocupada.");
                                iBarcode = 0;
                                return;
                            }
                            if (element.Serial == sText)
                            {
                                rBitacora.BeginInvoke(new UpdateRichTextBox(mensajeAdvertencia), "Error. Este serial está siendo testeado en la posición " + element.Posicion.ToString());
                                iBarcode = 0;
                                return;
                            }
                        }

                        actualizaLayoutConPiezaImplicada((byte)iBarcode);

                        crearNuevaPieza(sText);

                        //añadirPieza(Convert.ToByte(iBarcode), sText);
                        iBarcode = 0;
                        cBarcode = '0';
                    }
                    else
                    {
                        cBarcode = '0';
                        iBarcode = 0;
                    }
                }
                else
                {
                    cBarcode = '0';
                    iBarcode = 0;
                }

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            aTimer.Enabled = false;
            aTimer = null;
            mAnalizador.serialPort1Close();
            mGS.close();
            mGS = null;
            mDisplay.Close();
            mDisplay = null;
            
            Thread.Sleep(700);
            try
            {
                tLoopUpdate.Abort();
            }
            catch { }
        }

        #endregion

        private void tBarcode_Leave(object sender, EventArgs e)
        {
            tBarcode.Focus();
        }

        private void label15_Click(object sender, EventArgs e)
        {
            MessageBox.Show("hola");
            mTracer.InsertTestInSql();
            

        }
    }
}
