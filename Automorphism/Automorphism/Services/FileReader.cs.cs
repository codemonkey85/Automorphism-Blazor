namespace Automorphism.Services;

public static class FileReaderExtensions
{
    public static IServiceCollection TryAddFileReader(this IServiceCollection svcs, string fileWriteLocation)
    {
        svcs.TryAddSingleton(fr => new FileReader(fileWriteLocation));
        return svcs;
    }
}

public class FileReader(string fileWriteLocation)
{
    public string FileWriteLocation { get; internal set; } = fileWriteLocation;

    public UInt128 CurrentNumber { get; set; }

    public string[] Findings => File.ReadAllLines(FileWriteLocation);

    public AutomorphismData GetAutomorphismData() => new()
    {
        CurrentNumber = CurrentNumber,
        Findings = Findings
    };
}
