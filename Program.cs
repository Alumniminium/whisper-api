using System.Diagnostics;
using Microsoft.AspNetCore.Http.Features;
using speech2text.Services;

namespace speech2text;

public static class Program
{
    public static int THREAD_COUNT => Environment.GetEnvironmentVariable("THREAD_COUNT") is null ? Environment.ProcessorCount / 2 : int.Parse(Environment.GetEnvironmentVariable("THREAD_COUNT"));

    static void Main()
    {
        if (OperatingSystem.IsLinux() || OperatingSystem.IsWindows())
        {
            // I want to limit the CPU usage to 400%
            var proc = Process.GetCurrentProcess();
            proc.ProcessorAffinity = (1 << THREAD_COUNT) - 1;
        }

        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers(options => options.OutputFormatters.Add(new AsyncStringOutputFormatter()));
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.WebHost.UseKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = null;
            options.Limits.MaxRequestBufferSize = null;
        });
        builder.Services.Configure<FormOptions>(x =>
        {
            x.ValueLengthLimit = int.MaxValue;
            x.MultipartBodyLengthLimit = int.MaxValue;
        });

        builder.Services.AddTransient<ITranscriberService, TranscriberService>();

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Her.st Speech2Text API");
            c.RoutePrefix = string.Empty;
        });

        app.MapControllers();
        app.Run();
    }
}