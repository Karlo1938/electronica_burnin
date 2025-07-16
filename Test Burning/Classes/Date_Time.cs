using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Test_Burning.Classes
{
    class Date_Time
    {
        /// <summary>
        /// Devuelve la fecha en formato DDMMAA
        /// </summary>
        /// <returns></returns>
        public static string GetFecha()
        {
            ///Obtiene la fecha actual.
            string sYear = (DateTime.Now.Year - 2000).ToString().PadLeft(2, '0');
            string sMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string sDay = DateTime.Now.Day.ToString().PadLeft(2, '0');

            return string.Format("{0}{1}{2}", sDay, sMonth, sYear);
        }
        /// <summary>
        /// Devuelve la fecha en formato DDMMAA
        /// </summary>
        /// <returns></returns>
        public static string GetFechaFormat()
        {
            ///Obtiene la fecha actual.
            string sYear = (DateTime.Now.Year - 2000).ToString().PadLeft(2, '0');
            string sMonth = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string sDay = DateTime.Now.Day.ToString().PadLeft(2, '0');

            return string.Format("{0}/{1}/{2}", sDay, sMonth, sYear);
        }
        /// <summary>
        /// Devuelve la hora en formato HHMMSS
        /// </summary>
        /// <returns></returns>
        public static string GetHora()
        {
            ///Obtiene la hora actual.
            return string.Format("{0}{1}{2}", DateTime.Now.Hour.ToString().PadLeft(2, '0'),
                DateTime.Now.Minute.ToString().PadLeft(2, '0'), DateTime.Now.Second.ToString().PadLeft(2, '0'));
        }
        /// <summary>
        /// Devuelve la hora en formato HHMMSS
        /// </summary>
        /// <returns></returns>
        public static string GetHoraFormat()
        {
            ///Obtiene la hora actual.
            return string.Format("{0}:{1}", DateTime.Now.Hour.ToString().PadLeft(2, '0'),
                DateTime.Now.Minute.ToString().PadLeft(2, '0'), DateTime.Now.Second.ToString().PadLeft(2, '0'));
        }
        /// <summary>
        /// Devuelve el número de semana en formato int32
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int GetWeekOfYear(DateTime dateTime)
        {
            ///Se considera la primera semana del año si ésta tiene minimo 4 dias. Estandar ISO usado en Manusa.
            return CultureInfo.CurrentUICulture.Calendar.GetWeekOfYear(dateTime, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }


        public static double segTranscurridos(Pieza mPieza)
        {
            string fecha = mPieza.FechaInicio;
            int año = Convert.ToInt32("20" + fecha.Remove(0, 4));
            int mes = Convert.ToInt32(fecha.Remove(4, 2).Remove(0, 2));
            int dia = Convert.ToInt32(fecha.Remove(2, 4));
            string hora = mPieza.HoraInicio;
            int horas = Convert.ToInt32(hora.Remove(2, 4));
            int min = Convert.ToInt32(hora.Remove(4, 2).Remove(0, 2));
            int seg = Convert.ToInt32(hora.Remove(0, 4));

            DateTime inicio = new DateTime(año, mes, dia, horas, min, seg);
            TimeSpan ts = DateTime.Now - inicio;
            return ts.TotalSeconds;
        }

        public static void controlTiempoPieza(Pieza element)
        {
            string fecha = element.FechaInicio;
            int año = Convert.ToInt32("20" + fecha.Remove(0, 4));
            int mes = Convert.ToInt32(fecha.Remove(4, 2).Remove(0, 2));
            int dia = Convert.ToInt32(fecha.Remove(2, 4));
            string hora = element.HoraInicio;
            int horas = Convert.ToInt32(hora.Remove(2, 4));
            int min = Convert.ToInt32(hora.Remove(4, 2).Remove(0, 2));
            int seg = Convert.ToInt32(hora.Remove(0, 4));
            DateTime inicio = new DateTime(año, mes, dia, horas, min, seg);

            TimeSpan ts = DateTime.Now - inicio;
        }

    }
}
