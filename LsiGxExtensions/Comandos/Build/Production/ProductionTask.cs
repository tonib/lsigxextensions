using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{

    /// <summary>
    /// Base class for production tasks
    /// </summary>
    [XmlInclude(typeof(ExecuteScript))]
    [XmlInclude(typeof(EnvironmentTask))]
    public abstract class ProductionTask
    {

        /// <summary>
        /// Task enabled?
        /// </summary>
        public bool Enabled = true;

        /// <summary>
        /// Environment name where execute this task. If null or whitespace, it will be executed for all environments
        /// </summary>
        public string OnlyForEnvironmentName;

        /// <summary>
        /// Do the task
        /// </summary>
        /// <param name="productionProcess">Owner process</param>
        virtual public void Execute(PrepareProduction productionProcess)
        {
            productionProcess.LogLine(this.ToString());
        }

        public override string ToString()
        {
            string txt = string.Empty;
            if (!Enabled)
                txt += "(*** DISABLED ***) ";
            if (!string.IsNullOrEmpty(OnlyForEnvironmentName))
                txt += $"[{OnlyForEnvironmentName}] ";
            return txt;
        }

        /// <summary>
        /// List of files/directories that will be replaced by this task.
        /// </summary>
        virtual public List<string> ReplacedFiles
        {
            get { return new List<string>(); }
        }

    }
}
