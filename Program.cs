using Microsoft.AspNetCore.Http.Features;
using speech2text.Services;

namespace speech2text;

public static class Program
{
    public static int THREAD_COUNT => Environment.GetEnvironmentVariable("THREAD_COUNT") is null ? Environment.ProcessorCount / 2 : int.Parse(Environment.GetEnvironmentVariable("THREAD_COUNT"));
    public static string MODEL_DIR => Environment.GetEnvironmentVariable("MODEL_DIR") ?? "/models/whisper/";
    public static string DEFAULT_MODEL => Environment.GetEnvironmentVariable("DEFAULT_MODEL") ?? "whisper-base-q5_1.bin";

    static void Main()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Services.AddControllers(options => options.OutputFormatters.Add(new AsyncStringOutputFormatter()));
        builder.Services.AddControllers(options => options.OutputFormatters.Add(new StringOutputFormatter()));
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

        builder.Services.AddSingleton<ITranscriberService, TranscriberService>();

        var app = builder.Build();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Her.st Whisper API");
            c.RoutePrefix = string.Empty;
        });

        app.MapControllers();
        app.Run();
    }
}