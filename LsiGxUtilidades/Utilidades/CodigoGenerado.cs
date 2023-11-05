using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;

namespace LSI.Packages.Extensiones.Utilidades
{

    /// <summary>
    /// Utilidad para marcar y buscar codigo generado por extensiones
    /// </summary>
    public class CodigoGenerado
    {

        /// <summary>
        /// El texto que identifica las regiones de codigo generado
        /// </summary>
        private string Marca;

        /// <summary>
        /// Cache de la expresion regular para buscar regiones de codigo generadas.
        /// No se inicializa hasta que no se usa.
        /// </summary>
        private Regex _ExpRegionGenerada;

        /// <summary>
        /// La expresion regular para buscar regiones de codigo generadas.
        /// </summary>
        private Regex ExpRegionGenerada
        {
            get
            {
                if (_ExpRegionGenerada == null)
                {
                    // crear la expresion regular
                    // Cualquier texto:
                    string contenido = @"(.|\n)*";
                    _ExpRegionGenerada = new Regex(MarcaInicial + contenido + MarcaFinal, RegexOptions.Multiline);
                }
                return _ExpRegionGenerada;
            }
        }

        /// <summary>
        /// Parte inicial del comentario que marca el inicio de la region de codigo generada
        /// </summary>
        public string MarcaInicial { get { return "//@" + Marca + "-INICIO"; } }

        /// <summary>
        /// El comentario final que marca el fin de la region de codigo generada
        /// </summary>
        public string MarcaFinal { get { return "//@" + Marca + "-FIN"; } }

        /// <summary>
        /// Pone una marca de codigo generado al principio y al final de un codigo
        /// </summary>
        /// <param name="codigo">El codigo a marcar como generado</param>
        /// <returns>El codigo original con las marcas de generado al principio y al final</returns>
        public string MarcarCodigoGenerado(string codigo)
        {
            codigo = MarcaInicial + " - Código generado. No modificar" + Environment.NewLine +
                codigo;
            if (!codigo.EndsWith("\n") && !codigo.EndsWith(Environment.NewLine))
                codigo += Environment.NewLine;
            codigo += MarcaFinal + Environment.NewLine;
            return codigo;
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="marca">Identificador de la parte de codigo generado</param>
        public CodigoGenerado(string marca)  
        {
            Marca = marca;
        }

        /// <summary>
        /// Devuelve cierto si el codigo contiene codigo generado
        /// </summary>
        public bool ContieneMarca(string codigo)
        {
            return codigo.Contains(MarcaInicial);
        }

        /// <summary>
        /// Borra todo el codigo generado
        /// </summary>
        /// <param name="codigo">El codigo del que borrar la parte generada</param>
        /// <returns>El codigo con la parte generada borrada</returns>
        public string BorrarCodigoGenerado(string codigo)
        {
            return ExpRegionGenerada.Replace(codigo, "");
        }
    }
}
