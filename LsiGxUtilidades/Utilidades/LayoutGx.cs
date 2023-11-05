using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Layout;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Utilidades sobre la parte layout de un procedure de Genexus
    /// </summary>
    public class LayoutGx
    {

        /// <summary>
        /// Devuelve un enumerador de todos controles contenidos en todos los printbocks de un layout.
        /// </summary>
        /// <param name="layout">Layout del que enumerar los componentes</param>
        /// <returns>El enumerador de los componentes del layout</returns>
        static public IEnumerable<IReportComponent> EnumerarComponentes(LayoutPart layout)
        {
            // Buscar en cada uno de los printblocks
            foreach (IReportBand printBlock in layout.Layout.ReportBands)
            {
                foreach (IReportComponent control in printBlock.Controls)
                    yield return control;
            }
        }

        /// <summary>
        /// Devuelve un enumerador de todos controles contenidos en todos los printbocks de un layout
        /// que sean de un cierto tipo.
        /// </summary>
        /// <param name="layout">Layout del que enumerar los componentes</param>
        /// <returns>El enumerador de los componentes del layout del tipo indicado</returns>
        static public IEnumerable<Tipo> EnumerarComponentes<Tipo>(LayoutPart layout) where Tipo : IReportComponent
        {
            if (layout == null)
                yield break;

            // Buscar en cada uno de los printblocks
            foreach (IReportBand printBlock in layout.Layout.ReportBands)
            {
                foreach( Tipo control in EnumerarComponentes<Tipo>(printBlock) )
                    yield return (Tipo)control;
            }
        }

        /// <summary>
        /// Devuelve un enumerador de todos controles contenidos en un printblock de un layout.
        /// </summary>
        /// <param name="printBlock">Printblock del que enumerar los componentes</param>
        /// <returns>El enumerador de los componentes del printblock del tipo indicado</returns>
        static public IEnumerable<Tipo> EnumerarComponentes<Tipo>(IReportBand printBlock) where Tipo : IReportComponent
        {
            foreach (IReportComponent control in printBlock.Controls)
            {
                if (control is Tipo)
                    yield return (Tipo)control;
            }
        }

        /// <summary>
        /// Check if a report band is empty
        /// </summary>
        /// <param name="band">Band to check</param>
        /// <returns>true if the band is empty</returns>
        static public bool IsEmpty(IReportBand band)
        {
            foreach (IReportComponent control in band.Controls)
                return true;
            return false;
        }

        /// <summary>
        /// Check if a layout contains only the default report band created with the object.
        /// </summary>
        /// <param name="layout">Layout to check</param>
        /// <returns>True if the layout contains only the default empty report band</returns>
        static public bool OnlyDefaultBand(ReportLayout layout)
        {
            if (layout.BandsCount == 0 )
                // Default band removed
                return true;

            if (layout.BandsCount > 1)
                // there are manually created bands
                return false;

            // There is only one band
            foreach (IReportBand band in layout.ReportBands)
                return ! IsEmpty(band);
            return true;
        }

        /// <summary>
        /// Get names of layout printblocks
        /// </summary>
        /// <param name="layout">Layout to check</param>
        /// <returns>Print block names, lowercase</returns>
        static public HashSet<string> BandNames(ReportLayout layout)
        {
            HashSet<string> names = new HashSet<string>();
            foreach (IReportBand printBlock in layout.ReportBands)
                names.Add(printBlock.Name.ToLower());
            return names;
        }

    }
}
