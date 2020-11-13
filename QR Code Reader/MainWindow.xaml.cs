using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AForge.Video.DirectShow;
using AForge.Video;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using System.Media;
using ZXing.QrCode;
using ZXing;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;
using System.Xml;
using System.Data;
using System.Windows.Controls;
using System.Windows.Data;


namespace QR_Code_Reader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        int camSelected = 0;
        bool settingstoggle = false;



        FilterInfoCollection camSources;
        VideoCaptureDevice camVideo;

        // A stopWatch to test the processing speed.
        Stopwatch stopwatch = new Stopwatch();

        // Bitmap buffers
        Bitmap streamBitmap;
        Bitmap snapShotBitmap;
        Bitmap safeTempstreamBitmap;

        // Sound to be played when successful detection take a place.
        SoundPlayer player = new SoundPlayer(@"C:\Windows\Media\Windows Exclamation.wav");

        // Thread for decoding in parallel with the webcam video streaming.
        Thread decodingThread;

        // The QR Decoder variable from ZXing
        QRCodeReader decoder;


        public MainWindow()
        {
            InitializeComponent();
            InitUI();
            SetDatagridContent();
        }



        void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                streamBitmap = (Bitmap)eventArgs.Frame.Clone();
                safeTempstreamBitmap = (Bitmap)streamBitmap.Clone();
                //pictureBox1.Source = ImageSourceForBitmap(safeTempstreamBitmap);
                MemoryStream ms = new MemoryStream();
                streamBitmap.Save(ms, ImageFormat.Bmp);
                ms.Seek(0, SeekOrigin.Begin);
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = ms;
                bi.EndInit();

                bi.Freeze();
                Dispatcher.BeginInvoke(new ThreadStart(delegate
                {
                    pictureBox1.Source = bi;
                }));
            }
            catch (Exception exp)
            {
                Console.Write(exp.Message);
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
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }

        public void decodeLoop()
        {
            while (true)
            {
                // 1 second pause for the thread. This could be changed manually to a prefereable decoding interval.
                Thread.Sleep(1000);
                if (streamBitmap != null)
                    snapShotBitmap = (Bitmap)safeTempstreamBitmap.Clone();
                else
                    return;

                // Reset watch before decoding the streamed image.
                stopwatch.Reset();
                stopwatch.Start();

                // Decode the snapshot.
                LuminanceSource source;
                source = new BitmapLuminanceSource(snapShotBitmap);
                BinaryBitmap bitmap = new BinaryBitmap(new ZXing.Common.HybridBinarizer(source));
                Result result = new MultiFormatReader().decode(bitmap);
                //string decodeStr = decoder.decode(snapShotBitmap);


                stopwatch.Stop();
                //string decode = Detect(b);

                // If decodeStr is null then there was no QR detected, otherwise show the result of detection and play the sound.
                if (result == null)
                {
                    //System.Windows.MessageBox.Show("There is no QR Code!");
                }
                else
                {
                    player.Play();
                    var xDoc = ReturnConfig();

                    XmlNodeList prefix = xDoc.GetElementsByTagName("Prefix");
                    XmlNodeList checkBoxPrefix = xDoc.GetElementsByTagName("checkBoxPrefix");



                    var domainNodes = xDoc.SelectNodes("/Config/Domain");
                    if (result.ToString().Length > 4)
                    {
                        for (int i = 0; i < domainNodes.Count; i++)
                        {
                            var Prefix = domainNodes[i].Attributes["PrefixValue"].Value.ToString();
                            var Domain = domainNodes[i].Attributes["Value"].Value.ToString();
                            var NoPrefix = "";
                            if (Prefix == result.ToString().Substring(0, Prefix.Length) && Prefix.Length > 0)
                            {
                                if (Prefix == "")
                                {
                                    NoPrefix = result.ToString();
                                }
                                else
                                {
                                    NoPrefix = result.ToString().Replace(Prefix, "");
                                }

                                if (Domain == NoPrefix.Substring(0, Domain.Length))
                                {
                                    System.Diagnostics.Process.Start(NoPrefix);
                                }
                                
                            }
                            else
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    if (result.ToString() != consoleBox.Text)
                                    {
                                        consoleBox.AppendText(result.ToString());
                                    }
                                });
                                Thread.Sleep(3000);
                                Dispatcher.Invoke(() =>
                                {
                                    consoleBox.Text = "";
                                });
                                //System.Windows.MessageBox.Show(result.ToString());
                                //System.Windows.MessageBox.Show(stopwatch.Elapsed.TotalMilliseconds.ToString());
                            }
                        }
                    }





                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Initialize sound variable
            //player.Stream = Properties.Resources.connect;

            decoder = new QRCodeReader();

            // Start a decoding process
            decodingThread = new Thread(new ThreadStart(decodeLoop));
            decodingThread.Start();

            try
            {
                // enumerate video devices
                camSources = new FilterInfoCollection(FilterCategory.VideoInputDevice);

                if (camSources.Count < 1)
                {
                    System.Windows.MessageBox.Show("No camera detected.");
                    System.Environment.Exit(0);
                }
                else
                {
                    camStream(camSelected);
                }

            }
            catch (VideoException exp)
            {
                Console.Write(exp.Message);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            decodingThread.Abort();
            camVideo.Stop();
            stopwatch.Stop();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Environment.Exit(0);
        }

        public void toggleCam_Click(object sender, RoutedEventArgs e)
        {
            if (camSources.Count > 1)
            {
                decodingThread.Abort();
                camVideo.Stop();
                try
                {
                    decodingThread = new Thread(new ThreadStart(decodeLoop));
                    if (camSelected == 0)
                    {
                        camStream(1);
                    }
                    else
                    {
                        camStream(0);
                    }
                    decodingThread.Start();
                }
                catch (Exception exp)
                {
                    Console.Write(exp);
                }
            }
        }

        public void camStream(int camNum)
        {
            try
            {
                camVideo = new VideoCaptureDevice(camSources[camNum].MonikerString);
                camVideo.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
                camVideo.Start();
                camSelected = camNum;
            }
            catch (VideoException exp)
            {
                Console.Write(exp.Message);
            }
        }


        private void Settings_Click(object sender, RoutedEventArgs e)
        {


            if (settingstoggle == true)
            {
                textBox.Visibility = Visibility.Hidden;
                textBoxPrefix.Visibility = Visibility.Hidden;
                dataGrid.Visibility = Visibility.Hidden;
                groupBox2.Visibility = Visibility.Hidden;
                checkBoxPrefix.Visibility = Visibility.Hidden;
                label.Visibility = Visibility.Hidden;
                label1.Visibility = Visibility.Hidden;
                button.Visibility = Visibility.Hidden;
                button1.Visibility = Visibility.Hidden;

                settingstoggle = false;
            }
            else
            {
                textBox.Visibility = Visibility;
                textBoxPrefix.Visibility = Visibility;
                dataGrid.Visibility = Visibility;
                groupBox2.Visibility = Visibility;
                checkBoxPrefix.Visibility = Visibility;
                label.Visibility = Visibility;
                label1.Visibility = Visibility;
                button.Visibility = Visibility;
                button1.Visibility = Visibility;

                settingstoggle = true;
            }
        }

        public void InitUI()
        {

            textBox.Visibility = Visibility.Hidden;
            textBoxPrefix.Visibility = Visibility.Hidden;
            groupBox2.Visibility = Visibility.Hidden;
            checkBoxPrefix.Visibility = Visibility.Hidden;
            label.Visibility = Visibility.Hidden;
            label1.Visibility = Visibility.Hidden;
            button.Visibility = Visibility.Hidden;
            button1.Visibility = Visibility.Hidden;
            dataGrid.Visibility = Visibility.Hidden;
            checkBoxPrefix.IsChecked = false;
            checkBoxPrefix.IsChecked = true;
            checkBoxPrefix.IsChecked = false;

            dataGrid.Columns[0].Width = 220;
            dataGrid.Columns[1].Width = 250;
        }

        public void SetDatagridContent()
        {
            var xDoc = ReturnConfig();
            var domainNodes = xDoc.SelectNodes("/Config/Domain");
            string[] domains = new string[domainNodes.Count];
            dataGrid.Items.Clear();
            for (int i = 0; i < domainNodes.Count; i++)
            {
                domains[i] = domainNodes[i].Attributes["Value"].Value;

                Config theObject = new Config();
                theObject.Prefix = domainNodes[i].Attributes["PrefixValue"].Value;
                theObject.Domain = domainNodes[i].Attributes["Value"].Value;
                dataGrid.Items.Add(theObject);
                dataGrid.IsReadOnly = true;
            }
        }

        private void addwhitelist(object sender, RoutedEventArgs e)
        {

            var xDoc = ReturnConfig();
            var domainNodes = xDoc.SelectNodes("/Config");


            XmlElement elem = xDoc.CreateElement("Domain");
            elem.SetAttribute("PrefixValue", textBoxPrefix.Text);
            elem.SetAttribute("Value", textBox.Text);

            domainNodes[0].AppendChild(elem);
            SaveConfig(xDoc);

        }


        private void delwhitelist(object sender, RoutedEventArgs e)
        {
            
            var xDoc = ReturnConfig();

            var domainNodes = xDoc.SelectNodes("/Config/Domain");
            var domainNode = xDoc.SelectNodes("/Config");
            string[] domains = new string[domainNodes.Count];

            for (int i = 0; i < domainNodes.Count; i++)
            {


                if ((domainNodes[i].Attributes["Value"].Value == ((QR_Code_Reader.Config)dataGrid.SelectedValue).Domain) && 
                    (domainNodes[i].Attributes["PrefixValue"].Value == ((QR_Code_Reader.Config)dataGrid.SelectedValue).Prefix))
                {
                    domainNode[0].RemoveChild(domainNodes[i]);
                }
            }

            SaveConfig(xDoc);

        }


        private System.Xml.XmlDocument ReturnConfig()
        {

            XmlDocument xDoc = new XmlDocument();
            xDoc.Load("config.xml");
            return xDoc;
        }

        private void SaveConfig(System.Xml.XmlDocument xDoc)
        {
            xDoc.Save("config.xml");
            SetDatagridContent();
        }

        private void checkBoxPrefix_Checked(object sender, RoutedEventArgs e)
        {

            if (checkBoxPrefix.IsChecked == true)
            {
                textBoxPrefix.IsEnabled = true;
            }
            if (checkBoxPrefix.IsChecked == false)
            {
                textBoxPrefix.IsEnabled = false;
                textBoxPrefix.Text = "";
    
            }
            
        }

    }
}
