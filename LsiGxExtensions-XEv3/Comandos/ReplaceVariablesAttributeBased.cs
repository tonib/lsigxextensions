using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LSI.Packages.Extensiones.Utilidades.Threading;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using System.Windows.Forms;
using System.Threading;
using Artech.Architecture.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common;
using Artech.Udm.Framework.Multiuser;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Variables;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;

namespace LSI.Packages.Extensiones.Comandos
{
    /// <summary>
    /// Tool to replace the "attribute based on" of variables by other attribute
    /// </summary>
    public class ReplaceVariablesAttributeBased : IExecutable
    {

        /// <summary>
        /// Attribute to replace in variables definitions
        /// </summary>
        private Artech.Genexus.Common.Objects.Attribute OldAtribute;

        /// <summary>
        /// The new attribute. It can be null, to remove the attribute reference
        /// </summary>
        private Artech.Genexus.Common.Objects.Attribute NewAttribute;

        /// <summary>
        /// Process log
        /// </summary>
        private Log Log;

        /// <summary>
        /// Number of objects modified
        /// </summary>
        private int NObjectsModified;

        /// <summary>
        /// Number of variables modified
        /// </summary>
        private int NVariablesModified;

        /// <summary>
        /// Number of objects with errors
        /// </summary>
        private int NErrors;

        /// <summary>
        /// If its true, no object will be saved. Only messages will be shown
        /// </summary>
        public bool JustTest;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="oldAttribute"></param>
        /// <param name="newAttribute"></param>
        public ReplaceVariablesAttributeBased(Artech.Genexus.Common.Objects.Attribute oldAttribute, Artech.Genexus.Common.Objects.Attribute newAttribute)
        {
            this.OldAtribute = oldAttribute;
            this.NewAttribute = newAttribute;
        }

        /// <summary>
        /// Replaces variables based on old attribute by the new attribute
        /// </summary>
        /// <param name="sm">Tool to check if the object is locked</param>
        /// <param name="o">Object where to do the replacement</param>
        /// <param name="variables">Variables part of the object</param>
        private void ReplaceVariables(StateManager sm, KBObject o, VariablesPart variables)
        {
            List<Variable> variablesToReplace = variables.Variables
            .Where(v => v.AttributeBasedOn != null && v.AttributeBasedOn.Name == OldAtribute.Name)
            .ToList();

            if (variablesToReplace.Count > 0)
            {
                // Check if object is open anywhere
                if (sm.IsObjectLocked(o))
                {
                    Log.Output.AddErrorLine("Object " + o.QualifiedName + " is open: It cannot be modified");
                    Log.ProcesoOk = false;
                    return;
                }

                Log.Output.AddLine("Object " + o.QualifiedName + ":");

                // Replace variables
                variablesToReplace.ForEach(
                    v => {
                        Log.Output.AddLine("Variable " + v.Name + " modified");
                        VariableGX.ReplaceAttBasedOn(v, NewAttribute);
                    }
                );
                if (!JustTest)
                {
                    variables.KBObject.Parts.LsiUpdatePart(variables);
                    if (UIServices.Environment.InvokeRequired)
                        // Run save on the UI thread (concurrent saves are not supported)
                        UIServices.Environment.Invoke(() => o.Save());
                    else
                        o.Save();
                }
                NObjectsModified++;
                NVariablesModified += variablesToReplace.Count;
            }
        }

        /// <summary>
        /// Executes the attribute replacement
        /// </summary>
        public void Execute()
        {
            try
            {
                using (Log = new Log())
                {

                    if (JustTest)
                        Log.Output.AddLine("[TEST ONLY]");
                    else
                        Package.ObjectCheckingOnSaveEnabled = false;

                    if (NewAttribute == null)
                        Log.Output.AddLine("Removing 'Attribute based on' of attribute " +
                            OldAtribute.Name);
                    else
                        Log.Output.AddLine("Replacing 'Attribute based on' of attribute " +
                            OldAtribute.Name + " by " + NewAttribute.Name);

                    NObjectsModified = NVariablesModified = NErrors = 0;

                    StateManager sm = new StateManager()
                    {
                        Multiuser = true
                    };

                    // Get objects referencing attribute to replace
                    foreach (EntityReference r in OldAtribute.GetReferencesTo(LinkType.UsedObject))
                    {
                        // Load referencer object
                        KBObject o = KBObject.Get(UIServices.KB.CurrentModel, r.From);
                        if (o == null)
                            continue;

                        // Get and replace object variables
                        VariablesPart variables = o.Parts.LsiGet<VariablesPart>();
                        if (variables != null)
                        {
                            try
                            {
                                ReplaceVariables(sm, o, variables);
                            }
                            catch (Exception ex)
                            {
                                NErrors++;
                                Log.Output.AddErrorLine("Error modifying object " + o.QualifiedName);
                                Log.ShowException(ex);
                            }
                        }
                    }
                    Log.Output.AddLine(NObjectsModified + " objects modified. " + NVariablesModified +
                        " variables modified. " + NErrors + " errors");
                }
            }
            catch (Exception ex2)
            {
                Log.ShowException(ex2);
            }
            finally
            {
                Package.ObjectCheckingOnSaveEnabled = true;
            }
        }

    }
}
