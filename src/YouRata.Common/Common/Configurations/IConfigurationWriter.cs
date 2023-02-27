using System;

namespace YouRata.Common.Configurations;

public interface IConfigurationWriter
{
    void WriteSection(string name, object value);
}
