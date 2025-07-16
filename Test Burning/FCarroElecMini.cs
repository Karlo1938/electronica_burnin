using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Test_Burning
{
    public partial class FCarroElecMini : Test_Burning.FCarroMini
    {
        public FCarroElecMini()
        {
            InitializeComponent();
        }

        private void todo_Click(object sender, EventArgs e)
        {
            Point coordenadas = new Point();
            coordenadas = Form1.MousePosition;
            Point PRelativo = new Point();
            try
            {
                PRelativo = Form1.ActiveForm.PointToClient(coordenadas);
            }
            catch
            {
                return;
            }

            int tamañoPosicion = Form1.SIZE_LAYOUT / Form1.POSICIONES;

            byte bPosicion = (Byte)((PRelativo.X + tamañoPosicion - 1) / tamañoPosicion);
            bPosicion = Convert.ToByte(Math.Abs(bPosicion - 10));
            char cPosicion = Convert.ToChar(bPosicion + 64);

            Form1.layoutMarcado(cPosicion);
            Form1.eventoFCarroMini = true;
        }


    }
}
