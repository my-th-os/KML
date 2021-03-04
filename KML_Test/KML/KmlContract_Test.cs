using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using KML_Test.Data;
using System.Windows;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlContract_Test
    {
        private TestData data = new TestData();
        private bool _testEventHandlerVisited = false;

        private void TestEventHandler(object sender, RoutedEventArgs e)
        {
            _testEventHandlerVisited = true;
        }

        [TestMethod]
        public void CreateItem()
        {
            KmlItem item = KmlItem.CreateItem("CONTRACT");
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlContract);
            KmlContract contract = (KmlContract)item;
            Assert.AreEqual("", contract.Name);
            Assert.AreEqual(KmlContract.ContractOrigin.Other, contract.Origin);
            Assert.IsNull(contract.Parent);
            Assert.AreEqual("", contract.State);
            Assert.AreEqual("", contract.Type);
            Assert.AreEqual("", contract.Agent);
            Assert.IsNull(contract.RelatedPart);
            Assert.IsNull(contract.RelatedVessel);
        }

        [TestMethod]
        public void CreateItemFinished()
        {
            KmlItem item = KmlItem.CreateItem("CONTRACT_FINISHED");
            Assert.IsNotNull(item);
            Assert.IsTrue(item is KmlContract);
            KmlContract contract = (KmlContract)item;
            Assert.AreEqual("", contract.Name);
            Assert.AreEqual(KmlContract.ContractOrigin.Other, contract.Origin);
            Assert.IsNull(contract.Parent);
            Assert.AreEqual("", contract.State);
            Assert.AreEqual("", contract.Type);
            Assert.AreEqual("", contract.Agent);
            Assert.IsNull(contract.RelatedPart);
            Assert.IsNull(contract.RelatedVessel);
        }

        [TestMethod]
        public void AssignAttribs()
        {
            Assert.AreEqual("Active", data.Contract1.State);
            Assert.AreEqual("ExplorationContract", data.Contract1.Type);
            Assert.AreEqual("Ultimate Testing Inc.", data.Contract1.Agent);

            Assert.AreEqual("Offered", data.Contract2.State);
            Assert.AreEqual("SurveyContract", data.Contract2.Type);
            Assert.AreEqual("Bug Hunters", data.Contract2.Agent);

            Assert.AreEqual("Completed", data.Contract3.State);
            Assert.AreEqual("TourismContract", data.Contract3.Type);
            Assert.AreEqual("Persistence World Exploration Group", data.Contract3.Agent);
        }

        [TestMethod]
        public void AssignParent()
        {
            KmlContract contract = KmlItem.CreateItem("CONTRACT") as KmlContract;
            Assert.AreEqual(KmlContract.ContractOrigin.Other, contract.Origin);
            Assert.IsNull(contract.Parent);
            data.Contracts.Add(contract);
            Assert.AreEqual(KmlContract.ContractOrigin.Contracts, contract.Origin);
            Assert.AreEqual(data.Contracts, contract.Parent);
            data.Root2.Add(contract);
            Assert.AreEqual(KmlContract.ContractOrigin.Other, contract.Origin);
            Assert.AreEqual(data.Root2, contract.Parent);
        }

        [TestMethod]
        public void Clear()
        {
            data.Contract1.Clear();
            Assert.AreEqual("", data.Contract1.State);
            Assert.AreEqual("", data.Contract1.Type);
            Assert.AreEqual("", data.Contract1.Agent);
        }

        [TestMethod]
        public void ToStringChanged()
        {
            data.Contract2.ToStringChanged += TestEventHandler;

            _testEventHandlerVisited = false;
            data.Contract2.GetAttrib("type").Value = "NewType";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewType", data.Contract2.Type);

            _testEventHandlerVisited = false;
            data.Contract2.GetAttrib("state").Value = "NewState";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("NewState", data.Contract2.State);

            _testEventHandlerVisited = false;
            data.Contract2.GetAttrib("agent").Value = "New Agency";
            Assert.IsTrue(_testEventHandlerVisited);
            Assert.AreEqual("New Agency", data.Contract2.Agent);
        }
    }
}
