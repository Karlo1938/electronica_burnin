using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Input;
using Test_Burning.Classes;

namespace Test_Burning
{
    public partial class FCarro : UserControl
    {
        public const int ANCHO_CARRO = 240;
        public const int ALTO_CARRO = 310;
        private const int ANCHO_PIEZA = 37;
        private const int ALTO_PIEZA = 36;
        private const int POS_F1 = 35;
        private const int POS_F2 = 85;
        private const int POS_F3 = 135;
        private const int POS_F4 = 185;
        private const int POS_F5 = 235;
        private const int POS_C1 = 50;
        private const int POS_C2 = 93;
        private const int POS_C3 = 136;
        private const int POS_C4 = 179;
        private const byte CARRO = 0;
        private const byte FILA = 1;
        private const byte POSICION = 1;
        private const byte COLUMNA = 2;
        private const int X = 0;
        private const int Y = 1;
              


        public FCarro()
        {
            InitializeComponent();
        }
              

        private void pMouseLeave(object sender, EventArgs e)
        {
            FormDisplay.bPieza = 0;
            //p4.BackColor = Color.Transparent;
        }

        private void pMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //byte[] Pieza = new byte[3];
            //int[] PuntoRelativo = new int[2];

            //Pieza[CARRO] = findCarro(out PuntoRelativo);            
            //Pieza[FILA] = findFila(PuntoRelativo[Y]);
            //Pieza[COLUMNA] = findColumna (PuntoRelativo[X]);
            //if (Pieza[CARRO] == 0 || Pieza[FILA] == 0 || Pieza[COLUMNA] == 0) //Si el carro es igual a 0 es porque ha saltado una excepcion
            //    return;

            //FormDisplay.bPieza =Convert.ToByte( (Pieza[CARRO] * 20) + (Pieza[FILA] * 4) + Pieza[COLUMNA] - 24); //Se resta 24 porque no empezamos por el carro 0 fila 0
        }

        private byte findCarro(out int[] CoordenadaRelativa)
        {
            CoordenadaRelativa = new int[2];
            Point coordenadas = new Point();
            coordenadas = FormDisplay.MousePosition;
            Point PRelativo = new Point();
            try
            {
                PRelativo = FormDisplay.ActiveForm.PointToClient(coordenadas);
            }
            catch
            {
                CoordenadaRelativa[0] = 0;
                CoordenadaRelativa[1] = 0;                
                return 0;
            }
            
            return 0; //Si no se cumple ninguna condicion devuelve 0
        }

        private byte findFila(int PuntoY)
        {
            if (PuntoY >= POS_F1 && PuntoY <= POS_F1 + ALTO_PIEZA)
                return 1;
            else if (PuntoY >= POS_F2 && PuntoY <= POS_F2 + ALTO_PIEZA)
                return 2;
            else if (PuntoY >= POS_F3 && PuntoY <= POS_F3 + ALTO_PIEZA)
                return 3;
            else if (PuntoY >= POS_F4 && PuntoY <= POS_F4 + ALTO_PIEZA)
                return 4;
            else if (PuntoY >= POS_F5 && PuntoY <= POS_F5 + ALTO_PIEZA)
                return 5;
            else
                return 0;
        }

        private byte findColumna(int PuntoX)
        {
            if (PuntoX >= POS_C1 && PuntoX <= POS_C1 + ANCHO_PIEZA)
                return 1;
            else if (PuntoX >= POS_C2 && PuntoX <= POS_C2 + ANCHO_PIEZA)
                return 2;
            else if (PuntoX >= POS_C3 && PuntoX <= POS_C3 + ANCHO_PIEZA)
                return 3;
            else if (PuntoX >= POS_C4 && PuntoX <= POS_C4 + ANCHO_PIEZA)
                return 4;
            else
                return 0;
        }

        private void l_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            byte[] Pieza = new byte[3];
            int[] PuntoRelativo = new int[2];

            Pieza[CARRO] = findCarro(out PuntoRelativo);
            Pieza[FILA] = findFila(PuntoRelativo[Y]);
            Pieza[COLUMNA] = findColumna(PuntoRelativo[X]);
            if (Pieza[CARRO] == 0 || Pieza[FILA] == 0 || Pieza[COLUMNA] == 0) //Si el carro es igual a 0 es porque ha saltado una excepcion
                return;

            FormDisplay.bPieza = Convert.ToByte((Pieza[CARRO] * 20) + (Pieza[FILA] * 4) + Pieza[COLUMNA] - 24); //Se resta 24 porque no empezamos por el carro 0 fila 0            
        }
        
    }
}
