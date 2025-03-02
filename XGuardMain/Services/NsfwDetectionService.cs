using System.Diagnostics;
using Compunet.YoloSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuard.Services
{
    public class NsfwDetectionService
    {
        private class Detection
        {
            private DateTime _timestamp;

            public Detection(DateTime timestamp)
            {
                _timestamp = timestamp;
            }

            public DateTime Timestamp => _timestamp;
            public TimeSpan Lifetime => DateTime.Now - _timestamp + DETECTION_LIFETIME;
        }
        
        public const string MODEL_FILENAME = "adult-m-gen1.onnx";
        public const int MIN_RATE = 5000; // ms
        public static readonly TimeSpan DETECTION_LIFETIME = new TimeSpan(0, 0, 0, 30, 0);
        public const int MAX_DETECTION_COUNT = 2;
        public const int LOCK_TIME = 30;

        private YoloPredictor _yoloPredictor;
        private YoloConfiguration _yoloConfiguration;
        private int _detectionRate = MIN_RATE;
        private int _noDetectionsTimer; // in seconds
        private int _disableTimer; // in seconds
        private List<Detection> _detections = new List<Detection>(MAX_DETECTION_COUNT);    

        public int NoDetectionsTimer { get => _noDetectionsTimer; set => _noDetectionsTimer = value; }
        public int DisableTimer { get => _disableTimer; set => _disableTimer = value; }
        public int DetectionRate { get => _detectionRate; }

        public bool Locked { get; private set; }

        public event Action OnLock;
        public event Action OnUnlock;
        public event Action OnTimer;

        public NsfwDetectionService()
        {
            _yoloConfiguration = new YoloConfiguration()
            {
                Confidence = 0.75f,
            };

            _yoloPredictor = new YoloPredictor(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MODEL_FILENAME));

            Logger.Info($"Detection Model {MODEL_FILENAME} loaded");
        }

        ~NsfwDetectionService()
        {
            _yoloPredictor.Dispose();
        }

        public async void LogicLoop()
        {
            while (true)
            {
                try
                {
                    _detections.RemoveAll(detection =>
                    {
                        return DateTime.Now > detection.Timestamp + DETECTION_LIFETIME;
                    });

                    if (_detections.Count == 0)
                    {
                        _noDetectionsTimer++;
                        OnTimer?.Invoke();
                    }
                    else
                    {
                        _noDetectionsTimer = 0;
                        OnTimer?.Invoke();
                        _detectionRate = MIN_RATE;
                    }

                    if (_detections.Count >= MAX_DETECTION_COUNT)
                    {
                        _noDetectionsTimer = -LOCK_TIME;
                        OnTimer?.Invoke();
                        _detections.Clear();
                        Locked = true;
                        OnLock?.Invoke();
                    }

                    if (Locked && _noDetectionsTimer >= 0)
                    {
                        Locked = false;
                        OnUnlock?.Invoke();
                    }

                    if (_disableTimer > 0)
                    {
                        _disableTimer--;
                    }

                    _detectionRate = MIN_RATE;

                    if (_noDetectionsTimer > 40)
                    {
                        _detectionRate = MIN_RATE * 2;
                    }
                    if (_noDetectionsTimer > 120)
                    {
                        _detectionRate = MIN_RATE * 4;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Detector BL Loop Error: {ex.Message}");
                }

                await Task.Delay(1000);
            }
        }

        public async void DetectionLoop()
        {
            while (true)
            {
                int elapsedTime = 0;

                try
                {
                    if (_disableTimer <= 0 && _noDetectionsTimer >= 0)
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        bool hasNsfw = false;
                        var images = await XGuardUser.Instance.TakeScreenshots();
                        float maxConfidence = 0f;

                        for (int i = 0; i < images.Length; i++)
                        {
                            var result = await _yoloPredictor.DetectAsync(images[i], _yoloConfiguration);

                            if (result.Count > 0)
                            {
                                hasNsfw = true;

                                var confidence = result.Max(d => d.Confidence);
                                if (confidence > maxConfidence) maxConfidence = confidence;

                                // Save image
                                string uniqueName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + $"_{i}";
                                if (!Directory.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nsfw-images")))
                                {
                                    Directory.CreateDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nsfw-images"));
                                }
                                var imgPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"nsfw-images/image_{uniqueName}.png");
                                images[i].SaveAsPng(imgPath);

                                // Send image via telegram
                                BotService.SendNsfsNotification(imgPath);
                            }
                        }

                        stopwatch.Stop();

                        elapsedTime += (int)stopwatch.ElapsedMilliseconds;

                        if (hasNsfw)
                        {
                            _detections.Add(new Detection(DateTime.Now));
                            _noDetectionsTimer = 0;
                            OnTimer?.Invoke();
                            _detectionRate = MIN_RATE;
                            SoundPlayer.Play(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "warn.wav"));
                        }

                        Logger.Info($"Detection Result: {(hasNsfw ? $"NSFW Detected with {MathF.Round(maxConfidence * 100f)}% confidence" : "No Detections")} ({stopwatch.ElapsedMilliseconds} ms.)");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Detection failed: {ex}");
                }

                await Task.Delay(_detectionRate - elapsedTime < 100 ? 100 : _detectionRate - elapsedTime);
            }
        }
    }
}
