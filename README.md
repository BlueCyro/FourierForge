# Fourier Forge

Fourier Forge is a mod for NeosVR that lets you perform live FFT analysis on audio streams and exposes the results as a set of dynamic variables you can interface with. It also exposes
settings for the FFT analysis, such as the desired window size, the number of bands, and much more.

## Installation

Simply go to the releases page, download the latest DLL, and place it in the `nml_mods` folder in your Neos installation directory.

## Usage

If you're not a developer, you can skip this section and just drop your FFT stream into a pre-made visualizer of your choice. In fact, you can use an example I've made myself by copying the following link into your neos window and saving the public folder:

`neosrec:///U-Cyro/R-83292499-ad47-4422-9c6b-6419ddbf5276`

The default settings should work fine for most use cases, but you can change them in-game by opening the [NeosModSettings](https://github.com/badhaloninja/NeosModSettings) tab in your dash and navigating to the "Fourier Forge" tab. If you don't care about these and just want to use the mod, you can skip this section, but I would highly recommend reading this anyways.

**Do note that this mod changes a setting on your audio stream, see the "Audio Stream Type" bullet below for more information.**
### Setting descriptions
The following is a more verbose description of each setting in the order they appear in the settings tab.

- **Enabled**: Controls whether or not the functionality of the mod is enabled.

- **Window Size**: The size of the window used for the FFT analysis. You can think of this as the resolution of the FFT. The higher the value, the more granular the analysis will be, but the slower the FFT will respond to changes in the audio stream. This is due to the fact that the number of samples used for the FFT increases as the window size increases. When you're streaming audio, you're only getting about 400-900 samples per frame, so it takes a while for the buffer to fill up. The default value of 4096 should give you a pretty high resolution, but if you need something more responsive, you should try lowering this value.

- **Truncation Factor**: This is a value that is used to truncate the FFT results. By default this setting is set to 256, which means that the FFT results will be cut down to the first 256 values of the FFT result. Increasing this value may significantly decrease performance due to the sheer amount of data that exists in high resolution FFT results.

- **Window function**: This is the window function used for the FFT analysis. The default value is "Hann", which is a good all-around window function. If you're not sure what this is, you can leave it as-is. If you want to learn more about window functions, you can read more about them [here](https://en.wikipedia.org/wiki/Window_function).

- **Audio Stream Type**: This setting controls what best to tailer the audio stream for. By default this is set to 'RestrictedLowDelay' to reduce the amount delay between the audio stream and the FFT results. **Note**: This setting OVERRIDES the Neos' default audio stream type of 'Audio'. If you notice people complaining that your stream is choppy, you can change this setting back to Audio to help, but this *may* introduce desync between the audio stream and the FFT results.

- **Stream Encoding Type**: This controls what type of encoding to use. By default this is set to 'Quantized', which reduces the bit depth of the streamed values considerably, but still keeps enough data to be useful for visualizations and the like. If you and the parties connected to the session have unlimited amounts of bandwidth, you can change this to 'Full' but I highly recommend leaving this alone unless you positively need the extra resolution.

- **Stream bit depth**: This controls the bit depth of the streamed values. By default this is set to 10, which is a good balance between resolution and bandwidth usage. If you find that the values are stair-stepping too much, you can increase this value to 11 or 12 if you have the extra bandwidth to spare.

- **Interpolation Factor**: This setting will control how much the FFT results are interpolated. By default this is set to 0.15, which should be fine for the majority of use cases. If people find that the FFT results are too choppy for them, you can increase this value to 0.2 or 0.25 to help, but you may introduce desync if you go too far.

- **Stream update rate**: This controls how often the FFT streams are updated. By default this is set to 0, which means that you need the 'Force Update' setting to be true in order for the streams to update. If you want to save on bandwidth, you can turn off Force Update, and set this value to 1 or 2, though this may introduce desync.

- **Force Update**: This setting controls whether or not the FFT streams should update every frame. By default this is set to true. Setting this to false means you need to set the 'Stream Update Rate' to a value greater than 0 in order for the streams to update.

- **Band Monitors Only**: This setting controls whether or not to solely update the band monitors. By default this is set to false. Setting this to true will disable the FFT stream updates, and only update the band monitors. This is useful if you want to use the band monitors for visualizations (such as bass kicks, or mid-range frequencies), but don't need the full FFT stream.


### In-game usage

By default each audio stream does not have a variable space, but contains dynamic variables. Once you've placed the stream in a dynamic vriable space you can then access the FFT values by using dynamic references (e.g. DynamicReferenceVariable or DynamicReferenceVariableDriver) with the type ``IField`1[System.Single]`` and the variable naming scheme of 'StreamFFTValue(number)' (ex: 'StreamFFTValue27')

The reason these are IFields and not raw values is because syncing so many values through a dynamic variable space is very taxing, and this helps alleviate a lot of the overhead. If you're looking to drive a field based on one of these values. You can use a DynamicReferenceVariableDriver and point it at the source of a ValueCopy to drive a value with it.

To access band monitors (for more simplified visualizations), you can use more conventional dynamic references of just a float with the naming scheme of 'FFTBand(number)' (ex: 'FFTBand0' for sub-bass values). A reference for the indices of each band can be found [here](BandMonitorReference.txt).

Auxiliary information about the FFT stream can be accessed through the following dynamic variables:

- `StreamFFTWindowSize`: The size of the window used for the FFT analysis as an integer.
- `StreamFFTTruncationFactor`: The amount of values left over after truncation as an integer.
- `StreamType`: The type of stream used to send the data as a string. This is mostly for debug purposes.

## Tips

- For visualizations, I recommend applying a multiplier to each FFT value with: (FFTIndex + 2) * log10(value) * 10000 to get a more visually appealing result. Different window sizes may vary.

- You can convert each FFT value into decibels by the formula: 20 * log10(value) or 10 * log10(energy), this also applies to the band monitors.

- You can convert the FFT values into energy by squaring them.


That's about it, enjoy!
