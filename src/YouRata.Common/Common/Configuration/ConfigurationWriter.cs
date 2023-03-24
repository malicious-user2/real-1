using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;

namespace YouRata.Common.Configuration;

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
            if (propInfo.PropertyType.BaseType?.Equals(typeof(BaseValidatableConfiguration)) == true)
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
        throw new NotImplementedException("No writer for this configuration");
    }
}
