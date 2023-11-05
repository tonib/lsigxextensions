using System;
using System.Collections.Generic;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework.References;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Tokens;
using LSI.Packages.Extensiones.Utilidades.UI;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
    /// <summary>
    /// Busca tokens que hagaran referencia a un campo de un SDT / BC. P.ej. busca
    /// referencias al campo "CliCod" de un SDT "CliSdt" o a la funcion "save" de un BC "Tcliente"
    /// TODO: Renombrar esta clase a BuscadorTokensSdt
    /// </summary>
    public class BuscadorTokensSdtGx : UISearchBase
    {

        /// <summary>
        /// Funcion de los BC para cargar
        /// </summary>
        public const string FUNC_LOAD = "load";

        /// <summary>
        /// Funcion de los BC para guardar
        /// </summary>
        public const string FUNC_SAVE = "save";

        /// <summary>
        /// Funcion de los BC para guardar
        /// </summary>
        public const string FUNC_DELETE = "delete";

        /// <summary>
        /// Buscador de un texto el tipo &amp;variable.campo
        /// </summary>
        public TokensFinder BuscadorCampo;

        /// <summary>
        /// El modelo actual
        /// </summary>
        private KBModel CurrentModel;

        /// <summary>
        /// Agrega a la lista de sdts a revisar un sdt y sus referenciadores
        /// </summary>
        /// <param name="sdts">La lista de SDTS que hay que revisar</param>
        /// <param name="sdt">El sdt a añadir</param>
        private void AgregarSdtYReferenciadores(List<SDT> sdts, SDT sdt)
        {
            if (sdts.Contains(sdt))
                return;

            sdts.Add(sdt);

            // Revisar SDTs referenciadores
            foreach (EntityReference referencia in sdt.GetReferencesTo())
            {
                if (referencia.From.Type == ObjClass.SDT)
                {
                    SDT sdtReferenciador = (SDT) SDT.Get(CurrentModel, referencia.From);
                    AgregarSdtYReferenciadores(sdts, sdtReferenciador);
                }
            }

        }

        /// <summary>
        /// Crea el buscador para buscar referencias en los objetos
        /// </summary>
        private void Inicializar(IEnumerable<KBObject> sdtBc, KindOfIndirection tipoCampo, 
            List<string> campos)
        {

            CurrentModel = UIServices.KB.CurrentModel;

            // Configurar el token a buscar: Referencias a un campo/funcion incluido en la lista Campos
            // en cualquier variable
            TokenGx defToken = new TokenGx();
            defToken.AcceptIndirections = true;
            defToken.IndirectionsFilter = new TokenIndirectionFilter()
            {
                Kind = tipoCampo,
                PosicionFiltro = TokenIndirectionFilter.CUALQUIERPOSICION
            };

            // Nombre de campos a buscar
            defToken.IndirectionsFilter.MembersFilter = new List<string>();
            foreach (string campo in campos)
                defToken.IndirectionsFilter.MembersFilter.Add(campo.Trim().ToLower());

            // Tipos a buscar
            defToken.FiltroTipos = new FiltroTiposToken();
            foreach (KBObject tipo in sdtBc)
            {
                if (tipo is SDT)
                    //defToken.FiltroTipos.Sdts.Add((SDT)tipo);
                    AgregarSdtYReferenciadores(defToken.FiltroTipos.Sdts, (SDT)tipo);
                else if( KBaseGX.EsBussinessComponent(tipo) )
                    defToken.FiltroTipos.Bcs.Add((Transaction)tipo);
                else
                    throw new ArgumentException(tipo.QualifiedName + " no es un Sdt ni un Bc");
            }

            BuscadorCampo = new TokensFinder(defToken);

        }

        /// <summary>
        /// Constructor para buscar referencias a un campo / funcion de varios sdt / BC
        /// </summary>
        /// <param name="sdt">Sdt /bc del del que buscar referencias</param>
        /// <param name="tipoCampo">Indica si buscamos una funcion o un campo</param>
        /// <param name="campo">Nombre del campo o funcion que buscamos referencias</param>
        public BuscadorTokensSdtGx(IEnumerable<KBObject> sdtBc, KindOfIndirection tipoCampo, List<string> campos)
        {
            Inicializar(sdtBc, tipoCampo, campos);
        }

        /// <summary>
        /// Constructor para buscar referencias a un campo de un sdt / BC
        /// </summary>
        /// <param name="sdt">Sdt del campo del que buscar referencias</param>
        /// <param name="tipoCampo">Indica si buscamos una funcion o un sdt</param>
        /// <param name="campo">Campo del sdt</param>
        public BuscadorTokensSdtGx(KBObject sdtBc, KindOfIndirection tipoCampo, string campo)
        {
            List<KBObject> listaTipos = new List<KBObject>();
            listaTipos.Add(sdtBc);
            List<string> listaCampos = new List<string>();
            listaCampos.Add(campo);
            Inicializar(listaTipos, tipoCampo, listaCampos);
        }

        /// <summary>
        /// Constructor para buscar referencias a un campo de un sdt / BC
        /// </summary>
        /// <param name="sdt">Sdt del campo del que buscar referencias</param>
        /// <param name="tipoCampo">Indica si buscamos una funcion o un sdt</param>
        /// <param name="campo">Campo del sdt</param>
        public BuscadorTokensSdtGx(IEnumerable<KBObject> sdtBc, KindOfIndirection tipoCampo, string campo)
        {
            List<string> listaCampos = new List<string>();
            listaCampos.Add(campo);
            Inicializar(sdtBc, tipoCampo, listaCampos);
        }

        /// <summary>
        /// Ejecuta la busqueda
        /// </summary>
        public override void ExecuteUISearch()
        {

            // La lista de objetos ya revisados (un objeto se puede intentar revisar mas de una vez
            // si referencia a dos bc)
            HashSet<KBObject> revisados = new HashSet<KBObject>();

            // Revisar cada sdt / bc a buscar
            foreach (KBObject sdtBc in BuscadorCampo.Token.FiltroTipos.ObjetosFiltro())
            {
                // Revisar los referenciadores del bc / sdt:
                foreach (EntityReference r in sdtBc.GetReferencesTo())
                {

                    if (SearchCanceled)
                        break;

                    // Obtener el objeto referenciador:
                    KBObject referenciador = KBObject.Get(UIServices.KB.CurrentModel, r.From);

                    // Ver si ya se ha revisado
                    if (revisados.Contains(referenciador))
                        continue;
                    revisados.Add(referenciador);

                    // TODO: Objetos no soportados:
                    if (TokensFinder.IsUnsupportedObject(referenciador))
                    {
                        PublishUIResult(new RefObjetoGX(referenciador)
                            {
                                PosibleFalsoPositivo = true
                            });
                    }
                    else
                    {
                        // Revisar si contiene una referencia al campo / funcion:
                        if (ContieneReferencia(referenciador))
                            PublishUIResult(new RefObjetoGX(referenciador));
                    }
                    IncreaseSearchedObjects();
                }
            }

        }

        /// <summary>
        /// Verifica si un objeto contiene referencias al campo del sdt
        /// </summary>
        /// <param name="o">Objeto en el que buscar referencias</param>
        /// <returns>Cierto si el objeto contiene alguna referencia al campo del sdt</returns>
        public bool ContieneReferencia(KBObject o)
        {
            return BuscadorCampo.ContainsReference(o);
        }

    }
}
