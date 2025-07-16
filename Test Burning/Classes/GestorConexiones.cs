using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Test;

namespace Test_Burning.Classes
{
    
    public class GestorConexiones
    {
        private const byte MAX_CONEXIONES = 16;
        private List<BusSerieTCP> listBus;  //Lista de buses (1 por carro)
        private List<TestBoardTCP> listTB;  //Lista de testboard (1 por carro)
        
        public GestorConexiones()
        {
            listBus = new List<BusSerieTCP>();
            listTB = new List<TestBoardTCP>();

            for (int i = 0; i < MAX_CONEXIONES; i++)
            {
                listBus.Add(null);
                listTB.Add(null);
                //listTB.Add(new TestBoardTCP(listBus.ElementAt(i)));
            }
        }

        /// <summary>
        /// Añade un bus para el carro indicado
        /// </summary>
        /// <param name="carro">Numero de carro</param>
        /// <param name="IP">Direccion del socket</param>
        /// <param name="port">Puerto para el socket</param>
        public void addBus(byte carro, string IP, int port)
        {
            byte index = (byte)(carro - 1);
                      
            listBus.Insert(index, new BusSerieTCP(IP, port)); //Lo inserta en la posicion que toca desplazando toda la lista
            listBus.RemoveAt(carro); //Elimina el siguiente en la lista para dejarla bien.

            listTB.Insert(index, new TestBoardTCP(listBus.ElementAt(index))); //Lo inserta en la posicion que toca desplazando toda la lista
            listTB.RemoveAt(carro); //Elimina el siguiente en la lista para dejarla bien.
        }


        /// <summary>
        /// Devuelve el bus segun la posición, lo hace calculando el numero de carro que es
        /// </summary>
        /// <param name="posición">posición de la pieza</param>
        /// <returns></returns>
        public TestBoardTCP retTestBoard(byte posicion)
        {
            byte index;
            TestBoardTCP mTB;

            index = (byte)(Burning.numeroCarro(posicion) - 1);
            mTB = listTB.ElementAt(index);

            return mTB;
        }

        public void close()
        {
            for (int i = 1; i <= MAX_CONEXIONES; i++)
            {
                try { listBus.ElementAt(i).close(); } catch (Exception e) { Debug.WriteLine("Exception cerrando el puerto " + e.ToString()); }
            }

            for (int i = 1; i <= MAX_CONEXIONES; i++)
            {
                try { listTB.RemoveAt(MAX_CONEXIONES - i); } catch(Exception e) { Debug.WriteLine("Error gestorConexiones " + e.ToString() ); }
            }
            foreach (BusSerieTCP bus in listBus)
            {
                try { bus.close(); } catch (Exception e) { Debug.WriteLine("Error gestorConexiones " + e.ToString()); }
            }

            for (int i = 1; i <= MAX_CONEXIONES; i++)
            {
                try { listBus.RemoveAt(MAX_CONEXIONES - i); } catch (Exception e) { Debug.WriteLine("Error gestorConexiones " + e.ToString()); }
            }
        }

        public void anadirCarros(Burning mBurn)             
        {
            byte bCarro;
            string sCarro;

            for (int i = 0; i < mBurn.dDistribucionCarros.Count; i++)
            {
                bCarro = mBurn.dDistribucionCarros.ElementAt(i).Value;
                sCarro = bCarro.ToString();
                if (bCarro != 0)
                {
                    anadirCarro(bCarro);                    
                }
            }
        }
        
        public void anadirCarro(byte Carro)
        {
            string IP;
            int port;
            byte index;
            string sCarro = Carro.ToString();            
              
            index = (byte)(Carro - 1);
            IP = Carros.ListaCarros[Carro].IP;
            port = Carros.ListaCarros[Carro].Port;

            foreach (var v in listBus)
            {
                try
                {
                    if (v._sIP == IP) //Si ya hay un bus con esa IP añade a la lista uno con la misma instancia
                    {
                        listBus.Insert(index, v); //Lo inserta en la posicion que toca desplazando toda la lista
                        listBus.RemoveAt(Carro); //Elimina el siguiente en la lista para dejarla bien.

                        listTB.Insert(index, new TestBoardTCP(listBus.ElementAt(index))); //Lo inserta en la posicion que toca desplazando toda la lista
                        listTB.RemoveAt(Carro); //Elimina el siguiente en la lista para dejarla bien.
                        return;
                    }
                }
                catch { }
            }  

            listBus.RemoveAt(index); //Elimina el siguiente en la lista para dejarla bien.
            listBus.Insert(index, new BusSerieTCP(IP, port)); //Lo inserta en la posicion que toca desplazando toda la lista  
            listTB.RemoveAt(index); //Elimina el siguiente en la lista para dejarla bien.
            listTB.Insert(index, new TestBoardTCP(listBus.ElementAt(index))); //Lo inserta en la posicion que toca desplazando toda la lista                
        }

        public void eliminarCarro(byte Carro)
        {
     byte index;
            string sCarro = Carro.ToString();

            //index = (byte)(Carro - 1);
            //try { listBus.ElementAt(index).close(); } catch (Exception e) { Debug.WriteLine("Exception cerrando el puerto " + e.ToString()); }

            //listBus.RemoveAt(index);
            //listBus.Insert(index, null);  
            //listTB.RemoveAt(index);
            //listTB.Insert(index, null);              
        }

    }
}
