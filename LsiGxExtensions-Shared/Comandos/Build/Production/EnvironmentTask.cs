using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace LSI.Packages.Extensiones.Comandos.Build.Production
{
    /// <summary>
    /// Base class for production tasks that operate on a single enviroment
    /// </summary>
    [XmlInclude(typeof(ZipProduction))]
    [XmlInclude(typeof(CopyProduction))]
    public class EnvironmentTask : ProductionTask
    {
        /// <summary>
        /// Kind of production to prepare
        /// </summary>
        public EnvironmentType Environment = EnvironmentType.WIN;

        public override string ToString()
        {
            return base.ToString() + "[" + Environment.ToString() + "] ";
        }

        public string RelativeEnvironmentPath
        {
            get
            {
                if (Environment == EnvironmentType.WIN)
                    return "bin";
                else
                    return "web";
            }
        }

        public string ConfigFileName
        {
            get
            {
                if (Environment == EnvironmentType.WIN)
                    return "client.exe.config";
                else
                    return "web.config";
            }
        }

        public string GetAbsolutePath(PrepareProduction productionProcess)
        {
            return Path.Combine(productionProcess.TargetDirectory, RelativeEnvironmentPath);
        }
    }
}
