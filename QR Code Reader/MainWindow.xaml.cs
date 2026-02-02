using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video;
using AForge.Video.DirectShow;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using Brush = System.Windows.Media.Brush;

namespace barcode_Reader
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isImagePresent;
        private bool _imageCloneProcessActice;
        private bool _settingsToggle;
        
        private int _jumpedFrame;
        private int _totalFreeze;
        private int _camSelected;
        
        private string _scannedContent = "";
        
        private Bitmap _lastDetection;
        private Bitmap _safeTempStreamBitmap;
        private Bitmap _streamBitmap;

        private readonly object _bitmapLock = new object();
        private CancellationTokenSource _cancellationTokenSource;

        private Thread _decodingThread;
        private FilterInfoCollection _camSources;
        private VideoCaptureDevice _camVideo;
        private QRCodeReader _decoder;
        
        private readonly Barcode _barcodeObject = new Barcode();
        private readonly Config _configdata = new Config();

        
        public MainWindow()
        {
            try
            {
                InitializeComponent();
                InitUi();
                SetDatagridContent();
                _cancellationTokenSource = new CancellationTokenSource();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}", "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }


        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                if (_jumpedFrame > _configdata.Frames)
                {
                    _jumpedFrame = 0;
                    
                    lock (_bitmapLock)
                    {
                        // Dispose old bitmap
                        _streamBitmap?.Dispose();
                        _streamBitmap = Image2Disk.CloneBitmap(eventArgs.Frame, _imageCloneProcessActice);
                        _isImagePresent = true;
                    }

                    using (var ms = new MemoryStream())
                    {
                        if (_configdata.Freeze == _totalFreeze)
                        {
                            _totalFreeze = 0;
                            _barcodeObject.showPoints = false;
                        }

                        Bitmap displayBitmap = null;
                        
                        if (_barcodeObject.showPoints)
                        {
                            lock (_bitmapLock)
                            {
                                if (_safeTempStreamBitmap != null)
                                {
                                    var barcodePoint = _barcodeObject.DrawPoints(_safeTempStreamBitmap);
                                    displayBitmap = (Bitmap)barcodePoint.Clone();
                                    _lastDetection?.Dispose();
                                    _lastDetection = (Bitmap)barcodePoint.Clone();
                                    barcodePoint.Dispose();
                                    _totalFreeze += 1;
                                }
                            }
                        }
                        else
                        {
                            lock (_bitmapLock)
                            {
                                if (_streamBitmap != null)
                                {
                                    displayBitmap = Image2Disk.CloneBitmap(_streamBitmap, _imageCloneProcessActice);
                                }
                            }
                        }

                        if (displayBitmap != null)
                        {
                            displayBitmap.Save(ms, ImageFormat.Bmp);
                            displayBitmap.Dispose();
                            
                            ms.Seek(0, SeekOrigin.Begin);
                            var bi = new BitmapImage();
                            bi.BeginInit();
                            bi.CacheOption = BitmapCacheOption.OnLoad;
                            bi.StreamSource = ms;
                            bi.EndInit();
                            bi.Freeze();
                            
                            Dispatcher.BeginInvoke(new ThreadStart(delegate { 
                                try 
                                { 
                                    pictureBox1.Source = bi; 
                                } 
                                catch { /* Ignore UI update errors */ } 
                            }));
                        }
                    }
                }
                else
                {
                    _jumpedFrame += 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in VideoSource_NewFrame: {ex.Message}");
            }
        }


        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);


        public ImageSource ImageSourceForBitmap(Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(handle);
            }
        }

        public void DecodeLoop()
        {
            var token = _cancellationTokenSource.Token;
            
            while (!token.IsCancellationRequested)
            {
                try
                {
                    Bitmap currentBitmap = null;
                    
                    if (_isImagePresent)
                    {
                        try
                        {
                            _configdata.GetConfigBase();
                            
                            if (GroupBoxSettings.Visibility == Visibility.Hidden)
                                Dispatcher.Invoke(() => { 
                                    try 
                                    { 
                                        UpdateConfig(); 
                                    } 
                                    catch { /* Ignore config update errors */ } 
                                });
                            
                            Thread.Sleep(_configdata.Timeout);
                            
                            lock (_bitmapLock)
                            {
                                if (_streamBitmap != null)
                                {
                                    currentBitmap = Image2Disk.CloneBitmap(_streamBitmap, _imageCloneProcessActice);
                                    _safeTempStreamBitmap?.Dispose();
                                    _safeTempStreamBitmap = Image2Disk.CloneBitmap(_streamBitmap, _imageCloneProcessActice);
                                }
                                _isImagePresent = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Error preparing bitmap: {ex.Message}");
                            continue;
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    if (currentBitmap == null)
                        continue;

                    try
                    {
                        LuminanceSource source = new BitmapLuminanceSource(currentBitmap);
                        var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                        Result result = new MultiFormatReader().decode(bitmap);
                        
                        if (result != null && result.ToString().Length > 4)
                        {
                            _barcodeObject.setBarcodeText(_configdata, result.ToString());
                            
                            Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    _barcodeObject.SetPoints(result.ResultPoints);
                                    _barcodeObject.SetFormat(result.BarcodeFormat.ToString());
                                    _barcodeObject.PlaySound();
                                    _barcodeObject.AutoPaster();
                                    _barcodeObject.OpenWeblink();
                                    
                                    _scannedContent = result.ToString();
                                    if (result.ToString() != "Content: " + consoleBox.Content)
                                        consoleBox.Content = "Content: " + result;
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error updating UI: {ex.Message}");
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Decoding failed, continue to next frame
                        System.Diagnostics.Debug.WriteLine($"Decoding error (normal): {ex.Message}");
                    }
                    finally
                    {
                        currentBitmap?.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error in DecodeLoop: {ex.Message}");
                    Thread.Sleep(100); // Prevent tight error loop
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                _decoder = new QRCodeReader();

                // Start a decoding process
                _decodingThread = new Thread(DecodeLoop);
                _decodingThread.IsBackground = true;
                _decodingThread.Start();
                _configdata.GetConfigBase();

                // enumerate video devices
                _camSources = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (_camSources.Count < 1)
                {
                    MessageBox.Show("No camera detected.", "Camera Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Environment.Exit(0);
                }
                else
                {
                    if (_camSources.Count > _configdata.DefaultCam)
                    {
                        _camSelected = _configdata.DefaultCam; 
                    }
                    else
                    {
                        _camSelected = 0;
                        _configdata.SetConfigBase("DefaultCam", "0");
                    }
                    
                    CamStream(_camSelected);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera: {ex.Message}", "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                // Signal threads to stop
                _cancellationTokenSource?.Cancel();
                
                // Stop camera
                if (_camVideo != null && _camVideo.IsRunning)
                {
                    _camVideo.SignalToStop();
                    _camVideo.WaitForStop();
                }
                
                // Wait for decoding thread
                if (_decodingThread != null && _decodingThread.IsAlive)
                {
                    _decodingThread.Join(1000);
                }
                
                // Cleanup resources
                lock (_bitmapLock)
                {
                    _streamBitmap?.Dispose();
                    _safeTempStreamBitmap?.Dispose();
                    _lastDetection?.Dispose();
                }
                
                _cancellationTokenSource?.Dispose();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error during cleanup: {ex.Message}");
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public void ToggleCam_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_camSources.Count > 1)
                {
                    // Stop camera cleanly
                    if (_camVideo != null && _camVideo.IsRunning)
                    {
                        _camVideo.SignalToStop();
                        _camVideo.WaitForStop();
                    }
                    
                    // Stop and restart decoding thread
                    _cancellationTokenSource?.Cancel();
                    if (_decodingThread != null && _decodingThread.IsAlive)
                    {
                        _decodingThread.Join(1000);
                    }
                    
                    _cancellationTokenSource = new CancellationTokenSource();
                    _decodingThread = new Thread(DecodeLoop);
                    _decodingThread.IsBackground = true;

                    if (_camSources.Count - 1 > _camSelected)
                    {
                        CamStream(_camSelected + 1); 
                    }
                    else
                    {
                        _camSelected = 0;
                        CamStream(_camSelected);
                    }
                    
                    _configdata.DefaultCam = _camSelected;
                    _configdata.SetConfigBase("DefaultCam", _camSelected.ToString());
                    _decodingThread.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error switching camera: {ex.Message}", "Camera Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void CamStream(int camNum)
        {
            try
            {
                _camSelected = camNum;
                if (_camSources.Count > camNum)
                {
                    _camVideo = new VideoCaptureDevice(_camSources[camNum].MonikerString);
                    _camVideo.NewFrame += VideoSource_NewFrame;
                    _camVideo.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting camera stream: {ex.Message}", "Camera Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            if (_settingsToggle)
            {
                GroupBoxSettings.Visibility = Visibility.Hidden;
                Crosshair.Visibility = Visibility;

                _settingsToggle = false;
            }
            else
            {
                GroupBoxSettings.Visibility = Visibility;
                Crosshair.Visibility = Visibility.Hidden;

                _settingsToggle = true;
            }
        }

        private void SetDatagridContent()
        {
            try
            {
                dataGrid.Items.Clear();

                _configdata.GetDataGridContentData(dataGrid);
                dataGrid.IsReadOnly = true;
                var converter = new BrushConverter();
                var brush = (Brush)converter.ConvertFromString("#" + _configdata.ButtonColor);

                foreach (Button button in StackPanelMainButtons.Children) button.Foreground = brush;
                
                if (File.Exists(_configdata.Logo))
                {
                    image.Source = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + _configdata.Logo));
                }
                
                for (var i = 0; i < _configdata.Debug; i++)
                {
                    var count = 0;
                    foreach (UIElement element in StackPanelMainSettings.Children)
                    {
                        count++;
                        if (count > _configdata.Debug) continue;
                        element.Visibility = Visibility.Hidden;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error setting datagrid content: {ex.Message}");
            }
        }
        

        private void UpdateConfig()
        {
            try
            {
                if (_settingsToggle == false)
                {
                    if (_configdata.Aim > 0)
                        Crosshair.Visibility = Visibility;
                    else
                        Crosshair.Visibility = Visibility.Hidden;
                }

                if (sliderFrames.Value != _configdata.Frames || sliderThread.Value != _configdata.Timeout ||
                    sliderFreeze.Value != _configdata.Freeze || sliderFreeze.Value != _configdata.Freeze ||
                    Convert.ToBoolean(_configdata.Aim) != AimCheckBox.IsChecked ||
                    SoundcheckBox.IsChecked != Convert.ToBoolean(_configdata.Sound))
                {
                    sliderFrames.Value = _configdata.Frames;
                    sliderThread.Value = _configdata.Timeout;
                    sliderFreeze.Value = _configdata.Freeze;
                    LabelSliderFramesMax.Content = 25 - _configdata.Frames;
                    LabelSliderThreadMax.Content = _configdata.Timeout;
                    LabelSliderFreezeMax.Content = _configdata.Freeze;
                    
                    if (_configdata.Sound > 0)
                        SoundcheckBox.IsChecked = true;
                    else
                        SoundcheckBox.IsChecked = false;
                        
                    if (_configdata.Aim > 0)
                    {
                        AimCheckBox.IsChecked = true;
                        _configdata.Aim = 1;
                    }
                    else
                    {
                        AimCheckBox.IsChecked = false;
                        _configdata.Aim = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating config: {ex.Message}");
            }
        }

        private void InitUi()
        {
            GroupBoxSettings.Visibility = Visibility.Hidden;
            checkBoxPrefix.IsChecked = false;

            for (var i = 0; i < dataGrid.Columns.Count; i++) dataGrid.Columns[i].Width = 250;
        }

        private void AddWhitelist(object sender, RoutedEventArgs e)
        {
            try
            {
                if (textBox.Text.Length > 6)
                {
                    _configdata.AddwhitelistConfig(textBox.Text, textBoxPrefix.Text);
                    SetDatagridContent();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding whitelist entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DelWhitelist(object sender, RoutedEventArgs e)
        {
            try
            {
                if (dataGrid.SelectedCells.Count > 0)
                {
                    _configdata.DelwhitelistConfig(dataGrid);
                    SetDatagridContent();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting whitelist entry: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CheckBoxPrefix_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBoxPrefix.IsChecked == true) textBoxPrefix.IsEnabled = true;
            if (checkBoxPrefix.IsChecked == false)
            {
                textBoxPrefix.IsEnabled = false;
                textBoxPrefix.Text = "";
            }
        }


        private void Statusbar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(_barcodeObject.scannedContent))
                {
                    Clipboard.SetText(_barcodeObject.scannedContent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error copying to clipboard: {ex.Message}");
            }
        }

        private void SliderFPS_ValueChanged(object sender, MouseEventArgs e)
        {
            try
            {
                _configdata.SetConfigBase("Frames", sliderFrames.Value.ToString(CultureInfo.InvariantCulture));
                _configdata.Frames = (int)sliderFrames.Value;
                LabelSliderFramesMax.Content = 25 - _configdata.Frames;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating frames: {ex.Message}");
            }
        }


        private void SliderThread_ValueChanged(object sender, MouseEventArgs e)
        {
            try
            {
                _configdata.SetConfigBase("Timeout", sliderThread.Value.ToString(CultureInfo.InvariantCulture));
                _configdata.Timeout = (int)sliderThread.Value;
                LabelSliderThreadMax.Content = _configdata.Timeout;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating timeout: {ex.Message}");
            }
        }

        private void SliderFreeze_ValueChanged(object sender, MouseEventArgs e)
        {
            try
            {
                _configdata.SetConfigBase("Freeze", sliderFreeze.Value.ToString(CultureInfo.InvariantCulture));
                _configdata.Freeze = (int)sliderFreeze.Value;
                LabelSliderFreezeMax.Content = _configdata.Freeze;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error updating freeze: {ex.Message}");
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _barcodeObject.Reset();
                _imageCloneProcessActice = true;
                _imageCloneProcessActice = false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error resetting: {ex.Message}");
            }
        }


        private void Autopaste_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                if (_barcodeObject.autoPaster)
                {
                    bitmap.UriSource = new Uri(@"pack://application:,,,/Resources/uncheck.png", UriKind.RelativeOrAbsolute);
                    _barcodeObject.autoPaster = false;
                }
                else
                {
                    bitmap.UriSource = new Uri(@"pack://application:,,,/Resources/check.png", UriKind.RelativeOrAbsolute);
                    _barcodeObject.autoPaster = true;
                }

                bitmap.EndInit();
                Autopaste.Background = new ImageBrush(bitmap);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling autopaste: {ex.Message}");
            }
        }


        private void SoundcheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configdata.Sound > 0)
                {
                    _configdata.SetConfigBase("Sound", "0");
                    _configdata.Sound = 0;
                }
                else
                {
                    _configdata.SetConfigBase("Sound", "1");
                    _configdata.Sound = 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling sound: {ex.Message}");
            }
        }

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var image2Disk = new Image2Disk();
                lock (_bitmapLock)
                {
                    image2Disk.SaveImage(_lastDetection, _safeTempStreamBitmap);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenImageFolder(object sender, RoutedEventArgs e)
        {
            try
            {
                var image2Disk = new Image2Disk();
                image2Disk.OpenImageFolder();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening folder: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AimCheckBox_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_configdata.Aim > 0)
                {
                    _configdata.SetConfigBase("Aim", "0");
                    _configdata.Aim = 0;
                }
                else
                {
                    _configdata.SetConfigBase("Aim", "1");
                    _configdata.Aim = 1;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error toggling aim: {ex.Message}");
            }
        }
    }
}
