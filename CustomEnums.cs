using CSCore.DSP;
using FrooxEngine;

public static class WindowFunctionsList
{
    public static WindowFunction[] WindowFuncs = new WindowFunction[] {
        WindowFunctions.None,
        WindowFunctions.Hamming,
        WindowFunctions.HammingPeriodic,
        WindowFunctions.Hanning,
        WindowFunctions.HanningPeriodic
    };
}

public enum WindowFunctionEnum
{
    None,
    Hamming,
    HammingPeriodic,
    Hann,
    HannPeriodic
}

public enum StreamEncodingWrapper
{
    Quantized = ValueEncoding.Quantized,
    Full = ValueEncoding.Full
}