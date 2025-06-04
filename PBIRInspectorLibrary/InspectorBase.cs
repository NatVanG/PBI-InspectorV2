//using Newtonsoft.Json;
using PBIRInspectorLibrary.CustomRules;
using System.Text.Json;

namespace PBIRInspectorLibrary
{
    public class InspectorBase
    {
        public InspectorBase()
        {

        }

        public InspectorBase(string fabricItemPath, InspectionRules inspectionRules)
        {
            if (string.IsNullOrEmpty(fabricItemPath)) throw new ArgumentNullException(nameof(fabricItemPath));
            if (!File.Exists(fabricItemPath)) throw new FileNotFoundException();
        }

        public InspectorBase(string fabricItemPath, string rulesPath)
        {
            if (string.IsNullOrEmpty(fabricItemPath)) throw new ArgumentNullException(nameof(fabricItemPath));
        }

        public T? DeserialiseRulesFromPath<T>(string rulesPath)
        {
            if (!File.Exists(rulesPath)) throw new FileNotFoundException(string.Format("Rules with path \"{0}\" was not found", rulesPath));

            string jsonString = File.ReadAllText(rulesPath);

            return DeserialiseRules<T>(jsonString);

        }

        public static T? DeserialiseRules<T>(string jsonString)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<T>(jsonString, options);
        }

        public static T? DeserialiseRules<T>(Stream jsonStream)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            return JsonSerializer.Deserialize<T>(jsonStream, options);
        }
    }
}
