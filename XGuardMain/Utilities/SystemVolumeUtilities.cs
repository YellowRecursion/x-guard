using NAudio.CoreAudioApi;

namespace XGuard.Utilities
{
    public static class SystemVolumeUtilities
    {
        private static MMDevice GetDefaultDevice()
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                return deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
        }

        public static bool Mute
        {
            get
            {
                return GetDefaultDevice().AudioEndpointVolume.Mute;
            }
            set
            {
                GetDefaultDevice().AudioEndpointVolume.Mute = value;
            }
        }
    }
}
