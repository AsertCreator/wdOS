using Cosmos.HAL.Drivers.PCI.Audio;
using Cosmos.System.Audio;
using Cosmos.System.Audio.IO;
using IL2CPU.API.Attribs;
using System;

namespace wdOS.Core.Foundation
{
    internal static class AudioPlayer
    {
        internal static AudioMixer Mixer = new();
        internal static AudioStream BootChime;
        [ManifestResourceStream(ResourceName = "wdOS.Core.Resources.bootChime.wav")]
        internal static byte[] RawBootChime;
        internal static AudioManager Manager;
        internal static void AudioInitialization()
        {
            try
            {
                Manager = new()
                {
                    Output = AC97.Initialize(8196),
                    Stream = Mixer,
                    Enabled = true
                };
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
        internal static void PlaySound(byte[] bytes)
        {
            var stream = new MemoryAudioStream(new(Cosmos.HAL.Audio.AudioBitDepth.Bits8, 1, false), 6000, bytes);
            Mixer.Streams.Add(stream);
        }
    }
}
