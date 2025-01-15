using System;
using System.Runtime.InteropServices;

namespace XGuardLibrary.Utilities
{
    public static class SoundPlayer
    {
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern bool PlaySound(string pszSound, IntPtr hmod, uint fdwSound);

        private const uint SND_ASYNC = 0x0001;       // Воспроизведение асинхронно
        private const uint SND_FILENAME = 0x00020000; // Звук из файла
        private const uint SND_LOOP = 0x0008;        // Повторять звук (используется вместе с ASYNC)

        /// <summary>
        /// Воспроизводит звук из файла один раз асинхронно.
        /// </summary>
        /// <param name="filePath">Путь к файлу .wav.</param>
        public static void Play(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));

            if (!PlaySound(filePath, IntPtr.Zero, SND_FILENAME | SND_ASYNC))
                throw new InvalidOperationException("Не удалось воспроизвести звук: " + filePath);
        }

        /// <summary>
        /// Воспроизводит звук из файла в цикле асинхронно.
        /// </summary>
        /// <param name="filePath">Путь к файлу .wav.</param>
        public static void PlayLooping(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Путь к файлу не может быть пустым.", nameof(filePath));

            if (!PlaySound(filePath, IntPtr.Zero, SND_FILENAME | SND_ASYNC | SND_LOOP))
                throw new InvalidOperationException("Не удалось воспроизвести звук: " + filePath);
        }

        /// <summary>
        /// Останавливает любое воспроизведение звука.
        /// </summary>
        public static void Stop()
        {
            PlaySound(null, IntPtr.Zero, SND_ASYNC);
        }
    }
}
