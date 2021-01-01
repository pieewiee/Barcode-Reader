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


        private Thread _decodingThread;
        private FilterInfoCollection _camSources;
        private VideoCaptureDevice _camVideo;
        private QRCodeReader _decoder;
        
        private readonly Barcode _barcodeObject = new Barcode();
        private readonly Config _configdata = new Config();

        
        public MainWindow()
        {
            
            InitializeComponent();
            InitUi();
            SetDatagridContent();
            
        }


        private void VideoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (_jumpedFrame > _configdata.Frames)
            {
                _jumpedFrame = 0;
                _streamBitmap = Image2Disk.CloneBitmap(eventArgs.Frame, _imageCloneProcessActice);
                _isImagePresent = true;

                var ms = new MemoryStream();
                if (_configdata.Freeze == _totalFreeze)
                {
                    _totalFreeze = 0;
                    _barcodeObject.showPoints = false;
                }

                if (_barcodeObject.showPoints)
                {
                    var barcodePoint = _barcodeObject.DrawPoints(_safeTempStreamBitmap);
                    barcodePoint.Save(ms, ImageFormat.Bmp);
                    _lastDetection = (Bitmap) barcodePoint.Clone();
                    _totalFreeze += 1;
                }
                else
                {
                    var darwBitmap = Image2Disk.CloneBitmap(_streamBitmap, _imageCloneProcessActice);
                    darwBitmap.Save(ms, ImageFormat.Bmp);
                }
                
                ms.Seek(0, SeekOrigin.Begin);
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();
                bi.Freeze();
                
                Dispatcher.BeginInvoke(new ThreadStart(delegate { pictureBox1.Source = bi; }));

            }
            else
            {
                _jumpedFrame += 1;
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
            while (true)
            {
                if (_isImagePresent)
                {
                    _configdata.GetConfigBase();
                    
                    if (GroupBoxSettings.Visibility == Visibility.Hidden)
                        Dispatcher.Invoke(() => { UpdateConfig(); });
                    
                    Thread.Sleep(_configdata.Timeout);
                    
                    _safeTempStreamBitmap = Image2Disk.CloneBitmap(_streamBitmap, _imageCloneProcessActice);
                    _isImagePresent = false;
                }
                else
                {
                    continue;
                }

                LuminanceSource source;
                Result result;

                source = new BitmapLuminanceSource(_streamBitmap);
                var bitmap = new BinaryBitmap(new HybridBinarizer(source));
                result = new MultiFormatReader().decode(bitmap);
                
                if (result != null)
                {
                    if (result.ToString().Length > 4) _barcodeObject.setBarcodeText(_configdata, result.ToString());
                    Dispatcher.Invoke(() =>
                    {
                        _barcodeObject.SetPoints(result.ResultPoints);
                        _barcodeObject.SetFormat(result.BarcodeFormat.ToString());
                        _barcodeObject.PlaySound();
                        _barcodeObject.AutoPaster();
                        _barcodeObject.OpenWeblink();
                        
                        _scannedContent = result.ToString();
                        if (result.ToString() != "Content: " + consoleBox.Content)
                            consoleBox.Content = "Content: " + result;
                    });
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _decoder = new QRCodeReader();

            // Start a decoding process
            _decodingThread = new Thread(DecodeLoop);
            _decodingThread.Start();
            _configdata.GetConfigBase();

                // enumerate video devices
                _camSources = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (_camSources.Count < 1)
                {
                    MessageBox.Show("No camera detected.");
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
        
        private void Window_Closed(object sender, EventArgs e)
        {
            _decodingThread.Abort();
            _camVideo.Stop();
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        public void ToggleCam_Click(object sender, RoutedEventArgs e)
        {
            
            if (_camSources.Count > 1 )
            {
                _decodingThread.Abort();
                _camVideo.Stop();
                
                _decodingThread = new Thread(DecodeLoop);

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

        public void CamStream(int camNum)
        {
            _camSelected = camNum;
            if (_camSources.Count > camNum)
            {
                _camVideo = new VideoCaptureDevice(_camSources[camNum].MonikerString);
                _camVideo.NewFrame += VideoSource_NewFrame;
                _camVideo.Start();
                
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
            dataGrid.Items.Clear();

            _configdata.GetDataGridContentData(dataGrid);
            dataGrid.IsReadOnly = true;
            var converter = new BrushConverter();
            var brush = (Brush) converter.ConvertFromString("#" + _configdata.ButtonColor);

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
        

        private void UpdateConfig()
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

        private void InitUi()
        {
            GroupBoxSettings.Visibility = Visibility.Hidden;
            checkBoxPrefix.IsChecked = false;

            for (var i = 0; i < dataGrid.Columns.Count; i++) dataGrid.Columns[i].Width = 250;
        }

        private void AddWhitelist(object sender, RoutedEventArgs e)
        {
            if (textBox.Text.Length > 6)
            {
                _configdata.AddwhitelistConfig(textBox.Text, textBoxPrefix.Text);
                SetDatagridContent();
            }
        }

        private void DelWhitelist(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedCells.Count > 0)
            {
                _configdata.DelwhitelistConfig(dataGrid);
                SetDatagridContent();
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
            Clipboard.SetText(_barcodeObject.prefix == ""
                ? _scannedContent
                : _scannedContent.Replace(_barcodeObject.prefix, ""));
        }

        private void SliderFPS_ValueChanged(object sender, MouseEventArgs e)
        {
            _configdata.SetConfigBase("Frames", sliderFrames.Value.ToString(CultureInfo.InvariantCulture));
            _configdata.Frames = (int) sliderFrames.Value;
            LabelSliderFramesMax.Content = 25 - _configdata.Frames;
        }


        private void SliderThread_ValueChanged(object sender, MouseEventArgs e)
        {
            _configdata.SetConfigBase("Timeout", sliderThread.Value.ToString(CultureInfo.InvariantCulture));
            _configdata.Timeout = (int) sliderThread.Value;
            LabelSliderThreadMax.Content = _configdata.Timeout;
        }

        private void SliderFreeze_ValueChanged(object sender, MouseEventArgs e)
        {
            _configdata.SetConfigBase("Freeze", sliderFreeze.Value.ToString(CultureInfo.InvariantCulture));
            _configdata.Freeze = (int) sliderFreeze.Value;
            LabelSliderFreezeMax.Content = _configdata.Freeze;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            _barcodeObject.Reset();
            _imageCloneProcessActice = true;
            _imageCloneProcessActice = false;
        }


        private void Autopaste_Click(object sender, RoutedEventArgs e)
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


        private void SoundcheckBox_Click(object sender, RoutedEventArgs e)
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

        private void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            var image2Disk = new Image2Disk();
            image2Disk.SaveImage(_lastDetection, _safeTempStreamBitmap);
        }

        private void OpenImageFolder(object sender, RoutedEventArgs e)
        {
            var image2Disk = new Image2Disk();
            image2Disk.OpenImageFolder();
        }

        private void AimCheckBox_Click(object sender, RoutedEventArgs e)
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
    }
}