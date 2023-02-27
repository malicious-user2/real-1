using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace YouRata.Common.Configurations;

public class ConfigurationWriter : IConfigurationWriter
{
    private readonly string _path;

    public ConfigurationWriter(string path)
    {
        _path = path;
    }

    public void WriteBlankFile()
    {
        YouRataConfiguration blankConfiguration = new YouRataConfiguration();
        FillDefaultValues(blankConfiguration);
        File.WriteAllText(_path, JsonConvert.SerializeObject(blankConfiguration, Formatting.Indented));
    }

    private void FillDefaultValues(object? configSection)
    {
        if (configSection == null) return;
        foreach (PropertyInfo propInfo in configSection.GetType().GetProperties())
        {
            if (propInfo.PropertyType.BaseType != null && propInfo.PropertyType.BaseType == typeof(BaseValidatableConfiguration))
            {
                FillDefaultValues(propInfo.GetValue(configSection));
            }
            else
            {
                DefaultValueAttribute? defaultValue = propInfo.GetCustomAttribute<DefaultValueAttribute>();
                if (defaultValue != null)
                {
                    propInfo.SetValue(configSection, defaultValue.Value);
                }
            }
        }
    }

    public void WriteSection(string name, object value)
    {
        JToken jRoot = JObject.Parse(File.ReadAllText(_path));
        JToken jSeeker = jRoot;
        JToken jParent = null;
        string[] parts = name.Split(':');
        int part = 0;
        while (jSeeker != null && jSeeker is JObject && part < parts.Length)
        {
            jSeeker = jSeeker[parts[part]];
            part++;
        }
        if (jSeeker != null && (!(jSeeker is JObject)))
        {
            throw new FormatException("Section");
        }
        if (part == (parts.Length - 1))
        {
            jParent = jSeeker;
        }
        if (jParent == null)
        {
            jSeeker = jRoot;
            part = 0;
            while (jSeeker != null && jSeeker is JObject && part < (parts.Length - 1))
            {
                if (jSeeker[parts[part]] == null)
                {
                    jSeeker[parts[part]] = JObject.FromObject(new { });
                }
                jSeeker = jSeeker[parts[part]];
                part++;
            }
            jParent = jSeeker;
        }
        string sectionName = parts[parts.Length - 1];
        jParent[sectionName] = JToken.FromObject(value);
        File.WriteAllText(_path, JsonConvert.SerializeObject(jRoot, Formatting.Indented));
    }
}
