using Cosmos.HAL.Drivers.PCI.Audio;
using Cosmos.System.Audio;
using Cosmos.System.Audio.IO;
using IL2CPU.API.Attribs;
using System;

namespace wdOS.Core.OS.LowLevel
{
    internal static class AudioPlayer
    {
        internal static AudioMixer Mixer = new();
        internal static AudioStream BootChime;
        [ManifestResourceStream(ResourceName = "wdOS.Core.OS.Resources.bootChime.wav")]
        internal static byte[] RawBootChime;
        internal static AudioManager Manager;
        internal static void Setup()
        {
            try
            {
                BootChime = new MemoryAudioStream(new(Cosmos.HAL.Audio.AudioBitDepth.Bits8, 1, false), 6000, RawBootChime);
                Manager = new()
                {
                    Output = AC97.Initialize(8196),
                    Stream = Mixer,
                    Enabled = true
                };
                Mixer.Streams.Add(BootChime);
            }
            catch (Exception e)
            {
                Console.WriteLine("No audio devices found or device is broken");
                Console.WriteLine(e.Message);
                RawBootChime = null;
                BootChime = null;
                Manager = null;
                Mixer = null;
            }
        }
    }
}
