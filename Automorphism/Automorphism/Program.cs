var builder = WebApplication.CreateBuilder(args);
var services = builder.Services;

services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var config = builder.Configuration;
var fileWriteLocation = config.GetValue<string>("FileWriteLocation");
if (fileWriteLocation is not { Length: > 0 })
{
    throw new InvalidOperationException("FileWriteLocation must be set in appsettings.json");
}

var serverUrl = config.GetValue<string>("ServerUrl");
if (serverUrl is not { Length: > 0 })
{
    throw new InvalidOperationException("ServerUrl must be set in appsettings.json");
}

services
    .TryAddFileReader(fileWriteLocation)
    .AddSingleton<AutomorphismSearcher>()
    .AddHostedService(sp => sp.GetRequiredService<AutomorphismSearcher>());

services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(serverUrl) });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app
    .UseStaticFiles()
    .UseAntiforgery();

app
    .MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Automorphism.Client._Imports).Assembly);

var apiGroup = app.MapGroup("api");

apiGroup
    .MapGet("automorphismdata", (FileReader fileReader) => fileReader.GetAutomorphismData())
    .WithName(nameof(FileReader.GetAutomorphismData));

app.Run();
