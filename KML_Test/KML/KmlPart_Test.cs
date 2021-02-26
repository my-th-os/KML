using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using KML_Test.Data;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlPart_Test
    {
        private TestData data = new TestData();
        private bool _testEventHandlerVisited = false;

        private void TestEventHandler(object sender, RoutedEventArgs e)
        {
            _testEventHandlerVisited = true;
        }

        private int CountAttachedToPartsAll(KmlPart part)
        {
            return part.AttachedToPartsBottom.Count +
                part.AttachedToPartsTop.Count +
                part.AttachedToPartsLeft.Count +
                part.AttachedToPartsRight.Count +
                part.AttachedToPartsFront.Count +
                part.AttachedToPartsBack.Count;
        }

        private int CountAttachedPartsAll(KmlPart part)
        {
            return part.AttachedPartsBottom.Count +
                part.AttachedPartsTop.Count +
                part.AttachedPartsLeft.Count +
                part.AttachedPartsRight.Count +
                part.AttachedPartsFront.Count +
                part.AttachedPartsBack.Count;
        }

        [TestMethod]
        public void CreateItem()
        {
            KmlItem item = KmlItem.CreateItem("PART");
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlPart);
            KmlPart part = (KmlPart)item;
            Assert.AreEqual("", part.Name);
            Assert.AreEqual("", part.Flag);
            Assert.IsFalse(part.HasResources);
            Assert.AreEqual(-1, part.ParentPartIndex);
            Assert.AreEqual(new Point3D(0.0, 0.0, 0.0), 
                part.Position);
            Assert.AreEqual(0, part.Resources.Count);
            Assert.AreEqual(0, part.ResourceTypes.Count);
            Assert.AreEqual("", part.Uid);
        }

        [TestMethod]
        public void AssignAttribs()
        {
            Assert.AreEqual("Vessel1Part1", data.Vessel1Part1.Name);
            Assert.AreEqual("Vessel1Flag1", data.Vessel1Part1.Flag);
            Assert.IsTrue(data.Vessel1Part1.HasResources);
            Assert.AreEqual(0, data.Vessel1Part1.ParentPartIndex);
            Assert.AreEqual(new Point3D(0.0, 0.0, 0.0), 
                data.Vessel1Part1.Position);
            Assert.AreEqual(2, data.Vessel1Part1.Resources.Count);
            Assert.AreEqual(data.Vessel1Part1Resource1, data.Vessel1Part1.Resources[0]);
            Assert.AreEqual(data.Vessel1Part1Resource2, data.Vessel1Part1.Resources[1]);
            Assert.AreEqual(2, data.Vessel1Part1.ResourceTypes.Count);
            Assert.IsTrue(data.Vessel1Part1.ResourceTypes.Contains("Resource1"));
            Assert.IsTrue(data.Vessel1Part1.ResourceTypes.Contains("Resource2"));
            Assert.AreEqual("Vessel1Part1Uid", data.Vessel1Part1.Uid);
            Assert.AreEqual(0.5, data.Vessel1Part1.WorstResourceRatio);

            Assert.AreEqual("Vessel1Part2", data.Vessel1Part2.Name);
            Assert.AreEqual("Vessel1Flag2", data.Vessel1Part2.Flag);
            Assert.IsFalse(data.Vessel1Part2.HasResources);
            Assert.AreEqual(0, data.Vessel1Part2.ParentPartIndex);
            Assert.AreEqual(new Point3D(0.0, -1.0, 0.0), 
                data.Vessel1Part2.Position);
            Assert.AreEqual(0, data.Vessel1Part2.Resources.Count);
            Assert.AreEqual(0, data.Vessel1Part2.ResourceTypes.Count);
            Assert.AreEqual("Vessel1Part2Uid", data.Vessel1Part2.Uid);

            Assert.AreEqual("Vessel1Part8", data.Vessel1Part8.Name);
            Assert.AreEqual(1, data.Vessel1Part8.ParentPartIndex);
            Assert.AreEqual(new Point3D(0.0, -2.0, 0.0),
                data.Vessel1Part8.Position);
        }

        [TestMethod]
        public void AssignParent()
        {
            KmlPart part = KmlItem.CreateItem("PART") as KmlPart;
            Assert.AreEqual(KmlPart.PartOrigin.Other, part.Origin);
            Assert.IsNull(part.Parent);
            data.Vessel1.Add(part);
            Assert.AreEqual(KmlPart.PartOrigin.Vessel, part.Origin);
            Assert.AreEqual(data.Vessel1, part.Parent);
            data.Root2.Add(part);
            Assert.AreEqual(KmlPart.PartOrigin.Other, part.Origin);
            Assert.AreEqual(data.Root2, part.Parent);
        }

        [TestMethod]
        public void AssignAttachments()
        {
            Assert.AreEqual(6, data.Vessel1Part1.AttachedToNodeIndices.Count);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToNodeIndices[0]);
            Assert.AreEqual(2, data.Vessel1Part1.AttachedToNodeIndices[1]);
            Assert.AreEqual(3, data.Vessel1Part1.AttachedToNodeIndices[2]);
            Assert.AreEqual(4, data.Vessel1Part1.AttachedToNodeIndices[3]);
            Assert.AreEqual(5, data.Vessel1Part1.AttachedToNodeIndices[4]);
            Assert.AreEqual(6, data.Vessel1Part1.AttachedToNodeIndices[5]);
            Assert.AreEqual(1, data.Vessel1Part2.AttachedToNodeIndices.Count);
            Assert.AreEqual(0, data.Vessel1Part2.AttachedToNodeIndices[0]);
            Assert.AreEqual(1, data.Vessel1Part3.AttachedToNodeIndices.Count);
            Assert.AreEqual(0, data.Vessel1Part3.AttachedToNodeIndices[0]);
            Assert.AreEqual(1, data.Vessel1Part4.AttachedToNodeIndices.Count);
            Assert.AreEqual(0, data.Vessel1Part4.AttachedToNodeIndices[0]);
            Assert.AreEqual(1, data.Vessel1Part5.AttachedToNodeIndices.Count);
            Assert.AreEqual(0, data.Vessel1Part5.AttachedToNodeIndices[0]);
            Assert.AreEqual(1, data.Vessel1Part6.AttachedToNodeIndices.Count);
            Assert.AreEqual(0, data.Vessel1Part6.AttachedToNodeIndices[0]);
            Assert.AreEqual(1, data.Vessel1Part7.AttachedToNodeIndices.Count);
            Assert.AreEqual(0, data.Vessel1Part7.AttachedToNodeIndices[0]);
            Assert.AreEqual(1, data.Vessel1Part8.AttachedToSurfaceIndex);

            Assert.AreEqual(6, CountAttachedPartsAll(data.Vessel1Part1));
            Assert.AreEqual(6, CountAttachedToPartsAll(data.Vessel1Part1));
            Assert.AreEqual(1, CountAttachedPartsAll(data.Vessel1Part2));
            Assert.AreEqual(1, CountAttachedToPartsAll(data.Vessel1Part2));
            Assert.AreEqual(1, CountAttachedPartsAll(data.Vessel1Part3));
            Assert.AreEqual(1, CountAttachedToPartsAll(data.Vessel1Part3));
            Assert.AreEqual(1, CountAttachedPartsAll(data.Vessel1Part4));
            Assert.AreEqual(1, CountAttachedToPartsAll(data.Vessel1Part4));
            Assert.AreEqual(1, CountAttachedPartsAll(data.Vessel1Part5));
            Assert.AreEqual(1, CountAttachedToPartsAll(data.Vessel1Part5));
            Assert.AreEqual(1, CountAttachedPartsAll(data.Vessel1Part6));
            Assert.AreEqual(1, CountAttachedToPartsAll(data.Vessel1Part6));
            Assert.AreEqual(1, CountAttachedPartsAll(data.Vessel1Part7));
            Assert.AreEqual(1, CountAttachedToPartsAll(data.Vessel1Part7));

            Assert.AreEqual(1, data.Vessel1Part1.AttachedPartsBottom.Count);
            Assert.AreEqual(data.Vessel1Part2, data.Vessel1Part1.AttachedPartsBottom[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToPartsBottom.Count);
            Assert.AreEqual(data.Vessel1Part2, data.Vessel1Part1.AttachedToPartsBottom[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedPartsTop.Count);
            Assert.AreEqual(data.Vessel1Part3, data.Vessel1Part1.AttachedPartsTop[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToPartsTop.Count);
            Assert.AreEqual(data.Vessel1Part3, data.Vessel1Part1.AttachedToPartsTop[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedPartsLeft.Count);
            Assert.AreEqual(data.Vessel1Part4, data.Vessel1Part1.AttachedPartsLeft[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToPartsLeft.Count);
            Assert.AreEqual(data.Vessel1Part4, data.Vessel1Part1.AttachedToPartsLeft[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedPartsRight.Count);
            Assert.AreEqual(data.Vessel1Part5, data.Vessel1Part1.AttachedPartsRight[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToPartsRight.Count);
            Assert.AreEqual(data.Vessel1Part5, data.Vessel1Part1.AttachedToPartsRight[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedPartsBack.Count);
            Assert.AreEqual(data.Vessel1Part6, data.Vessel1Part1.AttachedPartsBack[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToPartsBack.Count);
            Assert.AreEqual(data.Vessel1Part6, data.Vessel1Part1.AttachedToPartsBack[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedPartsFront.Count);
            Assert.AreEqual(data.Vessel1Part7, data.Vessel1Part1.AttachedPartsFront[0]);
            Assert.AreEqual(1, data.Vessel1Part1.AttachedToPartsFront.Count);
            Assert.AreEqual(data.Vessel1Part7, data.Vessel1Part1.AttachedToPartsFront[0]);

            Assert.AreEqual(1, data.Vessel1Part2.AttachedPartsTop.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part2.AttachedPartsTop[0]);
            Assert.AreEqual(1, data.Vessel1Part2.AttachedToPartsTop.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part2.AttachedToPartsTop[0]);

            Assert.AreEqual(1, data.Vessel1Part3.AttachedPartsBottom.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part3.AttachedPartsBottom[0]);
            Assert.AreEqual(1, data.Vessel1Part3.AttachedToPartsBottom.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part3.AttachedToPartsBottom[0]);

            Assert.AreEqual(1, data.Vessel1Part4.AttachedPartsRight.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part4.AttachedPartsRight[0]);
            Assert.AreEqual(1, data.Vessel1Part4.AttachedToPartsRight.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part4.AttachedToPartsRight[0]);

            Assert.AreEqual(1, data.Vessel1Part5.AttachedPartsLeft.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part5.AttachedPartsLeft[0]);
            Assert.AreEqual(1, data.Vessel1Part5.AttachedToPartsLeft.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part5.AttachedToPartsLeft[0]);

            Assert.AreEqual(1, data.Vessel1Part6.AttachedPartsFront.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part6.AttachedPartsFront[0]);
            Assert.AreEqual(1, data.Vessel1Part6.AttachedToPartsFront.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part6.AttachedToPartsFront[0]);

            Assert.AreEqual(1, data.Vessel1Part7.AttachedPartsBack.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part7.AttachedPartsBack[0]);
            Assert.AreEqual(1, data.Vessel1Part7.AttachedToPartsBack.Count);
            Assert.AreEqual(data.Vessel1Part1, data.Vessel1Part7.AttachedToPartsBack[0]);

            Assert.AreEqual(1, data.Vessel1Part2.AttachedPartsSurface.Count);
            Assert.AreEqual(data.Vessel1Part8, data.Vessel1Part2.AttachedPartsSurface[0]);

            Assert.AreEqual(data.Vessel1Part2, data.Vessel1Part8.AttachedToPartSurface);
        }

        [TestMethod]
        public void AttribsChanged()
        {
            data.Vessel1Part1.ToStringChanged += TestEventHandler;
            _testEventHandlerVisited = false;
            data.Vessel1Part1.GetAttrib("name").Value = "NewName";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewName", data.Vessel1Part1.Name);

            data.Vessel1Part1.GetAttrib("flag").Value = "NewFlag";
            Assert.AreEqual("NewFlag", data.Vessel1Part1.Flag);

            data.Vessel1Part1.GetAttrib("uid").Value = "NewUid";
            Assert.AreEqual("NewUid", data.Vessel1Part1.Uid);
        }

        [TestMethod]
        public void Clear()
        {
            data.Vessel1Part1.Clear();
            Assert.AreEqual("", data.Vessel1Part1.Name);
            Assert.AreEqual("", data.Vessel1Part1.Flag);
            Assert.IsFalse(data.Vessel1Part1.HasResources);
            Assert.AreEqual(-1, data.Vessel1Part1.ParentPartIndex);
            Assert.AreEqual(new Point3D(0.0, 0.0, 0.0), data.Vessel1Part1.Position);
            Assert.AreEqual(0, data.Vessel1Part1.Resources.Count);
            Assert.AreEqual(0, data.Vessel1Part1.ResourceTypes.Count);
            Assert.AreEqual("", data.Vessel1Part1.Uid);
        }

        [TestMethod]
        public void RefillAll()
        {
            data.Vessel1Part1.Refill();
            Assert.AreEqual(1.0, data.Vessel1Part1.WorstResourceRatio);
            Assert.AreEqual(data.Vessel1Part1Resource1.MaxAmount.Value, data.Vessel1Part1Resource1.Amount.Value);
            Assert.AreEqual(data.Vessel1Part1Resource2.MaxAmount.Value, data.Vessel1Part1Resource2.Amount.Value);
        }

        [TestMethod]
        public void RefillType()
        {
            data.Vessel1Part1.Refill("nonsense");
            Assert.AreEqual(0.5, data.Vessel1Part1.WorstResourceRatio);
            Assert.AreEqual("50", data.Vessel1Part1Resource1.Amount.Value);
            data.Vessel1Part1.Refill("Resource1");
            Assert.AreEqual(1.0, data.Vessel1Part1.WorstResourceRatio);
            Assert.AreEqual(data.Vessel1Part1Resource1.MaxAmount.Value, data.Vessel1Part1Resource1.Amount.Value);
        }

        [TestMethod]
        public void FlagExchange()
        {
            data.Vessel1Part1.FlagExchange("nonsense", "NewFlag");
            Assert.AreEqual("Vessel1Flag1", data.Vessel1Part1.Flag);
            data.Vessel1Part1.FlagExchange("Vessel1Flag1", "NewFlag");
            Assert.AreEqual("NewFlag", data.Vessel1Part1.Flag);
        }
    }
}
