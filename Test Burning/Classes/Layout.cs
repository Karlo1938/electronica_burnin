using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test_Burning.Classes
{
    class Layout
    {
        public Dictionary<char, byte> dDistribucionCarros;

        public Layout()
        {
            dDistribucionCarros = new Dictionary<char, byte>();
            crearPosiciones();
        }

        private void crearPosiciones()
        {
            dDistribucionCarros.Add('A', 0);
            dDistribucionCarros.Add('B', 0);
            dDistribucionCarros.Add('C', 0);
            dDistribucionCarros.Add('D', 0);
            dDistribucionCarros.Add('E', 0);
            dDistribucionCarros.Add('F', 0);
            dDistribucionCarros.Add('G', 0);
            dDistribucionCarros.Add('H', 0);
            dDistribucionCarros.Add('I', 0);
        }
    }

}
