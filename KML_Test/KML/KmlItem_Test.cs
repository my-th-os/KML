using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using KML;
using System.IO;
using System.Collections.Generic;

namespace KML_Test.KML
{
    [TestClass]
    public class KmlItem_Test
    {
        private enum TestDataDir { Files_Ok, Files_Warning };

        private DirectoryInfo GetTestDataDir(TestDataDir subdir)
        {
            DirectoryInfo dir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory);
            int parentDirs = 1;
            for (int i = 0; i < parentDirs && dir != null; i++)
            {
                dir = dir.Parent;
            }
            DirectoryInfo[] subdirs = dir.GetDirectories("Data");
            if (subdirs.Length > 0)
            {
                dir = subdirs[0];
            }
            subdirs = dir.GetDirectories(subdir.ToString());
            if (subdirs.Length > 0)
            {
                return subdirs[0];
            }
            else
            {
                return null;
            }
        }

        [TestMethod]
        public void Create()
        {
            KmlItem item = new KmlItem("test");
            Assert.IsNull(item.Parent);
        }

        [TestMethod]
        public void CreateItem()
        {
            Assert.IsNull(KmlItem.CreateItem(null));
            Assert.IsNull(KmlItem.CreateItem("{xy"));
            Assert.IsNull(KmlItem.CreateItem("x{y"));
            Assert.IsNull(KmlItem.CreateItem("xy{"));
            Assert.IsNull(KmlItem.CreateItem("}xy"));
            Assert.IsNull(KmlItem.CreateItem("x}y"));
            Assert.IsNull(KmlItem.CreateItem("xy}"));

            KmlItem test;
            test = KmlItem.CreateItem("attrib = value");
            Assert.IsTrue(test is KmlAttrib);
            test = KmlItem.CreateItem("node");
            Assert.IsTrue(test is KmlNode);
            test = KmlItem.CreateItem("vessel");
            Assert.IsTrue(test is KmlVessel);
            test = KmlItem.CreateItem("kerbal");
            Assert.IsTrue(test is KmlKerbal);
            test = KmlItem.CreateItem("part");
            Assert.IsTrue(test is KmlPart);
            test = KmlItem.CreateItem("resource");
            Assert.IsTrue(test is KmlResource);
        }

        [TestMethod]
        public void CanBeDeleted()
        {
            KmlItem item = new KmlItem("test");
            Assert.IsTrue(item.CanBeDeleted);
            item.CanBeDeleted = false;
            Assert.IsFalse(item.CanBeDeleted);
        }

        [TestMethod]
        public void Delete()
        {
            KmlItem item = new KmlItem("test");
            Assert.IsFalse(item.Delete());
            item.CanBeDeleted = false;
            Assert.IsFalse(item.Delete());

            KmlNode root = new KmlNode("root");
            root.Add(item);
            Assert.AreEqual(root, item.Parent);
            Assert.IsFalse(item.Delete());
            item.CanBeDeleted = true;
            Assert.IsTrue(item.Delete());
        }

        [TestMethod]
        public void PathToString()
        {
            KmlNode root = new KmlNode("root");
            KmlNode node = new KmlNode("node");
            KmlItem item = new KmlNode("item");
            node.Add(item);
            root.Add(node);
            Assert.AreEqual("root", root.PathToString("-"));
            Assert.AreEqual("root-node", node.PathToString("-"));
            Assert.AreEqual("root/node/item", item.PathToString("/"));
        }

        [TestMethod]
        public void ParseFile()
        {
            string path = GetTestDataDir(TestDataDir.Files_Ok).FullName;
            string filename = Path.Combine(path, "SimpleTest.sfs");
            List<KmlItem> roots = KmlItem.ParseFile(filename);
            Assert.AreEqual(1, roots.Count);
            Assert.IsTrue(roots[0] is KmlNode);
            KmlNode root = (KmlNode)roots[0];
            Assert.AreEqual("ROOT", root.Tag);
            Assert.AreEqual(2, root.Attribs.Count);
            Assert.AreEqual(1, root.Children.Count);
            Assert.AreEqual(0, root.Unknown.Count);
            Assert.AreEqual(3, root.AllItems.Count);
        }

        [TestMethod]
        public void ParseFileOneRoot()
        {
            string path = GetTestDataDir(TestDataDir.Files_Ok).FullName;
            string filename = Path.Combine(path, "SimpleOneRoot.sfs");
            List<KmlItem> roots = KmlItem.ParseFile(filename);
            Assert.AreEqual(1, roots.Count);
            Assert.IsTrue(roots[0] is KmlNode);
            KmlNode root = (KmlNode)roots[0];
            Assert.AreEqual("ROOT", root.Tag);
            Assert.AreEqual(0, root.Attribs.Count);
            Assert.AreEqual(0, root.Children.Count);
            Assert.AreEqual(0, root.Unknown.Count);
            Assert.AreEqual(0, root.AllItems.Count);
        }

        [TestMethod]
        public void ParseFileTwoRoots()
        {
            string path = GetTestDataDir(TestDataDir.Files_Ok).FullName;
            string filename = Path.Combine(path, "SimpleTwoRoots.sfs");
            List<KmlItem> roots = KmlItem.ParseFile(filename);
            Assert.AreEqual(2, roots.Count);
            Assert.IsTrue(roots[0] is KmlNode);
            Assert.IsTrue(roots[1] is KmlNode);
        }

        [TestMethod]
        public void ParseFileAttribAboveRoot()
        {
            string path = GetTestDataDir(TestDataDir.Files_Ok).FullName;
            string filename = Path.Combine(path, "SimpleAttribAboveRoot.sfs");
            List<KmlItem> roots = KmlItem.ParseFile(filename);
            Assert.AreEqual(2, roots.Count);
            Assert.IsTrue(roots[0] is KmlAttrib);
            Assert.IsTrue(roots[1] is KmlNode);
        }

        [TestMethod]
        public void ParseFileUnknownLine()
        {
            string path = GetTestDataDir(TestDataDir.Files_Warning).FullName;
            string filename = Path.Combine(path, "SimpleUnknownLine.sfs");
            List<KmlItem> roots = KmlItem.ParseFile(filename);
            Assert.AreEqual(1, roots.Count);
            Assert.IsTrue(roots[0] is KmlNode);
            KmlNode root = (KmlNode)roots[0];
            Assert.AreEqual(0, root.Attribs.Count);
            Assert.AreEqual(0, root.Children.Count);
            Assert.AreEqual(1, root.Unknown.Count);
            Assert.AreEqual(1, root.AllItems.Count);
        }

        [TestMethod]
        public void ParseFileAllOk()
        {
            FileInfo[] files = GetTestDataDir(TestDataDir.Files_Ok).GetFiles("*.sfs");
            foreach (FileInfo file in files)
            {
                Syntax.Messages.Clear();
                List<KmlItem> roots = KmlItem.ParseFile(file.FullName);
                string resultname = file.Name;
                if (roots.Count == 0)
                {
                    resultname += " EMPTY!";
                }
                if (Syntax.Messages.Count > 0)
                {
                    resultname += " WARNINGS!";
                }
                Assert.AreEqual(file.Name, resultname);
            }
        }

        [TestMethod]
        public void ParseFileAllWarning()
        {
            FileInfo[] files = GetTestDataDir(TestDataDir.Files_Warning).GetFiles("*.sfs");
            foreach (FileInfo file in files)
            {
                Syntax.Messages.Clear();
                List<KmlItem> roots = KmlItem.ParseFile(file.FullName);
                string resultname = file.Name;
                if (roots.Count == 0)
                {
                    resultname += " EMPTY!";
                }
                if (Syntax.Messages.Count == 0)
                {
                    resultname += " NO EXPECTED WARNINGS!";
                }
                Assert.AreEqual(file.Name, resultname);
            }
        }

        [TestMethod]
        public void WriteFileCompareAllOk()
        {
            FileInfo[] files = GetTestDataDir(TestDataDir.Files_Ok).GetFiles("*.sfs");
            foreach (FileInfo file in files)
            {
                List<KmlItem> roots = KmlItem.ParseFile(file.FullName);
                string temp = Path.GetTempFileName();
                // GetTempFileName creates the file to protect that temp file name
                // but we dont want dest file to be existing, this would cause creating 
                // backup files in temp dir, we then won't clean up
                File.Delete(temp);
                KmlItem.WriteFile(temp, roots);
                string resultname = file.Name;
                if (!File.Exists(temp))
                {
                    resultname += " NOT WRITTEN TO " + temp + "!";
                }
                else
                {
                    // Compare source and dest files
                    string read = File.ReadAllText(file.FullName);
                    string written = File.ReadAllText(temp);
                    if (!read.Equals(written))
                    {
                        resultname += " NOT WRITTEN IDENTICALLY!";
                    }
                }
                File.Delete(temp);
                Assert.AreEqual(file.Name, resultname);
            }
        }
    }
}
