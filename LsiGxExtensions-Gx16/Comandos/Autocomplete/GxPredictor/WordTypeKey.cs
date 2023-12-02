using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
    /// <summary>
    /// Word types for prediction
    /// </summary>
    public enum WordTypeKey
    {
        Keyword = 0,

        StringConstant = 1,

        DecimalConstant = 2,

        IntegerConstant = 3,

        KbObject = 4,

        Function = 5,

        Attribute = 6,

        Variable = 7,

        StandardMember = 8,

        OtherMember = 9,

        UnknownIdentifier = 10,

        /// <summary>
        /// UI control name
        /// </summary>
        Control = 11
    }
    
    static public class WordTypeKeyExtensions
    {

        static public bool IsTrainable(this WordTypeKey wordType)
        {
            return wordType == WordTypeKey.Keyword || wordType == WordTypeKey.KbObject || wordType == WordTypeKey.Function ||
                wordType == WordTypeKey.Attribute || wordType == WordTypeKey.Variable || wordType == WordTypeKey.StandardMember ||
                wordType == WordTypeKey.OtherMember || wordType == WordTypeKey.UnknownIdentifier ||
                wordType == WordTypeKey.Control;
        }

        static public bool CanHaveUIControl(this WordTypeKey wordType)
        {
            return wordType == WordTypeKey.Attribute || wordType == WordTypeKey.Variable || wordType == WordTypeKey.Control;
        }

		/// <summary>
		/// This word type has a customizable name? (variables, atributes, SDT members, etc)
		/// </summary>
		/// <param name="wordType">Word to check</param>
		/// <returns>True if this word type has name</returns>
		static public bool HasName(this WordTypeKey wordType)
		{
			return wordType == WordTypeKey.Attribute || wordType == WordTypeKey.KbObject ||
				wordType == WordTypeKey.OtherMember || wordType == WordTypeKey.Variable ||
				wordType == WordTypeKey.Control;
		}
	}
}
