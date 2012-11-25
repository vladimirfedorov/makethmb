using System;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace makethmb
{
    class Program
    {
        enum ThmbOption { EqWidth, EqHeight, SquareCrop };
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                ShowInfo();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            CreateThumbnails(args[0], args[1]);
        }

        static void ShowInfo()
        {
            Console.WriteLine("Usage: makethmb resize_option folder");
            Console.WriteLine("  resize_otpion: Wx|Hx|Sx, where x=size in pixels");
            Console.WriteLine("  Examples:");
            Console.WriteLine("  makethmb W100 . - create thumbnails of images in the current folder with equal width of 100px");
            Console.WriteLine("  makethmb H100 . - create thumbnails of images in the current folder with equal height of 100px");
            Console.WriteLine("  makethmb S100 . - create thumbnails of images in the current folder with the largest side of 100px");
        }

        static private void CreateThumbnails(string ThmbOption, string SrcFolder)
        {
            string ThmbFolder = Environment.ExpandEnvironmentVariables(
                Path.GetFullPath(Path.GetDirectoryName(
                    SrcFolder + "\\"))) + "\\thmb\\";
            
            if (!Directory.Exists(ThmbFolder))
                Directory.CreateDirectory(ThmbFolder);

            if (!Directory.Exists(SrcFolder)) { Console.WriteLine("Folder {0} not found", SrcFolder); return; }
            ThmbOption option = Program.ThmbOption.SquareCrop;
            switch (ThmbOption.Substring(0, 1).ToUpper())
            {
                case "W": option = Program.ThmbOption.EqWidth; break;
                case "H": option = Program.ThmbOption.EqHeight; break;
                case "S": option = Program.ThmbOption.SquareCrop; break;
                default: Console.WriteLine("Resize option is not of a correct form."); break;
            }

            int ThmbSize = 100;
            int.TryParse(ThmbOption.Substring(1), out ThmbSize);

            string[] files = Directory.GetFiles(SrcFolder);

            List<string> exts = new List<string>();
            foreach (string ext in new string[] { ".png", ".jpg", ".jpeg", ".bmp", ".jpe", ".jfif", ".tif", ".tiff" })
                exts.Add(ext);

            foreach (string file in files)
            {
                if (!exts.Contains(Path.GetExtension(file).ToLower())) continue;                
                try
                {
                    string ThmbFileName = ThmbFolder + Path.GetFileNameWithoutExtension(file) + ".jpg";
                    Image img = Image.FromFile(file);
                    CreateThumbnail(ThmbFileName, img, ThmbSize, option, 80);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error processing file " + file);
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        static private void CreateThumbnail(string ThmbFileName, Image SrcImage, int ThmbSize, ThmbOption Option, long Quality)
        {
            if (File.Exists(ThmbFileName))
                File.Delete(ThmbFileName);

            int W = SrcImage.Width;
            int H = SrcImage.Height;
            int T = ThmbSize;

            int thmbwidth = T;
            int thmbheight = T;
            Rectangle CropArea = new Rectangle(new Point(0, 0), SrcImage.Size);

            switch (Option)
            {
                case ThmbOption.EqHeight:
                    thmbwidth = (T * W) / H;
                    break;
                case ThmbOption.EqWidth:
                    thmbheight = (T * H) / W;
                    break;
                case ThmbOption.SquareCrop:
                    CropArea = new Rectangle(
                        SrcImage.Width > SrcImage.Height ? (W - H) / 2 : 0,
                        SrcImage.Width > SrcImage.Height ? 0 : (H - W) / 2,
                        SrcImage.Width > SrcImage.Height ? H : W,
                        SrcImage.Width > SrcImage.Height ? H : W);
                    break;
            }


            Image thmb = new Bitmap(thmbwidth, thmbheight);

            Graphics gfx = Graphics.FromImage(thmb);
            gfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

            gfx.DrawImage(
                SrcImage,
                new Rectangle(0, 0, thmbwidth, thmbheight),
                CropArea,
                GraphicsUnit.Pixel);

            SaveJpeg(ThmbFileName, thmb, Quality);
        }

        private static void SaveJpeg(string filename, Image image, long Quality)
        {
            EncoderParameter param1 = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, Quality);
            ImageCodecInfo codec = null;
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo c in codecs)
                if (c.MimeType == "image/jpeg")
                {
                    codec = c;
                    break;
                }

            if (codec != null)
            {
                EncoderParameters encparams = new EncoderParameters(1);
                encparams.Param[0] = param1;
                image.Save(filename, codec, encparams);
            }
        }

    }
}
