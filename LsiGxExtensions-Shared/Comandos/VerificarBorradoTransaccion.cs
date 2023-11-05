using System;
using System.Threading;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Threading;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Verifica si el borrado de una transaccion reorganizara la base de datos
    /// </summary>
    public class VerificarBorradoTransaccion : IExecutable
    {

        /// <summary>
        /// La transaccion a revisar
        /// </summary>
        private Transaction Transaccion;

        /// <summary>
        /// El log del proceso
        /// </summary>
        private Log Salida;

        /// <summary>
        /// El modelo actual
        /// </summary>
        private KBModel Modelo;

        /// <summary>
        /// Cierto si se va a reorganizar la bbdd por el borrado de la transaccion
        /// </summary>
        private bool SeReorganizara;

        /// <summary>
        /// Constructor para verificar la transaccion actualmente seleccionada
        /// </summary>
        public VerificarBorradoTransaccion()
        {
            KBObject objetoSeleccionado = UIServices.Environment.ActiveDocument.Object;
            if (!(objetoSeleccionado is Transaction))
                throw new Exception("Current object is not a transaction");
            Transaccion = objetoSeleccionado as Transaction;
            Modelo = UIServices.KB.CurrentModel;
        }

        private bool ContieneReferenciaAtributo(Transaction transaccion, Table tabla, 
            Artech.Genexus.Common.Objects.Attribute atributo)
        {
            // Revisar cada nivel de la transaccion:
            foreach (TransactionLevel nivel in transaccion.Structure.GetLevels())
            {
                // Buscar solo niveles que referencien a la tabla
                if (nivel.AssociatedTable.Name.ToLower() != tabla.Name.ToLower())
                    continue;

                // Revisar atributos del nivel (y niveles superiores)
                foreach (TransactionAttribute atributoTrn in nivel.FullLevelAttributes)
                {
                    if (atributoTrn.Name.ToLower() == atributo.Name.ToLower())
                        // La transaccion contiene la referencia a la tabla/atributo
                        return true;
                }

                //if (nivel.GetAttribute(atributo.Name) != null) < no tiene en cuenta atributos de niveles superiores
                //    // La transaccion contiene la referencia a la tabla/atributo
                //    return true;
            }
            return false;
        }

        private void RevisarAtributo(Table tabla, TransactionAttribute atributo)
        {
            if (atributo.IsInferred)
                // Si es inferido el borrado no le afecta
                return;
            
            // Ver en que otras transacciones aparece el atributo
            foreach (EntityReference referencia in atributo.Attribute.GetReferencesTo())
            {
                if (referencia.From.Type == ObjClass.Transaction)
                {
                    Transaction txReferenciadora = Transaction.Get( Modelo, referencia.From ) as Transaction;
                    if (txReferenciadora == null)
                        continue;

                    if (txReferenciadora == Transaccion)
                        // Ignorar la transaccion que se esta revisando
                        continue;

                    // Ver si la transaccion contiene una referencia al atributo en la misma tabla
                    if (ContieneReferenciaAtributo(txReferenciadora, tabla, atributo.Attribute))
                        // Si es asi, el borrado de la transaccion no le afecta
                        return;
                }
            }

            // La tabla/atributo no es referenciado en ninguna otra transaccion: Se va a reorganizar.
            Salida.Output.AddWarningLine("Attribute " + atributo.Name + " will be removed from " +
                tabla.Name + " table");
            SeReorganizara = true;
        }

        /// <summary>
        /// Revisa la transaccion y muestra mensajes en la ventana de log sobre si es seguro
        /// borrar la transaccion
        /// </summary>
        private void RevisarTransaccion()
        {

            using (Salida = new Log())
            {
                Salida.Output.AddLine("Checking attributes of " + Transaccion.QualifiedName + " transaction...");
                SeReorganizara = false;
                // Revisar cada nivel de la transaccion:
                foreach (TransactionLevel nivel in Transaccion.Structure.GetLevels())
                {
                    // Revisar atributos del nivel
                    foreach (TransactionAttribute atributo in nivel.Attributes)
                        RevisarAtributo(nivel.AssociatedTable, atributo);
                }

                if (SeReorganizara)
                {
                    Salida.Output.AddWarningLine("Database will be reorganized if transaction is removed");
                    Salida.ProcesoOk = false;
                }
                else
                    Salida.Output.AddLine("Transaction removal will not reorganize database");
            }
        }
        
        /// <summary>
        /// Lanza un thread para ejecutar la revision
        /// </summary>
        public void Execute()
        {
            Thread t = new Thread(new ThreadStart(this.RevisarTransaccion));
            t.Start();
        }
    }
}
