using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;
using WhereToFly.Geo.DataFormats.Czml;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Tests czml object model classes
    /// </summary>
    [TestClass]
    public class CzmlObjectModelTest
    {
        /// <summary>
        /// Tests writing a label to CZML
        /// </summary>
        [TestMethod]
        public void TestWriteLabel()
        {
            // set up
            var objectList = new List<CzmlBase>
            {
                new PacketHeader("labeltest", "description"),
                new DataFormats.Czml.Object
                {
                    Name = "label element",
                    Description = "element description",
                    Position = new PositionList(
                        latitude: 48.2,
                        longitude: 11.8,
                        height: 1042.0),
                    Label = new DataFormats.Czml.Label
                    {
                        Text = "the label",
                        Font = "sans-serif",
                        LabelStyle = LabelStyle.FillAndOutline,
                        ShowBackground = true,
                        HorizontalOrigin = HorizontalOrigin.Left,
                        VerticalOrigin = VerticalOrigin.Baseline,
                        HeightReference = HeightReference.ClampToGround,
                        FillColor = Color.FromCssColorString("#007fff"),
                        BackgroundColor = Color.FromCssColorString("#ffffff"),
                        OutlineColor = Color.FromCssColorString("#00000000"),
                        OutlineWidth = 2.0,
                        DistanceDisplayCondition = new DistanceDisplayCondition(0, 20000.0),
                    },
                },
            };

            // run
            string czml = Serializer.ToCzml(objectList);

            // check
            Debug.WriteLine("CZML = " + czml);

            Assert.Contains("\"label\":{", czml, "CZML must contain label");
            Assert.Contains("FILL_AND_OUTLINE", czml, "CZML must contain label style");
            Assert.Contains("LEFT", czml, "CZML must contain horizontal origin");
            Assert.Contains("BASELINE", czml, "CZML must contain vertical origin");
            Assert.Contains("CLAMP_TO_GROUND", czml, "CZML must contain height reference");
        }

        /// <summary>
        /// Tests writing a billboard to CZML
        /// </summary>
        [TestMethod]
        public void TestWriteBillbord()
        {
            // set up
            var objectList = new List<CzmlBase>
            {
                new PacketHeader("billboardtest", "description"),
                new DataFormats.Czml.Object
                {
                    Name = "billboard element",
                    Description = "element description",
                    Position = new PositionList(
                        latitude: 48.2,
                        longitude: 11.8,
                        height: 1042.0),
                    Billboard = new DataFormats.Czml.Billboard
                    {
                        Image = "image.png",
                        Width = 640,
                        Height = 480,
                        SizeInMeters = false,
                        HorizontalOrigin = HorizontalOrigin.Right,
                        VerticalOrigin = VerticalOrigin.Center,
                        HeightReference = HeightReference.RelativeToGround,
                        DisableDepthTestDistance = 40000,
                        DistanceDisplayCondition = new DistanceDisplayCondition(0, 20000.0),
                    },
                },
            };

            // run
            string czml = Serializer.ToCzml(objectList);

            // check
            Debug.WriteLine("CZML = " + czml);

            Assert.Contains("\"billboard\":{", czml, "CZML must contain billboard");
            Assert.Contains("RIGHT", czml, "CZML must contain horizontal origin");
            Assert.Contains("CENTER", czml, "CZML must contain vertical origin");
            Assert.Contains("RELATIVE_TO_GROUND", czml, "CZML must contain height reference");
        }

        /// <summary>
        /// Tests writing a model to CZML
        /// </summary>
        [TestMethod]
        public void TestWriteModel()
        {
            // set up
            var objectList = new List<CzmlBase>
            {
                new PacketHeader("modeltest", "description"),
                new DataFormats.Czml.Object
                {
                    Name = "model element",
                    Description = "element description",
                    Position = new PositionList(
                        latitude: 48.2,
                        longitude: 11.8,
                        height: 1042.0),
                    Model = new DataFormats.Czml.Model
                    {
                        Uri = "data:null",
                        Scale = 42.0,
                        HeightReference = HeightReference.None,
                    },
                },
            };

            // run
            string czml = Serializer.ToCzml(objectList);

            // check
            Debug.WriteLine("CZML = " + czml);

            Assert.Contains("\"model\":{", czml, "CZML must contain model");
            Assert.Contains("NONE", czml, "CZML must contain height reference");
        }
    }
}
