using System;

namespace YouRatta.Common.Configurations;

public interface IConfigurationWriter
{
    void WriteSection(string name, object value);
}
