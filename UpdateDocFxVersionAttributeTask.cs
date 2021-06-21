using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

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
                    "Nothing to do.");
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

            return true;
        }
    }
}