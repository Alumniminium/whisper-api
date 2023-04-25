using System.Diagnostics;
using System.Runtime.CompilerServices;
using CliWrap;
using CliWrap.Buffered;
using Whisper.net;

namespace speech2text.Services
{
    public interface ITranscriberService
    {
        IAsyncEnumerable<SegmentData> Process(IFormFile file, string model, CancellationToken ctx = default);
    }
    public class TranscriberService : ITranscriberService
    {
        public async IAsyncEnumerable<SegmentData> Process(IFormFile file, string model, [EnumeratorCancellation] CancellationToken ctx)
        {
            while (AI.ActiveJobs > 0 && ctx.IsCancellationRequested == false)
                await Task.Delay(1000);
            if (ctx.IsCancellationRequested)
                yield break;

            Interlocked.Increment(ref AI.ActiveJobs);

            var inputTmpFilePath = Path.GetTempFileName();
            var decodedFile = Path.GetTempFileName();
            var decodedFileStream = Stream.Null;
            var inputFileStream = Stream.Null;

            try
            {
                inputFileStream = File.Create(inputTmpFilePath);
                file.CopyTo(inputFileStream);

                await Cli.Wrap("ffmpeg").WithArguments($"""-i "{inputTmpFilePath}" -vn -ar 16000 -ac 1 -c:a pcm_s16le -f wav -y "{decodedFile}""").WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ctx);

                using var processor = MakeProcessor(model);
                decodedFileStream = File.OpenRead(decodedFile);

                await foreach (var segment in processor.ProcessAsync(decodedFileStream, ctx))
                    yield return segment;
            }
            finally
            {
                inputFileStream.Dispose();
                decodedFileStream.Dispose();
                File.Delete(decodedFile);
                File.Delete(inputTmpFilePath);

                float cpuUsage;
                while ((cpuUsage = await GetCpuUsage()) > 10)
                    Console.WriteLine($"CPU usage too high ({cpuUsage:0.00})%, waiting...");

                Interlocked.Decrement(ref AI.ActiveJobs);
            }
        }

        private static WhisperProcessor MakeProcessor(string model)
        {
            var b = model switch
            {
                "fast" => AI.TinyFactory.CreateBuilder(),
                "normal" => AI.BaseFactory.CreateBuilder(),
                "slow" => AI.MediumFactory.CreateBuilder(),
                "veryslow" => AI.LargeFactory.CreateBuilder(),
                _ => throw new ArgumentException("invalid model. use 'GET /models' to get a list of available models"),
            };

            b = b.WithLanguage("auto")
                 .WithThreads(Program.THREAD_COUNT);

            return b.Build();
        }

        private static async Task<float> GetCpuUsage()
        {
            var startTimestamp = Stopwatch.GetTimestamp();
            var startCPU = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
            await Task.Delay(1000);
            var endCPU = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime;
            var msPassed = Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds;

            var msCPU = (endCPU - startCPU).TotalMilliseconds;
            var usage = msCPU / (Environment.ProcessorCount * msPassed);

            return (float)usage * 100f;
        }

    }
}