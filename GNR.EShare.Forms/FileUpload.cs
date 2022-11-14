using System.Collections.Concurrent;
using System.Net.Mime;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Net.Http.Headers;
using Guid = System.Guid;

public class PortProvider : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly IHostApplicationLifetime _lifetime;
    public int? Port { get; private set; } = null;

    public PortProvider(IServiceProvider services, IHostApplicationLifetime lifetime)
    {
        _services = services;
        _lifetime = lifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!await WaitForAppStartup(_lifetime, stoppingToken))
        {
            return;
        }

        var address = _services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses
            .FirstOrDefault();
        if (address != null)
        {
            Port = new Uri(address).Port;
        }
    }


    static async Task<bool> WaitForAppStartup(IHostApplicationLifetime lifetime, CancellationToken stoppingToken)
    {
        var startedSource = new TaskCompletionSource();
        var cancelledSource = new TaskCompletionSource();

        using var reg1 = lifetime.ApplicationStarted.Register(() => startedSource.SetResult());
        using var reg2 = stoppingToken.Register(() => cancelledSource.SetResult());

        Task completedTask = await Task.WhenAny(
            startedSource.Task,
            cancelledSource.Task).ConfigureAwait(false);

        // If the completed tasks was the "app started" task, return true, otherwise false
        return completedTask == startedSource.Task;
    }
}


[Route("File")]
public class FileUploadController : ControllerBase
{
    private SharedFileProviderService _fileProvider;

    public FileUploadController(IServiceProvider _service)
    {
        _fileProvider = _service.GetRequiredService<SharedFileProviderService>();
    }

    [HttpGet("{Id}")]
    public IActionResult GetFile(string id)
    {
        var validId = Guid.TryParse(id, out var guid);
        bool exists = _fileProvider.Files.TryGetValue(guid, out var info);
        if (!exists) return NotFound();
        return info!;
    }
}

public class SharedFileActionResult : IActionResult
{
    public readonly IFileInfo File;

    public SharedFileActionResult(IFileInfo file)
    {
        File = file;
    }


    public async Task ExecuteResultAsync(ActionContext context)
    {
        Stream? stream = null;
        try
        {
            IActionResult result;
            if (File.Exists)
            {
                string? contentType;
                new FileExtensionContentTypeProvider().TryGetContentType(File.Name, out contentType);
                stream = File.CreateReadStream();
                result = new FileStreamResult(stream, contentType ?? "application/octet-stream")
                {
                    LastModified = File.LastModified,
                    EnableRangeProcessing = true
                };
            }
            else
            {
                result = new NotFoundResult();
            }

            await result.ExecuteResultAsync(context);
        }
        finally
        {
            stream?.Dispose();
        }
    }
}

public class SharedFileProviderService
{
    private readonly ILogger<SharedFileProviderService> _logger;
    public readonly ConcurrentDictionary<Guid, SharedFileActionResult> Files = new();

    public SharedFileProviderService(ILogger<SharedFileProviderService> _logger)
    {
        this._logger = _logger;
    }

    public void AddFile(FileInfo file)
    {
        Files.TryAdd(Guid.NewGuid(), new SharedFileActionResult(new PhysicalFileInfo(file)));
    }
}