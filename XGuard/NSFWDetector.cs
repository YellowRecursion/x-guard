using System.Drawing.Imaging;
using Compunet.YoloSharp;
using System.Diagnostics;
using System.Media;
using Compunet.YoloSharp.Plotting;
using Compunet.YoloSharp.Data;
using SixLabors.ImageSharp;

namespace XGuard
{
    internal class NSFWDetector
    {
        public const string MODEL_FILENAME = "adult-m-gen1.onnx";
        public const string SCREENSHOT_FILENAME = "screenshot.png";
        public const string SCREENSHOT_PLOT_FILENAME = "screenshot_plot.png";
        public const int MIN_RATE = 3000; // ms
        public const int DETECTION_SCORE = 1;
        public const int NO_DETECTION_SCORE = -1;
        public const int MAX_DETECTION_SCORE = DETECTION_SCORE * 2;

        private YoloPredictor _yoloPredictor;
        private SoundPlayer _soundPlayer;
        private YoloConfiguration _yoloConfiguration;
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

        public NSFWDetector()
        {
            _yoloConfiguration = new YoloConfiguration()
            {
                Confidence = 0.6f,
            };

            _yoloPredictor = new YoloPredictor(Path.Combine(Program.CurrentDirectory, MODEL_FILENAME));

            _soundPlayer = new SoundPlayer(new MemoryStream(XGuard.Properties.Resources.WarnSound));

            Logger.Info($"Detection Model {MODEL_FILENAME} loaded");
        }

        ~NSFWDetector()
        {
            _yoloPredictor.Dispose();
        }

        public async void LogicLoop()
        {
            while (true)
            {
                try
                {
                    if (_noDetectionsTimer <= 0)
                    {
                        Program.OverlayForm.SetCloseButtonTimerValue(Math.Abs(_noDetectionsTimer));
                    }

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
                        _noDetectionsTimer = -60;
                        NsfwCounter = 0;
                        Program.OverlayForm.SetCloseButtonTimerValue(Math.Abs(_noDetectionsTimer));
                        Program.ShowOverlay = true;

                        Bot.SendNsfsNotification(Path.Combine(Program.CurrentDirectory, SCREENSHOT_FILENAME));
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
                    if (_disableTimer <= 0) 
                    {
                        Stopwatch stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var screenshotPath = Path.Combine(Program.CurrentDirectory, SCREENSHOT_FILENAME);
                        SaveScreenshot(screenshotPath);
                        using var image = SixLabors.ImageSharp.Image.Load(screenshotPath);
                        var result = await _yoloPredictor.DetectAsync(image, _yoloConfiguration);

                        stopwatch.Stop();

                        elapsedTime += (int)stopwatch.ElapsedMilliseconds;

                        var hasNsfs = result.Count > 0;

                        var score = hasNsfs ? DETECTION_SCORE : NO_DETECTION_SCORE;
                        NsfwCounter += score;

                        if (hasNsfs)
                        {
                            // SavePlot(image, result);
                            _noDetectionsTimer = 0;
                            _detectionRate = MIN_RATE;
                            _soundPlayer.Play();
                        }

                        Logger.Info($"Detection result: {result} {stopwatch.ElapsedMilliseconds} ms.");
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error($"Detection failed: {ex.Message}");
                }

                await Task.Delay(_detectionRate - elapsedTime < 100 ? 100 : _detectionRate - elapsedTime);
            }
        }

        public static async void SavePlot(SixLabors.ImageSharp.Image image, YoloResult<Detection> result)
        {
            DetectionPlottingOptions detectionPlottingOptions = new DetectionPlottingOptions();

            // Create plotted image from model results
            using var plotted = await result.PlotImageAsync(image, detectionPlottingOptions);

            // Write the plotted image to file
            plotted.Save(Path.Combine(Program.CurrentDirectory, SCREENSHOT_PLOT_FILENAME));
        }

        public static void SaveScreenshot(string fileName)
        {
            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;

            using (Bitmap screenshot = new Bitmap(w, h))
            {
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.CopyFromScreen(0, 0, 0, 0, screenshot.Size);
                }

                screenshot.Save(fileName, ImageFormat.Png);
            }
        }
    }
}
