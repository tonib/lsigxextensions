using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Common.Framework.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Form.DOM;

namespace LSI.Packages.Extensiones.Utilidades.WinForms
{

    /// <summary>
    /// Utilidad para enumerar controles de un winform
    /// TODO: Ver si se puede reemplazar la clase BuscadorWinform por estos enumeradores (son mas sencillos)
    /// </summary>
    public class EnumeradorWinform
    {
        /// <summary>
        /// Devuelve un enumerador de todos controles contenidos en el control raiz. Se hace
        /// una busqueda recursiva
        /// </summary>
        /// <param name="raiz">Nodo del que revisar los controles descendientes. Este nodo
        /// no es enumerado</param>
        /// <returns>El eneumerador de los controles descendientes de raiz</returns>
        static private IEnumerable<FormElement> EnumerarControles(IControl raiz, bool recursive)
        {
            if(raiz == null)
            {
                yield break;
            }

            // Revisar los hijos de este contenedor:
            foreach (IControl ctl in raiz.Children)
            {
                if( ctl is FormElement)
                    yield return ((FormElement)ctl);

                if (recursive)
                {
                    foreach (FormElement hijo in EnumerarControles(ctl, recursive))
                        yield return hijo;
                }
            }
        }

        /// <summary>
        /// Devuelve un enumerador de todos controles contenidos en el control raiz.
        /// </summary>
        /// <param name="winform">Winform to check</param>
        /// <param name="recursive">True if we must to do a recursive search. False to
        /// check only the first level</param>
        /// <returns>El eneumerador de los controles descendientes de raiz</returns>
        static public IEnumerable<FormElement> EnumerarControles(WinFormPart winform, bool recursive=true)
        {
            if(winform == null)
            {
                return Enumerable.Empty<FormElement>();
            }

            return EnumerarControles(winform.MyDocument.DefaultForm, recursive);
        }

    }
}
