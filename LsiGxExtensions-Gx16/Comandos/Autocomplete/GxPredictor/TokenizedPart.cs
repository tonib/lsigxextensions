using Artech.Architecture.Common.Objects;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Parts;
using LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor.DataDefinition;
using LSI.Packages.Extensiones.Comandos.Autocomplete.KbNames;
using System.IO;
using System.Runtime.Serialization;

namespace LSI.Packages.Extensiones.Comandos.Autocomplete.GxPredictor
{
    /// <summary>
    /// Stores the tokenized code for a code object part
    /// TODO: Rename to TokenizedCodePart
    /// </summary>
    [DataContract]
    public class TokenizedPart
    {
        public string Name = string.Empty;
		
        [DataMember]
        public TokensList Tokens = new TokensList();

        public TokenizedPart() { }

        public TokenizedPart(KBObjectPart part)
        {
			ObjectPartType partType = new ObjectPartType(part);
            Name = part.KBObject.Name + "_" + partType.PartTypeName;
        }

        public void SerializeToFile(DataInfo dataInfo, string directoryPath)
        {
            string filePath = Path.Combine(directoryPath, Name + ".csv");

            using(StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine(dataInfo.CsvTitlesRow);
                Tokens.WriteToCsv(dataInfo, writer);
            }
        }
    }
}
