using IronOcr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Tesseract;

namespace PortRoyalist
{
    public class ScreenShotParser
    {
        private readonly static string inputDir = "Data\\Input\\";
        private readonly static string preparedDir = "Data\\Prepared\\";

        public static void Init()
        {
            Directory.CreateDirectory(MapPath(inputDir));
            Directory.CreateDirectory(MapPath(preparedDir));
        }

        public static string MapPath(string relativePath)
        {
            //C:\Users\giczi\source\repos\PortRoyalist\PortRoyalist\bin\Debug\netcoreapp3.0
            string startupPath = Environment.CurrentDirectory;

            startupPath = startupPath.Substring(0, startupPath.IndexOf("bin\\"));
            //string startupPath = Environment.CurrentDirectory;

            var res = Path.Combine(startupPath, relativePath);
            return res;
        }

        /// <summary>
        /// Entferne top 75 px des Screnshots da dort die minie Bilder und Mengenauswahl sind
        /// </summary>
        public void PrepareScreenshots()
        {
            var di = new DirectoryInfo(MapPath(inputDir));
            var files = di.GetFiles("*.png");

            var removePixels = new List<Tuple<int, int, int>>
            {
                new Tuple<int, int, int>(204,171,147),
                new Tuple<int, int, int>(216,180,147),
                new Tuple<int, int, int>(167,113,67),
                new Tuple<int, int, int>(163,104,62),
                new Tuple<int, int, int>(176,115,61),
                new Tuple<int, int, int>(176,118,62),
                new Tuple<int, int, int>(174,117,62),
                //new Tuple<int, int, int>(158,148,129),
                //new Tuple<int, int, int>(161,154,98),
                //new Tuple<int, int, int>(177,107,55),
                //new Tuple<int, int, int>(174,122,75),             
                //new Tuple<int, int, int>(102,91,69),
                //new Tuple<int, int, int>(144,134,110),
                //new Tuple<int, int, int>(117,108,89),
                //new Tuple<int, int, int>(137,124,71)
            };

            var allowedPixels = new List<Tuple<int, int, int>>
            {
                new Tuple<int, int, int>(113,113,98),
                new Tuple<int, int, int>(10,9,6),
                new Tuple<int, int, int>(43,43,40),
                new Tuple<int, int, int>(38,37,32)
            };

            Func<Tuple<int, int, int>, Color, int, bool> isPixelInRange = delegate (Tuple<int, int, int> allowed, Color color, int maxOffset)
            {
                var currOffset = 0;

                currOffset += Math.Abs(allowed.Item1 - color.R);
                currOffset += Math.Abs(allowed.Item2 - color.G);
                currOffset += Math.Abs(allowed.Item3 - color.B);

                if (currOffset <= maxOffset)
                {
                    return true;
                }
                return false;
            };

            Func<Color, bool> isAllowedPixel = delegate (Color color)
            {
                if (color.R == 255 && color.G == 255 && color.B == 255)
                {
                    return true;
                }
                foreach (var allowed in allowedPixels)
                {
                    if (isPixelInRange(allowed, color, 120))
                    {
                        return true;
                    }
                }

                return false;

            };

            foreach (var fi in files)
            {
                Image img;
                var size = GetImageSizeAsRectangle(fi, out img);

                //oberhalb abschneiden
                var topOffset = 74;
                size.Y = topOffset;
                var botOffset = 93;
                size.Height -= botOffset;

                //links rehcts abschneiden
                var leftOffset = 75;
                size.X = leftOffset;
                var rightOffset = 192;
                size.Width -= rightOffset;

                using (var factory = new ImageProcessor.ImageFactory())
                {
                    using (var croppedImg = factory.Load(img).Crop(size).Image)
                    {
                        var bitmap = (Bitmap)croppedImg;

                        for (var x = 0; x < bitmap.Width; x++)
                        {
                            for (var y = 0; y < bitmap.Height; y++)
                            {
                                var pixel = bitmap.GetPixel(x, y);

                                //icons 160px von links ,22 breit 21 hoch
                                if (x >= 160 - leftOffset && x <= 182 - leftOffset)
                                {
                                    continue;
                                    //bitmap.SetPixel(x, y, Color.White);
                                }
                                else if (isPixelInRange(new Tuple<int, int, int>(247, 46, 41), pixel, 100) || isPixelInRange(new Tuple<int, int, int>(236, 121, 109), pixel, 50))
                                {
                                    bitmap.SetPixel(x, y, Color.Black);
                                }
                                else if (pixel.R > 140 && pixel.G > 140 && pixel.B > 140)
                                {
                                    bitmap.SetPixel(x, y, Color.White);
                                }
                                else if (true)
                                {
                                    var foundRemove = removePixels.FirstOrDefault(x => isPixelInRange(x, pixel, 20));
                                    if (foundRemove != null)
                                    {
                                        bitmap.SetPixel(x, y, Color.White);
                                    }
                                    else
                                    {
                                        var found = allowedPixels.FirstOrDefault(x => x.Item1 == pixel.R && x.Item2 == pixel.G && x.Item3 == pixel.B);
                                        if (!isAllowedPixel(pixel))
                                        {
                                            bitmap.SetPixel(x, y, Color.White);
                                        }
                                    }

                                }

                                //const float limit = 0.55f;
                                //if (pixel.GetBrightness() > limit)
                                //{
                                //    bitmap.SetPixel(x,y, Color.White);
                                //}

                            }

                        }


                        // TODO umgekehrt lösen -> annhand dem vorbereitetenm Prepared Bild
                        // ein Bild erstellen was alles weiß und an Stelle der behaltendne Pixel mit spezieller Farbe markieren
                        // danach auch die ganzen Pixel-Löscungen testweise raus und nur zu schauen was rauskommt
                        // die gleichen Sektoren können dann bei den segments auch zu besseren Ergebnissen führen

                        //Rectangle, repeat-y, execute
                        var clearAreas = new List<Tuple<Rectangle, int, bool>>()
                        {
                            new Tuple<Rectangle, int, bool>(new Rectangle(0,7,33,2),19, true)
                        };   

                        for (var x = 0; x < bitmap.Width; x++)
                        {
                            for (var y = 0; y < bitmap.Height; y++)
                            {
                                var pixel = bitmap.GetPixel(x, y);

                                foreach (var clearArea in clearAreas)
                                {

                                }
                            }
                        }


                                var size2 = new Rectangle();
                        size2.X = 0;
                        size2.Width = 31;
                        size2.Height = 14;
                        size2.Y = 0;

                        var croppedimg2 = croppedImg;// cropImage(croppedImg, size2);

                        //factory.Load(croppedimg2);

                        var scaleFactor = 1;// 5;
                        if(scaleFactor > 1)
                        {
                            croppedimg2 = factory
                            .Resize(new Size(croppedimg2.Width * scaleFactor, croppedimg2.Height * scaleFactor))
                            .Image;
                        }

                        croppedimg2.Save(Path.Combine(MapPath(preparedDir), fi.Name));

                        var stockHeight = 22 * scaleFactor;
                        var stockX = 0;
                        var stockWidth = 38 * scaleFactor; ;
                        var sellX = 50 * scaleFactor; ;
                        var sellWidth = 33 * scaleFactor; ;
                        var buyX = 117 * scaleFactor; ;
                        var buyWidth = 33 * scaleFactor; ;

                        var wares = new List<string> { "Grain", "Fish", "Meat", "Potato", "Rum", "Textil", "Salt", "Brick", "Wood", "Hemp", "Tobacco", "Dye", "Cocoa", "Sugar", "Cutton", "Grape", "Tools", "Vase", "Clothes" };

                        var segments = new List<Tuple<string, string, Rectangle>>();

                        var currentY = 0;
                        foreach (var ware in wares)
                        {
                            segments.Add(new Tuple<string, string, Rectangle>(ware, "stock", new Rectangle(stockX, currentY, stockWidth, stockHeight)));
                            segments.Add(new Tuple<string, string, Rectangle>(ware, "sell price", new Rectangle(sellX, currentY, sellWidth, stockHeight)));
                            segments.Add(new Tuple<string, string, Rectangle>(ware, "buy price", new Rectangle(buyX, currentY, buyWidth, stockHeight)));
                            currentY += stockHeight;
                        }

                        var splitDir = Path.Combine(MapPath(preparedDir), fi.Name.Replace(".", "_"));
                        if (Directory.Exists(splitDir)) Directory.Delete(splitDir, true);
                        Directory.CreateDirectory(splitDir);

                        var inx = 1;
                        Tuple<string, string, Rectangle> lastSegment = null;
                        foreach (var segment in segments)
                        {
                            if (lastSegment != null && lastSegment.Item1 != segment.Item1)
                            {
                                inx++;
                            }

                            var dirPath = Path.Combine(MapPath(splitDir), inx + "_" + segment.Item1);
                            Directory.CreateDirectory(dirPath);

                            using (var cropped = factory.Load(croppedimg2).Crop(segment.Item3).Image)// cropImage(croppedimg2, segment.Item3))
                            {
                                cropped.Save(Path.Combine(dirPath, segment.Item2 + Path.GetExtension(fi.FullName)));
                            }

                            lastSegment = segment;
                        }
                    }
                }
            }
        }

        private static Rectangle GetImageSizeAsRectangle(FileInfo fi, out Image image)
        {
            var img = Image.FromFile(fi.FullName);
            image = img;
            return new Rectangle(0, 0, img.Width, img.Height);
        }
        private static Image cropImage(Image img, Rectangle cropArea)
        {
            using (var bmpImage = new Bitmap(img))
            {
                return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            }
        }

        public FileInfo ParseScreenshots()
        {
            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                engine.SetVariable("tessedit_char_whitelist", "0123456789");

                var di = new DirectoryInfo(MapPath(preparedDir));
                var files = di.GetFiles("*.png");
                foreach (var fi in files)
                {
                    var splitDir = Path.Combine(MapPath(preparedDir), fi.Name.Replace(".", "_"));
                    if (!Directory.Exists(splitDir)) continue;

                    var di2 = new DirectoryInfo(splitDir);
                    foreach (var splitDi in di2.GetDirectories())
                    {
                        var files2 = splitDi.GetFiles("*.png");
                        foreach (var fi2 in files2)
                        {
                            using (var img = Pix.LoadFromFile(fi2.FullName))
                            {
                                using (var page = engine.Process(img))
                                {
                                    var text = page.GetText();

                                    //if (text.Length > 20)
                                    //{
                                    //    System.Windows.MessageBox.Show(text);
                                    //}

                                    return fi;
                                }
                            }
                        }

                    }
                }
            }

            return null;

        }


    }
}
