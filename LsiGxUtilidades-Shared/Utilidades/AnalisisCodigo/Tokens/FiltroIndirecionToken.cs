using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common.CustomTypes;
using Artech.Architecture.Language.Parser;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens
{
    /// <summary>
    /// Informacion para filtrar la indireccion de acceso a un token.
    /// </summary>
    /// <remarks>
    /// Por ejemplo si se quieren buscar solo referencias a una funcion miembro de un 
    /// atributo/variable (p.ej. "&amp;x.ToString()") o a un campo de un SDT 
    /// (p.ej. "&amp;x.campo")
    /// </remarks>
    public class TokenIndirectionFilter
    {

        /// <summary>
        /// Valor para indicar que el filtro de la indireccion puede estar en cualquier lugar
        /// </summary>
        public const int CUALQUIERPOSICION = -1;

        /// <summary>
        /// Tipo de indireccion por el que filtrar: una funcion o miembro
        /// </summary>
        public KindOfIndirection Kind = KindOfIndirection.FIELD;

        /// <summary>
        /// Lista de los nombres del campo/funcion de indireccion por los que filtrar. 
        /// Si el miembro es alguno de la lista, se pasa el filtro.
        /// DEBEN ESTAR EN MINUSCULAS
        /// </summary>
        public List<string> MembersFilter = new List<string>();

        /// <summary>
        /// Indice de la posicion del miembro en la indireccion. 
        /// Si vale CUALQUIERPOSICION, se buscara en cualquier posicion
        /// </summary>
        public int PosicionFiltro = 1;

        private bool MiembroCumpleFiltro(ObjectBase miembro)
        {
            // Ver si la indireccion base cumple con el filtro de indireccion:
            string nombreMiembro;
            if (Kind == KindOfIndirection.FIELD)
            {
                if (!(miembro is Property))
                    return false;
                nombreMiembro = ((Property)miembro).Text;
            }
            else
            {
                // Buscamos una funcion:
                if (!(miembro is Function))
                    return false;
                nombreMiembro = ((Function)miembro).Name.ToString();
            }

            // Ver si cumple con el nombre:
            nombreMiembro = nombreMiembro.ToLower().Trim();
            return MembersFilter.Contains(nombreMiembro);
        }

        public bool CumpleConFiltro(ObjectPEM expresion)
        {

            List<ObjectBase> indirecciones = ObjectPEMGx.ConvertirALista(expresion);
            if (PosicionFiltro == CUALQUIERPOSICION)
            {
                // Revisar las indirecciones, excepto la base
                for (int i = 1; i < indirecciones.Count; i++)
                {
                    if( MiembroCumpleFiltro( indirecciones[i] ) )
                        return true;
                }
                return false;
            }
            else 
            {
                // Revisar la indireccion de la posicion indicada:
                if (PosicionFiltro >= indirecciones.Count)
                    return false;
                else
                    return MiembroCumpleFiltro(indirecciones[PosicionFiltro]);
            }
        }

        public bool CumpleConFiltro(SDTLevelType refSdt)
        {
            // Convertir la referencia a SDT en un texto, parsearlo y verificarlo
            string expresion = refSdt.ToString();
            ObjectPEM arbolExpresion = ParserGx.ParsearExpresion(expresion) as ObjectPEM;
            if (arbolExpresion != null)
                return CumpleConFiltro(arbolExpresion);
            else
                return false;
        }
    }
}
