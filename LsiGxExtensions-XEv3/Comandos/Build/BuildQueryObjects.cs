using System;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Services;
using Artech.GXplorer.Common.Handlers;
using Artech.GXplorer.Common.Objects;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.Logging;

namespace LSI.Packages.Extensiones.Comandos.Build
{

    /// <summary>
    /// Tool to generate query objects
    /// </summary>
    public class BuildQueryObjects : BuildProcess
    {

        /// <summary>
        /// Model where to generate the objects
        /// </summary>
        private KBModel TargetModel;

        /// <summary>
        /// True (this class uses the build functions of genexus)
        /// </summary>
        override public bool IsInternalGxBuild { get { return true; } }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="targetModel">Target model where to generate the query objects</param>
        public BuildQueryObjects(KBModel targetModel)
        {
            this.TargetModel = targetModel;
        }

        private bool RecalculateSentences()
        {
            bool any = false;
            foreach (QueryObject q in TargetModel.GetDesignModel().GetObjects<QueryObject>())
            {
                q.LoadVirtualParts();
                q.QueryCacheInfoPart.SetCalculateSQLSentence(true);
                q.QuerySQLSentencePart.DeleteSQLSentence();
                q.SaveVirtualParts();
                any = true;
            }
            return any;
        }

        /// <summary>
        /// Build the query objects
        /// </summary>
        override public void Execute()
        {
            try
            {
                using (Log log = new Log(Log.BUILD_OUTPUT_ID, false))
                {
                    try
                    {
                        if (GenexusUIServices.Build.IsBuilding)
                        {
                            LogErrorLine("There is a Genexus build running. This extension cannot be executed");
                            return;
                        }
                    }
                    catch
                    {
                        // This will fail if we are running a msbuild script
                    }

                    // Generate query objects
                    try
                    {
                        SubscribeGXOutput();
                        if (RecalculateSentences())
                        {
                            // Do the build:
                            BuildEventHandler build = new BuildEventHandler(TargetModel);
                            build.Handle();
                        }
                        else
                            log.Output.AddLine("No query objects found");
                    }
                    finally
                    {
                        UnsuscribeGxOutput();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                BuildWithErrors = true;
            }
        }

        public override string ToString()
        {
            return "Generate query objects";
        }
    }
}
