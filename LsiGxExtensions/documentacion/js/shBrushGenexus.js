/**
 * SyntaxHighlighter
 * http://alexgorbatchev.com/SyntaxHighlighter
 *
 * SyntaxHighlighter is donationware. If you are using it, please donate.
 * http://alexgorbatchev.com/SyntaxHighlighter/donate.html
 *
 * @version
 * 3.0.83 (July 02 2010)
 * 
 * @copyright
 * Copyright (C) 2004-2010 Alex Gorbatchev.
 *
 * @license
 * Dual licensed under the MIT and GPL licenses.
 *
 *****************************************************************************************************
 *****************************************************************************************************
 * BRUSH DE GENEXUS POR LSI: http://www.lsisa.com
 * SI HAY ALGÚN ERROR, REPORTARLO EN http://sourceforge.net/p/lsigxextensions/tickets/
 *****************************************************************************************************
 *****************************************************************************************************
 * 
 */
;(function()
{
	// CommonJS
	typeof(require) != 'undefined' ? SyntaxHighlighter = require('shCore').SyntaxHighlighter : null;

	function Brush()
	{
		var keywords = 	'for each order none when using where in defined by blocking option distinct duplicate none ' +
						'endfor new endnew do case while exit step if else endif ' +
						'return commit rollback sql csharp dbase java xfor first to enddo cp eject footer header end lineno ' + 
						'mb mt noskip pl print delete refresh keep load in out inout line like';

        var rules = 'color default error_handler noaccept noload noread order parm prompt search workfile_lines ' + 
                    'xorder output_file printer accept add color default default_mode equal error ' + 
                    'noprompt refcall refmsg serial submit subtract';
            
        var subs = 'sub endsub endevent';
        
        var functions = 'AddMth AddYr AgeAsc Ask Average BrowserID BrowserVersion ByteCount CDoW Chr ' + 
            'CMonth Color Cols Concat Confirmed Count Create CreateFromURL CtoD CtoT Cursor Day ' + 
            'Decrypt64 DeleteFile  DFRClose DFRGDate  DFRGNum  DFRGTxt DFRNext DFROpen DFWClose DFWNext ' + 
            'DFWOpen DFWPDate DFWPNum DFWPTxt DoW DtoC Encrypt64 EoM Exists FileExist Find Format ' + 
            'GetCookie GetDataStore GetEncryptionKey GetLanguage GetLocation GetMessageText GetSOAPErr ' + 
            'GetSOAPErrMsg GetTheme GXGetMLi GXMLines Hour HTMLClean iif Int IsNull Len Link LoadBitmap ' + 
            'Lower LTrim Max Min Minute Mod Month NewLine Now Null NullValue OpenDocument PadL PadR ' + 
            'PathToUrl PrintDocument Random ReadRegKey RemoteAddr ReturnOnClick RGB Round RoundToEven ' + 
            'Rows RSeed RTrim Second ServerDate ServerNow ServerTime SetCookie SetEnvProperty ' + 
            'SetLanguage SetTheme SetUserId SetWrkSt Shell Sleep Space Str StrReplace StrSearch ' + 
            'StrSearchRev SubStr Sum SysDate SysTime TAdd TDiff Time Today ToFormattedString Trim ' + 
            'Trunc TtoC UDP UDF Upper UserCls UserID Val WriteRegKey WriteRegKey WrkSt XSLTApply ' + 
            'XToD Year YMDHMStoT YMDtoD';
        
		function fixComments(match, regexInfo)
		{
			var css = (match[0].indexOf("///*") == 0)
				? 'color1'
				: 'comments'
				;
			
			return [new SyntaxHighlighter.Match(match[0], match.index, css)];
		}

		this.regexList = [
			{ regex: SyntaxHighlighter.regexLib.singleLineCComments,	func : fixComments },	// one line comments
			{ regex: SyntaxHighlighter.regexLib.multiLineCComments,		css: 'comments' },		// multiline comments
			{ regex: new RegExp(this.getKeywords(keywords), 'gmi'),		css: 'keyword' },       // Genexus keywords on event / procedures
			{ regex: new RegExp(this.getKeywords(rules), 'gmi'),		css: 'rules' },		    // Genexus keywords on rules
			{ regex: /\&amp;\w+/g,										css: 'variable' },      // Variables
			{ regex: SyntaxHighlighter.regexLib.doubleQuotedString,		css: 'string' },	    // strings
			{ regex: SyntaxHighlighter.regexLib.singleQuotedString,		css: 'string' }, 	    // strings
			{ regex: new RegExp(this.getKeywords(subs), 'gmi'),         css: 'subroutines' },   // Events / subs definitions
			{ regex: /^\s*event\s.*$/gmi,                               css: 'subroutines' },   // Event definition start
			{ regex: new RegExp(this.getKeywords(functions), 'gmi'),    css: 'gxfunctions' }	// Genexus functions
			];
		
		this.forHtmlScript(SyntaxHighlighter.regexLib.aspScriptTags);
	};

	Brush.prototype	= new SyntaxHighlighter.Highlighter();
	Brush.aliases	= ['genexus', 'Genexus', 'GeneXus'];

	SyntaxHighlighter.brushes.Genexus = Brush;

	// CommonJS
	typeof(exports) != 'undefined' ? exports.Brush = Brush : null;
})();

