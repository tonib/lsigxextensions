using Artech.Architecture.Common.Descriptors;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common.Objects;
using Artech.Architecture.Common.Objects;

namespace LSI.Packages.Extensiones.Utilidades.UI
{
    /// <summary>
    /// Utilidad para seleccionar elementos (atributos, tablas, objetos) de una kbase
    /// </summary>
    public class SelectKbObject
    {

        /// <summary>
        /// Abre el dialogo de seleccion de un objeto de la kbase.
        /// </summary>
        /// <param name="descriptor">Tipo del objeto. Se obtiene con KBObjectDescriptor.Get</param>
        /// <returns>El objeto seleccionado. null si se cancela la seleccion</returns>
        static private object SelectKbElement(KBObjectDescriptor descriptor)
        {
            SelectObjectOptions options = new SelectObjectOptions();
            options.ObjectTypes.Add(descriptor);
            return UIServices.SelectObjectDialog.SelectObject(options);
        }

        /// <summary>
        /// Open the selection dialog to choose an attribute
        /// </summary>
        /// <returns>The choosen attribute. null if the selection was cancelled</returns>
        static public Attribute SelectAttribute()
        {
            return (Attribute) SelectKbElement(KBObjectDescriptor.Get<Attribute>());
        }

        /// <summary>
        /// Abre el dialogo de seleccion de un procedimiento de la kbase
        /// </summary>
        /// <returns>El procedimiento seleccionado. null si se cancelo la seleccion</returns>
        static public Procedure SeleccionarProcedure()
        {
            return (Procedure)SelectKbElement(KBObjectDescriptor.Get<Procedure>());
        }

        /// <summary>
        /// Abre el dialogo de seleccion de una imagen de la kbase
        /// </summary>
        /// <returns>La imagen seleccionada. null si se cancelo la seleccion</returns>
        static public Image SeleccionarImagen()
        {
            return (Image)SelectKbElement(KBObjectDescriptor.Get<Image>());
        }

        /// <summary>
        /// Abre el dialogo de seleccion de una tabla de la kbase
        /// </summary>
        /// <returns>La tabla seleccionada. null si se cancelo la seleccion</returns>
        static public Table SeleccionarTabla()
        {
            return (Table)SelectKbElement(KBObjectDescriptor.Get<Table>());
        }

        /// <summary>
        /// Abre el dialogo de seleccion de un sdt de la kbase
        /// </summary>
        /// <returns>El sdt seleccionado. null si se cancelo la seleccion</returns>
        static public SDT SeleccionarSdt()
        {
            return (SDT)SelectKbElement(KBObjectDescriptor.Get<SDT>());
        }

        /// <summary>
        /// Abre la seleccion de objetos para objetos que se pueden llamar (que tienen parametros)
        /// TODO: El filtro de los llamables no esta funcionando. Preguntar en el foro porque
        /// </summary>
        /// <returns>El objeto seleccionado. null si se cancelo la seleccion</returns>
        static public KBObject SeleccionarObjetoLlamables()
        {
            SelectObjectOptions opciones = new SelectObjectOptions();
            //opciones.Filters.Add(new System.Predicate<IKBObject>(o => o is ICallableInfo));
            return UIServices.SelectObjectDialog.SelectObject(opciones);
        }

        /// <summary>
        /// Abre el dialogo de seleccion de un tema de la kbase
        /// </summary>
        /// <returns>El tema seleccionado. null si se cancelo la seleccion</returns>
        static public Theme SeleccionarTema()
        {
            return (Theme)SelectKbElement(KBObjectDescriptor.Get<Theme>());
        }

        /// <summary>
        /// Open the selection dialog to choose a kbase folder
        /// </summary>
        /// <returns>Selected folder. null if no folder was selected</returns>
        static public IKBObjectParent SelectFolderOrModule()
        {
            SelectObjectOptions options = new SelectObjectOptions();
            options.ObjectTypes.Add(KBObjectDescriptor.Get<Module>());
            options.ObjectTypes.Add(KBObjectDescriptor.Get<Folder>());
            return UIServices.SelectObjectDialog.SelectObject(options) as IKBObjectParent;
        }

    }
}
