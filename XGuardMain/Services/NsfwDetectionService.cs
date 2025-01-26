using System.Diagnostics;
using Compunet.YoloSharp;
using XGuardLibrary;
using XGuardLibrary.Utilities;

namespace XGuard.Services
{
    public class NsfwDetectionService
    {
        public const string MODEL_FILENAME = "adult-m-gen1.onnx";
        public const string SCREENSHOT_FILENAME = "screenshot_{0}.png";
        public const int MIN_RATE = 3000; // ms
        public const int DETECTION_SCORE = 1;
        public const int NO_DETECTION_SCORE = -1;
        public const int MAX_DETECTION_SCORE = DETECTION_SCORE * 2;

        private ProcessObserver _screenshoterObserver;
        private YoloPredictor _yoloPredictor;
        private YoloConfiguration _yoloConfiguration;
        private readonly string _screenshoterPath;
        private int _nsfwCounter;
        private int _detectionRate = MIN_RATE;
        private int _noDetectionsTimer; // in seconds
        private int _disableTimer; // in seconds

        public int NsfwCounter
        {
            get => _nsfwCounter;
            set
            {
                _nsfwCounter = Math.Clamp(value, 0, MAX_DETECTION_SCORE);
            }
        }

        public bool MaxNsfwScoreReached => NsfwCounter == MAX_DETECTION_SCORE;

        public int NoDetectionsTimer { get => _noDetectionsTimer; set => _noDetectionsTimer = value; }
        public int DisableTimer { get => _disableTimer; set => _disableTimer = value; }
        public int DetectionRate { get => _detectionRate; }

        public bool Locked { get; private set; }

        public event Action OnLock;
        public event Action OnUnlock;

        public NsfwDetectionService()
        {
            _yoloConfiguration = new YoloConfiguration()
            {
                Confidence = 0.6f,
            };

            _yoloPredictor = new YoloPredictor(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, MODEL_FILENAME));

            Logger.Info($"Detection Model {MODEL_FILENAME} loaded");

            _screenshoterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "XGuardScreenshoter.exe");
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
                    if (NsfwCounter == 0)
                    {
                        _noDetectionsTimer++;
                    }
                    else
                    {
                        _noDetectionsTimer = 0;
                        _detectionRate = MIN_RATE;
                    }

                    if (MaxNsfwScoreReached)
                    {
                        _noDetectionsTimer = -30;
                        NsfwCounter = 0;
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

                    if (_noDetectionsTimer > 30)
                    {
                        _detectionRate = MIN_RATE * 2;
                    }
                    if (_noDetectionsTimer > 120)
                    {
                        _detectionRate = MIN_RATE * 4;
                    }
                    if (_noDetectionsTimer > 300)
                    {
                        _detectionRate = MIN_RATE * 6;
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
                        ProcessExtensions.StartProcessAsCurrentUser(_screenshoterPath, workDir: AppDomain.CurrentDomain.BaseDirectory);

                        do
                        {
                            await Task.Delay(100);
                            elapsedTime += 100;
                        }
                        while (ProcessExtensions.GetCountOfProcessesInCurrentSession("XGuardScreenshoter") > 0);

                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var existingFiles = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "screenshot-*.png");

                        bool hasNsfw = false;

                        for (int i = 0; i < existingFiles.Length; i++)
                        {
                            using var image = SixLabors.ImageSharp.Image.Load(existingFiles[i]);
                            var result = await _yoloPredictor.DetectAsync(image, _yoloConfiguration);

                            if (result.Count > 0)
                            {
                                hasNsfw = true;
                            }
                        }

                        stopwatch.Stop();

                        elapsedTime += (int)stopwatch.ElapsedMilliseconds;

                        var score = hasNsfw ? DETECTION_SCORE : NO_DETECTION_SCORE;
                        NsfwCounter += score;

                        if (hasNsfw)
                        {
                            _noDetectionsTimer = 0;
                            _detectionRate = MIN_RATE;
                            SoundPlayer.Play(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "warn.wav"));
                        }

                        Logger.Info($"Detection Result: {(hasNsfw ? "NSFW Detected" : "No Detections")} ({stopwatch.ElapsedMilliseconds} ms.)");
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
