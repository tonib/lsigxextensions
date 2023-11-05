using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Patterns.WorkWithDevices.Parts;
using Artech.Udm.Framework;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{

	/// <summary>
	/// Busca referencias de lectura / escritura a un atributo 
	/// </summary>
	public class BuscadorReferenciasAtributo : UISearchBase
    {

        /// <summary>
        /// Atributo al que buscar referencias
        /// </summary>
        private Artech.Genexus.Common.Objects.Attribute Atributo;

        /// <summary>
        /// Buscador de asignaciones al atributo
        /// </summary>
        private BuscadorAsignaciones BusAsignaciones;

        /// <summary>
        /// Buscador de referencias al atributo
        /// </summary>
        private TokensFinder BusAtributo;

        /// <summary>
        /// Buscador de referencias al campo del atributo en los BC que lo referencian
        /// </summary>
        private BuscadorTokensSdtGx BuscadorBc;

        /// <summary>
        /// Tipos de referencias a buscar (lecturas y/o escrituras)
        /// </summary>
        private List<TipoOperacion> TiposReferencias;

        /// <summary>
        /// La lista de objetos que referencian al atributo
        /// </summary>
        private List<EntityKey> Referenciadores = new List<EntityKey>();

        /// <summary>
        /// El modelo actual
        /// </summary>
        private KBModel ModeloActual;

        /// <summary>
        /// Una relacion de los BC que referencian al atributo a buscar, y los referenciadores de dicho
        /// BC
        /// </summary>
        private Dictionary<KBObject, HashSet<EntityKey>> BCReferenciadores =
            new Dictionary<KBObject, HashSet<EntityKey>>();

        /// <summary>
        /// If its not null, only objects that reference to this table will be reported.
        /// </summary>
        public Table TableFilter;

        /// <summary>
        /// Set of objects that reference to any BC that contains the attribute to search.
        /// </summary>
        private HashSet<EntityKey> AllBCReferencers = new HashSet<EntityKey>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="atributo">Atributo a buscar</param>
        /// <param name="tiposReferencias">Tipos de referencias a buscar (lectura y/o escritura)</param>
        public BuscadorReferenciasAtributo(Artech.Genexus.Common.Objects.Attribute atributo, List<TipoOperacion> tiposReferencias)
        {
            Atributo = atributo;
            TiposReferencias = tiposReferencias;
            if (TiposReferencias.Count == 0)
                throw new Exception("The kind of operation to search was not specified");

            // Inicializar los buscadores el atributo
            TokenGx tokenAtributo = new TokenGx(Atributo.Name);
            tokenAtributo.AcceptIndirections = true;
            BusAsignaciones = new BuscadorAsignaciones(tokenAtributo, false, true);
            BusAtributo = new TokensFinder(tokenAtributo);
            ModeloActual = UIServices.KB.CurrentModel;            
        }

        /// <summary>
        /// Guarda un BC referenciador al atributo, y los objetos que referencian a dicho BC
        /// </summary>
        private void GuardarBcReferenciador(Transaction bc)
        {
            if (!TiposReferencias.Contains(TipoOperacion.LECTURA))
            {
                // Solo buscamos escrituras: Ver si la transaccion BC escribe el atributo. Si no,
                // no hace falta revisarla
                if (!TransaccionEscribeAtributo(bc))
                    return;
            }

            // Obtener los referenciadores al BC:
            HashSet<EntityKey> referenciadoresBC = new HashSet<EntityKey>();
            foreach (EntityReference referencia in bc.GetReferencesTo(LinkType.UsedObject))
            {
                if (SearchCanceled)
                    break;
                referenciadoresBC.Add(referencia.From);
                AllBCReferencers.Add(referencia.From);
            }
            BCReferenciadores.Add(bc, referenciadoresBC);
        }

        /// <summary>
        /// It searches objects that reference to TableFilter, checking operations on the table. 
        /// </summary>
        /// <returns>Objects that reference to TableFilter. If TableFilter is null, it will
        /// return null</returns>
        private HashSet<EntityKey> GetTableFilterReferencers()
        {
            if (TableFilter == null)
                return null;

            // Get table filter referencers:
            PublishUIResult("Searching filter table references...");
            bool searchReadings = this.TiposReferencias.Contains(TipoOperacion.LECTURA);
            HashSet<EntityKey> tableFilterReferencers = new HashSet<EntityKey>();
            foreach (EntityReference reference in TableFilter.GetReferencesTo(LinkType.UsedObject))
            {
                // Check the reference operation:
                TipoReferenciaTabla refType = new TipoReferenciaTabla(reference.LinkTypeInfo);
                if (refType.OperationsAreUndefined)
                    tableFilterReferencers.Add(reference.From);
                else if (searchReadings)
                {
                    // Search read / write
                    tableFilterReferencers.Add(reference.From);
                }
                else
                {
                    // Search writtings only
                    if( refType.OperacionPresente(TipoOperacion.ESCRITURA) ||
                        refType.OperacionPresente(TipoOperacion.INSERCION) )
                        tableFilterReferencers.Add(reference.From);
                }
            }

            return tableFilterReferencers;
        }

        /// <summary>
        /// Busca los objetos y BC que referencian al atributo
        /// </summary>
        private void BuscarReferenciadores()
        {

            HashSet<EntityKey> tableFilterReferencers = GetTableFilterReferencers();

            PublishUIResult("Searching attribute references...");
            List<EntityKey> referencers = Atributo.GetReferencesTo(LinkType.UsedObject)
                .Select(x => x.From)
                .ToList();

            int contador = 0;

            // First search references from transactions (to store BC)
            foreach (EntityKey referencia in referencers.Where(x => x.Type == ObjClass.Transaction))
            {
                if (SearchCanceled)
                    return;

                if (tableFilterReferencers != null && !tableFilterReferencers.Contains(referencia))
                    // Transaction does not reference the table filter
                    continue;

                // Guardar la referencia
                Referenciadores.Add(referencia);

                // Ver si el objeto es un BC
                KBObject referenciador = KBObject.Get(ModeloActual, referencia);
                if (KBaseGX.EsBussinessComponent(referenciador))
                    GuardarBcReferenciador((Transaction)referenciador);

                contador++;
                if ((contador % 10) == 0)
                    PublishUIResult("Found " + contador + " references...");
            }

            // Then other kind of objects
            foreach (EntityKey referencia in referencers.Where(x => x.Type != ObjClass.Transaction))
            {
                if (SearchCanceled)
                    return;

                if (tableFilterReferencers != null ) 
                {
                    // If the object contains a reference to a BC, ignore the filter:
                    // The object will reference to the attribute, but not to the table
                    if (!AllBCReferencers.Contains(referencia) && !tableFilterReferencers.Contains(referencia))
                        continue;
                }

                // Guardar la referencia
                Referenciadores.Add(referencia);

                contador++;
                if ((contador % 10) == 0)
                    PublishUIResult("Found " + contador + " references...");
            }

            // Inicializar el buscador de referencias a los BC
            BuscadorBc = new BuscadorTokensSdtGx(BCReferenciadores.Keys,
                KindOfIndirection.FIELD, Atributo.Name);

        }


        /// <summary>
        /// Devuelve cierto si una parte un objeto referencia a un atributo
        /// </summary>
        /// <param name="parte">Parte del objeto</param>
        /// <param name="atributo">Atributo a verificar si es referenciado</param>
        /// <returns></returns>
        private bool ParteReferenciaAtributo(KBObjectPart parte)
        {
            if (parte is ISource)
                // Esto es mas rapido que la funcion general
                return FuenteReferenciaAtributo(parte);

            // SD panels crap
            if (parte is VirtualLayoutPart)
                return BusAtributo.ContainsReference((VirtualLayoutPart)parte, false);

            return ParteObjetoKb.ReferenciaEntidad(parte, Atributo.Key);
        }

        /// <summary>
        /// Devuelve cierto si una parte de codigo fuente referencia a un atributo
        /// Esta funcion es MUCHO mas rapida que GetPartReferences()
        /// </summary>
        private bool FuenteReferenciaAtributo(KBObjectPart fuente)
        {
            ParsedCode analizador = new ParsedCode(fuente);
            return BusAtributo.ContieneReferencia(analizador);
        }

        /// <summary>
        /// Devuelve cierto si el objeto contiene alguna referencia al atributo
        /// mediante algun BC
        /// </summary>
        /// <param name="objeto">Objeto a revisar</param>
        private bool ObjetoReferenciaAtributoBC(KBObject objeto)
        {
            // Ver si hay referencias a alguno de los BC por parte del objeto:
            foreach (KBObject bc in BCReferenciadores.Keys)
            {
                if (BCReferenciadores[bc].Contains(objeto.Key))
                {
                    // El objeto contiene referencias al BC. Ver si el atributo del BC
                    // es referenciado. Notese que esto busca referencias a cualquiera de los
                    // BC buscados.
                    if( TokensFinder.IsUnsupportedObject(objeto) )
                        // Vete a saber: Decimos que si
                        return true;
                    else
                        return BuscadorBc.ContieneReferencia(objeto);
                }
            }
            return false;
        }

        /// <summary>
        /// Devuelve cierto si alguna parte del objeto, excepto la de variables,
        /// referencia al atributo que buscamos
        /// </summary>
        /// <param name="objeto">Objeto a revisar</param>
        /// <returns>Cierto si el objeto referencia al atributo</returns>
        private bool ObjetoLeeAtributo(KBObject objeto)
        {

            // Objetos que referencian atributos, pero que no los leen:
            if (objeto is Table)
                return false;
            if (objeto is DataView)
                return false;
            if (objeto is DataViewIndex)
                return false;
            if (objeto is SDT)
                return false;
            if (objeto is Table)
                return false;

            // Buscar referencias de lectura: Buscar si alguna parte del objeto,
            // excepto la de variables, referencia al atributo:
            foreach (KBObjectPart parte in objeto.Parts.LsiEnumerate())
            {
                if (parte is VariablesPart)
                    continue;

                if (ParteReferenciaAtributo(parte))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Revisa si la transaccion escribe el atributo
        /// </summary>
        /// <param name="transaccion">Transaccion a revisar</param>
        /// <returns>Cierto si la transaccion escribe el atributo</returns>
        private bool TransaccionEscribeAtributo(Transaction transaccion)
        {
            // Revisar la estructura de la trn
            TransactionAttribute atr = transaccion.Structure.GetAttribute(Atributo);
            if (atr == null)
                // Si el atributo no esta en la estructura, no se escribe
                return false;

            // Ver si es inferido o forma parte de la estructura de la tabla:
            if (!atr.IsInferred)
                // Es escrito
                return true;

            // Si es inferido, puede ser escrito si hay una regla update o si contiene una asignacion
            // directa al atributo (esto ultimo no deberia hacerlo, pero revisando las navegaciones, lo hace)
            ParsedCode codigo = new ParsedCode(transaccion.Rules);
            if( ReglaUpdate.ContieneUpdateAtributo(codigo, Atributo) )
                return true;

            return BusAsignaciones.ContieneAsignacion(codigo);
        }

        /// <summary>
        /// Verifica si el objeto escribe el atributo que buscamos
        /// </summary>
        /// <param name="objeto">Objeto a revisar</param>
        /// <returns>Una referencia al objeto si el objeto modifica (o no se sabe si modifica) el atributo.
        /// null si el objeto no modifica el atributo</returns>
        private RefObjetoGX ObjetoEscribeAtributo(KBObject objeto)
        {

            if (objeto is Transaction && TransaccionEscribeAtributo((Transaction)objeto))
                // Es una transaccion, y escribe el atributo
                return new RefObjetoGX(objeto);

            // Si es un objeto no soportado, dar un posible falso positivo
            if (TokensFinder.IsUnsupportedObject(objeto))
                return new RefObjetoGX(objeto)
                {
                    PosibleFalsoPositivo = true
                };

            // Solo se permiten escrituras de atributos en procedures:
            if (!(objeto is Procedure))
                return null;

            // Ver si hay asignaciones en el codigo de la parte de procedimiento
            ProcedurePart codigo = objeto.Parts.LsiGet<ProcedurePart>();
            ParsedCode analizador = new ParsedCode(codigo);
            if (BusAsignaciones.ContieneAsignacion(analizador))
                return new RefObjetoGX(objeto);
            else
                return null;
        }

        /// <summary>
        /// Ejecuta la busqueda desde la interface de usuario
        /// </summary>
        override public void ExecuteUISearch()
        {

            // Buscar los objetos que referencian al atributo.
            BuscarReferenciadores();

            // Revisar los referenciadores
            NToSearch = Referenciadores.Count;
            foreach (EntityKey claveObjeto in Referenciadores)
            {
                if (SearchCanceled)
                    break;

                // Cargar el objeto
                KBObject objeto = KBObject.Get(ModeloActual, claveObjeto);

                IncreaseSearchedObjects();

                bool revisadaLectura = false;
                if (TiposReferencias.Contains(TipoOperacion.LECTURA) &&
                    ObjetoLeeAtributo(objeto))
                {
                    revisadaLectura = true;
                    // Encontrada una referencia de lectura
                    PublishUIResult(new RefObjetoGX(objeto)
                    {
                        PosibleFalsoPositivo = TokensFinder.IsUnsupportedObject(objeto)
                    });
                    continue;
                }

                // Las referencias de lectura ya encuentran las escrituras
                if (!revisadaLectura && TiposReferencias.Contains(TipoOperacion.ESCRITURA) )
                {
                    RefObjetoGX referencia = ObjetoEscribeAtributo(objeto);
                    if (referencia != null)
                    {
                        // Encontrada una referencia de escritura
                        PublishUIResult(referencia);
                        continue;
                    }
                }

                // Ver si el objeto contiene referencias a un BC que referencie al atributo. 
                // La referencia no es segura.
                if (ObjetoReferenciaAtributoBC(objeto))
                {
                    // Encontrada referencia
                    PublishUIResult(new RefObjetoGX(objeto)
                    {
                        PosibleFalsoPositivo = true
                    });
                    continue;
                }

            }

        }
    }
}
