using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Common.Diagnostics;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.Multiuser;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Llamadas.Edicion
{
    /// <summary>
    /// Clase de utilidad que permite editar llamadas a un objeto genexus en toda la kbase.
    /// Permite añadir o quitar un parametro en las llamadas encontradas al objeto.
    /// </summary>
    public class EditorLlamadas : UISearchBase
    {

        /// <summary>
        /// Objeto del tipo ICallableInfo llamado del que se estan editando las llamadas.
        /// </summary>
        private KBObject ObjetoLlamado;

        /// <summary>
        /// Si es cierto, solo se revisan los cambios a hacer y se detectan errores, y no se toca nada.
        /// Si es falso, se hacen las modifiaciones correspondientes en los llamadores.
        /// Por defecto es cierto.
        /// </summary>
        public bool SoloRevisar = true;

        /// <summary>
        /// El buscador de llamadas en el codigo
        /// </summary>
        private CallsFinder BuscadorLlamadas;

        /// <summary>
        /// Operacion a hacer sobre las llamadas
        /// </summary>
        private IOperacionLlamada Operacion;

        /// <summary>
        /// Indica si, al revisar los cambios a hacer, Genexus tiene que hacer la validacion
        /// de los objetos. Si esta marcado, este proceso tarda mucho mas
        /// </summary>
        public bool ValidarObjetoAlRevisar;

        /// <summary>
        /// If its not emply, only callers with a name starting with this string will be modified.
        /// The filter is for the NOT qualified name.
        /// </summary>
        public string CallersNameFilter;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objeto">Objeto llamado del que editar las llamadas. Debe ser ICallableInfo</param>
        /// <param name="operacion">Operacion a hacer sobre las llamadas</param>
        /// <param name="soloRevisar">Si es cierto, solo se revisan los cambios a hacer y se detectan errores, y no se toca nada.
        /// Si es falso, se hacen las modifiaciones correspondientes en los llamadores.</param>
        public EditorLlamadas(KBObject objeto, IOperacionLlamada operacion, bool soloRevisar)
        {
            ObjetoLlamado = objeto;
            Operacion = operacion;
            SoloRevisar = soloRevisar;

            if (!(ObjetoLlamado is ICallableInfo))
                throw new ArgumentException("Object " + objeto + " is not ICallableInfo");

            BuscadorLlamadas = new CallsFinder(ObjetoLlamado);
        }

        /// <summary>
        /// La lista de objetos que llaman al llamador
        /// </summary>
        private IEnumerable<EntityReference> ObjetosLlamadores
        {
            get { return ObjetoLlamado.GetReferencesTo(); }
        }

        /// <summary>
        /// Edita las llamadas en un objeto que referencia a objeto llamado
        /// </summary>
        /// <param name="modeloActual">El modelo actual de la kbase</param>
        /// <returns>Informacion sobre los cambios hechos en el objeto. null if the object does not need to be modified</returns>
        public InfoCambioLlamadasObjeto EditarLlamadasEnLlamador(EntityReference referenciaLlamadador)
        {
            // Obtener el objeto llamador:
            KBObject llamador = KBObject.Get(UIServices.KB.CurrentModel, referenciaLlamadador.From);

            // Check name filter:
            // The filter is for the NOT qualified name
            if (!string.IsNullOrEmpty(CallersNameFilter) && !llamador.Name.ToLower().StartsWith(CallersNameFilter.ToLower()))
                // Object does not pass name filter
                return null;

            InfoCambioLlamadasObjeto infoCambioObjeto = new InfoCambioLlamadasObjeto(llamador);

            try
            {

                // Si el objeto esta abierto, no modificarlo (se pueden perder cambios)
                //if (UIServices.DocumentManager.IsOpenDocument(llamador))
                StateManager sm = new StateManager()
                {
                    Multiuser = true
                };
                if (sm.IsObjectLocked(llamador))
                {
                    infoCambioObjeto.AgregarEstadoLlamada(new InfoCambioLlamada(
                        Estado.ERROR, "Object " + llamador.QualifiedName + " is open. It cannot be " +
                        "modified"));
                    return infoCambioObjeto;
                }

                // Hacer los cambios globales sobre el objeto:
                Operacion.HacerCambiosGlobalesLlamador(llamador, infoCambioObjeto);
                if (infoCambioObjeto.EstadoCambiosObjeto == Estado.ERROR)
                    // Se produjo algun error en la operacion
                    return infoCambioObjeto;

                // Revisar las partes de codigo del objeto
                foreach (KBObjectPart p in llamador.Parts.LsiEnumerate())
                {
                    if (!(p is ISource))
                        continue;

                    // This still happens on Ev3:
                    /* HAY un problema gordo con las conditions: 
                     * Al parsear, el analizador.CodigoParseado convierte 
                     * CliNomFis LIKE &x WHEN &CodicionBooleana;
                     * en esto
                     * CliNomFis LIKE &x;\r\n\r\nWHEN &CodicionBooleana
                     * Mientras no lo arreglen, o como no encuentre como parchearlo, 
                     * la extension no se puede aplicar en las conditions.
                    */
                    /*if (p is ConditionsPart)
                    {
                        // TODO: El arbol de parseado tiene una funcion SerializeToText. Ver si arregla esto
                        if (((ConditionsPart)p).Source.ToLower().Contains("when"))
                            continue;
                    }*/

                    ISource fuente = (ISource)p;

                    try
                    {
                        // Revisar las llamadas en el codigo:
                        ParsedCode analizador = new ParsedCode(p);
                        bool algunaModificacion = false;
                        foreach (Llamada l in BuscadorLlamadas.BuscarTodas(analizador))
                        {
                            // Hacer la edicion
                            InfoCambioLlamada infoLlamada = Operacion.EditarLlamada(l);
                            // Guardar la informacion sobre el cambio
                            infoCambioObjeto.AgregarEstadoLlamada(infoLlamada);
                            algunaModificacion = true;
                        }
                        if (algunaModificacion)
                        {
                            // Actualizar el codigo de la parte del objeto gx
                            fuente.Source = analizador.ParsedCodeString;
                            llamador.Parts.LsiUpdatePart(p);
                        }
                    }
                    catch (Exception exLlamada)
                    {
                        infoCambioObjeto.AgregarEstadoLlamada(new InfoCambioLlamada(
                            Estado.ERROR, "Error editing calls: " + exLlamada.ToString()));
                    }

                }

                if (infoCambioObjeto.EstadosLlamadas.Count == 0)
                    // Deberia haber alguna llamada...
                    infoCambioObjeto.AgregarEstadoLlamada(new InfoCambioLlamada(Estado.WARNING, 
                        "No call has been found on object, but there should be some"));

                if (infoCambioObjeto.EstadoCambiosObjeto != Estado.ERROR)
                {
                    if (SoloRevisar)
                    {
                        if (ValidarObjetoAlRevisar)
                        {
                            // Revisar que el objeto quede valido:
                            OutputMessages mensajes = new OutputMessages();
                            if (!llamador.Validate(mensajes))
                            {
                                string erroresValidacion = "There are Genexus validation errors:";
                                // Hay errores de validacion:
                                foreach (BaseMessage msg in mensajes)
                                    erroresValidacion += Environment.NewLine + msg.Text;
                                infoCambioObjeto.AgregarEstadoLlamada(new InfoCambioLlamada(Estado.ERROR, erroresValidacion));
                            }
                        }
                    }
                    else
                    {
                        if( UIServices.Environment.InvokeRequired )
                            // Guardar los cambios en el thread de la UI (sincrono):
                            UIServices.Environment.Invoke( () => llamador.Save() );
                        else
                            llamador.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
                infoCambioObjeto.AgregarEstadoLlamada(
                    new InfoCambioLlamada(Estado.ERROR, "Error editing calls in object  " + 
                        llamador.QualifiedName + ": " + ex.Message)
                );
            }

            return infoCambioObjeto;
        }

        /// <summary>
        /// Ejecuta la edicion de llamadas
        /// </summary>
        public override void ExecuteUISearch()
        {
            // Obtener la lista de objetos que llaman al objeto
            foreach (EntityReference r in ObjetosLlamadores)
            {
                if (SearchCanceled)
                    // Se ha cancelado la busqueda
                    break;

                // Reportar cambios en el objeto
                InfoCambioLlamadasObjeto info = EditarLlamadasEnLlamador(r);
                if (info != null)
                    PublishUIResult(info);

                IncreaseSearchedObjects();
            }
        }
    }
}
