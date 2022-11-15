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
    public class ScreenShotPreparer
    {
        public DirectoryInfo PrepareScreenshot(FileInfo fi)
        {
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

            var splitDir = FileStructure.SplitDir(fi.Name);

            var templateImg = Image.FromFile(FileStructure.TemplateImg);

            using (var factory = new ImageProcessor.ImageFactory())
            {
                var img = Image.FromFile(fi.FullName);

                Image blankImg;
                using (var blank = new Bitmap(templateImg.Width, templateImg.Height))
                {
                    var memStream = new MemoryStream();
                    blank.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                    //return memStream.ToArray();
                    blankImg = Image.FromStream(memStream);
                }

                var templateBitmap = (Bitmap)templateImg;
                var sourceBitmap = (Bitmap)img;
                var blankBitmap = (Bitmap)blankImg;

                var wares = new List<string> { "Grain", "Fish", "Meat", "Potato", "Rum", "Textil", "Salt", "Brick", "Wood", "Hemp", "Tobacco", "Dye", "Cocoa", "Sugar", "Cutton", "Grape", "Tools", "Vase", "Clothes" };

                var stockCol = new Rectangle(0, 0, 36, 10);
                var sellCol = new Rectangle(0, 0, 28, 12);
                var buyCol = new Rectangle(0, 0, 26, 12);

                var stockRectangles = new List<WareRectangle>();

                var wareInx = 0;
                for (var y = 0; y < templateBitmap.Height; y++)
                {

                    Func<Color, bool> isTemplateColor = delegate (Color color)
                     {
                         var templateColor = Color.FromArgb(0, 255, 0);
                         int maxOffset = 10;
                         return isPixelInRange(new Tuple<int, int, int>(templateColor.R, templateColor.G, templateColor.B), color, maxOffset);
                     };

                    for (var x = 0; x < templateBitmap.Width; x++)
                    {
                        var pixel = templateBitmap.GetPixel(x, y);

                        if (isTemplateColor(pixel))
                        {
                            blankBitmap.SetPixel(x, y, sourceBitmap.GetPixel(x, y));

                            //Find top left corner
                            int foundNX = 0, foundNY = 0;
                            for (int nX = x - 1; nX > 0; nX--)
                            {
                                var tryPixel = templateBitmap.GetPixel(nX, y);
                                if (!isTemplateColor(tryPixel))
                                {
                                    foundNX = nX + 1;
                                    break;
                                }
                            }

                            for (int nY = y - 1; nY > 0; nY--)
                            {
                                var tryPixel = templateBitmap.GetPixel(x, nY);
                                if (!isTemplateColor(tryPixel))
                                {
                                    foundNY = nY + 1;
                                    break;
                                }
                            }

                            //Top Left corner found

                            var topLeft = new Point(foundNX, foundNY);
                            //Check if top left not already saved
                            if (!stockRectangles.Any(x => x.TopLeft == topLeft))
                            {
                                //Top Left corner not yet assigned
                                var rect = new WareRectangle(x, y);
                                rect.TopLeft = topLeft;

                                //Try width of col1, col2 and then col3
                                var tryCol3 = templateBitmap.GetPixel(topLeft.X + buyCol.Width, topLeft.Y);
                                if (isTemplateColor(tryCol3))
                                {
                                    var tryCol2 = templateBitmap.GetPixel(topLeft.X + sellCol.Width, topLeft.Y);
                                    if (isTemplateColor(tryCol2))
                                    {
                                        var tryCol1 = templateBitmap.GetPixel(topLeft.X + stockCol.Width, topLeft.Y);
                                        if (isTemplateColor(tryCol1))
                                        {
                                            int a = 5; //kann nicht sein
                                        }
                                        else
                                        {
                                            rect.Width = stockCol.Width - 1;
                                            rect.Height = stockCol.Height - 1;
                                            rect.Col = WareRectangle.enCol.Stock;
                                        }
                                    }
                                    else
                                    {
                                        rect.Width = sellCol.Width - 1;
                                        rect.Height = sellCol.Height - 1;
                                        rect.Col = WareRectangle.enCol.Sell;
                                    }
                                }
                                else
                                {
                                    rect.Width = buyCol.Width - 1;
                                    rect.Height = buyCol.Height - 1;
                                    rect.Col = WareRectangle.enCol.Buy;
                                }

                                rect.Ware = wares[wareInx];

                                //System.Diagnostics.Debug.WriteLine($"found x:{x} y:{y}, width: {rect.Width}, height: {rect.Height} ({rect.Ware}) ");

                                stockRectangles.Add(rect);

                                if (stockRectangles.Where(x => x.Ware == rect.Ware).Count() == 3)
                                {
                                    wareInx++;
                                }

                            }



                        }
                        else
                        {
                            blankBitmap.SetPixel(x, y, Color.White);
                        }
                    }
                }


                //blankImg.Save(Path.Combine(splitDir, fi.Name));

                var bitmap = (Bitmap)blankImg;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        var pixel = bitmap.GetPixel(x, y);
                        //Set red numbers (shortage) to black
                        if (true && (isPixelInRange(new Tuple<int, int, int>(247, 46, 41), pixel, 50) ||
                            isPixelInRange(new Tuple<int, int, int>(236, 121, 109), pixel, 50)))
                        {
                            bitmap.SetPixel(x, y, Color.Black);
                        }
                    }
                }

                //blankImg.Save(Path.Combine(splitDir, fi.Name));


                //gauss zeugs geht nicht
                if (true)
                {
                    //Convert to grayscale
                    blankImg = factory
                        .Load(blankImg)
                        .Filter(ImageProcessor.Imaging.Filters.Photo.MatrixFilters.GreyScale)
                        .Image;
                    //blankImg.Save(Path.Combine(splitDir, fi.Name));

                }
                if (false)
                {
                    Image testImg;
                    using (var blank = new Bitmap(templateImg.Width, templateImg.Height))
                    {
                        var memStream = new MemoryStream();
                        //return memStream.ToArray();

                        using (var g = Graphics.FromImage(blank))
                        {
                            var pen = new Pen(Brushes.Red);
                            var inx2 = 0;
                            foreach (var rect in stockRectangles)
                            {
                                g.DrawRectangle(pen, rect.ToRectangle());

                                var rectf = new RectangleF(rect.X, rect.Y, rect.Width, rect.Height);
                                g.DrawString(rect.Ware, new Font("Tahoma", 4), Brushes.Red, rectf);
                                inx2++;
                            }
                        }

                        blank.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                        testImg = Image.FromStream(memStream);
                        testImg.Save(Path.Combine(splitDir, "test.png"));
                    }
                }

                blankImg.Save(Path.Combine(splitDir, "raw" + fi.Extension));


                var removePixels = new List<Tuple<int, int, int>>
                    {
                        new Tuple<int, int, int>(204,171,147),
                        new Tuple<int, int, int>(216,180,147),
                        new Tuple<int, int, int>(167,113,67),
                        new Tuple<int, int, int>(163,104,62),
                        new Tuple<int, int, int>(176,115,61),
                        new Tuple<int, int, int>(176,118,62),
                        new Tuple<int, int, int>(174,117,62),
                        new Tuple<int, int, int>(157,143,120)
                    };

                var allowedPixels = new List<Tuple<int, int, int>>
                    {
                        new Tuple<int, int, int>(113,113,98),
                        new Tuple<int, int, int>(10,9,6),
                        new Tuple<int, int, int>(43,43,40),
                        new Tuple<int, int, int>(38,37,32)
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

                bitmap = (Bitmap)blankImg;

                for (var x = 0; x < bitmap.Width; x++)
                {
                    for (var y = 0; y < bitmap.Height; y++)
                    {
                        var pixel = bitmap.GetPixel(x, y);

                        if (true && (isPixelInRange(new Tuple<int, int, int>(247, 46, 41), pixel, 100)
                            || isPixelInRange(new Tuple<int, int, int>(236, 121, 109), pixel, 100)
                            || isPixelInRange(new Tuple<int, int, int>(215, 151, 126), pixel, 20)
                            || isPixelInRange(new Tuple<int, int, int>(255,37, 34), pixel, 100)))
                        {
                            bitmap.SetPixel(x, y, Color.Black);
                        }
                        else if (false && pixel.R > 140 && pixel.G > 140 && pixel.B > 140)
                        {
                            //bitmap.SetPixel(x, y, Color.White);
                        }
                        else if (false)
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

                        const float limit = 0.55f;
                        if (false && pixel.GetBrightness() > limit)
                        {
                            //bitmap.SetPixel(x, y, Color.White);
                        }

                    }

                }


                if (false)
                {
                    factory.Load(blankImg);

                    blankImg = new ImageProcessor.Processors.GaussianSharpen()
                    { DynamicParameter = new ImageProcessor.Imaging.GaussianLayer(8) }.ProcessImage(factory);


                    blankImg.Save(FileStructure.MapPreparedDir(fi.Name));
                }


                var croppedimg2 = blankImg;// croppedImg;// cropImage(croppedImg, size2);

                var scaleFactor = 5;
                if (scaleFactor > 1)
                {
                    croppedimg2 = factory.Load(croppedimg2).Resize(new Size(croppedimg2.Width * scaleFactor, croppedimg2.Height * scaleFactor)).Image;
                }

                if (true)
                {
                    croppedimg2 = new ImageProcessor.Imaging.Filters.Binarization.BinaryThreshold(121).ProcessFilter((Bitmap)croppedimg2);
                }

                //croppedimg2.Save(Path.Combine(splitDir, fi.Name));

                var inx = 0;
                WareRectangle lastRectangle = null;
                foreach (var rectangle in stockRectangles)
                {
                    if (lastRectangle != null && lastRectangle.Ware != rectangle.Ware)
                    {
                        inx++;
                    }

                    var cropRectangle = rectangle.ToRectangle(scaleFactor);
                    if (rectangle.Col != WareRectangle.enCol.Stock)
                    {
                        cropRectangle.Height -= 0;
                    }
                    else
                    {
                    }
                    using (var cropped = factory.Load(croppedimg2).Crop(cropRectangle).Image)
                    {
                        cropped.Save(Path.Combine(splitDir, rectangle.Ware + "_" + rectangle.Col.ToString() + Path.GetExtension(fi.FullName)));
                    }

                    lastRectangle = rectangle;
                }
            }

            return new DirectoryInfo(splitDir);
        }

        private Bitmap getDifferencBitmap(Bitmap bmp1, Bitmap bmp2, Color diffColor)
        {
            Size s1 = bmp1.Size;
            Size s2 = bmp2.Size;
            if (s1 != s2) return null;

            Bitmap bmp3 = new Bitmap(s1.Width, s1.Height);

            for (int y = 0; y < s1.Height; y++)
                for (int x = 0; x < s1.Width; x++)
                {
                    Color c1 = bmp1.GetPixel(x, y);
                    Color c2 = bmp2.GetPixel(x, y);
                    if (c1 == c2) bmp3.SetPixel(x, y, c1);
                    else bmp3.SetPixel(x, y, diffColor);
                }
            return bmp3;
        }
    }
}
