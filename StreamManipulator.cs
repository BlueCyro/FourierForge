using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.LogiX.Operators;
using CodeX;
using BaseX;

// This is potentially a little overkill, but it'll allow me to easily add more types of streams to carry the same data in the future.
namespace FourierForge;
public static class StreamManipulator<T>
{
    static StreamManipulator()
    {
        if (typeof(T) == typeof(float))
        {
            _apply = ApplyFloat;
            _setupStreams = SetupFloatStreams;
            _explodeStreams = ExplodeFloatStreams;
        }
        else if (typeof(T) == typeof(float4))
        {
            _apply = ApplyFloat4;
            _setupStreams = SetupFloat4Streams;
            _explodeStreams = ExplodeFloat4Streams;
        }
        else
        {
            throw new Exception("Invalid type for StreamApplier!");
        }
    }
    private static Action<StreamFFTPlayer<T>, bool> _apply;
    private static Func<UserAudioStream<StereoSample>, int, StreamProperties, ValueStream<T>[]> _setupStreams;
    private static Func<UserAudioStream<StereoSample>, Slot, Slot, ValueField<float>[]?> _explodeStreams;
    public static void Apply(StreamFFTPlayer<T> fftStream, bool forceUpdate = false)
    {
        _apply(fftStream, forceUpdate);
    }
    public static ValueStream<T>[] SetupStreams(UserAudioStream<StereoSample> audioStream, int numStreams, StreamProperties props = default(StreamProperties))
    {
        return _setupStreams(audioStream, numStreams, props);
    }

    public static ValueField<float>[]? ExplodeStreams(UserAudioStream<StereoSample> audioStream, Slot target, Slot? variableSlot)
    {
        return _explodeStreams(audioStream, target, variableSlot ?? target);
    }

    private static void ApplyFloat(StreamFFTPlayer<T> fftStream, bool forceUpdate = false)
    {
        for (int i = 0; i < fftStream.NumSamplesSnipped; i++)
        {
            fftStream.FFTVals[i].Value = (T)(object)fftStream.SampleBuffer[i];
            if (forceUpdate)
            {
                fftStream.FFTVals[i].ForceUpdate();
            }
        }
    }

    private static void ApplyFloat4(StreamFFTPlayer<T> fftStream, bool forceUpdate = false)
    {
        for (int i = 0; i < fftStream.NumSamplesSnipped / 4; i++)
        {
            float4 val = new float4(fftStream.SampleBuffer[i * 4], fftStream.SampleBuffer[i * 4 + 1], fftStream.SampleBuffer[i * 4 + 2], fftStream.SampleBuffer[i * 4 + 3]);
            fftStream.FFTVals[i].Value = (T)(object)val;
            if (forceUpdate)
            {
                fftStream.FFTVals[i].ForceUpdate();
            }
        }
    }

    private static ValueStream<T>[] SetupFloatStreams(UserAudioStream<StereoSample> audioStream, int numStreams, StreamProperties props)
    {
        var user = audioStream.LocalUser;
        var streams = new ValueStream<T>[numStreams];
        for (int i = 0; i < numStreams; i++)
        {
            var stream = user.GetStreamOrAdd<ValueStream<T>>($"FFTStreamItem.{audioStream.ReferenceID}.{i}", stream => {
                stream.SetInterpolation();
                stream.SetUpdatePeriod(props.UpdatePeriod, props.UpdatePhase);
                ((Sync<float>)stream.GetSyncMember("InterpolationOffset")).Value = props.InterpolationOffset;
                stream.Encoding = props.Encoding;
                stream.FullFrameBits = props.FullFrameBits;
                stream.FullFrameMin = (T)(object)0f;
                stream.FullFrameMax = (T)(object)1f;
            });
            streams[i] = stream;
        }
        return streams;
    }

    private static ValueStream<T>[] SetupFloat4Streams(UserAudioStream<StereoSample> audioStream, int numStreams, StreamProperties props)
    {
        var user = audioStream.LocalUser;
        var streams = new ValueStream<T>[numStreams / 4];
        for (int i = 0; i < numStreams / 4; i++)
        {
            var stream = user.GetStreamOrAdd<ValueStream<T>>($"FFTStreamItem.{audioStream.ReferenceID}.{i}", stream => {
                stream.SetInterpolation();
                stream.SetUpdatePeriod(props.UpdatePeriod, props.UpdatePhase);
                ((Sync<float>)stream.GetSyncMember("InterpolationOffset")).Value = props.InterpolationOffset;
                stream.Encoding = props.Encoding;
                stream.FullFrameBits = props.FullFrameBits;
                stream.FullFrameMin = (T)(object)new float4(0f, 0f, 0f, 0f);
                stream.FullFrameMax = (T)(object)new float4(1f, 1f, 1f, 1f);
            });
            streams[i] = stream;
        }
        return streams;
    }

    private static ValueField<float>[]? ExplodeFloatStreams(UserAudioStream<StereoSample> audioStream, Slot targetSlot, Slot variableSlot)
    {
        if (audioStream.TryGetFFTStream(out IStreamFFTPlayer fftPlayer))
        {
            ValueField<float>[] floats = new ValueField<float>[fftPlayer.NumSamplesSnipped];
            for (int i = 0; i < fftPlayer.FFTStreams.Length; i++)
            {
                var driver = targetSlot.AttachComponent<ValueDriver<float>>();
                var field = targetSlot.AttachComponent<ValueField<float>>();
                variableSlot.CreateReferenceVariable<IField<float>>($"FFTStreamValue{i}", field.Value);

                driver.ValueSource.Target = (ValueStream<float>)fftPlayer.FFTStreams[i];
                driver.DriveTarget.Target = field.Value;
            }
            return floats;
        }
        return null;
    }
    private static ValueField<float>[]? ExplodeFloat4Streams(UserAudioStream<StereoSample> audioStream, Slot targetSlot, Slot variableSlot)
    {
        if (audioStream.TryGetFFTStream(out IStreamFFTPlayer fftPlayer))
        {
            List<ValueField<float>> floats = new List<ValueField<float>>();
            for (int i = 0; i < fftPlayer.FFTStreams.Length; i++)
            {
                var deconstructor = targetSlot.AttachComponent<Deconstruct_Float4>();
                var float4Driver = targetSlot.AttachComponent<ValueDriver<float4>>();
                var float4Field = targetSlot.AttachComponent<ValueField<float4>>();

                float4Driver.ValueSource.Target = (ValueStream<float4>)fftPlayer.FFTStreams[i];
                float4Driver.DriveTarget.Target = float4Field.Value;

                deconstructor.V.Target = float4Field.Value;
                
                var xDriver = targetSlot.AttachComponent<DriverNode<float>>();
                xDriver.Source.Target = deconstructor.X;
                var xValueField = targetSlot.AttachComponent<ValueField<float>>();
                var xVariable = variableSlot.CreateReferenceVariable<IField<float>>($"FFTStreamValue{i * 4}", xValueField.Value);
                xDriver.DriveTarget.Target = xValueField.Value;
                floats.Add(xValueField);

                var yDriver = targetSlot.AttachComponent<DriverNode<float>>();
                yDriver.Source.Target = deconstructor.Y;
                var yValueField = targetSlot.AttachComponent<ValueField<float>>();
                var yVariable = variableSlot.CreateReferenceVariable<IField<float>>($"FFTStreamValue{i * 4 + 1}", yValueField.Value);
                yDriver.DriveTarget.Target = yValueField.Value;
                floats.Add(yValueField);

                var zDriver = targetSlot.AttachComponent<DriverNode<float>>();
                zDriver.Source.Target = deconstructor.Z;
                var zValueField = targetSlot.AttachComponent<ValueField<float>>();
                var zVariable = variableSlot.CreateReferenceVariable<IField<float>>($"FFTStreamValue{i * 4 + 2}", zValueField.Value);
                zDriver.DriveTarget.Target = zValueField.Value;
                floats.Add(zValueField);

                var wDriver = targetSlot.AttachComponent<DriverNode<float>>();
                wDriver.Source.Target = deconstructor.W;
                var wValueField = targetSlot.AttachComponent<ValueField<float>>();
                var wVariable = variableSlot.CreateReferenceVariable<IField<float>>($"FFTStreamValue{i * 4 + 3}", wValueField.Value);
                wDriver.DriveTarget.Target = wValueField.Value;
                floats.Add(wValueField);
            }
            return floats.ToArray();
        }
        return null;
    }
}