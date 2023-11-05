using System;
using System.Collections.Generic;
using System.Text;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using System.Globalization;

namespace LSI.Packages.Extensiones.Utilidades.CodeGeneration
{
    /// <summary>
    /// NEW statement generator
    /// </summary>
    public class NewGenerator
    {

        /// <summary>
        /// Codigo interno del new
        /// </summary>
        public string CodigoInterno = String.Empty;

        /// <summary>
        /// Codigo interno de la clausula when duplicate.
        /// Si esta vacio, no se genera
        /// </summary>
        public string CodigoWhenDuplicate = String.Empty;

        /// <summary>
        /// Devuelve el texto de la sentencia new
        /// </summary>
        public override string ToString()
        {
            string txtNew = "NEW\n" +
                ForEachGenerator.IndentarCodigo(CodigoInterno);
            if (!string.IsNullOrEmpty(CodigoWhenDuplicate))
                txtNew += "WHEN DUPLICATE\n" + 
                    ForEachGenerator.IndentarCodigo(CodigoWhenDuplicate);
            txtNew += "ENDNEW\n";

            return Entorno.StringFormatoKbase( txtNew );
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public NewGenerator() { }

        /// <summary>
        /// Crea una sentencia new para asignar todos los campos de una tabla
        /// </summary>
        /// <param name="tabla"></param>
        public NewGenerator(Table tabla)
        {
            // Asignacion de los campos de la tabla
            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {
                if( !atr.IsFormula )
                    CodigoInterno += AssignmentsGenerator.AsignacionAtributo(atr.Name) + "\n";
            }
        }

    }
}
