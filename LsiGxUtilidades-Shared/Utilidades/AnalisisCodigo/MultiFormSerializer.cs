using Artech.Architecture.Common.Objects;
using Artech.Common.Diagnostics;
using Artech.Common.Exceptions;
using Artech.Genexus.Common.Parts;
using Artech.Genexus.Common.Parts.WebForm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace LSI.Packages.Extensiones.Utilidades.AnalisisCodigo
{
	public class MultiFormSerializer
	{
		public class Form
		{
			public int Id;

			public IMultiFormHandler Handler;

			public XmlElement RootElement;

			public Form(int id, IMultiFormHandler handler, XmlElement root)
			{
				this.Id = id;
				this.Handler = handler;
				this.RootElement = root;
			}

			public string GetXml(KBObject kbObj, GetXmlOptions options)
			{
				return this.Handler.GetXml(kbObj, this.RootElement, options);
			}

			public void Convert(KBObject kbObj, Version version)
			{
				if (version < this.Handler.Version)
				{
					this.Handler.Convert(kbObj, this.RootElement, version);
				}
			}

			public void Import(KBObject kbObj, Version version, Action<string, MessageLevel, string> addError)
			{
				this.Handler.Import(kbObj, this.RootElement, version, addError);
			}

			public void Export(KBObject kbObj)
			{
				this.Handler.Export(kbObj, this.RootElement);
			}

			public void Export(KBObject kbObj, SerializationMode mode)
			{
				this.Handler.Export(kbObj, this.RootElement, mode);
			}
		}

	private const string XML_ROOT_NAME = "GxMultiForm";

	private const string ATT_ROOT_FORM_ID = "rootId";

	private const string ATT_VERSIONS_ID = "version";

	private const string XML_FORM_NAME = "Form";

	private const string ATT_FORM_ID = "id";

	private const string ATT_FORM_TYPE = "type";

	private static void CheckNull(ref XmlDocument xmlDoc)
	{
		if (xmlDoc == null || xmlDoc.DocumentElement == null)
		{
			xmlDoc = new XmlDocument();
			xmlDoc.AppendChild(xmlDoc.CreateElement("BODY"));
		}
	}

	public static IEnumerable<MultiFormSerializer.Form> GetForms(XmlDocument xmlDoc)
	{
		MultiFormSerializer.CheckNull(ref xmlDoc);
		XmlElement documentElement = xmlDoc.DocumentElement;
		if (documentElement.Name != "GxMultiForm")
		{
			yield return new MultiFormSerializer.Form(1, MultiForm.Html, xmlDoc.DocumentElement);
		}
		else
		{
			foreach (XmlElement xmlElement in documentElement.SelectNodes("Form"))
			{
				if (xmlElement.ChildNodes.Count >= 1)
				{
					string attribute = xmlElement.GetAttribute("id");
					int id;
					if (attribute != null && int.TryParse(attribute, out id))
					{
						string attribute2 = xmlElement.GetAttribute("type");
						yield return new MultiFormSerializer.Form(id, MultiForm.Get(attribute2), (XmlElement)xmlElement.ChildNodes[0]);
					}
				}
			}
			IEnumerator enumerator = null;
		}
		yield break;
		yield break;
	}

	public static bool IsOnlyHtml(XmlDocument xmlDoc)
	{
		if (xmlDoc == null)
		{
			return true;
		}
		XmlElement documentElement = xmlDoc.DocumentElement;
		return documentElement == null || documentElement.Name != "GxMultiForm";
	}

	public static int GetRootFormId(XmlDocument xmlDoc)
	{
		if (xmlDoc == null)
		{
			return 1;
		}
		XmlElement documentElement = xmlDoc.DocumentElement;
		if (documentElement == null || documentElement.Name != "GxMultiForm")
		{
			return 1;
		}
		string attribute = documentElement.GetAttribute("rootId");
		int result;
		if (attribute != null && int.TryParse(attribute, out result))
		{
			return result;
		}
		return 1;
	}

	public static IDictionary<string, Version> GetVersions(XmlDocument xmlDoc)
	{
		if (xmlDoc == null)
		{
			return null;
		}
		XmlElement documentElement = xmlDoc.DocumentElement;
		if (documentElement == null || documentElement.Name != "GxMultiForm")
		{
			return null;
		}
		string attribute = documentElement.GetAttribute("version");
		if (attribute == null)
		{
			return null;
		}
		Dictionary<string, Version> dictionary = new Dictionary<string, Version>();
		string[] array = attribute.Split(new char[]
		{
				';'
		});
		for (int i = 0; i < array.Length; i++)
		{
			string text = array[i];
			string[] array2 = text.Split(new char[]
			{
					':'
			});
			if (array2.Length >= 2)
			{
				dictionary.Add(array2[0].Trim(), new Version(array2[1].Trim()));
			}
		}
		return dictionary;
	}

	public static void SetCurrentVersions(XmlDocument xmlDoc)
	{
		/*if (xmlDoc == null)
		{
			return;
		}
		XmlElement documentElement = xmlDoc.DocumentElement;
		if (documentElement == null || documentElement.Name != "GxMultiForm")
		{
			return;
		}
		documentElement.SetAttribute("version", MultiFormSerializer.GetCurrentVersionsValue());*/
	}

	public static XmlDocument SaveForms(int rootFormId, IEnumerable<MultiFormSerializer.Form> forms)
	{
			/*List<MultiFormSerializer.Form> list = new List<MultiFormSerializer.Form>(forms);
			XmlDocument xmlDocument = new XmlDocument();
			if (list.Count == 1)
			{
				MultiFormSerializer.Form form = list[0];
				if (form.Handler == MultiForm.Html)
				{
					if (form.RootElement != null)
					{
						xmlDocument.LoadXml(form.RootElement.OuterXml);
					}
					return xmlDocument;
				}
			}
			XmlElement xmlElement = xmlDocument.CreateElement("GxMultiForm");
			xmlDocument.AppendChild(xmlElement);
			xmlElement.SetAttribute("rootId", rootFormId.ToString());
			xmlElement.SetAttribute("version", MultiFormSerializer.GetCurrentVersionsValue());
			foreach (MultiFormSerializer.Form current in list)
			{
				XmlElement xmlElement2 = xmlDocument.CreateElement("Form");
				xmlElement.AppendChild(xmlElement2);
				xmlElement2.SetAttribute("id", current.Id.ToString());
				xmlElement2.SetAttribute("type", current.Handler.XmlTypeName);
				xmlElement2.InnerXml = current.RootElement.OuterXml;
			}
			return xmlDocument;*/
			return null;
	}

	public static string BeginMultiFormXml(int rootFormId)
	{
			/*return string.Format("<{0} {1}=\"{2}\" {3}=\"{4}\">", new object[]
			{
					"GxMultiForm",
					"rootId",
					rootFormId.ToString(),
					"version",
					MultiFormSerializer.GetCurrentVersionsValue()
			});*/
			return null;
	}

	public static string EndMultiFormXml()
	{
		return string.Format("</{0}>", "GxMultiForm");
	}

	public static string BeginFormXml(int formId, IMultiFormHandler formHandler)
	{
		return string.Format("<{0} {1}=\"{2}\" {3}=\"{4}\">", new object[]
		{
				"Form",
				"id",
				formId,
				"type",
				formHandler.XmlTypeName
		});
	}

	public static string EndFormXml()
	{
		return string.Format("</{0}>", "Form");
	}

	public static XmlDocument ExpandForms(KBObject kbObj, XmlDocument xmlDoc, GetXmlOptions options)
	{
		MultiFormSerializer.CheckNull(ref xmlDoc);
		if (xmlDoc.DocumentElement.Name != "GxMultiForm")
		{
			return xmlDoc;
		}
		XmlDocument doc = new XmlDocument();
		int rootFormId = MultiFormSerializer.GetRootFormId(xmlDoc);
		Dictionary<int, MultiFormSerializer.Form> dictionary = new Dictionary<int, MultiFormSerializer.Form>();
		foreach (MultiFormSerializer.Form current in MultiFormSerializer.GetForms(xmlDoc))
		{
			dictionary.Add(current.Id, current);
		}
		GetXmlOptions getXmlOptions = options.Clone();
		getXmlOptions.IncludeBody = true;
		/*using (new ScopedValuesCache())
		{*/
			MultiFormSerializer.ExpandForms_SetXml(kbObj, rootFormId, getXmlOptions, delegate (string html)
			{
				doc.LoadXml(html);
			}, () => doc.DocumentElement, dictionary);
		//}
		return doc;
	}

	private static void ExpandForms_ReplaceInnerForms(KBObject kbObj, XmlElement xmlRoot, GetXmlOptions options, Dictionary<int, MultiFormSerializer.Form> map)
	{
		IEnumerator enumerator = xmlRoot.SelectNodes(".//gxLayout").GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				XmlElement xmlElement = (XmlElement)enumerator.Current;
				XmlElement divElem = xmlElement.OwnerDocument.CreateElement("div");
				string attribute = xmlElement.GetAttribute("formId");
				string attribute2 = xmlElement.GetAttribute("ControlName");
				xmlElement.ParentNode.ReplaceChild(divElem, xmlElement);
				int formId;
				if (attribute != null && int.TryParse(attribute, out formId))
				{
					MultiFormSerializer.ExpandForms_SetXml(kbObj, formId, options, delegate (string html)
					{
						divElem.InnerXml = html;
					}, () => divElem, map);
				}
				if (attribute2 != null)
				{
					divElem.SetAttribute("id", attribute2);
				}
			}
		}
		finally
		{
			IDisposable disposable = enumerator as IDisposable;
			if (disposable != null)
			{
				disposable.Dispose();
			}
		}
	}

	private static void ExpandForms_SetXml(KBObject kbObj, int formId, GetXmlOptions options, Action<string> setXml, Func<XmlElement> nextXmlRoot, Dictionary<int, MultiFormSerializer.Form> map)
	{
		string text = null;
		MultiFormSerializer.Form form;
		if (map.TryGetValue(formId, out form))
		{
			map.Remove(formId);
			string xml = form.GetXml(kbObj, options);
			if (xml != null)
			{
				try
				{
					setXml(xml);
					GetXmlOptions getXmlOptions = options.Clone();
					getXmlOptions.IncludeBody = false;
					MultiFormSerializer.ExpandForms_ReplaceInnerForms(kbObj, nextXmlRoot(), getXmlOptions, map);
					goto IL_8F;
				}
				catch (Exception ex)
				{
					text = string.Format("{0}<p>{1}</p>", HtmlHelpers.ConvertToSpecialXMLCharacters(ex.Message), HtmlHelpers.ConvertToSpecialXMLCharacters(xml));
					goto IL_8F;
				}
			}
			text = string.Format("convertion of formId={0} returned null", formId);
		}
		else
		{
			text = string.Format("formId={0} not found", formId);
		}
	IL_8F:
		if (text != null)
		{
			text = string.Format("{1}<div style=\"COLOR: red; BORDER: red 1px solid\">Internal Error, {0}</div>{2}", text, options.IncludeBody ? "<BODY>" : "", options.IncludeBody ? "</BODY>" : "");
			try
			{
				setXml(text);
			}
			catch (Exception ex2)
			{
				ExceptionManager.LogException(ex2);
			}
		}
	}
}
}
