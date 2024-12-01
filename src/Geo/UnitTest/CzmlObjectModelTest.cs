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
            var objectList = new List<object>
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

            Assert.IsTrue(czml.Contains("\"label\":{"), "CZML must contain label");
            Assert.IsTrue(czml.Contains("FILL_AND_OUTLINE"), "CZML must contain label style");
            Assert.IsTrue(czml.Contains("LEFT"), "CZML must contain horizontal origin");
            Assert.IsTrue(czml.Contains("BASELINE"), "CZML must contain vertical origin");
            Assert.IsTrue(czml.Contains("CLAMP_TO_GROUND"), "CZML must contain height reference");
        }

        /// <summary>
        /// Tests writing a billboard to CZML
        /// </summary>
        [TestMethod]
        public void TestWriteBillbord()
        {
            // set up
            var objectList = new List<object>
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

            Assert.IsTrue(czml.Contains("\"billboard\":{"), "CZML must contain billboard");
            Assert.IsTrue(czml.Contains("RIGHT"), "CZML must contain horizontal origin");
            Assert.IsTrue(czml.Contains("CENTER"), "CZML must contain vertical origin");
            Assert.IsTrue(czml.Contains("RELATIVE_TO_GROUND"), "CZML must contain height reference");
        }

        /// <summary>
        /// Tests writing a model to CZML
        /// </summary>
        [TestMethod]
        public void TestWriteModel()
        {
            // set up
            var objectList = new List<object>
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

            Assert.IsTrue(czml.Contains("\"model\":{"), "CZML must contain model");
            Assert.IsTrue(czml.Contains("NONE"), "CZML must contain height reference");
        }
    }
}
