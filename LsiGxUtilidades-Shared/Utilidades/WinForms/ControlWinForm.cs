using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Artech.Genexus.Common.Parts.Form.DOM;
using Artech.Genexus.Common;
using Artech.Genexus.Common.CustomTypes;
using Artech.Genexus.Common.Types;

namespace LSI.Packages.Extensiones.Utilidades.WinForms
{
    /// <summary>
    /// Utilidades sobre controles en un winform
    /// </summary>
    public class ControlWinForm 
    {

        /// <summary>
        /// Compara posiciones de dos controles
        /// </summary>
        public class ComparadorPosicionesAbsolutas : IComparer<FormElement>
        {
            /// <summary>
            /// Compara las posiciones de dos controles dentro de un winform
            /// </summary>
            /// <returns>negativo si control1 aparece antes que control2. positivo si aparece despues. 
            /// 0 si estan en el mismo sitio</returns>
            public int Compare(FormElement control1, FormElement control2)
            {
                return ControlWinForm.Compare(control1, control2);
            }
        }

        /// <summary>
        /// Compara posiciones horizontales de dos controles
        /// </summary>
        public class ComparadorPosicionesHorizontales : IComparer<FormElement>
        {
            /// <summary>
            /// Compara las posiciones horizontales
            /// </summary>
            /// <returns>negativo si control1 aparece antes que control2. positivo si aparece despues. 
            /// 0 si estan en el mismo sitio</returns>
            public int Compare(FormElement control1, FormElement control2)
            {
                return ControlWinForm.CompareHorizontal(control1, control2);
            }
        }

        /// <summary>
        /// Devuelve la posicion de un control en el form
        /// </summary>
        static public Point PosicionControl(FormElement control)
        {
            // TODO: Ver como obtener un  Artech.Genexus.Common.Parts.Form.DOM.ElementPosition y usarlo
            return new Point((int)control.GetPropertyValue(Properties.FORMATT.Left),
                (int)control.GetPropertyValue(Properties.FORMATT.Top));
        }

        /// <summary>
        /// Devuelve el tamaño de un control en el form
        /// </summary>
        static public Size TamanyoControl(FormElement control, bool useMeasureString)
        {
            int width = (int)control.GetPropertyValue(Properties.FORMATT.Width);
            int height = (int)control.GetPropertyValue(Properties.FORMATT.Height);
            if (control.Type == RuntimeControlType.CTRL_TEXT && useMeasureString)
            {
                // TODO: Control size returns a wrong value for new labels.. I did something wrong?
                Font f = control.GetPropertyValue(Properties.FORMTEXT.Font) as Font;
                if (f != null)
                {
                    string caption = control.GetPropertyValue(Properties.FORMTEXT.Caption) as string;
                    if (caption != null)
                    {
                        Graphics g = Graphics.FromHwnd(IntPtr.Zero);
                        if (g != null)
                        {
                            SizeF sf = g.MeasureString(caption, f);
                            width = (int)sf.Width;
                            height = (int)sf.Height;
                        }
                    }
                }
            }
            return new Size(width, height);
        }

        /// <summary>
        /// Devuelve el rectangulo de un control en el form
        /// </summary>
        static public Rectangle RectanguloControl(FormElement control, bool useMeasureString)
        {
            return new Rectangle(PosicionControl(control), TamanyoControl(control, useMeasureString));
        }

        /// <summary>
        /// Compara las posiciones de dos controles dentro de un winform
        /// </summary>
        /// <returns>negativo si control1 aparece antes que control2. positivo si aparece despues. 
        /// 0 si estan en el mismo sitio</returns>
        static public int Compare(FormElement control1, FormElement control2)
        {
            Point posicion1 = PosicionControl(control1);
            Point posicion2 = PosicionControl(control2);
            // Comparar posicion vertical
            int dif = posicion1.Y - posicion2.Y;
            if (dif != 0)
                return dif;
            // Comparar posicion horizontal
            return posicion1.X - posicion2.X;
        }

        /// <summary>
        /// Compara las posiciones horizontales de dos controles
        /// </summary>
        /// <returns>negativo si control1 aparece antes que control2. positivo si aparece despues. 
        /// 0 si estan en el mismo sitio</returns>
        static public int CompareHorizontal(FormElement control1, FormElement control2) 
        {
            Point posicion1 = PosicionControl(control1);
            Point posicion2 = PosicionControl(control2);
            // Comparar posicion horizontal
            return posicion1.X - posicion2.X;
        }

        /// <summary>
        /// Ordena los controles de una lista por su posicion horizontal en el form
        /// </summary>
        /// <param name="lista">La lista a ordenar</param>
        static public void OrdenarHorizontalmente(List<FormElement> lista) {
            ComparadorPosicionesHorizontales comparador = new ComparadorPosicionesHorizontales();
            lista.Sort(comparador);
        }

        /// <summary>
        /// Devuelve cierto si el control es el boton de ayuda del form
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        static public bool EsBotonAyuda(FormElement control)
        {
            if (!(control is ButtonElement))
                return false;

            ButtonElement btn = control as ButtonElement;
            GxEventReference referenciaEvento = control.GetPropertyValue(Properties.FORMBTN.OnClickEvent) as GxEventReference;
            if (referenciaEvento == null)
                return false;

            string evento = referenciaEvento.Key.ToLower();
            return evento == "'ayuda'" || evento == "help";
        }

        /// <summary>
        /// Devuelve cierto si un rectangulo intersecta con algun control de una lista
        /// </summary>
        /// <param name="rec">Rectangulo a revisar si intersecta con algun control</param>
        /// <param name="controles">Lista de controles con los que comparar el rectangulo</param>
        /// <returns>Cierto si el rectangulo intersecta con alguno de los controles</returns>
        static public bool IntersectaConAlgunControl(Rectangle rec, List<FormElement> controles)
        {
            foreach (FormElement e in controles)
            {
                if (RectanguloControl(e, false).IntersectsWith(rec))
                    return true;
            }
            return false;
        }
    }
}
