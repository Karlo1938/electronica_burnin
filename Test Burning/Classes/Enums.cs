using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test_Burning.Classes
{
    public enum EstadosTest
    {
        Vacio, Iniciando, Cargando_Param, Testeando, Espera, Finalizando, VolcadoMemoria, OK, NOK
    }
    public enum EstadosTestBoard
    {
        Stop, Init, Burn, End, Prestop, Alim
    }
    public enum ControlTestBoard
    {
        InicioNormal = 1, Reinicio
    }
    public enum Alarma
    {
        SinAnomalia,
        Sobrecorriente,
        Entrada_SOS,
        Bloqueo_Cerrar,
        Entrada_Llave,
        Memoria_Parametros,
        Bloqueo_Abrir,
        Fotocelula_1,
        Fotocelula_1_o_2,
        Incendio,
        Bateria = 11,
        Radar_Interior,
        Radar_Exterior,
        Fotocelula_3,
        Comunicacion_DSP,
        Secur,
        No_Sensor = 23,
        Sensor_Seguridad,
        Modulo_Motor = 34,
        Isolation,
        Autoajuste,
        Comunicacion = 100,
        Bloqueo,
        Antipanico,
        Parametros,
        Proceso_Autoajuste,
        Tasa_Ciclos_Minuto = 200,
        TBAM,
        Inicializando,
        Desconocido
    }
    public enum Resultado
    {
        Unknown, OK, NOK
    }
    public enum Semaforo
    {
        Rojo, Amarillo, Verde
    }
    public enum Familia
    {
        Unknown, Retrofit, Activa, Visio, Visio100, ElectVisio, ElectVisioPlus
    }
    public enum ControlParametros
    {
        CargarParametros = 1, CargarConResetCiclos = 2, AntipanicoAuto = 4, AntipanicoManual = 8, AntipanicoOK = 16, AntipanicoNOK = 32, SinAntipanico = 64
    }

    public enum TipoCarro
    {
        Moto, Elec, Plus
    }
}
