using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using Artech.Architecture.UI.Framework.Services;
using Artech.Genexus.Common;
using Artech.Genexus.Common.Objects;
using Artech.Genexus.Common.Types;
using Artech.Genexus.Common.CustomTypes;
using Artech.Architecture.Common.Objects;
using Artech.Architecture.Common.Descriptors;
using Artech.Udm.Framework.References;
using Artech.Udm.Framework;
using System.Text.RegularExpressions;
using Artech.Genexus.Common.Entities;
using Artech.Architecture.Common.Converters;
using Artech.Packages.Patterns.Objects;
using Artech.Patterns.WorkWithDevices.Objects;
using Artech.Genexus.Common.Run;
using Artech.Genexus.Common.Services;
using Artech.Genexus.Common.Wiki;

namespace LSI.Packages.Extensiones.Utilidades
{
    /// <summary>
    /// Main objects / kb generators utils
    /// </summary>
    public class MainsGx
    {

        /// <summary>
        /// Get the kbase main objects
        /// </summary>
        static public List<KBObject> GetKBMainObjects(KBModel model)
        {
            return model.Objects
                .GetByPropertyValue(Properties.PRC.MainProgram, true)
                .Where( x => !(x is WikiPageKBObject))  // Ignore wiki pages (yes, they can be main)
                .ToList();
        }

        /// <summary>
        /// The kbase main objects (only to use from the IDE extensions)
        /// </summary>
        static public List<KBObject> KBMainObjects
        {
            get
            {
                if (!UIServices.IsKBAvailable || UIServices.KB.CurrentModel == null)
                    return new List<KBObject>();

                return GetKBMainObjects(UIServices.KB.WorkingEnvironment.DesignModel);
            }
        }

        /// <summary>
        /// Get a generator by its description
        /// </summary>
        /// <param name="gxModel">The target model where to search the generator</param>
        /// <param name="description">The generator description</param>
        /// <returns>The generator. null if the generator was not found</returns>
#if GX_17_OR_GREATER
        static public GxGenerator GetGeneratorByDescription(GxModel gxModel, string description)
        {
            if (string.IsNullOrEmpty(description))
                return null;
            description = description.Trim().ToLower();
            return gxModel.Generators
                .Where(x => x.Description.ToLower() == description)
                .FirstOrDefault();
        }
#else
        static public GxEnvironment GetGeneratorByDescription(GxModel gxModel, string description)
        {
            if (string.IsNullOrEmpty(description))
                return null;
            description = description.Trim().ToLower();
            return gxModel.Environments
                .Where(x => x.Description.ToLower() == description)
                .FirstOrDefault();
        }
#endif

#if GX_17_OR_GREATER
        static public GxGenerator GetGeneratorByType(KBModel targetModel, GeneratorType type)
        {
            return targetModel.GetAs<GxModel>().Generators.Where(x => x.Generator == (int)type).FirstOrDefault();
        }
#else
        static public GxEnvironment GetGeneratorByType(KBModel targetModel, GeneratorType type)
        {
            return targetModel.GetAs<GxModel>().Environments.Where(x => x.Generator == (int)type).FirstOrDefault();
        }
#endif

#if GX_17_OR_GREATER
        static public GxGenerator GetMainGenerator(KBObject main)
        {
            ItemReference<GxGenerator> genRef = main.GetPropertyValue(Properties.PRC.Generator)
                        as ItemReference<GxGenerator>;
#else
        static public GxEnvironment GetMainGenerator(KBObject main)
        {
            ItemReference<GxEnvironment> genRef = main.GetPropertyValue(Properties.PRC.Generator)
                        as ItemReference<GxEnvironment>;
#endif
            if (genRef != null)
                return genRef.Definition;

            if (main is SDPanel)
                return GetGeneratorByType(main.KB.DesignModel.Environment.TargetModel, GeneratorType.SmartDevices);

            PatternInstance pattern = main as PatternInstance;
            if (pattern != null)
                return GetGeneratorByType(main.KB.DesignModel.Environment.TargetModel, GeneratorType.SmartDevices);

            return null;

        }

        /// <summary>
        /// Generator is web generator?
        /// </summary>
        /// <param name="generator">Generator</param>
        /// <returns></returns>
#if GX_17_OR_GREATER
        static public bool IsWebGenerator(GxGenerator generator)
#else
        static public bool IsWebGenerator(GxEnvironment generator)
#endif
        {
            bool? isWeb = generator.Properties.GetPropertyValue(Properties.CSHARP.IsWebGenerator) as bool?;
            if (isWeb != null && (bool)isWeb)
                return true;
            return false;
        }

        static public bool IsMain(KBObject o)
        {
            bool? isMain = o.GetPropertyValue(Properties.PRC.MainProgram) as bool?;
            return isMain != null && (bool)isMain;
        }

        /// <summary>
        /// It checks if an object is main web
        /// </summary>
        /// <param name="mainObject">Object to check</param>
        /// <returns>True if the object is web</returns>
        static public bool IsMainWeb(KBObject mainObject)
        {
            var generator = GetMainGenerator(mainObject);
            if (generator == null)
                return false;

            return IsWebGenerator(generator);
        }

        /// <summary>
        /// Get kbase mains grouped by generator of the current UI kb
        /// </summary>
        /// <returns>Main kb objects grouped by generator</returns>
#if GX_17_OR_GREATER
        static public Dictionary<GxGenerator, List<KBObject>> GetMainsByGenerator()
#else
        static public Dictionary<GxEnvironment, List<KBObject>> GetMainsByGenerator()
#endif
        {
            return GetMainsByGenerator(UIServices.KB.CurrentKB.DesignModel.Environment.DesignModel);
        }

        /// <summary>
        /// Get kbase mains grouped by generator
        /// </summary>
        /// <returns>Main kb objects grouped by generator</returns>
#if GX_17_OR_GREATER
        static public Dictionary<GxGenerator, List<KBObject>> GetMainsByGenerator(KBModel model)
#else
        static public Dictionary<GxEnvironment, List<KBObject>> GetMainsByGenerator(KBModel model)
#endif
        {
#if GX_17_OR_GREATER
            Dictionary<GxGenerator, List<KBObject>> mainsByGenerator =
                    new Dictionary<GxGenerator, List<KBObject>>();
#else
            Dictionary<GxEnvironment, List<KBObject>> mainsByGenerator =
                    new Dictionary<GxEnvironment, List<KBObject>>();
#endif
            foreach (KBObject main in GetKBMainObjects(model))
            {
                var generator = GetMainGenerator(main);
                if (generator == null)
                    // Mains for Smart devices do not have "Generator" property (fuck) 
                    continue;

                List<KBObject> generatorMains;
                if (!mainsByGenerator.TryGetValue(generator, out generatorMains))
                {
                    generatorMains = new List<KBObject>();
                    mainsByGenerator.Add(generator, generatorMains);
                }
                generatorMains.Add(main);
            }
            return mainsByGenerator;
        }

        /// <summary>
        /// Get main objects with a given generator type
        /// </summary>
        /// <param name="generatorType">The generator type to search</param>
        /// <param name="model">The model where to search</param>
        /// <returns>The list of main objects</returns>
        static public List<KBObject> GetMainsByGenerator(KBModel model, GeneratorType generatorType)
        {
            var mainsbyGenerator = GetMainsByGenerator(model);
            var env = mainsbyGenerator.Keys.FirstOrDefault(x => x.GeneratorType == generatorType);
            if (env == null)
                return new List<KBObject>();
            return mainsbyGenerator[env];
        }
    }
}
