using FrooxEngine;
using CodeX;
using CSCore.DSP;

namespace FourierForge;
public class StreamFFTBase
{
    public static Dictionary<UserAudioStream<StereoSample>, IStreamFFTPlayer> StreamSamples = new Dictionary<UserAudioStream<StereoSample>, IStreamFFTPlayer>();
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
        SampleBuffer = new float[FFTBinSize];
    }

    public float[] GetFFTData<S>(Span<S> samples) where S : unmanaged, IAudioSample
    {
        foreach (S s in samples)
        {
            _fftProvider.Add(s[0], s[1]);
        }
        _fftProvider.GetFftData(SampleBuffer);
        return SampleBuffer;
    }
}