using System.Text;
using Microsoft.AspNetCore.Mvc;
using speech2text.Services;

namespace speech2text.Controllers;

[ApiController]
[Route("[controller]")]
public class Speech2TextController : ControllerBase
{
    private readonly ILogger<Speech2TextController> _logger;
    private readonly ITranscriberService _trans;
    public Speech2TextController(ILogger<Speech2TextController> logger, ITranscriberService trans)
    {
        _logger = logger;
        _trans = trans;
    }

    [HttpGet]
    [Route("/models")]
    [Produces("text/plain")]
    public IEnumerable<string> GetModels()
    {
        var models = Directory.GetFiles(Program.MODEL_DIR);
        foreach (var model in models)
        {
            _logger.LogInformation("{model}", model);
            yield return Path.GetFileName(model) + Environment.NewLine;
        }
    }

    [HttpPost]
    [Route("/stream")]
    [Produces("text/plain")]
    public async IAsyncEnumerable<string> GetTextStream(IFormFile file, string model, bool timestamps = false, bool singleLine = false, bool statistics =true)
    {
        _logger.LogInformation("Streaming Plaintext {Length}) using model: {Model}", file.Length, model);
        var dto = new TranscribeDto(file,  model, timestamps, singleLine, statistics);
        await foreach (var segment in _trans.Process(dto, HttpContext.RequestAborted))
        {
            _logger.LogInformation("[{start} - {end}] {text}", segment.Start, segment.End, segment.Text);
            yield return segment.Text + (singleLine ? string.Empty : Environment.NewLine);
        }
        yield return Environment.NewLine;
        if(dto.statistics)
        yield return _trans.GetStatistics();
    }
}
