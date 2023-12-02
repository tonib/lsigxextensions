using Artech.Architecture.Common.Objects;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Udm.Framework;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor;
using LSI.Packages.Extensiones.Utilidades.CallsAnalisys;
using LSI.Packages.Extensiones.Utilidades.GxClassExtensions;
using LSI.Packages.Extensiones.Utilidades.Logging;
using LSI.Packages.Extensiones.Utilidades.Threading;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames
{

	// TODO: Make this thread safe

	/// <summary>
	/// Background task to prefetch names info refernced in open objects. 
	/// To get an acceptable performance for autocompletion
	/// </summary>
	class OpenObjectsPrefech : IDisposable
	{

		/// <summary>
		/// Prefech info referenced objects from a given object
		/// </summary>
		class PrefechObjectReferences : IExecutable
		{
			EntityKey EntityKey;
			ObjectNamesCache NamesCache;
			ColaTareas<PrefechObjectReferences> PendingTasks;

			public PrefechObjectReferences(ObjectNamesCache namesCache, EntityKey entityKey, ColaTareas<PrefechObjectReferences> pendingTasks)
			{
				NamesCache = namesCache;
				EntityKey = entityKey;
				PendingTasks = pendingTasks;
			}

			public void Execute()
			{
				try
				{
					KBObject o = UIServices.KB.CurrentModel.Objects.Get(EntityKey);
					if (o == null)
						return;

					// References to objects with lazy loading info: Attributes or callable objects with parameters
					//Stopwatch time = new Stopwatch();
					//time.Start();
					//int nAtts = 0, nCallable = 0;
					foreach(var reference in o.GetReferences()
						.Where(r => r.To.Type == ObjClass.Attribute || ObjClassLsi.LsiIsCallableType(r.To.Type) || r.To.Type == ObjClass.SDT))
					{

						if(reference.To.Type == ObjClass.Attribute)
						{
							// Load attribute extra info, if it's not loaded
							AttributeNameInfo attInfo = NamesCache.GetNameByKey(reference.To) as AttributeNameInfo;
							if (attInfo == null)
								continue;
							attInfo.LoadAttribute();
							// nAtts++;
						}
						else if(reference.To.Type == ObjClass.SDT)
						{
							NamesCache.SdtStructuresCache.GetSdtInfo(o.Model, reference.To);
						}
						else
						{
							// Callable object. Cache object signature
							if (KbSignaturesCache.ObjectSignatureCached(reference.To))
								continue;
							ICallableInfo callable = UIServices.KB.CurrentModel.Objects.Get(reference.To) as ICallableInfo;
							if (callable == null)
								continue;
							KbSignaturesCache.GetMainSignature(callable);
							// nCallable++;
						}

						if (PendingTasks.Disposed)
							// Kb is closing, terminate
							return;
					}

					//time.Stop();
					//using (Log l = new Log(borrarLogPrevio: false, false))
					//{
					//	l.Output.AddLine($"{o.Name}: N.atts: {nAtts}, N. callable: {nCallable}, miliseconds: {time.ElapsedMilliseconds}");
					//}
				}
#if DEBUG
				catch (Exception ex)
				{
					Log.ShowException(ex);
				}
#else
				catch { }
#endif
			}
		}

		ObjectNamesCache NamesCache;
		ColaTareas<PrefechObjectReferences> PendingTasks = new ColaTareas<PrefechObjectReferences>(1, 0);

		public OpenObjectsPrefech(ObjectNamesCache namesCache) 
		{
			NamesCache = namesCache;

			// Queue objects currently open
			UIServices.DocumentManager.OpenedDocuments().ToList().ForEach(o => ObjectOpen(o.Key));

			// Get events about open objects
			UIServices.DocumentManager.AfterOpenDocument += DocumentManager_AfterOpenDocument;
		}

		private void DocumentManager_AfterOpenDocument(object sender, Artech.Architecture.UI.Framework.Objects.DocumentEventArgs e)
		{
			// Prefetch open object references to make predictions faster
			ObjectOpen(e.Document.Key);
		}

		private void ObjectOpen(EntityKey objectKey)
		{
			if (!NamesCache.Ready)
				return;

			if (PendingTasks.Disposed)
				// Closing kb
				return;

			if (Package.Importing)
				// Importing objects
				return;

			if (!Autocomplete.SupportedPartTypes.Any(part => part.ObjectType == objectKey.Type))
				// Unsupported object
				return;

			// Queue object to prefetch its references
			PendingTasks.NuevaTarea(new PrefechObjectReferences(NamesCache, objectKey, PendingTasks));
		}

		public void Dispose()
		{
			// Closing kb
			PendingTasks.Dispose();
		}
	}
}
