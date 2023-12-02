using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common.CustomTypes;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Udm.Framework.References;
using Artech.Udm.Framework;
using System.Text.RegularExpressions;
using Artech.Genexus.Common.Entities;
using Artech.Architecture.Common.Converters;
using Artech.Packages.Patterns.Objects;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Genexus.Common.Run;
using Artech.Genexus.Common.Helpers;
using Artech.Genexus.Common.Services;
using Artech.Udm.Framework.Multiuser;
using Artech.Architecture.Common.Services;
using System.IO;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// KBase utilities
    /// </summary>
    public class KBaseGX
    {

        // TODO: I have no found the constant definitions of namespaces on genexus dlls
        // TODO: That's why are here these constants...
        // TODO: The list is uncompleted

        /// <summary>
        /// Namespace for callable objects (procedures, webpanels, etc), SDTs, Masterpages
        /// </summary>
        public const string NAMESPACE_OBJECTS = "Objects";

        /// <summary>
        /// Namespace for images
        /// </summary>
        public const string NAMESPACE_IMAGES = "Images";

        /// <summary>
        /// Namespace for themes
        /// </summary>
        public const string NAMESPACE_THEMES = "Themes";

        /// <summary>
        /// Get a callable object (procedure, webpanel, etc) by its name
        /// </summary>
        /// <param name="nombre">Object name to search</param>
        /// <returns>The object found. null if no object was found, or it is not callable</returns>
        static public KBObject GetCallableObject(string nombre)
        {
            if (string.IsNullOrEmpty(nombre))
                return null;

            QualifiedName qName = new QualifiedName(nombre);
            KBObject o = UIServices.KB.CurrentModel.Objects.Get(NAMESPACE_OBJECTS, qName);
            if (!(o is ICallableInfo))
                o = null;

            return o;
        }

        /// <summary>
        /// Get an unused object name on the kb, given a name. If the name is used, a number
        /// will be append to this name (ex. if "Achilipu" name is used, it will return 
        /// "Achilipu2" or "Achilipu3", and so.
        /// </summary>
        /// <param name="nameSpace">Namespace where to search names. See NAMESPACE_* constants</param>
        /// <param name="objectName">Object name to search</param>
        /// <returns>Unused name</returns>
        static public string GetUnusedName(string nameSpace, string objectName)
        {

            int number = 0;
            QualifiedName currentName = new QualifiedName(objectName);
            IKBModelObjects objects = UIServices.KB.CurrentModel.Objects;
            while (objects.Get(nameSpace, currentName) != null)
            {
                number++;
                currentName = new QualifiedName(objectName + number);
            }
            return currentName.ObjectName;
        }

        /// <summary>
        /// Devuelve cierto si el objeto es main
        /// </summary>
        static public bool EsMain(KBObject o)
        {
            try
            {
                return o.GetPropertyValue<bool>(Artech.Genexus.Common.Properties.TRN.MainProgram);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Devuelve cierto si el objeto es un bussiness component
        /// </summary>
        static public bool EsBussinessComponent(KBObject o)
        {
            if( !(o is Transaction) )
                return false;

            object valorEsBC = o.GetPropertyValue(Properties.TRN.BusinessComponent);
            return valorEsBC != null && ((bool)valorEsBC);
        }

        /// <summary>
        /// Devuelve cierto si el objeto es referenciado por algun otro en la kbase
        /// </summary>
        static public bool EsReferenciado(KBObject o)
        {
            foreach (EntityReference r in o.GetReferencesTo(LinkType.UsedObject))
                return true;
            return false;
        }

        /// <summary>
        /// Intenta obtener un nombre de objeto Genexus a partir de un texto
        /// </summary>
        /// <param name="texto">Texto a partir del que obtener un nombre de objeto</param>
        /// <param name="reemplazarNombreVacio">Indica, si el texto va a quedar vacio, si hay que 
        /// reemplazar el nombre por por defecto. Este valor por defecto sera "Objeto"</param>
        /// <returns>El nombre de objeto para el texto</returns>
        static public string ConvertirANombreObjeto(string texto, bool reemplazarNombreVacio) {

            texto = texto.Trim();

            // Quitar todo lo que no sean letras, caracteres o undescores:
            Regex rgx = new Regex("[^a-zA-Z0-9_]");
            texto = rgx.Replace(texto, "");

            if (reemplazarNombreVacio && string.IsNullOrEmpty(texto))
                // Poner un nombre por defecto para un texto vacio
                texto = "Objeto";

            if (texto.Length > 0 && !Char.IsLetter(texto[0]))
                // Los nombres deben empezar por una letra:
                texto = "X" + texto;

            return texto;
        }

        /// <summary>
        /// Return true if object is a win /web form / sd form
        /// </summary>
        /// <param name="o">Object to check</param>
        /// <returns>True if object is form</returns>
        static public bool IsForm(KBObject o)
        {
            return o is Transaction || o is WorkPanel || o is WebPanel || o is SDPanel;
        }

        /// <summary>
        /// Get the normalized program file name of an object
        /// </summary>
        /// <param name="withInitialLetter">True if the file name should contain the initial
        /// letter of the object type</param>
        /// <param name="environment">Enviroment where to get the file name</param>
        /// <param name="o">Object to get the name</param>
        /// <param name="fileExtension">Extension of the file name. null to don't add
        /// any extension</param>
        /// <returns>Normalized file name, without any path</returns>
#if GX_17_OR_GREATER
        static public string GetProgramFileName(KBObject o, GxGenerator environment, bool withInitialLetter, string fileExtension)
#else
        static public string GetProgramFileName(KBObject o, GxEnvironment environment,
            bool withInitialLetter, string fileExtension)
#endif
        {
            string fileName;
            if (withInitialLetter)
            {
                Guid pgmType;
                string pgmName;
                bool isMain;
                string mainType;
                RunHelper.GetProgramInfo(environment.Model, o.Key,
                    out pgmType, out pgmName, out isMain, out mainType);
                fileName = RunHelper.GetPgmFileName(environment.Generator, pgmType, pgmName, isMain);
				// RunHelper.GetPgmFileName returns name with a directory for modules if object is inside a module. Remove it
				fileName = Path.GetFileName(fileName);
            }
            else
                fileName = o.GetSignificantName();

            if (fileExtension != null)
                fileName += "." + fileExtension;
            return fileName;
        }

        /// <summary>
        /// It does a copy model lock, check if there are pending reorganizations and finally it 
        /// does a copy 
        /// </summary>
        /// <param name="workingSet">Working set for this build</param>
        /// <param name="log">Process log</param>
        /// <returns>True if the copy model was done, or false if some error happend</returns>
        static public bool DoCopyModel(DevelopmentWorkingSet workingSet, IOutputTarget log)
        {
            // Do a copy model lock:
            Guid trnGuid = Guid.NewGuid();
            bool lockAdquired = false;
            try
            {
                // If there is a genexus build running, we should not use this:
                if (GenexusUIServices.Build.IsBuilding)
                {
                    log.AddErrorLine("There is a Genexus build running. Copy model cannot be executed");
                    return false;
                }

                if (KBStateManager.AcquireState(ModelState.K_COPYINGMODEL, workingSet.DesignModel, trnGuid))
                    lockAdquired = true;
                else
                {
                    log.AddErrorLine("There is a lock. Copy model cannot be executed");
                    return false;
                }

                if (GenexusBLServices.ModelInformation.NeedReorg(workingSet.DesignModel,
                    workingSet.WorkingModel))
                {
                    log.AddErrorLine("There are pending reorganizations. Copy model cannot be executed");
                    return false;
                }

                log.AddLine("Target enviroment update...");
                // Run copy model on UI thread (genexus does it like this...)
                UIServices.Environment.Invoke(() =>
                {
                    UIServices.KB.CopyModel(workingSet.DesignModel, workingSet.WorkingModel);
                });
                return true;
            }
            finally
            {
                if (lockAdquired)
                    KBStateManager.ReleaseState(ModelState.K_COPYINGMODEL, workingSet.DesignModel, trnGuid);
            }
        }

    }
}
