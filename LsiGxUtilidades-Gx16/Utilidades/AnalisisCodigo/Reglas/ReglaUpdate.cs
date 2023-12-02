//using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas
{
    /// <summary>
    /// Utilidades de la regla update
    /// </summary>
    public class ReglaUpdate
    {

        /// <summary>
        /// Verifica si en el codigo existe una regla update para un cierto atributo
        /// </summary>
        /// <param name="codigoReglas">Codigo de las reglas</param>
        /// <param name="atributo">Atributo del que ver si hay una regla update</param>
        /// <returns>Cierto si contiene una regla update para el atributo</returns>
        static public bool ContieneUpdateAtributo(ParsedCode codigoReglas, Attribute atributo)
        {
            string nombreAtributo = atributo.Name.ToLower();

            BuscadorReglas buscador = new BuscadorReglas(BuscadorReglas.REGLAUPDATE);
            foreach( Rule regla in buscador.BuscarTodas(codigoReglas) ) 
            {
                foreach (ObjectBase parametro in regla.Parameters)
                {
                    if (parametro is AttributeName && ((AttributeName)parametro).Text.ToLower() ==
                        nombreAtributo)
                        return true;
                }
            }
            return false;
        }

    }
}
