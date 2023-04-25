using Whisper.net;

namespace speech2text
{
    public static class AI
{
    private static readonly Lazy<WhisperFactory> tinyFactory = new(() => WhisperFactory.FromPath("/models/ggml-tiny.bin"));
    private static readonly Lazy<WhisperFactory> baseFactory = new(() => WhisperFactory.FromPath("/models/ggml-base.bin"));
    private static readonly Lazy<WhisperFactory> mediumFactory = new(() => WhisperFactory.FromPath("/models/ggml-medium.bin"));
    private static readonly Lazy<WhisperFactory> largeFactory = new(() => WhisperFactory.FromPath("/models/ggml-large-v1.bin"));

    public static WhisperFactory TinyFactory => tinyFactory.Value;
    public static WhisperFactory BaseFactory => baseFactory.Value;
    public static WhisperFactory MediumFactory => mediumFactory.Value;
    public static WhisperFactory LargeFactory => largeFactory.Value;

    public static int ActiveJobs = 0;
}

}