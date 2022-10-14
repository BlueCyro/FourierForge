using FrooxEngine;
using CodeX;
using BaseX;
using System.Reflection;


namespace FourierForge;
public static class AudioStreamExtensions
{
    public static bool TryGetFFTStream(this UserAudioStream<StereoSample> audioStream, out IStreamFFTPlayer streamFFT, bool createIfNotFound = false)
    {
        return StreamFFTBase.StreamSamples.TryGetValue(audioStream, out streamFFT);
    }
    public static IStreamFFTPlayer CreateFFTStream<T>(this UserAudioStream<StereoSample> audioStream, int fftBinSize, int sliceTo, int channels = 1, StreamProperties streamProperties = default(StreamProperties))
    {
        User user = audioStream.LocalUser;
        void removeSelf(IChangeable c)
        { 
            audioStream.RemoveFFTStream();
            audioStream.Destroyed -= removeSelf;
        };
        audioStream.Destroyed += removeSelf;
        IStreamFFTPlayer fftStream = new StreamFFTPlayer<T>(audioStream, fftBinSize, sliceTo, channels, streamProperties);
        StreamFFTBase.StreamSamples.Add(audioStream, fftStream);
        
        Slot extraVariables = audioStream.Slot.AddSlot("<color=#FFAAAA>Extra Variables</color>");

        extraVariables.CreateVariable<int>("StreamFFTWindowSize", fftBinSize);
        extraVariables.CreateVariable<int>("StreamFFTTruncationFactor", fftStream.NumSamplesSnipped);
        extraVariables.CreateVariable<string>("StreamType", typeof(T).Name);
        extraVariables.CreateVariable<string>("StreamModVersion", FourierForge.VersionString);
        extraVariables.CreateVariable<int>("StreamModVersionInt", FourierForge.VersionInt);
        
        Slot bandMonitors = audioStream.Slot.AddSlot("<color=purple>Band Monitors</color>");
        Slot variables = audioStream.Slot.AddSlot("Dynamic Variables (<color=orange>This is mildly large, open at your own expense</color>)");
        Slot drivers = audioStream.Slot.AddSlot("Stream Data (<color=red>This will hurt a lot to open, will also jump up to the nearest active slot if in an inactive hierarchy</color>)");

        drivers.PersistentSelf = false;
        drivers.DestroyWhenUserLeaves(audioStream.LocalUser);
        audioStream.Slot.DestroyWhenDestroyed(drivers);
        void activeChanged(Slot s)
        {
            if (drivers != null && !drivers.IsDestroyed)
            {
                if (audioStream.Slot.IsActive)
                {
                    drivers.SetParent(audioStream.Slot);
                }
                else
                {
                    Slot nearestActive = drivers.FindParent(s => s.IsActive);
                    if (nearestActive != null)
                    {
                        drivers.SetParent(nearestActive);
                    }
                }
            }
            else
            {
                audioStream.Slot.ActiveChanged -= activeChanged;
            }
        }
        audioStream.Slot.ActiveChanged += activeChanged;

        fftStream.ExplodeStreams(drivers, variables);
        fftStream.SetupBandMonitors(bandMonitors, drivers);
        return fftStream;
    }

    public static IStreamFFTPlayer CreateFFTStream(this UserAudioStream<StereoSample> audioStream, Type type, int fftBinSize, int sliceTo, int channels = 1)
    {
        MethodInfo method = typeof(AudioStreamExtensions).GetMethod("CreateFFTStream", BindingFlags.Static | BindingFlags.Public, null, new Type[] { typeof(UserAudioStream<StereoSample>), typeof(int), typeof(int), typeof(int) }, null);
        MethodInfo generic = method.MakeGenericMethod(type);
        return (IStreamFFTPlayer)generic.Invoke(null, new object[] { audioStream, fftBinSize, sliceTo, channels });
    }
    public static void RemoveFFTStream(this UserAudioStream<StereoSample> audioStream)
    {
        if (audioStream.TryGetFFTStream(out IStreamFFTPlayer streamFFT))
        {
            foreach (var stream in streamFFT.FFTStreams)
            {
                stream.Destroy();
            }
            foreach (var stream in streamFFT.BandMonitors)
            {
                stream.Destroy();
            }
            StreamFFTBase.StreamSamples.Remove(audioStream);
            FourierForge.Msg($"Removed FFT Streams for {audioStream.ReferenceID}");
        }
        else
        {
            FourierForge.Msg($"Stream {audioStream.ReferenceID} doesn't have any streams! ({audioStream == null})");
        }
    }
    public static void SetupFloatMultiplexer(this ValueField<float>[] array)
    {
        if (array.Any(v => v == null))
        {
            throw new Exception("Null field in array!");
        }
        var Multiplexer = array[0].Slot.AttachComponent<FrooxEngine.LogiX.Operators.Multiplexer<float>>();
        for (int i = 0; i < array.Length; i++)
        {
            if (Multiplexer.Operands[i] == null)
                Multiplexer.Operands.Add();
            
            Multiplexer.Operands[i].Target = (IElementContent<float>)array[i];
        }
    }
}