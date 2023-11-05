using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using Artech.Architecture.UI.Framework.Services;
using LSI.Packages.Extensiones.Utilidades.UI;
using Artech.Udm.Framework;
using Artech.Packages.Patterns.Objects;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{

    /// <summary>
    /// Busca referencias de lectura, escritura, insercion o borrado de una tabla
    /// </summary>
    public class BuscadorReferenciasTabla : UISearchBase
    {

        /// <summary>
        /// Tipos de referencias a buscar
        /// </summary>
        public List<TipoOperacion> TiposReferencias;

        /// <summary>
        /// Tabla a la que buscar referencias
        /// </summary>
        private Table Tabla;

        /// <summary>
        /// El modelo actual
        /// </summary>
        private KBModel ModeloActual;

        /// <summary>
        /// Los BC que hacen referencia a la tabla que estamos buscando
        /// </summary>
        private HashSet<KBObject> BcReferenciadores = new HashSet<KBObject>();

        /// <summary>
        /// Lista de identificadores de los objetos ya publicados
        /// </summary>
        private HashSet<Guid> ResultadosYaPublicados = new HashSet<Guid>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objeto">Tabla a buscar</param>
        /// <param name="tiposReferencias">Tipos de referencias a buscar</param>
        public BuscadorReferenciasTabla(Table tabla, List<TipoOperacion> tiposReferencias)
        {
            Tabla = tabla;
            TiposReferencias = tiposReferencias;
            ModeloActual = UIServices.KB.CurrentModel;
        }

        /// <summary>
        /// Busca llamadas de actualizacion en la bbdd de BC que referencian a la tabla
        /// </summary>
        private void BuscarReferenciasBC()
        {
            // Ver que funciones de los BC vamos a buscar:
            List<string> funcionesBuscar = new List<string>();
            bool contieneInsercion = TiposReferencias.Contains(TipoOperacion.INSERCION);
            bool contieneEscritura = TiposReferencias.Contains(TipoOperacion.ESCRITURA);
            bool referenciasSaveSeguras = true;
            if (contieneInsercion || contieneEscritura)
            {
                if (!contieneInsercion || !contieneEscritura)
                    // Vamos a buscar referencias al Save() de BC, pero algunas pueden ser 
                    // incorrectas
                    referenciasSaveSeguras = false;

                // TODO: Check if this GX_15_OR_GREATER check is needed
#if GX_15_OR_GREATER
                // TODO: If FUNC_INSERT or FUNC_UPDATE is found, the reference is 100% sure,
                // TODO: but is reported as "maybe"
                funcionesBuscar.Add(BuscadorTokensSdtGx.FUNC_INSERTORUPDATE);
                if (contieneInsercion)
                    funcionesBuscar.Add(BuscadorTokensSdtGx.FUNC_INSERT);
                if (contieneEscritura)
                    funcionesBuscar.Add(BuscadorTokensSdtGx.FUNC_UPDATE);
#else
                funcionesBuscar.Add(BuscadorTokensSdtGx.FUNC_SAVE);
#endif
            }

            if (TiposReferencias.Contains(TipoOperacion.LECTURA))
                funcionesBuscar.Add(BuscadorTokensSdtGx.FUNC_LOAD);

            if (TiposReferencias.Contains(TipoOperacion.BORRADO))
                funcionesBuscar.Add(BuscadorTokensSdtGx.FUNC_DELETE);

            // Crear el buscador de las funciones:
            BuscadorTokensSdtGx buscador = new BuscadorTokensSdtGx(BcReferenciadores,
                KindOfIndirection.FUNCTION, funcionesBuscar);

            // Recorrer los bc que referencian a la tabla:
            foreach (KBObject bc in BcReferenciadores)
            {

                if (SearchCanceled)
                    break;

                // Revisar los referenciadores del bc:
                foreach (EntityReference r in bc.GetReferencesTo())
                {

                    if (SearchCanceled)
                        break;

                    IncreaseSearchedObjects();

                    // Obtener el objeto referenciador:
                    KBObject referenciador = KBObject.Get(UIServices.KB.CurrentModel, r.From);

                    // Ver si alguna de las funciones del BC es llamada
                    if( TokensFinder.IsUnsupportedObject(referenciador) )
                    {
                        // Vete a saber: Decimos que si
                        PublishUIResult(new RefObjetoGX(referenciador)
                        {
                            PosibleFalsoPositivo = true
                        });
                    }
                    else if (buscador.ContieneReferencia(referenciador))
                        PublishUIResult(new RefObjetoGX(referenciador)
                        {
                            // Si se llama al save, no podemos distingir entre inserciones y
                            // actualizaciones
                            PosibleFalsoPositivo = !referenciasSaveSeguras
                        });
                }
            }
        }

        /// <summary>
        /// Ejecuta la busqueda desde la interface de usuario
        /// </summary>
        override public void ExecuteUISearch()
        {

            // Revisar los referenciadores normales
            foreach( EntityReference r in Tabla.GetReferencesTo() ) 
            {

                if (SearchCanceled)
                    break;

                IncreaseSearchedObjects();

                // Ver si alguna de las operaciones que buscamos esta en la referencia a la tabla:
                TipoReferenciaTabla refTabla = new TipoReferenciaTabla(r.LinkTypeInfo);
                bool operationFound = false;
                foreach (TipoOperacion op in TiposReferencias) 
                {
                    if (refTabla.OperacionPresente(op)) 
                    {
                        // TODO: la propia referencia guarda el tipo de objeto que lo llama. El rendimiento
                        // TODO: mejoraria si se mira si r.from.type == ObjClass.Transaction antes de
                        // TODO: cargar el objeto
                        KBObject objeto = KBObject.Get(UIServices.KB.CurrentModel, r.From);
                        PublishUIResult(new RefObjetoGX(objeto));
                        operationFound = true;

                        if (KBaseGX.EsBussinessComponent(objeto))
                            // Guardar el BC referenciador
                            BcReferenciadores.Add(objeto);

                        break;
                    }
                }

                if (!operationFound && refTabla.OperationsAreUndefined) 
                { 
                    // If the object contains a reference to the table, but there is no operations
                    // defined on the reference, show a possibly false reference (unspecified 
                    // object?)
                    KBObject o = KBObject.Get(UIServices.KB.CurrentModel, r.From);
                    PublishUIResult(new RefObjetoGX(o)
                    {
                        PosibleFalsoPositivo = true
                    });
                }

            }

            // Revisar los referenciadores por uso de BC
            BuscarReferenciasBC();

        }

        /// <summary>
        /// Publica un objeto referenciador, si no se ha hecho ya antes
        /// </summary>
        protected override void PublishUIResult(object resultado, bool forceFound = false)
        {
            if (resultado is RefObjetoGX ) 
            {
                // No duplicar resultados
                RefObjetoGX referencia = (RefObjetoGX) resultado;
                if (ResultadosYaPublicados.Contains(referencia.IdObjetoGX))
                    return;
                ResultadosYaPublicados.Add(referencia.IdObjetoGX);
            }

            base.PublishUIResult(resultado, forceFound);
        }

    }
}
