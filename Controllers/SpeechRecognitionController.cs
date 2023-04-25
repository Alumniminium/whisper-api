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
    public string[] GetModels() => new string[] { "fast", "normal", "slow", "veryslow" };

    [HttpPost]
    [Route("/streamPlainText")]
    [Produces("text/plain")]
    public async IAsyncEnumerable<string> GetTextStream(IFormFile f, string model = "fast", bool singleLine = false, bool includeTimestamps = true)
    {
        _logger.LogInformation("Streaming Plaintext of '{FileName}' ({Length}) using model: {Model}", f.FileName, f.Length, model);

        await foreach (var segment in _trans.Process(f, model, HttpContext.RequestAborted))
        {
            _logger.LogInformation("[{start} - {end}] {text}", segment.Start, segment.End, segment.Text);
            yield return includeTimestamps
                            ? $"[{segment.Start.ToString(@"hh\:mm\:ss")} - {segment.End.ToString(@"hh\:mm\:ss")}]\t{segment.Text}" + (singleLine ? string.Empty : Environment.NewLine)
                            : segment.Text + (singleLine ? string.Empty : Environment.NewLine);
        }
        _logger.LogInformation("Finished transcribing and streaming: {FileName}", f.FileName);
        yield return Environment.NewLine;
    }

    [HttpPost]
    [Route("/getPlainText")]
    [Produces("text/plain")]
    public async Task<string> GetText(IFormFile f, string model = "fast", bool singleLine = false, bool includeTimestamps = true)
    {
        _logger.LogInformation("Transcribing '{FileName}' ({Length}) using model: {Model}", f.FileName, f.Length, model);

        var sb = new StringBuilder();
        await foreach (var segment in _trans.Process(f, model, HttpContext.RequestAborted))
        {
            _logger.LogInformation("[{start} - {end}] {text}", segment.Start, segment.End, segment.Text);
            if (singleLine && !includeTimestamps)
                sb.Append(segment.Text);
            else
                sb.AppendLine(includeTimestamps
                    ? $"[{segment.Start.ToString(@"hh\:mm\:ss")} - {segment.End.ToString(@"hh\:mm\:ss")}]\t{segment.Text}"
                    : segment.Text);
        }
        sb.AppendLine();
        _logger.LogInformation("Finished transcribing: {FileName}", f.FileName);
        return sb.ToString();
    }
}
