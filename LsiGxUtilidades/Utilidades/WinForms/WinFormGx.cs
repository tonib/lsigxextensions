using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Helpers;
using System.Xml;
using System.IO;
using System.Runtime.Serialization;
using System.Drawing;
using Artech.Common.Framework.Objects;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common.Parts.Form;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Common.Properties;

namespace LSI.Packages.Extensiones.Utilidades.WinForms
{
    /// <summary>
    /// Operaciones sobre un winform de genexus
    /// </summary>
    public class WinFormGx
    {

        /// <summary>
        /// El winform a gestionar
        /// </summary>
        public WinFormPart ParteWinForm;

        /// <summary>
        /// El objeto a gestionar
        /// </summary>
        public KBObject Objeto;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objeto">Transaccion o workpanel con el que se va a operar
        /// </param>
        public WinFormGx(KBObject objeto)
        {
            // Obtener la parte del winform:
            if (objeto is Transaction)
                ParteWinForm = ((Transaction)objeto).WinForm;
            else if (objeto is WorkPanel)
                ParteWinForm = ((WorkPanel)objeto).WinForm;
            else
                throw new Exception("El objeto " + objeto.Name + " no es un workpanel ni una transaccion");

            this.Objeto = objeto;
        }

        static public bool IsWinform(KBObject o) {
            return o is WorkPanel || o is Transaction;
        }

        /// <summary>
        /// Busca en el boton que ejecuta un cierto evento
        /// </summary>
        /// <param name="evento">El evento a buscar</param>
        /// <param name="soloPrimerNivel">Cierto si hay que buscar solo controles en el primer nivel el form</param>
        /// <returns>El primer boton que ejecuta el evento. null si no se ha encontrado</returns>
        public ButtonElement BotonConEvento(string evento, bool soloPrimerNivel)
        {
            evento = evento.ToLower();

            foreach (FormElement e in EnumeradorWinform.EnumerarControles(ParteWinForm, !soloPrimerNivel))
            {
                ButtonElement btn = e as ButtonElement;
                if (btn == null)
                    continue;
                GxEventReference referenciaEvento = btn.GetPropertyValue(Properties.FORMBTN.OnClickEvent) as GxEventReference;
                if (referenciaEvento != null && referenciaEvento.Key.ToLower() == evento)
                    return btn;
            }
            return null;
        }

        /// <summary>
        /// Busca un boton con un cierto nombre de control
        /// </summary>
        /// <param name="nombreBoton">El nombre de control del boton a buscar</param>
        /// <param name="soloPrimerNivel">Cierto si hay que buscar solo controles en el primer nivel el form</param>
        /// <returns>El boton con el nombre de control dado. null si no se ha encontrado</returns>
        public ButtonElement BotonConNombre(string nombreBoton, bool soloPrimerNivel)
        {
            foreach (FormElement e in EnumeradorWinform.EnumerarControles(ParteWinForm, !soloPrimerNivel))
            {
                ButtonElement btn = e as ButtonElement;
                if (btn == null)
                    continue;
                if (btn.Name == nombreBoton)
                    return btn;
            }
            return null;
        }

        /// <summary>
        /// El tamaño del form
        /// </summary>
        public Size TamanyoForm
        {
            get
            {
                // On Gx15 this throws a NullReferenceException
                try
                {
                    FormContainer frm = ParteWinForm.MyDocument.DefaultForm;
                    return new Size((int)frm.GetPropertyValue(Properties.FORMWND.Width),
                        (int)frm.GetPropertyValue(Properties.FORMWND.Height));
                }
                catch
                {
                    return new Size(0, 0);
                }
            }
            set
            {
                FormContainer frm = ParteWinForm.MyDocument.DefaultForm;
                frm.SetPropertyValue(Properties.FORMWND.Width, value.Width);
                frm.SetPropertyValue(Properties.FORMWND.Height, value.Height);
                ParteWinForm.Dirty = true;
            }
        }

        /// <summary>
        /// Busca un grid en el winform
        /// </summary>
        /// <returns>El primer grid encontrado en el winform. null si no se encontro ninguno</returns>
        public GridElement BuscarPrimerGrid()
        {
            List<GridElement> result = new List<GridElement>();
            foreach (FormElement e in EnumeradorWinform.EnumerarControles(ParteWinForm))
            {
                GridElement g = e as GridElement;
                if (g != null)
                    return g;
            }
            return null;
        }

        /// <summary>
        /// Get the grids on the form and their positions
        /// </summary>
        /// <remarks>
        /// Those positions are used to generate name of the source file for the grid
        /// </remarks>
        /// <returns>The grids and their positions</returns>
        public Dictionary<GridElement, int> GetGridPositions()
        {
            Dictionary<GridElement, int> result = new Dictionary<GridElement, int>();
            int currentId = 1;  // 1 is for the form
            foreach (FormElement e in EnumeradorWinform.EnumerarControles(ParteWinForm))
            {
                currentId++;
                GridElement g = e as GridElement;
                if (g != null)
                    result.Add(g, currentId);
            }
            return result;
        }

        /// <summary>
        /// Devuelve una lista de los grids en el winform
        /// </summary>
        public List<GridElement> BuscarGrids()
        {
            return GetGridPositions().Keys.ToList();
        }

        public Rectangle ControlsRegion()
        {
            Rectangle region = new Rectangle();
            bool first = true;
            foreach (FormElement e in EnumeradorWinform.EnumerarControles(ParteWinForm, false))
            {
                Rectangle r = ControlWinForm.RectanguloControl(e, true);
                if (first)
                {
                    first = false;
                    region = r;
                }
                else
                    region = Rectangle.Union(region, r);
            }
            return region;
        }

    }
}
