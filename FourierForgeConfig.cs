using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using CodeX;
using System.Reflection.Emit;
using POpusCodec.Enums;
using CSCore.DSP;

namespace FourierForge;
public partial class FourierForge : NeosMod
{
    [AutoRegisterConfigKey]
    public static ModConfigurationKey<bool> Enabled = new ModConfigurationKey<bool>("Enabled", "Controls whether or not the mod is enabled", () => true);

    [AutoRegisterConfigKey]
    public static ModConfigurationKey<FftSize> FftBinSize = new ModConfigurationKey<FftSize>("FFT window size", "Controls the resolution of the FFT (high values may be slow, 2048-4096 recommended)", () => FftSize.Fft4096);
    
    [AutoRegisterConfigKey]
    public static ModConfigurationKey<int> TruncateTo = new ModConfigurationKey<int>("Truncation Factor", "Controls how many values of FFT are kept to be streamed, the rest are truncated. (Useful for saving processing/bandwidth)", () => 256);
    
    /*
    [AutoRegisterConfigKey]
    public static ModConfigurationKey<Type> StreamType = new ModConfigurationKey<Type>("Stream type", "Controls the type of stream used to send the FFT data", () => typeof(float4));
    */
    [AutoRegisterConfigKey]
    public static ModConfigurationKey<WindowFunctionEnum> WindowFunction = new ModConfigurationKey<WindowFunctionEnum>("Window function", "Controls the window function used for the FFT (Hann is best)", () => WindowFunctionEnum.Hann);

    [AutoRegisterConfigKey]
    public static ModConfigurationKey<OpusApplicationType> AudioStreamAppType = new ModConfigurationKey<OpusApplicationType>("Stream application type", "Control what best to tailor the audio stream for. RestrictedLowDelay may help with FFT/Audio syncing issues.", () => OpusApplicationType.RestrictedLowDelay);
    
    /*
    [AutoRegisterConfigKey]
    public static ModConfigurationKey<StreamEncodingWrapper> ValueEncoding = new ModConfigurationKey<StreamEncodingWrapper>("Value Encoding Type", "Controls what type of encoding to use (only change if you don't care about anyone's bandwidth, you heathen)", () => StreamEncodingWrapper.Quantized);
    */
    [AutoRegisterConfigKey]
    public static ModConfigurationKey<byte> FullFrameBits = new ModConfigurationKey<byte>("Full frame bit depth", "How many bits are used for each stream, high values reduce stepping on FFT values, but don't go over 11 or 12 unless you absolutely must!!", () => 10);

    [AutoRegisterConfigKey]
    public static ModConfigurationKey<float> InterpolationOffset = new ModConfigurationKey<float>("Interpolation offset amount", "Controls how interpolated the streams are (higher values may cause desync)", () => 0.15f);

    [AutoRegisterConfigKey]
    public static ModConfigurationKey<uint> UpdatePeriod = new ModConfigurationKey<uint>("Update period", "Higher values make the streams update slower, 1 is full-speed, 2 is half, etc. (has no effect if force update is true)", () => 0);

    [AutoRegisterConfigKey]
    public static ModConfigurationKey<bool> ForceUpdateStreams = new ModConfigurationKey<bool>("Force update streams", "Controls whether or not to force update streams, this makes them update each frame", () => true);

    [AutoRegisterConfigKey]
    public static ModConfigurationKey<bool> OnlyApplyBandMonitors = new ModConfigurationKey<bool>("Only apply to Band Monitors", "When enabled, will skip the full FFT and only apply the band monitors (this helps if you don't need the full FFT)", () => false);
    
    public static ModConfiguration? Config;
}