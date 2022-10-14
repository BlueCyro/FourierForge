using FrooxEngine;
using CodeX;

namespace FourierForge;
public interface IStreamFFTPlayer
{
    public ImplicitStream[] FFTStreams { get; }
    public int FFTBinSize { get; }
    public int NumSamplesSnipped { get; }
    float[] SampleBuffer { get; }
    ValueStream<float>[] BandMonitors { get; }
    float[] GetFFTData<S>(Span<S> samples) where S : unmanaged, IAudioSample;
    void ApplyStreams(bool forceUpdate = false, bool onlyApplyBandMonitors = false);
    public void ModifyStreamProperties(StreamProperties props);
    public void ExplodeStreams(Slot target, Slot? variableSlot = null);
}