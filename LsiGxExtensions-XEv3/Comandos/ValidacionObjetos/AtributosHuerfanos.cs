using Artech.Architecture.Common.Objects;
using Artech.Architecture.Language.Parser.Data;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.Layout;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Patterns.WorkWithDevices.Parts;
using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo;
using LSI.Packages.Extensiones.Utilidades.AnalisisCodigo.Reglas;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Validation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LSI.Packages.Extensiones.Comandos.ValidacionObjetos
{
	/// <summary>
	/// Utilidad para buscar atributos que son referenciados fuera de un contexto de acceso
	/// a la  base de datos: Fuera de un FOR EACH / NEW, en un winform que no referencia atributos, etc
	/// </summary>
	public class AtributosHuerfanos : IValidator
    {

        /// <summary>
        /// Objeto a validar
        /// </summary>
        private ValidationTask Validador;

        /// <summary>
        /// Buscador de atributos huerfanos
        /// </summary>
        private OrphanAttributesFinder OrphansFinder;

        /// <summary>
        /// La lista de atributos huerfanos
        /// </summary>
        private HashSet<string> Huerfanos = new HashSet<string>();

        /// <summary>
        /// La parte de codigo principal (eventos/procedure)
        /// </summary>
        private ParsedCode CodigoCuerpo;

        /// <summary>
        /// La parte de codigo de las reglas
        /// </summary>
        private ParsedCode CodigoReglas;
 
		public AtributosHuerfanos()
		{
			OrphansFinder = new OrphanAttributesFinder(LsiExtensionsConfiguration.Load().CheckOrphanInsideSubs);
		}

        /// <summary>
        /// Revisa si el objeto es un win/webform con atributos en la interface de usuario o en algun
        /// evento LOAD
        /// </summary>
        /// <returns>Cierto si el form referencia atributos</returns>
        private bool EsFormConAtributos()
        {

            bool esForm = false;
            WebPanel webpanel = Validador.ObjectToCheck as WebPanel;
            if (webpanel != null)
            {
                // Ver si la UI contiene algun atributo:
                if( OrphansFinder.ContieneReferencia(webpanel.WebForm) )
                    return true;
                esForm = true;
            }

            WorkPanel workpanel = Validador.ObjectToCheck as WorkPanel;
            if( workpanel != null) 
            {
                // Ver si la UI contiene algun atributo:
                if( OrphansFinder.ContieneReferencia(workpanel.WinForm) )
                    return true;
                esForm = true;
            }

            SDPanel sd = Validador.ObjectToCheck as SDPanel;
            if (sd != null)
            {
                // Ver si la UI contiene algun atributo:
                if (OrphansFinder.ContainsReference(sd.Parts.LsiGet<VirtualLayoutPart>()))
                    return true;
                esForm = true;
            }

            if (esForm)
            {
                // Ver si el form tiene eventos load con atributos huerfanos. 
                // Si es asi, tambien hay que ignorarlo (el form por si solo tiene tabla)
                BuscadorEventos buscadorLoads = new BuscadorEventos(EventoGX.LOAD);
                foreach (CommandEvent eventoLoad in buscadorLoads.BuscarTodos(CodigoCuerpo))
                {
                    if (OrphansFinder.ContieneReferencia(Validador.ObjectToCheck, eventoLoad))
                        return true;
                }

                // Ver si el form contiene una regla hidden con atributos. 
                // Si es asi, hay tabla base y hay que ignorar el objeto
                BuscadorReglas buscadorHidden = new BuscadorReglas(BuscadorReglas.REGLAHIDDEN);
                Rule reglaHidden = buscadorHidden.BuscarPrimera(CodigoReglas);
                if (reglaHidden != null)
                {
                    // Buscar atributos en la regla hidden
                    if (OrphansFinder.ContieneReferencia(Validador.ObjectToCheck, reglaHidden))
                        // Hay atributos en la regla hidden: Ignorar el objeto
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Añade nombre de atributos huerfanos a la lista de resultados
        /// </summary>
        /// <param name="nombresHuerfanos">Nombre de atributos huerfanos a añadir</param>
        private void AgregarNombresHuerfanos(IEnumerable<string> nombresHuerfanos)
        {
            foreach (string nombreAtributo in nombresHuerfanos)
                Huerfanos.Add(nombreAtributo);
        }

        /// <summary>
        /// Busca atributos huerfanos en una parte del objeto
        /// </summary>
        /// <param name="parte">Parte a revisar</param>
        private void RevisarParte(KBObjectPart parte)
        {
            if (parte == null)
                return;

            ParsedCode codigo = new ParsedCode(parte);
            AgregarNombresHuerfanos(OrphansFinder.SearchAllNames(codigo, false));
        }

        /// <summary>
        /// Parsea y guarda las partes del objeto que se van a revisar mas de una vez, para que sea 
        /// mas rapido
        /// </summary>
        /// <returns>Falso si no se encontro alguna de las partes</returns>
        private bool ParsearPartesComunes()
        {
            // Parte principal (eventos / procedure)
            KBObjectPart partePrincipal = null;
            if (Validador.ObjectToCheck is WebPanel || Validador.ObjectToCheck is WorkPanel || 
                Validador.ObjectToCheck is SDPanel ) 
                partePrincipal = Validador.ObjectToCheck.Parts.LsiGet<EventsPart>();
            else if( Validador.ObjectToCheck is Procedure )
                partePrincipal = Validador.ObjectToCheck.Parts.LsiGet<ProcedurePart>();
            if (partePrincipal == null)
                return false;
            CodigoCuerpo = new ParsedCode(partePrincipal);

            // Reglas
            RulesPart reglas = Validador.ObjectToCheck.Parts.LsiGet<RulesPart>();
            if (reglas == null)
                return false;
            CodigoReglas = new ParsedCode(reglas);
            return true;
        }

		/// <summary>
		/// Check object main code part (Events/Procedure)
		/// </summary>
		private void CheckMainCodePart()
		{
			IEnumerable<string> orphanAttributes = OrphansFinder.SearchAllNames(CodigoCuerpo, false);
			if (OrphansFinder.HasNewCommands)
			{
				// Exception: If attribute is Autonumber, the only way to get the assigned value is reference the attribute outside the NEW
				// See: https://sourceforge.net/p/lsigxextensions/tickets/3/
				orphanAttributes = orphanAttributes
					.Select(attName => Artech.Genexus.Common.Objects.Attribute.Get(Validador.ObjectToCheck.Model, attName))
					.Where(att => att != null)
					.Where(att => !att.GetPropertyValue<bool>(Properties.ATT.Autonumber)) // Ignore autonumber attributes
					.Select(att => att.Name);
			}

			AgregarNombresHuerfanos(orphanAttributes);
		}

        /// <summary>
        /// Ejecuta la revision del objeto
        /// </summary>
        public void Validate(ValidationTask task)
        {
            try
            {
                if( !LsiExtensionsConfiguration.Load().CheckOrphanAttributes )
                    return;

                this.Validador = task;

                // Calcular cuanto tiempo tarda el proceso.
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                if (Validador.ObjectToCheck is Transaction)
                    // Las transacciones no se revisan
                    return;

                // Obtener partes de codigo que se van a revisar mas de una vez:
                if (!ParsearPartesComunes())
                    // Algo malo ha pasado. Salir
                    return;

                // Ver si es un form que tiene tabla base. Si es asi, no revisarlo
                if (EsFormConAtributos())
                    return;

				// Check object main code part (Events/Procedure)
				CheckMainCodePart();

				// Revisar las reglas
				AgregarNombresHuerfanos(OrphansFinder.SearchAllNames(CodigoReglas, false));

                // Si no se ha encontrado ningun for each, no deberia haber ningun atributo en 
                // las conditions. 
                // TODO: De hecho, las conditions deberian estar vacias. No se revisa
                if (!OrphansFinder.ContainsForEachs)
                    RevisarParte(Validador.ObjectToCheck.Parts.LsiGet<ConditionsPart>());

                if (OrphansFinder.PrintBlocksImpresos.Count > 0)
                {
                    // Se han encontrado impresiones de printblocks fuera de for eachs
                    // Revisar si contienen atributos
                    LayoutPart layout = Validador.ObjectToCheck.Parts.LsiGet<LayoutPart>();
                    if (layout != null)
                    {
                        foreach (IReportBand printBlock in layout.Layout.ReportBands)
                        {
                            if( OrphansFinder.PrintBlocksImpresos.Contains(printBlock.Name.ToLower()) )
                                // Revisar el printblock en busca de atributos:
                                AgregarNombresHuerfanos(OrphansFinder.BuscarTodosNombres(printBlock, false));
                        }
                    }
                }

                // Todos los atributos que aparecen en la regla parm y que aparecen referenciados en
                // en alguna otra parte del objeto fuera de un FOR EACH / NEW no son huerfanos. 
                // Los atributos que estan en el parm y no aparecen en ningun sitio son huerfanos.
                foreach (string atributoReglaParm in OrphansFinder.AtributosParametros)
                {
                    if (Huerfanos.Contains(atributoReglaParm))
                        // El atributo de la regla parm es referenciado en alguna parte del objeto
                        Huerfanos.Remove(atributoReglaParm);
                    else if (!OrphansFinder.HasDbAcessCommands)
                        // El atributo esta en la regla parm, no aparece en ningun otro sitio y no
                        // hay accesos a la bbdd en el objeto: Es un error
                        Huerfanos.Add(atributoReglaParm);
                }

                stopWatch.Stop();

                if (Huerfanos.Count > 0)
                {
                    // Hay errores:
                    Validador.InicializarOutput(false);
                    Validador.Log.Output.AddWarningLine(
                        "Object " + Validador.ObjectToCheck.QualifiedName + ": There are attributes referenced outside FOR EACH / NEW statements. The attributes are:");
                    foreach (string nombreAtributo in Huerfanos)
                        Validador.Log.Output.AddLine(nombreAtributo);
                    Validador.Log.PrintExecutionTime(stopWatch);
                }
            }
            catch (Exception ex)
            {
                Log.ShowException(ex);
            }
        }

    }
}
