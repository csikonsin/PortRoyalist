using IronOcr;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Tesseract;

namespace PortRoyalist
{
    public class ScreenShotParser
    {
        public class ParseResult
        {
            public List<ImageResult> Results = new List<ImageResult>();
            public int TotalCount { get; set; } = 0;
            public int ParsedCount { get; set; } = 0;
        }

        public class ImageResult
        {
            public Image Img { get; set; } = null;
            public string ImgPath { get; set; } = "";
            public string ParsedValue { get; set; } = "";
            public bool Failed { get; set; } = false;
        }

        public ParseResult ParseScreenshot(DirectoryInfo di)
        {
            var result = new ParseResult();

            using (var engine = new TesseractEngine(@"./tessdata", "eng", EngineMode.Default))
            {
                engine.SetVariable("tessedit_char_whitelist", "0123456789");

                var foundCnt = 0;
                var fileCount = 0;
                foreach (var fi in di.GetFiles("*.png"))
                {
                    if (di.Name == fi.Name.Replace(".", "_")) continue;
                    if ("test.png" == fi.Name) continue;

                    fileCount++;
                    using (var img = Pix.LoadFromFile(fi.FullName))
                    {
                        using (var page = engine.Process(img))
                        {
                            var text = page.GetText();
                            text = Regex.Replace(text, "\n", "", RegexOptions.Compiled);

                            var res = new ImageResult
                            {
                                Img = Image.FromFile(fi.FullName),
                                ImgPath = fi.FullName,
                                ParsedValue = text
                            };

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                foundCnt++;
                            }
                            else
                            {
                                res.Failed = true;
                            }
                            result.Results.Add(res);
                        }
                    }

                    result.TotalCount = fileCount;
                    result.ParsedCount = foundCnt;
                }
            }

            return result;
        }
    }
}