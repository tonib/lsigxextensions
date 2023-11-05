using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Core.UI.Documents;
using Artech.Genexus.Common.Objects;
using Artech.Common.Framework.Selection;
using System.Globalization;
using System.Text;
using System;
using System.Linq;
using System.Collections.Generic;
using Artech.Architecture.UI.Framework.Packages;
using System.IO;
using System.Security.Principal;
using Artech.Patterns.WorkWithDevices.Parts;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Consultas del estado actual de genexus
    /// </summary>
    public class Entorno
    {

        /// <summary>
        /// Longitud maxima de una linea de codigo en un objeto de genexus para que se vea bien
        /// en el editor
        /// </summary>
        public const int LONGMAXLINEACODIGO = 60;

        /// <summary>
        /// Postfix added to file names to do a backup
        /// </summary>
        public const string BACKUPPOSTFIX = "-BACKUP";

        /// <summary>
        /// Cierto si actualmente estamos en una kbase
        /// </summary>
        static public bool EstamosEnUnaKbase
        {
            get { return UIServices.KB != null && UIServices.KB.CurrentKB != null; }
        }

        /// <summary>
        /// Cierto si el editor actual esta en un objeto de la kbase
        /// </summary>
        static public bool EstamosEnObjeto
        {
            get
            {
                if (!EstamosEnUnaKbase)
                    return false;

                // Ver si estamos en un objeto:
                IEnvironmentService env = UIServices.Environment;
                return env != null && env.ActiveDocument != null && env.ActiveDocument.Object != null;
            }
        }

        /// <summary>
        /// Cierto si el editor actual esta en un procedimiento de la kbase
        /// </summary>
        static public bool EstamosEnProcedimiento
        {
            get
            {
                if (!EstamosEnObjeto)
                    return false;

                // Ver si estamos en un procediimento
                KBObject objetoActual = UIServices.Environment.ActiveDocument.Object;
                if (!(objetoActual is Procedure))
                    return false;

                return true;
            }
        }

        /// <summary>
        /// Obtiene el editor de la parte del objeto actualmente en edicion.
        /// Si no estamos editando una parte de un objeto devuelve null.
        /// </summary>
        static public IGxView EditorParteActual
        {
            get
            {
                if (!EstamosEnUnaKbase)
                    return null;

                // Ver cual es el editor del objeto
                IDocumentView editorObjeto = UIServices.Environment.ActiveView;
                if (editorObjeto == null)
                    return null;

                // Ver cual es editor de la parte
                IGxView editorParte = editorObjeto.ActiveView;
                if (editorParte == null)
                    return null;

                return editorParte;
            }
        }

        /// <summary>
        /// Obtiene el objeto seleccionado en el editor actual.
        /// Si no se esta editando nada o no hay nada seleccionado, devuelve null.
        /// </summary>
        static public object ObjetoSeleccionadoActualmenteEnEditor
        {
            get
            {
                IGxView editorParte = EditorParteActual;
                if (editorParte == null || !(editorParte is ISelectionContainer))
                    return null;
                ISelectionContainer seleccionesEditor = (ISelectionContainer)editorParte;
                return seleccionesEditor.SelectedObject;
            }
        }

        /// <summary>
        /// Devuelve cierto si en el editor actual se tiene seleccionado un objeto que se 
        /// puede llamar (un procedimiento, una transaccion, etc). Un atributo p.ej. no es llamable.
        /// </summary>
        static public bool ObjetoLlamableActualmenteSeleccionado
        {
            get
            {
                object objetoSeleccionado = ObjetoSeleccionadoActualmenteEnEditor;
                return objetoSeleccionado is ICallableInfo;
            }
        }

        /// <summary>
        /// Get the current editing part of the current object. null if there is no object part
        /// currently editing
        /// </summary>
        static public KBObjectPart CurrentEditingPart
        {
            get
            {
                try
                {
                    IGxView editorParte = EditorParteActual;
                    if (editorParte == null)
                        return null;

                    // Ver que parte se esta editando:
                    if (editorParte.Viewed == null || !(editorParte.Viewed is GxDocumentPart))
                        return null;
                    GxDocumentPart editionPart = editorParte.Viewed as GxDocumentPart;
                    if (editionPart == null)
                        return null;

                    return editionPart.Part;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Cierto si el editor actual sobre la parte del source de un objeto
        /// </summary>
        static public bool EstamosEnSourceDeObjeto
        {
            get
            {
                KBObjectPart part = CurrentEditingPart;
                // I don't know why, but VirtualConditionsPart is not a ISource...
                return part is ISource || part is VirtualConditionsPart;
            }
        }

        /// <summary>
        /// True if the current object is a Workpanel
        /// </summary>
        static public bool AtWorkpanel
        {
            get
            {
                if (!EstamosEnObjeto)
                    return false;

                // Ver si estamos en un procediimento
                KBObject objetoActual = UIServices.Environment.ActiveDocument.Object;
                return objetoActual is WorkPanel;
            }
        }

        /// <summary>
        /// Cierto si el objeto actual es un workpanel o una transaccion
        /// </summary>
        static public bool EstamosEnWinform
        {
            get
            {
                if (!EstamosEnObjeto)
                    return false;

                // Ver si estamos en un procediimento
                KBObject objetoActual = UIServices.Environment.ActiveDocument.Object;
                return objetoActual is WorkPanel || objetoActual is Transaction;
            }
        }

        /// <summary>
        /// Cierto si el objeto actual es una transaccion
        /// </summary>
        static public bool EstamosEnTransaccion
        {
            get
            {
                if (!EstamosEnObjeto)
                    return false;

                // Ver si estamos en un procediimento
                KBObject objetoActual = UIServices.Environment.ActiveDocument.Object;
                return objetoActual is Transaction;
            }
        }

        /// <summary>
        /// Convierte el codigo al formato de texto del codigo que quiere genexus
        /// Hay un fallo en el parser de genexus. Necesita que los saltos de linea sean "\r\n".
        /// Si no son asi, se hace la picha un lio.
        /// </summary>
        /// <param name="texto">Texto original</param>
        /// <returns>El texto convertido al formato de genexus</returns>
        static public string StringFormatoKbase(string texto)
        {
            if (texto == null)
                return string.Empty;

            // Unificar, por si vienen "\r\n"s y "\n"s mezclados:
            texto = texto.Replace(Environment.NewLine, "\n");
            // Hacer la conversion final de los saltos de linea:
            texto = texto.Replace("\n", Environment.NewLine);
            return texto;
        }

        /// <summary>
        /// List of currently selected objects in the navigator window. 
        /// null if the navigator window is not available.
        /// </summary>
        static public List<KBObject> NavigatorSelectedObjects
        {
            get
            {
                // Get the selected objects
                if (!UIServices.IsNavigatorAvailable)
                    return null;
                INavigator navigator = UIServices.Navigator.GetNavigator();
                if (navigator == null)
                    return null;
                ISelectionContainer selectionContainer = navigator.CurrentSelection;
                if (selectionContainer == null)
                    return null;
                System.Collections.ICollection selectedObjects = selectionContainer.SelectedObjects;
                if (selectedObjects == null)
                    return null;

                return selectedObjects.Cast<object>().Where(x => x is KBObject).Cast<KBObject>().ToList();
            }
        }

        /// <summary>
        /// Get a safe version of a text to put on a file name
        /// </summary>
        /// <remarks>
        /// Wrong characters are replaced by '-' characters
        /// </remarks>
        /// <param name="text">Text to put on a filename</param>
        /// <returns>Safe text filename version</returns>
        static public string ToSafeFilename(string text)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                text = text.Replace(c, '-');
            return text;
        }

        /// <summary>
        /// Get absolute path of a model
        /// </summary>
        /// <param name="targetModel">The model. If its null, the current IU target model will be
        /// used</param>
        /// <returns>Absolute path of the model</returns>
        static public string GetTargetDirectory(KBModel targetModel)
        {
            if (targetModel == null)
                targetModel = UIServices.KB.WorkingEnvironment?.TargetModel;
            return Path.Combine(targetModel.KB.Location, targetModel.TargetPath);
        }

        /// <summary>
        /// Current environment destination directory absolute path
        /// </summary>
        static public string TargetDirectory
        {
            get { return GetTargetDirectory(null); }
        }

        /// <summary>
        /// Get absolute path for file on the target destination directory
        /// </summary>
        /// <param name="filename">file name</param>
        /// <param name="targetModel">The target model. If it's null, the current IU target model 
        /// will be used</param>
        /// <returns>Absolute file path</returns>
        static public string GetTargetDirectoryFilePath(string filename, KBModel targetModel)
        {
            return Path.Combine(GetTargetDirectory(targetModel), filename);
        }

        /// <summary>
        /// Get absolute path for file on the current UI environment destination directory
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns>Absolute file path</returns>
        static public string GetTargetDirectoryFilePath(string filename)
        {
            return GetTargetDirectoryFilePath(filename, null);
        }

        /// <summary>
        /// Absolute path of the KB folder for the extensions files
        /// </summary>
        static public string GetLsiExtensionsKBFolder(KnowledgeBase kb)
        {
            return Path.Combine(kb.Location, "LsiExtensions");
        }

        /// <summary>
        /// Get absolute path for file on the current kb inside the extensions folder. If the 
        /// extensions folder does not exists, it will be created
        /// </summary>
        /// <param name="filename">file name</param>
        /// <returns>Absolute file path</returns>
        static public string GetLsiExtensionsFilePath(KnowledgeBase kb, string filename)
        {
            string path = GetLsiExtensionsKBFolder(kb);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            return Path.Combine(path, filename);
        }

        /// <summary>
        /// Current logged Windows username. Empty string if the username cannot be retrieved
        /// </summary>
        static public string WindowsUsername
        {
            get
            {
                string username;
                try
                {
                    username = WindowsIdentity.GetCurrent().Name;
                    // Remove domain:
                    int idx = username.LastIndexOf('\\');
                    if (idx >= 0)
                        username = username.Substring(idx + 1);
                }
                catch {
                    username = string.Empty;
                }
                return username;
            }
        }

        /// <summary>
        /// Check if a file is open
        /// </summary>
        /// <param name="path">File path to check</param>
        /// <returns>True if the file is open.</returns>
        static public bool FileIsOpen(string path)
        {
            // Try to open the file in exclusive mode:
            try
            {
                using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (FileNotFoundException)
            {
                return false;
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        static public string GetUnusedFileName(string originalPath)
        {
            string basePath = Path.Combine(
                Path.GetDirectoryName(originalPath),
                Path.GetFileNameWithoutExtension(originalPath));
            string extension = Path.GetExtension(originalPath);

            string newPath;
            int nVersion = -1;
            do
            {
                nVersion++;
                newPath = basePath;
                if (nVersion > 0)
                    newPath += "-" + nVersion;
                newPath += extension;
            }
            while (File.Exists(newPath));

            return newPath;
        }

        /// <summary>
        /// Do a file backup 
        /// </summary>
        /// <param name="originalPath">File wich to do a backup</param>
        /// <param name="move">True if the file should be renamed (moved). False if the file
        /// should be copied</param>
        /// <returns>The backup file path</returns>
        static public string DoFileBackup(string originalPath, bool move)
        {
            // Get available backup file name
            string backupPath = Path.Combine( 
                Path.GetDirectoryName(originalPath),
                Path.GetFileNameWithoutExtension(originalPath) );
            backupPath += BACKUPPOSTFIX + Path.GetExtension(originalPath);

            backupPath = GetUnusedFileName(backupPath);            

            // Do backup
            if( move )
                File.Move(originalPath, backupPath);
            else
                File.Copy(originalPath, backupPath);

            return backupPath;
        }

    }
}
