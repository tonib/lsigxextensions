using LSI.Packages.Extensiones.Utilidades;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.Commands
{
    /// <summary>
    /// Commands definition and autocomplete
    /// </summary>
    class CommandsAutocomplete
    {

        /// <summary>
        /// Event command
        /// </summary>
        public const string EVENT = "Event";

        static public List<AutocompleteItem> GetCommands(AutocompleteContext context)
        {
            List<AutocompleteItem> result = new List<AutocompleteItem>();

            foreach ( CommandDefinition cmd in Definitions)
                cmd.AddWords(context, result);
            return result;
        }

        static public bool AutocompleteEnabled(LineParser lineParser)
        {
            // Autocomplete should be disabled after native commands. Check the line first token
            if (lineParser.CompletedTokensCount == 0)
                return true;
            string firstLineToken = lineParser.GetCompletedTextToken(0).ToLower();

            if (KeywordGx.NATIVECODEKEYWORDS.Any(x => x.ToLower() == firstLineToken))
                // Native code: Disable autocomplete
                return false;

            return true;
        }

        static private List<CommandDefinition> _Definitions;

        static CommandDefinition AddDefinition(CommandDefinition cmd)
        {
            _Definitions.Add(cmd);
            return cmd;
        }

		static private ObjectPartType[] EventsAndProcedures => 
            new ObjectPartType[] { ObjectPartType.Procedure , ObjectPartType.TransactionEvents ,
                ObjectPartType.WebPanelEvents , ObjectPartType.WorkPanelEvents, ObjectPartType.SDPanelEvents };

        static private ObjectPartType[] EventsAndProceduresButTransactions =>
            new ObjectPartType[] { ObjectPartType.Procedure , 
                ObjectPartType.WebPanelEvents ,ObjectPartType.WorkPanelEvents, ObjectPartType.SDPanelEvents };

        /// <summary>
        /// All events, but transaction events
        /// </summary>
        static private ObjectPartType[] PanelEvents =>
            new ObjectPartType[] { ObjectPartType.WebPanelEvents ,ObjectPartType.WorkPanelEvents,
            ObjectPartType.SDPanelEvents };

        /// <summary>
        /// Panel and transaction events
        /// </summary>
        static private ObjectPartType[] Events =>
            new ObjectPartType[] { ObjectPartType.WebPanelEvents, ObjectPartType.WorkPanelEvents ,
                ObjectPartType.TransactionEvents, ObjectPartType.SDPanelEvents };

        /// <summary>
        /// Conditions parts
        /// </summary>
        static private ObjectPartType[] Conditions =>
            new ObjectPartType[] { ObjectPartType.WebPanelConditions, ObjectPartType.WorkPanelConditions ,
                ObjectPartType.ProcedureConditions, ObjectPartType.SDPanelConditions };

		static private ObjectPartType[] Rules =>
			new ObjectPartType[] { ObjectPartType.ProcedureRules , ObjectPartType.SDPanelRules , ObjectPartType.WorkPanelRules ,
				ObjectPartType.WebPanelRules , ObjectPartType.TransactionRules };

		static private void CreateDefinitions()
        {
            _Definitions = new List<CommandDefinition>();

            AddDefinition(new CommandDefinition("If", "Endif", ScopeId.IfCodeBlock))
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Else"))
                .AddScope(ScopeId.IfCodeBlock)
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("New", "Endnew", ScopeId.NewCodeBlock))
                .OnlyInParts(ObjectPartType.Procedure);

            AddDefinition(new CommandDefinition("When duplicate"))
                .AddScope(ScopeId.NewCodeBlock)
                .OnlyInParts(ObjectPartType.Procedure);

            AddDefinition(new CommandDefinition("Footer", "End", ScopeId.HeaderFooterCodeBlock)
                {
                    DefaultPriority = -1    // Otherwise appears before "FOR"
                })
                .OnlyInParts(ObjectPartType.Procedure);

            AddDefinition(new CommandDefinition("Header", "End", ScopeId.HeaderFooterCodeBlock))
                .OnlyInParts(ObjectPartType.Procedure);

            AddDefinition(new CommandDefinition("Do case", "Endcase", ScopeId.DoCaseCodeBlock) {
                    PreText = Environment.NewLine + "\tCase"
                })
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Case"))
                .AddScope(ScopeId.DoCaseCodeBlock)
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Otherwise"))
                .AddScope(ScopeId.DoCaseCodeBlock)
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Do while", "Enddo", ScopeId.DoWhileCodeBlock))
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Sub", "Endsub", ScopeId.SubCodeBlock))
                .AddScope(ScopeId.Root)
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Do"))
                .OnlyInParts(EventsAndProcedures);

            foreach (string nativeCodeCmd in KeywordGx.NATIVECODEKEYWORDS.Except(KeywordGx.DEPRECATEDKEYWORDS))
            {
                AddDefinition(new CommandDefinition(KeywordGx.Capitalize(nativeCodeCmd)))
                    .OnlyInParts(EventsAndProcedures);
            }

            foreach (string op in KeywordGx.BOOLEAN_OPERATORS)
            {
                AddDefinition(new CommandDefinition(op)
                {
                    OnlyAtLineStart = false
                });
            }

            foreach(string cmdTxt in KeywordGx.PRINTCOMMANDS)
            {
                AddDefinition(new CommandDefinition(KeywordGx.Capitalize(cmdTxt)))
                    .OnlyInParts(ObjectPartType.Procedure);
            }

            AddDefinition(new CommandDefinition(KeywordGx.Capitalize(KeywordGx.FOREACH),
                KeywordGx.Capitalize(KeywordGx.ENDFOR), ScopeId.ForEachBlock))
                .OnlyInParts(EventsAndProceduresButTransactions);
            AddDefinition(new CommandDefinition("Xfor each", "Xendfor", ScopeId.XforCodeBlock))
                .OnlyInParts(EventsAndProceduresButTransactions);
            AddDefinition(new CommandDefinition("Xfor first", "Xendfor", ScopeId.XforCodeBlock))
                .OnlyInParts(EventsAndProceduresButTransactions);
            AddForEachCmd("defined by");
            AddForEachCmd("where", true, 1);
            AddForEachCmd("using");
            AddForEachCmd("unique");
            AddForEachCmd("blocking");
            AddForEachCmd("when", false); 
            AddForEachCmd("when none");
            AddForEachCmd("order", false);
            AddForEachCmd("order none", false);

            AddDefinition(new CommandDefinition("Exit"))
                .AddScope(ScopeId.ForEachBlock, true)
                .AddScope(ScopeId.XforCodeBlock, true)
                .AddScope(ScopeId.ForCodeBlock, true)
                .AddScope(ScopeId.DoWhileCodeBlock, true)
                .OnlyInParts(EventsAndProcedures);

            AddDefinition(new CommandDefinition("Delete"))
                .AddScope(ScopeId.ForEachBlock, true)
                .AddScope(ScopeId.XforCodeBlock, true)
                .OnlyInParts(ObjectPartType.Procedure);

            AddDefinition(new CommandDefinition(KeywordGx.Capitalize(KeywordGx.FOR), KeywordGx.Capitalize(KeywordGx.ENDFOR),
                ScopeId.ForCodeBlock))
                .OnlyInParts(EventsAndProcedures);
            AddForCmd("to");
            AddForCmd("step");

            AddDefinition(new CommandDefinition(KeywordGx.Capitalize(KeywordGx.FOREACHLINE), 
                KeywordGx.Capitalize(KeywordGx.ENDFOR), ScopeId.ForCodeBlock))
                .OnlyInParts(PanelEvents);
            AddDefinition(new CommandDefinition(KeywordGx.Capitalize(KeywordGx.FOREACHLINEIN), 
                KeywordGx.Capitalize(KeywordGx.ENDFOR), ScopeId.ForCodeBlock))
                .OnlyInParts(PanelEvents);
			AddDefinition(new CommandDefinition(KeywordGx.Capitalize(KeywordGx.FOREACHSELECTEDLINEIN),
				KeywordGx.Capitalize(KeywordGx.ENDFOR), ScopeId.ForCodeBlock))
				.OnlyInParts(new ObjectPartType[] { ObjectPartType.SDPanelEvents });

			foreach (string c in new string[] { "msg", KeywordGx.CALL , KeywordGx.SUBMIT , KeywordGx.POPUP } )
            {
                AddDefinition(new CommandDefinition(c)
                {
                    AllowChangeCase = false
                })
                .OnlyInParts(EventsAndProcedures);
            }

            foreach (string c in new string[] { "true", "false" })
            {
                AddDefinition(new CommandDefinition(c)
                {
                    AllowChangeCase = false,
                    OnlyAtLineStart = false
                });
            }

            AddDefinition(new CommandDefinition(KeywordGx.NEWSDTOPERATOR)
            {
                AllowChangeCase = false,
                OnlyAtLineStart = false
            })
                .OnlyInParts(EventsAndProcedures);

            // TODO: ScopeId.SubCodeBlock, really ???, check this
            AddDefinition(new CommandDefinition(KeywordGx.Capitalize(KeywordGx.EVENT), "EndEvent", ScopeId.EventCodeBlock)
            {
                AllowChangeCase = false
            })
                .AddScope(ScopeId.Root)
                .OnlyInParts(Events);

            foreach(string command in KeywordGx.TRANSACTIONCOMMANDS)
            {
                AddDefinition(new CommandDefinition(KeywordGx.Capitalize(command)))
                    .OnlyInParts(EventsAndProceduresButTransactions);
            }

            AddDefinition(new CommandDefinition("Return"))
                .OnlyInParts(EventsAndProcedures);

            foreach (string command in KeywordGx.REFRESHCOMMANDS)
            {
                AddDefinition(new CommandDefinition(KeywordGx.Capitalize(command)))
                    .OnlyInParts(Events);
            }

            AddDefinition(new CommandDefinition("Load"))
                .OnlyInParts(PanelEvents);

            AddDefinition(new CommandDefinition("confirm")
            {
                AllowChangeCase = false
            })
                .OnlyInParts(Events);

            AddDefinition(new CommandDefinition("when")
            {
                OnlyAtLineStart = false
            })
                .OnlyInParts(Conditions);

			AddDefinition(new CommandDefinition(KeywordGx.THEME_CLASS)
			{
				AllowChangeCase = false,
				OnlyAtLineStart = false
			});

			// TODO: Composite is allowed only in Event blocks
			AddDefinition(new CommandDefinition("Composite", "EndComposite", ScopeId.CompositeBlock))
                .OnlyInParts(ObjectPartType.SDPanelEvents);

			// Rules:
			DefineRulesKeywords(new string[] { "Insert", "Delete", "Update" ,
				"BeforeValidate", "BeforeInsert", "BeforeUpdate" , "BeforeDelete" , "BeforeComplete",
				"AfterInsert", "AfterUpdate" , "AfterDelete" , "AfterComplete" , "AfterValidate" , "AfterLevel" , "level",
				"Confirm" ,
				"Trn",
				"dependencies" , "in" , "out" , "inout"
			}, false);
			DefineRulesKeywords(new string[] { "If", "On" }, true);

			// [web] { ... } , etc
			foreach(string keyword in new string[] { "win", "web", "bc" })
			{
				AddDefinition(new CommandDefinition(keyword)
				{
					AllowChangeCase = false,
					OnlyAtLineStart = false
				})
				.OnlyInParts(ObjectPartType.TransactionRules, ObjectPartType.TransactionEvents);
			}


		}

		static private void DefineRulesKeywords(string[] ruleNames, bool changeCase)
		{
			foreach (string ruleKeyword in ruleNames)
			{
				AddDefinition(new CommandDefinition(ruleKeyword)
				{
					AllowChangeCase = changeCase,
					OnlyAtLineStart = false
				})
				.OnlyInParts(Rules);
			}
		}

        static private void AddForCmd(string command)
        {
            AddDefinition(new CommandDefinition(command)
            {
                OnlyAtLineStart = false
            })
                .AddScope(ScopeId.ForCodeBlock)
                .OnlyInParts(EventsAndProcedures);
        }

        static private void AddForEachCmd(string command, bool onlyAtLineStart = true, int defaultPriority = 0)
        {
            AddDefinition(new CommandDefinition(command)
            {
                OnlyAtLineStart = onlyAtLineStart,
                DefaultPriority = defaultPriority
            })
                .AddScope(ScopeId.ForEachBlock)
                .AddScope(ScopeId.XforCodeBlock)
                .OnlyInParts(EventsAndProceduresButTransactions);
        }

        static private List<CommandDefinition> Definitions
        {
            get
            {
                if (_Definitions == null)
                    CreateDefinitions();
                return _Definitions;
            }
        }

        static internal HashSet<string> KeywordsLowercase()
        {
            HashSet<string> result = new HashSet<string>();
            foreach (CommandDefinition cmd in Definitions)
            {
                foreach (string keyword in cmd.StartKeywords)
                    result.Add(keyword.ToLower());
                if( !string.IsNullOrEmpty(cmd.BlockCloseKeyword))
                    result.Add(cmd.BlockCloseKeyword.ToLower());
            }
            return result;
        }
    }

}
