using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSI.Packages.Extensiones.Utilidades.GxClassExtensions
{
	/// <summary>
	/// Extensions for ObjClass. It's a static class, so we cannot extend
	/// </summary>
	public class ObjClassLsi
	{
		static Guid _Module;

		/// <summary>
		/// Module class id (not defined in ObjClass)
		/// </summary>
		static public Guid Module
		{
			get
			{
				if (_Module != null)
					_Module = typeof(Module).GUID;
				return _Module;
			}
		}

		static public string GetClassName(Guid cls)
		{
			// ObjClass.GetType returns null for these in GxXEv3:
			if (cls == ObjClass.SDPanel)
				return "SDPanel";
			else if (cls == ObjClassLsi.Module)
				return "Module";
			else if (cls == ObjClass.WorkWithDevices)
				return "WorkWithDevices";
			else if (cls == ObjClass.Folder)
				return "Folder";
			else if (cls == ObjClass.TranslationMessage)
				return "TranslationMessage";
			else if(cls == ObjClass.Dashboard)
				return "Dashboard";
			else if (cls == ObjClass.ThemeClass)
				return "ThemeClass";
			else if (cls == ObjClass.ThemeTransformation)
				return "ThemeTransformation";
			else if (cls == ObjClass.ThemeColor)
				return "ThemeColor";

			Type t = ObjClass.GetType(cls);
			return t?.Name;
		}

		static public bool LsiIsCallableType(Guid cls)
		{
			// Ev3U3: ObjClass.IsCallableType() returns false for SdPanels...
			return ObjClass.IsCallableType(cls) || cls == ObjClass.SDPanel;
		}

		static public List<Guid> GetCallableTypes()
		{
			return KnowledgeBase.GetKBObjectTypes().Where(t => LsiIsCallableType(t)).ToList();
		}
	}
}
