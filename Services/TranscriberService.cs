using System.Diagnostics;
using System.Runtime.CompilerServices;
using CliWrap;
using CliWrap.Buffered;
using Whisper.net;

namespace speech2text.Services
{
    public interface ITranscriberService
    {
        IAsyncEnumerable<SegmentData> Process(TranscribeDto dto, CancellationToken ctx = default);
        public string GetStatistics();
    }
    public class TranscriberService : ITranscriberService
    {
        public WhisperFactory Factory = WhisperFactory.FromPath(Path.Combine(Program.MODEL_DIR, Program.DEFAULT_MODEL));
        public WhisperProcessor Runner;
        private double _WaitTime = 0d;
        private double _ModelLoadTime = 0d;
        private double _IngestTime = 0d;
        private double _InferTime = 0d;
        private double TotalTime => _WaitTime + _ModelLoadTime + _IngestTime + _InferTime;

        public bool Busy;
        public string ModelName = Program.DEFAULT_MODEL;

        public TranscriberService() => Runner = Factory.CreateBuilder().WithThreads(Program.THREAD_COUNT).Build();

        public async IAsyncEnumerable<SegmentData> Process(TranscribeDto dto, [EnumeratorCancellation] CancellationToken ctx)
        {
            ResetStatistics();
            await BusyWait();
            await SetupModel(dto);

            var inputTmpFilePath = Path.GetTempFileName();
            var decodedFile = Path.GetTempFileName();
            var decodedFileStream = Stream.Null;
            var inputFileStream = Stream.Null;

            try
            {
                var start = Stopwatch.GetTimestamp();
                inputFileStream = File.Create(inputTmpFilePath);
                dto.file.CopyTo(inputFileStream);

                await Cli.Wrap("ffmpeg").WithArguments($"""-i "{inputTmpFilePath}" -vn -ar 16000 -ac 1 -c:a pcm_s16le -f wav -y "{decodedFile}""").WithValidation(CommandResultValidation.None).ExecuteBufferedAsync(ctx);
                decodedFileStream = File.OpenRead(decodedFile);
                _IngestTime = Stopwatch.GetElapsedTime(start).TotalSeconds;

                start = Stopwatch.GetTimestamp();
                await foreach (var segment in Runner.ProcessAsync(decodedFileStream, ctx))
                    yield return segment;
                _InferTime = Stopwatch.GetElapsedTime(start).TotalSeconds;
            }
            finally
            {
                inputFileStream.Dispose();
                decodedFileStream.Dispose();
                File.Delete(decodedFile);
                File.Delete(inputTmpFilePath);
                Busy=false;
            }
        }

        private async Task BusyWait()
        {
            var start = Stopwatch.GetTimestamp();

            while (Busy)
                await Task.Delay(100);
            Busy = true;
            _WaitTime = Stopwatch.GetElapsedTime(start).TotalSeconds;
        }

        private async Task SetupModel(TranscribeDto dto)
        {
            var start = Stopwatch.GetTimestamp();
            if (!string.IsNullOrWhiteSpace(dto.model) && ModelName != dto.model)
            {
                await Runner.DisposeAsync();
                Factory.Dispose();
                Factory = WhisperFactory.FromPath(Path.Combine(Program.MODEL_DIR, dto.model));
                Runner = Factory.CreateBuilder().WithThreads(Program.THREAD_COUNT).WithPrintTimestamps(dto.timestamps).Build();
            }
            _ModelLoadTime = Stopwatch.GetElapsedTime(start).TotalSeconds;
        }

        public string GetStatistics()
        {
            var stats = new Dictionary<string, double>
            {
                { "Wait Time ", _WaitTime },
                { "Model Load", _ModelLoadTime },
                { "Decoding  ", _IngestTime },
                { "Transcribe", _InferTime },
                { "Total Time", TotalTime },
            };

            return $"{Environment.NewLine}{{END}}{Environment.NewLine}" + string.Join(Environment.NewLine, stats.Select(kv => $"{kv.Key}: {kv.Value:0.00} sec")) + Environment.NewLine;
        }

        private void ResetStatistics()
        {
            _WaitTime = 0d;
            _ModelLoadTime = 0d;
            _IngestTime = 0d;
            _InferTime = 0d;
        }
    }
}