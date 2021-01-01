using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

namespace barcode_Reader
{
    public class Image2Disk
    {
        private string documentFolder = "\\Barcode-Reader\\";
        public string filetype = ".jpg";
        

        public void SaveImage(Bitmap lastDetection, Bitmap safeTempstreamBitmap)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string datestring = getTime().Replace(":", "_");


            if (!Directory.Exists(path + documentFolder))
            {
                Directory.CreateDirectory(path + documentFolder);
            }

            if (lastDetection != null)
            {
                lastDetection.Save(path + documentFolder + datestring + filetype, ImageFormat.Jpeg);
            }

            else
            {

                safeTempstreamBitmap.Save(path + documentFolder + datestring + filetype, ImageFormat.Jpeg);
            }
        }
        
        public string getTime()
        {
            DateTime localDate = DateTime.Now;
            String[] cultureNames = { "de-DE", "en-GB", "en-US", "fr-FR", "ru-RU" };
            string datetimestring = "";

            foreach (var cultureName in cultureNames)
            {
                var culture = new CultureInfo(cultureName);
                //Console.WriteLine("{0}: {1}", cultureName,
                //                  localDate.ToString(culture));
                datetimestring = localDate.ToString(culture);
                break;
            }
            return datetimestring;
        }

        public void OpenImageFolder()
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!Directory.Exists(path + documentFolder))
            {
                Directory.CreateDirectory(path + documentFolder);
            }

            System.Diagnostics.Process.Start(path + documentFolder);
        }
        
        public static Bitmap CloneBitmap(Bitmap bitmap, bool isCloneing)
        {

            Bitmap ImageClone = new Bitmap(1,1);
            if (isCloneing == false)
            {
                isCloneing = true;
                ImageClone = (Bitmap) bitmap.Clone();
                isCloneing = false;
            }

            return ImageClone;
        }
    }
    
    
}