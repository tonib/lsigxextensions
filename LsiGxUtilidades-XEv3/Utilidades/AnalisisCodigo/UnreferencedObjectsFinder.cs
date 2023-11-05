using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.GXplorer.Common.Objects;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.UI;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Tool to search unreferenced objects on the kbase
    /// </summary>
    /// <remarks>
    /// Searched objects:
    /// - Unreferenced objects (ICallable, except transactions)
    /// - Attributes with no table
    /// - Attributes only referenced by transactions
    /// </remarks>
    public class UnreferencedObjectsFinder : UISearchBase
    {
        /// <summary>
        /// Kind of forms to check attribute references from transactions
        /// </summary>
        public enum FormsToCheckEnum { WIN, WEB, BOTH };

        /// <summary>
        /// If a ICallable is inside some of those folders / modules, it will be ignored.
        /// </summary>
        private HashSet<IKBObjectParent> FoldersOrModulesToIgnore = new HashSet<IKBObjectParent>();

        /// <summary>
        /// Should we search unreferenced ICallableObjects (procedures, workpanels, etc)?
        /// </summary>
        public bool CheckUnreferencedCallables = true;

        /// <summary>
        /// Should we search attributes with no owner table?
        /// </summary>
        public bool CheckAttributesWithNoTable = true;

        /// <summary>
        /// Should we seach attributes only in transactions?
        /// </summary>
        public bool CheckAttributesOnlyTrn = true;

        /// <summary>
        /// Should we search attributes with read only references?
        /// </summary>
        public bool CheckReadOnlyAttributes = true;

        /// <summary>
        /// Kind of forms to check attribute references from transactions
        /// </summary>
        public FormsToCheckEnum FormsToCheck = FormsToCheckEnum.BOTH;

        /// <summary>
        /// Ignore objects where the last modifier object was genexus?
        /// </summary>
        public bool IgnoreGenexusUser = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="foldersToIgnore">List of folders or modules to ignore</param>
        public UnreferencedObjectsFinder(List<IKBObjectParent> foldersToIgnore)
        {
            foldersToIgnore.ForEach(x => ExpandFoldersOrModulesToIgnore(x));
        }

        /// <summary>
        /// Search recursivelly folders to ignore
        /// </summary>
        /// <param name="folderOrModule">Folder to ignore</param>
        private void ExpandFoldersOrModulesToIgnore(IKBObjectParent folderOrModule)
        {
            if (FoldersOrModulesToIgnore.Contains(folderOrModule))
                return;
            FoldersOrModulesToIgnore.Add(folderOrModule);

            foreach (KBObject child in folderOrModule.Objects)
            {
                IKBObjectParent subFolder = child as IKBObjectParent;
                if(subFolder != null )
                    ExpandFoldersOrModulesToIgnore(subFolder);
            }
                
        }

        /// <summary>
        /// Check if the object is inside a folder to ignore
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        private bool ObjectInFolderOrModuleToIgnore(KBObject o)
        {
            IKBObjectParent f = o.Parent as IKBObjectParent;
            if (f == null)
                return false;

            return FoldersOrModulesToIgnore.Contains(f);
        }

        /// <summary>
        /// Checks if a callable object is called
        /// </summary>
        /// <param name="o">Object to check</param>
        private void CheckCallableObject(KBObject o)
        {
            if (!CheckUnreferencedCallables)
                return;

            // Ignore transactions: They define the database structure:
            // TODO: If the transaction has duplicated structure and it's not called, 
            // TODO: it could be deleted
            if (o is Transaction)
                return;

            if (KBaseGX.EsMain(o))
                // Ignore main objects
                return;

            // Ignore Messages SDT (it's standard)
            SDT sdt = o as SDT;
            if (sdt != null && sdt.Name.ToLower() == "messages")
                return;

            if (!o.GetReferencesTo(LinkType.UsedObject).Any())
                PublishUIResult(new RefObjetoGX(o));
        }

        /// <summary>
        /// Check if a transaction has internal hard references to an attribute
        /// </summary>
        /// <param name="tx">Transaction to check</param>
        /// <param name="a">Attribute to check</param>
        /// <returns>True if transaction has references to the attribute.</returns>
        private bool TxHasInternalReferences(Transaction tx, Artech.Genexus.Common.Objects.Attribute a)
        {
            TokenGx tokenAtr = new TokenGx(a.Name);
            TokensFinder atrFinder = new TokensFinder(tokenAtr);

            if (atrFinder.ContieneReferencia(new ParsedCode(tx.Rules)))
                return true;
            if (atrFinder.ContieneReferencia(new ParsedCode(tx.Events)))
                return true;
            if ((FormsToCheck == FormsToCheckEnum.WIN || FormsToCheck == FormsToCheckEnum.BOTH) &&
                atrFinder.ContieneReferencia(tx.WinForm, false))
                return true;
            if ((FormsToCheck == FormsToCheckEnum.WEB || FormsToCheck == FormsToCheckEnum.BOTH) &&
                atrFinder.ContieneReferencia(tx.WebForm, false))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a transaction reference to an attribute is useful
        /// </summary>
        /// <param name="tx">Transaction to check</param>
        /// <param name="a">Attribute to check</param>
        /// <returns>True if the reference is useful</returns>
        private bool TxReferenceToAtrIsUseful(Transaction tx, Artech.Genexus.Common.Objects.Attribute a)
        {
            if (tx == null)
                return false;

            if (!tx.GetReferencesTo(LinkType.UsedObject).Any() && !KBaseGX.EsMain(tx))
                // Transaction is not called: Not useful
                return false;

            TransactionAttribute ta = tx.Structure.GetAttribute(a);
            if (ta == null || ta.IsInferred)
                // Transaction structure does not store attribute
                return false;

            if (KBaseGX.EsBussinessComponent(tx))
                // TODO: We should check if the atribute is used on BC references. Pending
                return true;

            // Check if attribute is referenced internally by the transaccion (on forms, rules or events)
            if (TxHasInternalReferences(tx, a))
                return true;

            return false;
        }

        /// <summary>
        /// Checks if a transactions is only referenced from transactions structure
        /// </summary>
        /// <param name="a">Attribute to check</param>
        /// <param name="references">References to attribute</param>
        /// <returns>Return true if the attribute has been reported as suspicious</returns>
        private bool CheckAtributeOnlyInTransactions(Artech.Genexus.Common.Objects.Attribute a,
            IEnumerable<EntityReference> references)
        {
            if (!CheckAttributesOnlyTrn)
                return false;

            if ( !( references.Any() &&
                    references.All(x => x.From.Type == ObjClass.Transaction || x.From.Type == ObjClass.Table)
                  ) )
                // Referenced outside transactions
                return false;

            // Check each transaction. If none use the attribute, the attribute is suspicious
            foreach( EntityReference rTransaction in references.Where( x => x.From.Type == ObjClass.Transaction ) ) 
            {
                Transaction tx = Transaction.Get(UIServices.KB.CurrentModel, rTransaction.From) as Transaction;
                if (TxReferenceToAtrIsUseful(tx, a))
                    return false;
            }

            // Suspicious attribute
            PublishUIResult(new RefObjetoGX(a));
            return true;
        }

        /// <summary>
        /// Check if a transaction references to an attribute are all readings
        /// </summary>
        /// <param name="txReference">Transaction to check</param>
        /// <param name="a">Attribute to check</param>
        /// <returns>True if all refereces are readings</returns>
        private bool TxHasOnlyReadReferences(EntityReference txReference, Artech.Genexus.Common.Objects.Attribute a)
        {
            Transaction tx = Transaction.Get(UIServices.KB.CurrentModel, txReference.From) as Transaction;
            if (tx == null)
                return true;

            TransactionAttribute ta = tx.Structure.GetAttribute(a);
            if (ta == null)
            {
                // The attribute is not stored by the transaction.
                return true;
            }
            if (ta.IsInferred)
            {
                // Inferred attribute. Check if there is an update rule:
                if (!ReglaUpdate.ContieneUpdateAtributo(new ParsedCode(tx.Rules), a))
                    return true;
            }

            if (!tx.GetReferencesTo(LinkType.UsedObject).Any() && !KBaseGX.EsMain(tx) )
                // The transaction is not called:
                return true;

            if (!TxHasInternalReferences(tx, a))
                // The transaction references the attribute only on the structure
                return true;

            return false;
        }

        /// <summary>
        /// Check if a procedure references to an attribute are all readings
        /// </summary>
        /// <param name="txReference">Transaction to check</param>
        /// <param name="a">Attribute to check</param>
        /// <returns>True if all refereces are readings</returns>
        private bool ProcedureHasOnlyReadReferences(EntityReference txReference, Artech.Genexus.Common.Objects.Attribute a)
        {
            Procedure p = Procedure.Get(UIServices.KB.CurrentModel, txReference.From) as Procedure;
            if (p == null)
                return true;

            BuscadorAsignaciones asignmentsFinder = new BuscadorAsignaciones(new TokenGx(a.Name), false, true);
            return !asignmentsFinder.ContieneAsignacion(new ParsedCode(p.ProcedurePart));
        }

        /// <summary>
        /// Check if an attribute has only read references
        /// </summary>
        /// <param name="a">Attribute to check</param>
        /// <param name="references">References to attribute</param>
        private void CheckIsReadOnlyAttribute(Artech.Genexus.Common.Objects.Attribute a,
            IEnumerable<EntityReference> references)
        {
            if (!CheckReadOnlyAttributes)
                return;

            if (a.Formula != null)
                // Ignore formulas
                return;

            // Check references: Only transactions, work with devices and procedures can write attributes
            if(references.Any(r => r.From.Type == ObjClass.WorkWithDevices))
                // TODO: Not supported. Ignore the attribute
                return;

            foreach (EntityReference r in references.Where(x => x.From.Type == ObjClass.Transaction) )
            {
                if (!TxHasOnlyReadReferences(r, a))
                    return;
            }

            foreach (EntityReference r in references.Where(x => x.From.Type == ObjClass.Procedure) )
            {
                if (!ProcedureHasOnlyReadReferences(r, a))
                    return;
            }

            // Report suspicious attribute
            PublishUIResult(new RefObjetoGX(a));
        }

        /// <summary>
        /// Checks if an attribute is not at any table
        /// </summary>
        /// <param name="a">Attribute to check</param>
        private void CheckAttributes(Artech.Genexus.Common.Objects.Attribute a)
        {
            IEnumerable<EntityReference> references = a.GetReferencesTo(LinkType.UsedObject);

            if (CheckAttributesWithNoTable)
            {
                if (!references
                    .Any(x => (x.From.Type == ObjClass.Table || x.From.Type == ObjClass.DataView)))
                {
                    PublishUIResult(new RefObjetoGX(a));
                    return;
                }
            }

            if (CheckAtributeOnlyInTransactions(a, references))
                return;

            CheckIsReadOnlyAttribute(a, references);
        }

        /// <summary>
        /// Perform the search
        /// </summary>
        override public void ExecuteUISearch()
        {
            // Get objects to check:
            IEnumerable<KBObject> objectsToCheck;
            if (this.CheckUnreferencedCallables)
                objectsToCheck = UIServices.KB.CurrentModel.Objects.GetAll();
            else
                // Only attributes
                objectsToCheck = UIServices.KB.CurrentModel.Objects.GetAll(ObjClass.Attribute);

            foreach (KBObject o in objectsToCheck)
            {
                IncreaseSearchedObjects();

                // Check folder:
                if (ObjectInFolderOrModuleToIgnore(o))
                    continue;

                // Check genexus object:
                if(IgnoreGenexusUser && o.User != null && o.User.Name != null)
                {
                    string userName = o.User.Name.ToLower();
                    if (userName.EndsWith("\\genexus") || userName == "genexus")
                        continue;
                }

                // Explictly ignored types (LSI troubles): Image, Theme
                if (o is ICallableInfo || o is ExternalObject ||
                    o is Menubar || o is SDPanel || o is SDT  || o is QueryObject || o is Domain )
                {
                    // Dont report theme classes
                    if (o is ThemeClass)
                        continue;

                    CheckCallableObject(o);
                }
                else if (o is Artech.Genexus.Common.Objects.Attribute)
                    CheckAttributes((Artech.Genexus.Common.Objects.Attribute)o);

                if (base.SearchCanceled)
                    return;
            }
        }

    }
}
