namespace Automorphism.Services;

public class AutomorphismSearcher(FileReader fr) : IHostedService, IDisposable
{
    private readonly FileReader _fileReader = fr;
    private bool _go = false;
    private Task? _backgroundTask;
    private CancellationTokenSource? _cts;

    public UInt128 CurrentNumber { get; private set; } = 5;

    private void CrunchNumbers()
    {
        while (_go)
        {
            if (Matches(CurrentNumber))
            {
                File.AppendAllText(_fileReader.FileWriteLocation, $"{CurrentNumber},{CurrentNumber * CurrentNumber},{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n");
            }

            if (Matches(CurrentNumber + 1))
            {
                var plusOne = CurrentNumber + 1;
                File.AppendAllText(_fileReader.FileWriteLocation, $"{plusOne},{plusOne * plusOne},{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}\n");
            }

            _fileReader.CurrentNumber = CurrentNumber;

            CurrentNumber += 10;
        }
    }

    private static bool Matches(UInt128 n)
    {
        var nString = $"{n}";
        var sString = $"{n * n}";
        var size = nString.Length;
        var ending = sString[^size..];
        return ending == nString;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (File.Exists(_fileReader.FileWriteLocation))
        {
            var lines = File.ReadAllLines(_fileReader.FileWriteLocation);
            if (lines.Length != 0)
            {
                CurrentNumber = UInt128.Parse(lines[^1].Split(',')[0]) + 10;
            }
        }

        _go = true;
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _backgroundTask = Task.Run(CrunchNumbers, _cts.Token);

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _go = false;

        if (_backgroundTask == null)
        {
            return;
        }

        _cts?.Cancel();
        await Task.WhenAny(_backgroundTask, Task.Delay(Timeout.Infinite, cancellationToken));
    }

    public void Dispose() => _cts?.Cancel();
}
