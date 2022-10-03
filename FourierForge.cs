using HarmonyLib;
using NeosModLoader;
using FrooxEngine;
using BaseX;
using CodeX;
using System.Reflection.Emit;
using POpusCodec.Enums;


namespace FourierForge;
public partial class FourierForge : NeosMod
{
    public override string Author => "Cyro";
    public override string Name => "FourierForge";
    public override string Version => "1.0.2";
    public override void OnEngineInit()
    {
        Config = GetConfiguration();
        Config!.Save(true);
        Harmony harmony = new Harmony("net.Cyro.FourierForge");
        harmony.PatchAll();
        Config.OnThisConfigurationChanged += c => {
            if (!Config!.GetValue(Enabled))
            {
                return;
            }
            var streamsDict = StreamFFTBase.StreamSamples;
            foreach (var stream in streamsDict.Values)
            {
                var props = new StreamProperties(
                    ValueEncoding.Quantized,
                    Config!.GetValue(InterpolationOffset),
                    Config!.GetValue(FullFrameBits),
                    Config!.GetValue(UpdatePeriod),
                    0,
                    Config!.GetValue(WindowFunction),
                    Config!.GetValue(AudioStreamAppType)
                );
                stream.ModifyStreamProperties(props);
            }
        };
    }

    [HarmonyPatch]
    static class UserAudioStreamPatcher
    {
        [HarmonyPatch(typeof(UserAudioStream<StereoSample>), "OnAwake")]
        [HarmonyPostfix]
        public static void OnAwake_Postfix(UserAudioStream<StereoSample> __instance)
        {
            if (__instance.ReferenceID.User != __instance.LocalUser.AllocationID || !Config!.GetValue(Enabled))
                return;
            
            int binSize = (int)Config!.GetValue(FftBinSize);
            int sliceTo = Config!.GetValue(TruncateTo);
            __instance.RunSynchronously(() => {
                __instance.CreateFFTStream<float4>(binSize, MathX.Clamp(sliceTo, 1, Math.Min(2048, binSize)), StereoSample.CHANNEL_COUNT);
            });
        }

        [HarmonyPatch(typeof(UserAudioStream<StereoSample>), "OnNewAudioData")]
        [HarmonyPostfix]
        public static void OnNewAudioData_Postfix(UserAudioStream<StereoSample> __instance, Span<StereoSample> buffer)
        {
            if (Config!.GetValue(Enabled) && __instance.World.Focus == World.WorldFocus.Focused && __instance.TryGetFFTStream(out IStreamFFTPlayer fftStream))
            {
                fftStream.GetFFTData(buffer);
                fftStream.ApplyStreams(Config!.GetValue(ForceUpdateStreams), Config!.GetValue(OnlyApplyBandMonitors));
            }
        }

        [HarmonyPatch(typeof(AudioStreamController), "BuildUI")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> BuildUI_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = instructions.ToList();
            int foundIndex = -1;

            for (int i = 0; i < codes.Count(); i++)
            {
                if (codes[i].opcode == OpCodes.Ldc_I4 && codes[i].operand != null && (int)codes[i].operand == 2049)
                {
                    foundIndex = i;
                    Msg($"Found OpusApplicationType opcode in AudioStreamController at index {i}!");
                    break;
                }
            }
            if (foundIndex == -1)
            {
                Msg("Failed to find OpusApplicationType opcode in AudioStreamController, aborting.");
                return codes;
            }
            codes[foundIndex].opcode = OpCodes.Call;
            codes[foundIndex].operand = typeof(UserAudioStreamPatcher).GetMethod("getAppType");
            return codes;
        }
        public static OpusApplicationType getAppType()
        {
            bool enabled = Config!.GetValue(Enabled);
            return enabled ? Config!.GetValue(AudioStreamAppType) : OpusApplicationType.Audio;
        }
    }
}
