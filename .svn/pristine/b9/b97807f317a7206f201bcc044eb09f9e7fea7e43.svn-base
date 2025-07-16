using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Timers;

using Test_Burning.Classes;

namespace Test_Burning
{
    public partial class FormDisplay : Form
    {
        //public static int X_CARRO_1Y5 = 10;
        //public static int X_CARRO_2Y6 = 260;
        //public static int X_CARRO_3Y7 = 510;
        //public static int X_CARRO_4Y8 = 760;
        //public static int Y_CARRO_1A4 = 20;
        //public static int Y_CARRO_5A8 = 340;
        public static int SEPARACION_X = 50;
        
        public static byte bPieza = 0;
        private const int NUM_CARROS = 9;
        //private static System.Timers.Timer tRefresh;
        private RealCarroXL[] CarroLayoutXL;
        private RealCarroElecXL[] CarroElecLayoutXL;
        private RealCarroElecPlusXL[] CarroElecPlusLayoutXL;
        private RealCarroMini[] CarroLayoutMini;
        private RealCarroElecMini[] CarroElecLayoutMini;
        private RealCarroElecPlusMini[] CarroElecPlusLayoutMini;

        private Dictionary<char, byte> dLayout;

        public FormDisplay()
        {
            InitializeComponent();
            CarroLayoutXL = new RealCarroXL[NUM_CARROS];
            CarroElecLayoutXL = new RealCarroElecXL[NUM_CARROS];
            CarroElecPlusLayoutXL = new RealCarroElecPlusXL[NUM_CARROS];
            CarroLayoutMini = new RealCarroMini[NUM_CARROS];
            CarroElecLayoutMini = new RealCarroElecMini[NUM_CARROS];
            CarroElecPlusLayoutMini = new RealCarroElecPlusMini[NUM_CARROS];

            for (int i = 0; i < NUM_CARROS; i++){
                CarroLayoutXL[i] = new RealCarroXL();
                CarroElecLayoutXL[i] = new RealCarroElecXL();
                CarroElecPlusLayoutXL[i] = new RealCarroElecPlusXL();
                CarroLayoutMini[i] = new RealCarroMini();
                CarroElecLayoutMini[i] = new RealCarroElecMini();
                CarroElecPlusLayoutMini[i] = new RealCarroElecPlusMini();
            }           
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //Form1.refrescaFormulario();
        }

        public void actualizaLayout(Dictionary<char, byte> dLayout)
        {
            this.dLayout = dLayout;
            Label l;
            for (int i = tableLayoutPanel1.ColumnCount; i > 0; i--)
            {
                try
                {
                    tableLayoutPanel1.Controls.RemoveAt(i - 1);
                }
                catch { }
            }
            for (int i = 0; i < dLayout.Count; i++)
            {
                if (dLayout.ElementAt(i).Value == 0)
                {
                    l = new Label();
                    l.AutoSize = false;
                    l.Font = new System.Drawing.Font("Microsoft Sans Serif", 24, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
                    l.Size = new System.Drawing.Size(170, 245);
                    l.ForeColor = Color.Yellow;
                    l.Text = dLayout.ElementAt(i).Key.ToString();
                    l.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
                    tableLayoutPanel1.Controls.Add(l, i, 0);
                }
                else
                {
                    if (Carros.ListaCarros[dLayout.ElementAt(i).Value].Tipo == TipoCarro.Elec)
                    {
                        CarroElecLayoutMini[i].lPos.Text = Convert.ToString(dLayout.ElementAt(i).Key);
                        CarroElecLayoutMini[i].lNum.Text = Convert.ToString(dLayout.ElementAt(i).Value);
                        tableLayoutPanel1.Controls.Add(CarroElecLayoutMini[i], i, 0);
                    }
                    else if (Carros.ListaCarros[dLayout.ElementAt(i).Value].Tipo == TipoCarro.Plus)
                    {
                        CarroElecPlusLayoutMini[i].lPos.Text = Convert.ToString(dLayout.ElementAt(i).Key);
                        CarroElecPlusLayoutMini[i].lNum.Text = Convert.ToString(dLayout.ElementAt(i).Value);
                        tableLayoutPanel1.Controls.Add(CarroElecPlusLayoutMini[i], i, 0);
                    }
                    else
                    {
                        CarroLayoutMini[i].lPos.Text = Convert.ToString(dLayout.ElementAt(i).Key);
                        CarroLayoutMini[i].lNum.Text = Convert.ToString(dLayout.ElementAt(i).Value);
                        tableLayoutPanel1.Controls.Add(CarroLayoutMini[i], i, 0);
                    }
                }
            }
            actualizaForm(dLayout);
        }

        public void actualizaPieza(Pieza mPieza)
        {
            try
            {
                RealCarroXL XLTemp;
                RealCarroMini MiniTemp;
                byte NumCarro = getNumCarro(mPieza.Posicion);
                char posLayout = '0';

                for (byte i = 0; i < dLayout.Count; i++)
                {
                    if (dLayout.ElementAt(i).Value == NumCarro)
                        posLayout = dLayout.ElementAt(i).Key;
                }

                if (Carros.ListaCarros[NumCarro].Tipo == TipoCarro.Elec)
                {
                    XLTemp = CarroElecLayoutXL[posLayout - 65];
                    MiniTemp = CarroElecLayoutMini[posLayout - 65];
                }
                else if (Carros.ListaCarros[NumCarro].Tipo == TipoCarro.Plus)
                {
                    XLTemp = CarroElecPlusLayoutXL[posLayout - 65];
                    MiniTemp = CarroElecPlusLayoutMini[posLayout - 65];
                }
                else
                {
                    XLTemp = CarroLayoutXL[posLayout - 65];
                    MiniTemp = CarroLayoutMini[posLayout - 65];
                }

                Label lPosicionXL = labelPosicion(mPieza.Posicion, XLTemp);
                Label lPosicionMini = labelPosicion(mPieza.Posicion, MiniTemp);

                if (mPieza.eEstadoTest == EstadosTest.Vacio)
                {
                    lPosicionMini.Image = null;
                    lPosicionXL.Image = null;
                }
                else
                {
                    lPosicionMini.Image = miniIconoPosicion(mPieza.eFamilia, mPieza.eResultado);
                    lPosicionXL.Image = XLIconoPosicion(mPieza.eFamilia, mPieza.eResultado);
                }
            }
            catch (Exception)
            {
              
            }

           
        }

        public void eliminaPieza(byte posicion)
        {
            RealCarroXL XLTemp;
            RealCarroMini MiniTemp;
            byte NumCarro = getNumCarro(posicion);
            char posLayout = '0';

            for (byte i = 0; i < dLayout.Count; i++)
            {
                if (dLayout.ElementAt(i).Value == NumCarro)
                    posLayout = dLayout.ElementAt(i).Key;
            }


            if (Carros.ListaCarros[NumCarro].Tipo == TipoCarro.Elec)
            {
                XLTemp = CarroElecLayoutXL[posLayout - 65];
                MiniTemp = CarroElecLayoutMini[posLayout - 65];
            }
            else if (Carros.ListaCarros[NumCarro].Tipo == TipoCarro.Plus)
            {
                XLTemp = CarroElecPlusLayoutXL[posLayout - 65];
                MiniTemp = CarroElecPlusLayoutMini[posLayout - 65];
            }
            else
            {
                XLTemp = CarroLayoutXL[posLayout - 65];
                MiniTemp = CarroLayoutMini[posLayout - 65];
            }

            Label lPosicionXL = labelPosicion(posicion, XLTemp);
            Label lPosicionMini = labelPosicion(posicion, MiniTemp);
            
            lPosicionMini.Image = ((System.Drawing.Image)(null));
            lPosicionXL.Image = ((System.Drawing.Image)(null));
        }

        private Label labelPosicion(byte Posicion, RealCarroMini Carro)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) / 20) * 20);

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    return Carro.lPos1;
                case 2:
                    return Carro.lPos2;
                case 3:
                    return Carro.lPos3;
                case 4:
                    return Carro.lPos4;
                case 5:
                    return Carro.lPos5;
                case 6:
                    return Carro.lPos6;
                case 7:
                    return Carro.lPos7;
                case 8:
                    return Carro.lPos8;
                case 9:
                    return Carro.lPos9;
                case 10:
                    return Carro.lPos10;
                case 11:
                    return Carro.lPos11;
                case 12:
                    return Carro.lPos12;
                case 13:
                    return Carro.lPos13;
                case 14:
                    return Carro.lPos14;
                case 15:
                    return Carro.lPos15;
                case 16:
                    return Carro.lPos16;
                case 17:
                    return Carro.lPos17;
                case 18:
                    return Carro.lPos18;
                case 19:
                    return Carro.lPos19;
                case 20:
                    return Carro.lPos20;
                default:
                    return null;
            }
        }

        private Label labelPosicion(byte Posicion, RealCarroXL Carro)
        {
            byte bPosPiezaSelec;
            bPosPiezaSelec = Convert.ToByte(Posicion + 20 - ((Posicion + 19) / 20) * 20);

            switch (bPosPiezaSelec)
            {
                case 0:
                    return null;
                case 1:
                    return Carro.lPos1;
                case 2:
                    return Carro.lPos2;
                case 3:
                    return Carro.lPos3;
                case 4:
                    return Carro.lPos4;
                case 5:
                    return Carro.lPos5;
                case 6:
                    return Carro.lPos6;
                case 7:
                    return Carro.lPos7;
                case 8:
                    return Carro.lPos8;
                case 9:
                    return Carro.lPos9;
                case 10:
                    return Carro.lPos10;
                case 11:
                    return Carro.lPos11;
                case 12:
                    return Carro.lPos12;
                case 13:
                    return Carro.lPos13;
                case 14:
                    return Carro.lPos14;
                case 15:
                    return Carro.lPos15;
                case 16:
                    return Carro.lPos16;
                case 17:
                    return Carro.lPos17;
                case 18:
                    return Carro.lPos18;
                case 19:
                    return Carro.lPos19;
                case 20:
                    return Carro.lPos20;
                default:
                    return null;
            }
        }

        private Image miniIconoPosicion(Familia family, Resultado res)
        {
            switch (family)
            {
                case Familia.Unknown:
                    return null;
                case Familia.Activa:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MiniMotACT;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MiniMotActRed;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MiniMotActGreen;
                    else
                        return null;
                case Familia.Visio100:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MiniMotACT;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MiniMotActRed;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MiniMotActGreen;
                    else
                        return null;
                case Familia.Visio:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MiniMotVi;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MiniMotViRed;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MiniMotViGreen;
                    else
                        return null;
                case Familia.Retrofit:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MiniMotRetro;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MiniMotRetroRed;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MiniMotRetroGreen;
                    else
                        return null;
                case Familia.ElectVisio:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MiniGiel01;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MiniGiel01Red;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MiniGiel01Green;
                    else
                        return null;
                case Familia.ElectVisioPlus:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MiniVisioPlus;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MiniVisioPlusRED;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MiniVisioPlusGreen;
                    else
                        return null;
                default:
                    return null;
            }
        }

        private Image XLIconoPosicion(Familia family, Resultado res)
        {
            switch (family)
            {
                case Familia.Unknown:
                    return null;
                case Familia.Activa:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MotActXL;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MotActRedXL;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MotActGreenXL;
                    else
                        return null;
                case Familia.Visio100:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MotActXL;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MotActRedXL;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MotActGreenXL;
                    else
                        return null;
                case Familia.Visio:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MotViXL;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MotViRedXL;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MotViGreenXL;
                    else
                        return null;
                case Familia.Retrofit:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.MotRetroXL;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.MotRetroRedXL;
                    else if (res == Resultado.OK)
                        return Properties.Resources.MotRetroGreenXL;
                    else
                        return null;
                case Familia.ElectVisio:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.Giel01XL;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.Giel01RedXL;
                    else if (res == Resultado.OK)
                        return Properties.Resources.Giel01GreenXL;
                    else
                        return null;
                case Familia.ElectVisioPlus:
                    if (res == Resultado.Unknown)
                        return Properties.Resources.VisioPlusXL;
                    else if (res == Resultado.NOK)
                        return Properties.Resources.VisioPlusRedXL;
                    else if (res == Resultado.OK)
                        return Properties.Resources.VisioPlusGreenXL;
                    else
                        return null;
                default:
                    return null;
            }
        }
  
        private void actualizaForm(Dictionary<char, byte> dLayout)
        {
            this.dLayout = dLayout;
            List<byte> listCarros = new List<byte>();

            for (byte i = 0; i < NUM_CARROS; i++)
            {
                try
                {
                    RealCarroXL CarroTemp = CarroLayoutXL[i]; //xlCarroToObject(i);
                    this.Controls.Remove(CarroTemp);
                    CarroTemp = CarroElecLayoutXL[i]; //xlCarroToObject(i);
                    this.Controls.Remove(CarroTemp);
                    CarroTemp = CarroElecPlusLayoutXL[i]; //xlCarroToObject(i);
                    this.Controls.Remove(CarroTemp);
                }
                catch { }
            }
            
            foreach (byte NumCarro in dLayout.Values)
            {
                if (NumCarro > 0)
                    listCarros.Add(NumCarro);
            }

            RealCarroXL mCarro = new RealCarroXL();
            int tamañoXTotalCarros = listCarros.Count * mCarro.Size.Width;
            int separacioX = (this.Size.Width - tamañoXTotalCarros) / (listCarros.Count() + 1);
            
            int posicionXProximoCarro = separacioX;//= (this.Size.Width / 2) - ((tamañoXTotalCarros + separacioX) / 2);
            int posicionY = ((this.Size.Height - tableLayoutPanel1.Size.Height - mCarro.Size.Height) / 2) + tableLayoutPanel1.Size.Height;

            for (byte i = 0; i < listCarros.Count; i++)
            {
                RealCarroXL CarroTemp;// = CarroElecLayoutXL[posicionCarro(dLayout, listCarros.ElementAt(i)) - 65];//xlCarroToObject(posicionCarro(dLayout, listCarros.ElementAt(i))-65);
                if (Carros.ListaCarros[listCarros.ElementAt(i)].Tipo == TipoCarro.Elec)
                    CarroTemp = CarroElecLayoutXL[posicionCarro(dLayout, listCarros.ElementAt(i)) - 65];
                else if (Carros.ListaCarros[listCarros.ElementAt(i)].Tipo == TipoCarro.Plus)
                    CarroTemp = CarroElecPlusLayoutXL[posicionCarro(dLayout, listCarros.ElementAt(i)) - 65];
                else
                    CarroTemp = CarroLayoutXL[posicionCarro(dLayout, listCarros.ElementAt(i)) - 65];

                CarroTemp.Location = new Point(posicionXProximoCarro, posicionY);
                CarroTemp.lNum.Text = Convert.ToString(listCarros.ElementAt(i));
                CarroTemp.lPosLyout.Text = posicionCarro(dLayout, listCarros.ElementAt(i)).ToString();
                this.Controls.Add(CarroTemp);
                posicionXProximoCarro += mCarro.Size.Width + separacioX;
            }
        }
     
        private char posicionCarro(Dictionary<char, byte> dLayout, byte posicion)
        {
            this.dLayout = dLayout;
            for (byte i = 0; i < dLayout.Count; i++)
            {
                if (dLayout.ElementAt(i).Value == posicion)
                    return dLayout.ElementAt(i).Key;
            }
            return '0';
        }

        public void ubicaCarros(List<Pieza> lote)
        {
            int iSizeW = this.Width;
            int iSizeH = this.Height;
            int iSeparacionW = (iSizeW - (FCarro.ANCHO_CARRO * 4)) / 5;
            //int iSeparacionH = (iSizeH - gInfo.Size.Height - (FCarro.ALTO_CARRO * 2)) / 3;
            int iSeparacionH = (iSizeH - (FCarro.ALTO_CARRO * 2)) / 3;

            //X_CARRO_1Y5 = iSeparacionW;
            //X_CARRO_2Y6 = iSeparacionW * 2 + FCarro.ANCHO_CARRO;
            //X_CARRO_3Y7 = iSeparacionW * 3 + (FCarro.ANCHO_CARRO * 2);
            //X_CARRO_4Y8 = iSeparacionW * 4 + (FCarro.ANCHO_CARRO * 3);
            //Y_CARRO_1A4 = 100; //Cambiado de 20
            //Y_CARRO_5A8 = iSeparacionH + FCarro.ALTO_CARRO + 60; //Se la añade la ultima suma

            //this.fCarro1.Location = new System.Drawing.Point(X_CARRO_1Y5, Y_CARRO_1A4);
            //this.fCarro2.Location = new System.Drawing.Point(X_CARRO_2Y6, Y_CARRO_1A4);
            //this.fCarro3.Location = new System.Drawing.Point(X_CARRO_3Y7, Y_CARRO_1A4);
            //this.fCarro4.Location = new System.Drawing.Point(X_CARRO_4Y8, Y_CARRO_1A4);
            //this.fCarro5.Location = new System.Drawing.Point(X_CARRO_1Y5, Y_CARRO_5A8);
            //this.fCarro6.Location = new System.Drawing.Point(X_CARRO_2Y6, Y_CARRO_5A8);
            //this.fCarro7.Location = new System.Drawing.Point(X_CARRO_3Y7, Y_CARRO_5A8);
            //this.fCarro8.Location = new System.Drawing.Point(X_CARRO_4Y8, Y_CARRO_5A8);
        }

        public void añadirCarro(char posicion, byte numCarro)
        {
            
        }

        public void eliminarCarro(char posicion, byte numCarro)
        {

        }

        private void Form_Resize(object sender, EventArgs e)
        {
            //ubicaCarros();
        }               

        private void button1_Click(object sender, EventArgs e)
        {            
            //carro = new RealCarroXL();
            //carro.Location = new Point(611, 190);            
            //carro.lPos15.Image = Test_Burning.Properties.Resources.MotActGreenXL;
            //this.Controls.Add(carro);
        }

        private byte getNumCarro(byte posicion)
        {
            byte bCarro = Convert.ToByte((posicion + 19) / 20);
            return bCarro;
        }

    }
}
