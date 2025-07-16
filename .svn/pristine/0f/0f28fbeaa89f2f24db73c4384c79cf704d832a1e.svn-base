//------------------------------------------------------------------------------
// Clase para manejar ficheros INIs
// Permite leer secciones enteras y todas las secciones de un fichero INI
//------------------------------------------------------------------------------
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Test
{
	/// La clase IniManager sirve para trabajar con ficheros.ini 
	
	public class IniManager
	{
		public string file;			// Fichero Ini, con el path competo
		private string sBuffer; // Para usarla en las funciones GetSection(s)

		#region Declaración de DLL para leer ficheros INI
		// Leer todas las secciones de un fichero INI, esto seguramente no funciona en Win95
		// Esta función no estaba en las declaraciones del API que se incluye con el VB
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int GetPrivateProfileSectionNames(
			string lpszReturnBuffer,  // address of return buffer
			int    nSize,             // size of return buffer
			string lpFileName         // address of initialization filename
		);

		// Leer una sección completa
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int GetPrivateProfileSection(
			string lpAppName,         // address of section name
			string lpReturnedString,  // address of return buffer
			int    nSize,             // size of return buffer
			string lpFileName         // address of initialization filename
		);

		// Leer una clave de un fichero INI
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int GetPrivateProfileString(
			string  lpAppName,        // points to section name
			string  lpKeyName,        // points to key name
			string  lpDefault,        // points to default string
			string  lpReturnedString, // points to destination buffer
			int     nSize,            // size of destination buffer
			string  lpFileName        // points to initialization filename
		);
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int GetPrivateProfileString(
			string  lpAppName,        // points to section name
			int     lpKeyName,        // points to key name
			string  lpDefault,        // points to default string
			string  lpReturnedString, // points to destination buffer
			int     nSize,            // size of destination buffer
			string  lpFileName        // points to initialization filename
		);

		// Escribir una clave de un fichero INI (también para borrar claves y secciones)
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int WritePrivateProfileString(
			string  lpAppName,  // pointer to section name
			string  lpKeyName,  // pointer to key name
			string  lpString,   // pointer to string to add
			string  lpFileName  // pointer to initialization filename
		);
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int WritePrivateProfileString(
			string  lpAppName,  // pointer to section name
			string  lpKeyName,  // pointer to key name
			int     lpString,   // pointer to string to add
			string  lpFileName  // pointer to initialization filename
		);
		[DllImport("Kernel32.dll", CharSet=CharSet.Auto)]
		public static extern int WritePrivateProfileString(
			string  lpAppName,  // pointer to section name
			int     lpKeyName,  // pointer to key name
			int     lpString,   // pointer to string to add
			string  lpFileName  // pointer to initialization filename
		);
		# endregion

		/// <summary>Constructor.</summary>
		public IniManager(string file)
		{
			this.file=file;
		}

		/// <summary>
		/// Borrar una clave o entrada de un fichero INI.
		/// Si no se indica sKey, se borrará la sección indicada en sSection.
		/// En otro caso, se supone que es la entrada (clave) lo que se quiere borrar.
		/// </summary>
		/// <remarks>Para borrar una sección se debería usar IniDeleteSection.</remarks>
		/// <param name="section"></param>
		/// <param name="key"></param>
		public void DeleteKey(string section, string key)
		{
			if( key == string.Empty ) WritePrivateProfileString(section, 0, 0, file);		// Borrar una sección
			else WritePrivateProfileString(section, key, 0, file);					// Borrar una entrada
		}  

		/// <summary>Borrar una sección de un fichero INI.</summary>
		/// <param name="section"></param>
		public void DeleteSection(string section)
		{
			WritePrivateProfileString(section, 0, 0, file);		// Borrar una sección
		}

		/// <summary>Devuelve el valor de una clave de un fichero INI.</summary>
		/// <param name="section">La sección de la que se quiere leer</param>
		/// <param name="key">Clave</param>
		/// <param name="defaultValuet">Valor opcional que devolverá si no se encuentra la clave</param>
		/// <returns>El valor leído</returns>
		public string ReadValue(string section, string key, string defaultValue)
		{
			int ret;
			string sRetVal;
			sRetVal = new string(' ', 255);

			ret = GetPrivateProfileString(section, key, defaultValue, sRetVal, sRetVal.Length, file);
			if( ret == 0 ) return defaultValue;
			else return sRetVal.Substring(0, ret);
		}  

		/// <summary>Escribe el valor de una clave de un fichero INI.</summary>
		/// <param name="section">La sección de la que se quiere escribir</param>
		/// <param name="key">Clave</param>
		/// <param name="value">Valor a guardar.</param>
		public void WriteValue(string section, string key, string value)
		{
			WritePrivateProfileString(section, key, value, file);
		}  

		/// <summary>Lee todas las entradas de una sección entera de un fichero INI.</summary>
		/// <param name="section">Nombre de la sección a leer</param>
		/// <returns>Esta función devolverá un array de índice cero con las claves y valores de la sección</returns>
		/// <remarks>
		/// Para leer los datos:
		///		For i = 0 To UBound(elArray) -1 Step 2
		///			sClave = elArray(i)
		///			 sValor = elArray(i+1)
		///		Next 
		///</remarks>
		public string[] ReadSection(string section)
		{
			string[] aSeccion;
			int n;

			aSeccion = new string[0];
			
			sBuffer = new string('\0', 32767);		// El tamaño máximo para Windows 95
			n = GetPrivateProfileSection(section, sBuffer, sBuffer.Length, file);
			if( n > 0 )
			{
				// Cortar la cadena al número de caracteres devueltos menos los dos últimos que indican el final de la cadena
				sBuffer = sBuffer.Substring(0, n - 2).TrimEnd();
				// Cada una de las entradas estará separada por un Chr$(0) y cada valor estará en la forma: clave = valor
				aSeccion = sBuffer.Split(new char[]{'\0', '='});
			}
			
			return aSeccion;		// Devolver el array
		}  

		/// <summary>Devuelve todas las secciones de un fichero INI.</summary>
		/// <returns> Un array con todos los nombres de las secciones.</returns>
		/// <remarks>La primera sección estará en el elemento 1, por tanto, si el array contiene cero elementos es que no hay secciones.</remarks>
		public string[] ReadSections()
		{
			int n;
			string[] aSections;

			aSections = new string[0];
			sBuffer = new string('\0', 32767);			// El tamaño máximo para Windows 95
			n = GetPrivateProfileSectionNames(sBuffer, sBuffer.Length, file);
			if( n > 0 )
			{
				sBuffer = sBuffer.Substring(0, n - 2).TrimEnd();		// Cortar la cadena al número de caracteres devueltos menos los dos últimos que indican el final de la cadena
				aSections = sBuffer.Split('\0');
			}
			
			return aSections;		// Devolver el array
		}  
	}
}