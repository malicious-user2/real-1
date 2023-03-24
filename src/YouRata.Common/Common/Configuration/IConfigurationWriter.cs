namespace YouRata.Common.Configuration;

public interface IConfigurationWriter
{
    void WriteSection(string name, object value);
}
