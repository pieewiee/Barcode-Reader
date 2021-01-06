using System.Diagnostics;
using System.Drawing;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Input;
using ZXing;

namespace barcode_Reader
{
    internal class Barcode
    {
      
        public float point1X;
        public float point1Y;
        public float point2X;
        public float point2Y;
        public float point3X;
        public float point3Y;
        public float point4X;
        public float point4Y;
        public bool is2d;
        public bool showPoints;
        public string format;
        public string barcode;
        public string prefix;
        public string rawBarcode;
        public string oldBarcode;
        public bool autoPaster;
        private string _scannedContentChanged;
        public string scannedContent;
        private string _scannedContentChangedSound;
        private Config _configdata;


        public void SetPoints(ResultPoint[] resultPoints)
        {
            point1X = resultPoints[0].X;
            point1Y = resultPoints[0].Y;
            point2X = resultPoints[1].X;
            point2Y = resultPoints[1].Y;

            if (resultPoints.Length == 4)
            {
                point3X = resultPoints[2].X;
                point3Y = resultPoints[2].Y;
                point4X = resultPoints[3].X;
                point4Y = resultPoints[3].Y;
                is2d = true;
            }
            else
            {
                is2d = false;
            }

            showPoints = true;
        }

        public void SetFormat(string barcodeFormat)
        {
            format = barcodeFormat;
        }
        
        public Bitmap DrawPoints(Bitmap bitmap)
        {
            Pen blackPen = new System.Drawing.Pen(System.Drawing.Color.Red, 3);

            // Draw line to screen.
            using (var graphics = Graphics.FromImage(bitmap))
            {
                            
                if (is2d)
                {
                    var offset = 30;
                    graphics.DrawLine(blackPen, point1X - offset, point1Y + offset, point2X - offset, point2Y - offset);
                    graphics.DrawLine(blackPen, point2X - offset, point2Y - offset, point3X + offset, point3Y - offset);
                    graphics.DrawLine(blackPen, point3X + offset, point3Y - offset, point4X + offset, point4Y + offset);
                    graphics.DrawLine(blackPen, point4X + offset, point4Y + offset, point1X - offset, point1Y + offset);
                }
                else
                {
                    var offset = 0;
                    graphics.DrawLine(blackPen, point1X - offset, point1Y + offset, point2X - offset, point2Y - offset);
                }

                            
                var Fontoffset = 30;
                RectangleF rectf = new RectangleF(point1X + Fontoffset, point1Y + Fontoffset, point2X + Fontoffset, point2Y + Fontoffset);
                graphics.DrawString(format, new Font("Tahoma", 30), Brushes.Red, rectf);

            }

            return bitmap;
        }

        public void setBarcodeText(Config configdata, string barcodeText)
        {
           
            var xDoc = configdata.GetConfig();
            _configdata = configdata;
            scannedContent = barcodeText;

            var domainNodes = xDoc.SelectNodes("/Config/Domain");
            if (barcodeText.Length > 4)
            {
                if (domainNodes != null)
                    for (int i = 0; i < domainNodes.Count; i++)
                    {
                        var xmlAttributeCollection = domainNodes[i].Attributes;
                        if (xmlAttributeCollection != null)
                        {
                            var prefixValue = xmlAttributeCollection["PrefixValue"].Value;
                            var domain = xmlAttributeCollection["Value"].Value;
                            var noPrefix = "";
                            string completedomain;


                            completedomain = prefixValue + domain;
                            completedomain = completedomain.Replace(" ", "");
                          

                                Match m = Regex.Match(barcodeText, "^" + completedomain, RegexOptions.IgnoreCase);


                                if (m.Success)
                                {
                                    if (prefixValue == "")
                                    {
                                        rawBarcode = barcodeText;
                                        barcode = barcodeText;
                                        prefix = "";
                                    }
                                    else
                                    {
                                        prefixValue = prefixValue.Replace("\\", "");
                                        rawBarcode = barcodeText;
                                        noPrefix = barcodeText.Replace(prefixValue, "");
                                        barcode = noPrefix;
                                        prefix = prefixValue;
                                    }
                                }

                        }
                    }
            }
        }

        public void OpenWeblink()
        {
            if (oldBarcode != rawBarcode)
            {
                oldBarcode = rawBarcode;
                Process.Start(barcode);
            }
        }

        public void AutoPaster()
        {
            if (autoPaster && _scannedContentChanged != scannedContent)
            {
                _scannedContentChanged = scannedContent;
                if (string.IsNullOrEmpty(prefix))
                {
                    //Copytoclipboard();
                    VirtualKeyboard.SendUnicode(scannedContent + "\n");
                    VirtualKeyboard.SendKeyBoradKey((short) Key.Return);
                }
                else
                {
                    //Copytoclipboard();
                    VirtualKeyboard.SendUnicode(scannedContent.Replace(prefix, "") + "\n");
                    VirtualKeyboard.SendKeyBoradKey((short) Key.Return);
                }
            }
        }
            

        public void Reset()
        {
            barcode = "";
            oldBarcode = "";
            prefix = "";
            rawBarcode = "";
            _scannedContentChanged = "";
            _scannedContentChangedSound = "";
            scannedContent = "";

        }
        
        public void PlaySound()
        {
            if (_scannedContentChangedSound != scannedContent)
            {
                //nvironment.Exit(1);
                _scannedContentChangedSound = scannedContent;
                var playSound = new SoundPlayer(Properties.Resources.Beep);
                for (var i = 0; i < _configdata.Sound; i++)
                {
                    playSound.Play();
                }
            }
        }
    }
}