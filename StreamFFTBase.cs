using FrooxEngine;
using CodeX;
using CSCore.DSP;

namespace FourierForge;
public class StreamFFTBase
{
    public static Dictionary<UserAudioStream<StereoSample>, IStreamFFTPlayer> StreamSamples = new Dictionary<UserAudioStream<StereoSample>, IStreamFFTPlayer>();
    internal ShiftQueue<float> _sampleBuffer;
    internal readonly FftProvider _fftProvider;
    public int FFTBinSize { get; private set; }
    public int NumSamplesSnipped { get; private set; }
    internal readonly int _channels;
    public float[] SampleBuffer { get; private set; }
    public StreamFFTBase(int binSize, int snipTo, int channels = 1)
    {
        _channels = channels;
        FFTBinSize = binSize;
        NumSamplesSnipped = snipTo;
        _fftProvider = new FftProvider(1, (FftSize)FFTBinSize);
        _fftProvider.WindowFunction = WindowFunctions.Hanning;
        _sampleBuffer = new ShiftQueue<float>(FFTBinSize);
        SampleBuffer = new float[FFTBinSize];
        _sampleBuffer.Fill(FFTBinSize);
    }
    ~StreamFFTBase()
    {
        FourierForge.Msg("StreamFFTBase destroyed");
    }

    public float[] GetFFTData<S>(Span<S> samples) where S : unmanaged, IAudioSample
    {
        // I know the FftProvider has the option for multiple channels, but either I'm not using it correctly or it's broken because when I try to
        // MemoryMarshal cast the StereoSamples into a Span<float> and pipe it into the provider, it makes the FFT really staggered and it ends up looking terrible.
        foreach (var sample in samples)
        {
            _sampleBuffer.Enqueue(sample.ToMono());
        }
        _fftProvider.Add(_sampleBuffer, FFTBinSize);
        _fftProvider.GetFftData(SampleBuffer);
        return SampleBuffer;
    }
}