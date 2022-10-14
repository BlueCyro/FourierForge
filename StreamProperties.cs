using FrooxEngine;
using POpusCodec.Enums;


namespace FourierForge;
public struct StreamProperties
{
    public ValueEncoding Encoding;
    public float InterpolationOffset;
    public byte FullFrameBits;
    public uint UpdatePeriod;
    public uint UpdatePhase;
    public WindowFunctionEnum Window;
    public OpusApplicationType AppType;
    public static StreamProperties Default = new StreamProperties(
        ValueEncoding.Quantized,
        0.15f,
        10,
        0,
        0,
        WindowFunctionEnum.Hann,
        OpusApplicationType.RestrictedLowDelay
    );

    public StreamProperties(ValueEncoding encoding, float interpOffset, byte fullFrameBits, uint updatePeriod, uint updatePhase, WindowFunctionEnum window, OpusApplicationType appType)
    {
        Encoding = encoding;
        InterpolationOffset = interpOffset;
        FullFrameBits = fullFrameBits;
        UpdatePeriod = updatePeriod;
        UpdatePhase = updatePhase;
        Window = window;
        AppType = appType;
    }
}