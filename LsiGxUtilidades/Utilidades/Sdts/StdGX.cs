using System;
using System.Collections.Generic;
using System.Text;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.SDT;


namespace LSI.Packages.Extensiones.Utilidades.Sdts
{

    /// <summary>
    /// Utilidadades para gestionar sdts de genexus
    /// </summary>
    public class StdGX
    {

        /// <summary>
        /// El sdt gestionado
        /// </summary>
        public SDT Sdt;

        /// <summary>
        /// Crea un sdt copiando la estructura de una tabla
        /// </summary>
        /// <param name="tabla">Tabla de la que copiar la estructura</param>
        public StdGX(Table tabla)
        {

            // Crear el sdt
            Sdt = SDT.Create(UIServices.KB.CurrentModel);
            Sdt.Name = KBaseGX.GetUnusedName(KBaseGX.NAMESPACE_OBJECTS, tabla.Name + "Sdt");

            Sdt.Description = tabla.Description;

            // Añadir los atributos de la tabla.
            foreach (TableAttribute atr in tabla.TableStructure.Attributes)
            {
                SDTItem item = new SDTItem(Sdt.SDTStructure);
                item.Name = atr.Name;
                item.Description = atr.Attribute.Description;
                item.AttributeBasedOn = atr;
                Sdt.SDTStructure.Root.AddItem(item);
            }
        }

        /// <summary>
        /// Guarda el sdt y lo abre en el editor
        /// </summary>
        virtual public void GuardarYAbrir()
        {
            // Poner el codigo del objeto y guardarlo:
            Sdt.Save();
            UIServices.Objects.Open(Sdt, OpenDocumentOptions.CurrentVersion);
        }

        /// <summary>
        /// Busca un campo en la raiz del sdt
        /// </summary>
        /// <param name="sdt">sdt en el que buscar</param>
        /// <param name="campo">Campo a buscar</param>
        /// <returns>Cierto si el sdt contiene el campo</returns>
        static public bool ContieneCampo(SDT sdt, string campo)
        {
            string campoMinusculas = campo.ToLower();
            foreach (SDTItem item in sdt.SDTStructure.Root.Items)
            {
                if (item.Name.ToLower() == campoMinusculas)
                    return true;
            }
            return false;
        }

    }
}
