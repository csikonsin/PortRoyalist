using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortRoyalist.Tests
{
    [TestClass]
    public class ScreenShotParserTests
    {
        private TestContext testContextInstance;

        /// <summary>
        /// Gets or sets the test context which provides
        /// information about and functionality for the current test run.
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [TestInitialize]
        public void Init()
        {
            FileStructure.Init();
        }

        [TestCleanup]
        public void CleanUp()
        {
            //todo remove all generate files
        }

        [TestMethod]
        public void PrepareScreenShotTests()
        {
            var preparer = new ScreenShotPreparer();
            preparer.PrepareScreenshot(new FileInfo(FileStructure.MapInputDir("ct.png")));

        }

        [TestMethod]
        public void ParseScreenShotTests()
        {
            var preparer = new ScreenShotPreparer();
            preparer.PrepareScreenshot(new FileInfo(FileStructure.MapInputDir("ct.png")));
            
            var parser = new ScreenShotParser();

            var res = parser.ParseScreenshot(new DirectoryInfo(FileStructure.MapPreparedDir("ct_png")));

            var success = 0;
            var excepteds = new List<string> { "308", "208", "", "414", "336", "36", "654", "513", "16", "160", "127", "63", "343", "264", "19", "239", "190", "89", "96", "76", "110", "534", "434", "32", "584", "430", "10", "371", "279", "20", "231", "185", "51", "439", "348", "46", "286", "227", "40", "219", "171", "33", "358", "285", "89", "1475", "1065", "5", "624", "467", "2", "212", "172", "33", "88", "68", "" };
            for (int i = 0; i < excepteds.Count; i++)
            {
                var expected = excepteds[i];
                var actual = res.Results[i];
                if(expected != actual.ParsedValue)
                {
                    TestContext.WriteLine($"Failed {expected} as {actual.ParsedValue}");
                }
                else
                {
                    TestContext.WriteLine($"Parsed {expected} as {actual.ParsedValue}");
                    success++;
                }
            }
            
            TestContext.WriteLine($"Successfully Parsed {success}/{res.TotalCount}");

            Assert.AreEqual(success, res.TotalCount);

            //CollectionAssert.AreEquivalent(excepteds, res.Results.Select(x => x.ParsedValue).ToList());

        }
    }
}
