using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace DhyMik.DocFx
{
    /// <summary>
    /// This build task updates the custom '_DocumentationVersion' attribute
    /// in 'globalMetadata' of the DocFx json file with the
    /// InformalVersion value of the dll indicated by <see cref="DllPath"/>.
    /// </summary>
    public class UpdateDocFxVersionAttributeTask : Task
    {
        /// <summary>
        /// File name for generation of GlobalMetadata css variables
        /// </summary>
        private const string cssFileName = "globalMetadataVariables.css";

        /// <summary>
        /// The dll from which to take the version information
        /// </summary>
        public string DllPath { get; set; }

        /// <summary>
        /// The DocFx json confog file into which the version information will be placed
        /// </summary>
        public string DocFxJsonPath { get; set; }

        public override bool Execute()
        {
            string _docFxJsonPath = "docfx.json";
            var _logPrefix = $"--{nameof(UpdateDocFxVersionAttributeTask)}: ";

            // check
            if (string.IsNullOrWhiteSpace(DllPath))
            {
                Log.LogError($"{_logPrefix}Error: '{nameof(DllPath)}' parameter is null or empty.");
                return false;
            }

            if (!string.IsNullOrWhiteSpace(DocFxJsonPath))
            {
                Log.LogMessage(
                    importance: MessageImportance.Normal,
                    $"{_logPrefix}using '{DocFxJsonPath}' as DocFx json file to update.");
                _docFxJsonPath = DocFxJsonPath;
            }


            string json;

            try
            {
                json = File.ReadAllText(_docFxJsonPath);
            }
            catch (Exception)
            {
                Log.LogError($"{_logPrefix}Error reading file '{_docFxJsonPath}'");
                throw;
            }

            dynamic jsonObj = JsonConvert.DeserializeObject(json);

            if (jsonObj == null)
            {
                Log.LogError($"{_logPrefix}Error: Could not deserialize file '{_docFxJsonPath}'.");
                return false;
            }

            string dllInformalVersion;

            try
            {
                dllInformalVersion = FileVersionInfo.GetVersionInfo(DllPath).ProductVersion;
            }
            catch (Exception)
            {
                Log.LogError($"{_logPrefix}Error reading file '{DllPath}'");
                throw;
            }

            Log.LogMessage(
                importance: MessageImportance.Low,
                $"{_logPrefix}Dll informal version is '{dllInformalVersion}'");

            var currentVersionInDocFxJson = Convert.ToString(jsonObj.build.globalMetadata._DocumentationVersion);

            if (string.IsNullOrWhiteSpace(currentVersionInDocFxJson))
                Log.LogMessage(
                    importance: MessageImportance.Low,
                    $"{_logPrefix}No '_DocumentationVersion' attribute " +
                    $"in '{_docFxJsonPath}' found,");
            else
                Log.LogMessage(
                    importance: MessageImportance.Low,
                    $"{_logPrefix}Current '_DocumentationVersion' attribute in " +
                    $"'{_docFxJsonPath}' is '{currentVersionInDocFxJson}'.");

            if (currentVersionInDocFxJson == dllInformalVersion)
            {
                Log.LogMessage(
                    importance: MessageImportance.Normal,
                    $"{_logPrefix}Dll informal version and '_DocumentationVersion' attribute " +
                    $"in '{_docFxJsonPath}' are equal. Current value is '{currentVersionInDocFxJson}'. " +
                    "No need to update docfx.json.");
            }
            else
            {
                // Update DocFx.json
                jsonObj.build.globalMetadata._DocumentationVersion = dllInformalVersion;
                string output = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText(_docFxJsonPath, output);

                if (string.IsNullOrWhiteSpace(currentVersionInDocFxJson))
                {
                    Log.LogMessage(
                        importance: MessageImportance.Normal,
                        $"{_logPrefix}Created '_DocumentationVersion' attribute in " +
                        $"'{_docFxJsonPath}' with value '{dllInformalVersion}'.");
                }
                else
                {
                    Log.LogMessage(
                       importance: MessageImportance.Normal,
                       $"{_logPrefix}Updated '_DocumentationVersion' attribute in " +
                       $"'{_docFxJsonPath}' to '{dllInformalVersion}'.");
                }
            }

            /*
             * 
             * Write css file with generated globalMetadata css variabes
             * 
             * */

            // extract attributesd from the updated 'jsonObj'
            var attributes = (Dictionary<string, string>)JsonConvert.DeserializeObject<Dictionary<string, string>>(Convert.ToString(jsonObj.build.globalMetadata));
            var templates = (string[])JsonConvert.DeserializeObject<string[]>(Convert.ToString(jsonObj.build.template));

            // default output path is just the file name
            // (save in project root if no template path is found later)
            var outputFile = cssFileName;

            if (attributes == null || attributes.Count == 0)
            {
                Log.LogMessage(
                   importance: MessageImportance.Normal,
                   $"{_logPrefix}No 'globalMetadata' attributes found in ' docfx.json'. " +
                   $"Skipping generation of '{cssFileName}'.");
            }
            else
            {
                if(templates != null && templates.Length != 0)
                {
                    // if template attribute contains template paths,
                    // use the last item as a path to the template
                    // and point the output path to its 'styles' subfolder
                    outputFile =
                        (templates[templates.Length - 1] + "/styles/" + cssFileName)
                        .Replace("\\", "/")
                        .Replace("//", "/");
                }

                var css = new StringBuilder();

                css.Append("/*\n * This file was created by DhyMik.DocFx.UpdateDocFxVersionAttributeTask.\n *\n");
                css.Append(" * Do not change this file - changes will be overwritten on next build.\n *\n");
                css.Append(" * This file is based on data from 'globalMetadata' section in docfx.json.\n");
                css.Append(" * Edit 'globalMetadata' section of docfx.json instead of changing this file.\n");
                css.Append(" */\n");

                // convert the attributes to css and append
                css.Append(attributes.ExtractToCss());

                // try to write the css file
                try
                {
                    File.WriteAllText(outputFile, css.ToString());

                    Log.LogMessage(
                       importance: MessageImportance.Normal,
                       $"{_logPrefix}Css file {outputFile} written with {attributes.Count} css variable declarations.");
                }
                catch (Exception)
                {
                    Log.LogError($"{_logPrefix}Error writing css file '{outputFile}'");
                    throw;
                }
            }

            return true;
        }
    }
}