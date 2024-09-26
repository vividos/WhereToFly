using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace WhereToFly.Geo.UnitTest
{
    /// <summary>
    /// Tests for <see cref="EncodedPolylineGeometry"/> class
    /// </summary>
    [TestClass]
    public class EncodedPolylineGeometryTest
    {
        /// <summary>
        /// Polyline geometry without elevation values
        /// </summary>
        private const string PolylineGeometryWithoutElevation = "sx_bH}fqgAh@A@e@s@[q@e@[_@Q]Ms@Eg@BUDGG_A@YEg@JUf@OFS?]K_AZCXHt@HFe@]sAS]GWkAaB`@q@B_@Ng@Vc@@qALYLDNp@FUKgB?gAZs@Ve@Sk@Fe@?e@K]NUd@HQYEk@GYq@SQALg@P]Bk@FYVQ\\AJORiB@U_@g@PWNOLc@FSOm@AUDWW?JUA_@b@MIMg@WO@?UFa@MBCEDQ@SLU^J?MEOPDLLF@^MR?fCf@EW`@XtAZRMJCNRXN\\XZj@NHRDL@ZO@Y]cABOjAmDd@{@X{@h@y@J[B]E[FSZQP]AEy@aAuAo@oAs@m@k@g@_@GS";

        /// <summary>
        /// Polyline geometry with elevation values
        /// </summary>
        private const string PolylineGeometryWithElevation = "sx_bH}fqgAc~eEh@Asr@@e@ks@s@[rOq@e@~Q[_@eCQ]l`@Ms@}hDEg@nGBUdIDGg|@G_AgjC@YhBEg@xBJUhCf@Oua@FS{|H?]eTK_Aaq@ZCcNXHcOt@HEFe@C]sACS]EGW_}FkAaB_gD`@q@co@B_@mSNg@aa@Vc@mm@@qAu[LYgHLDXNp@fVFUuHKgBq~@?gAgoAZs@od@Ve@oUSk@kUFe@uX?e@ix@K]{MNUaBd@HeOQYoNEk@{c@GY_Hq@Sa@QAdGLg@eZP]{RBk@wJFYeGVQf@\\AcGJO^RiBqd@@Ue@_@g@pEPWsYNOnFLc@chLFS?Om@aeDAUkDDWeWW?hKJU}ZA_@qIb@MmJIMgAg@WvAO@jH?UaUFa@yEMBjFCEgFDQqF@S{SLU}F^J{D?MaCEO`APDbALLa@F@`A^MjCR?aRfCf@c_CEWfD`@X{]tAZi{ARM\\JCnFNR~FXNwB\\X}BZj@sCNHkbCRDnvAL@?ZO?@Y?]cA?BO?jAmDoqBd@{@kGX{@rCh@y@s[J[_HB]bFE[w~@FS?ZQ?P]?AE?y@aAgiBuAo@xFoAs@il@m@k@}h@g@_@pOGSD";

        /// <summary>
        /// Tests decoding polyline geometry, with and without elevation
        /// </summary>
        [TestMethod]
        public void TestDecodePolylineGeometry()
        {
            var trackPointsWithoutElevation = EncodedPolylineGeometry.DecodeGeometryToTrackPoints(
                PolylineGeometryWithoutElevation,
                withElevation: false)
                .ToList();

            var trackPointsWithElevation = EncodedPolylineGeometry.DecodeGeometryToTrackPoints(
                PolylineGeometryWithElevation,
                withElevation: true)
                .ToList();

            Assert.AreEqual(
                trackPointsWithoutElevation.Count,
                trackPointsWithElevation.Count,
                "number of track points must be equal");

            for (int trackPointIndex = 0; trackPointIndex < trackPointsWithoutElevation.Count; trackPointIndex++)
            {
                var trackPointWithoutElevation = trackPointsWithoutElevation[trackPointIndex];
                var trackPointWithElevation = trackPointsWithElevation[trackPointIndex];

                Assert.AreEqual(
                    trackPointWithoutElevation.Longitude,
                    trackPointWithElevation.Longitude,
                    1e-5,
                    "longitude values must match");

                Assert.AreEqual(
                    trackPointWithoutElevation.Latitude,
                    trackPointWithElevation.Latitude,
                    1e-5,
                    "latitude values must match");
            }
        }
    }
}
