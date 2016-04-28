using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace UAssetTools
{
    [TestClass]
    public class Tests
    {
        public bool OpenSave(string file_in, bool bSoftMode = false)
        {
            bool bResult = false;
            string file_out = Path.GetTempFileName();
            try
            {
                PackageReader pr = new PackageReader();
                if (bSoftMode)
                    PackageReader.bEnableSoftMode = true;
                pr.OpenPackageFile(file_in);
                pr.SavePackageFile(file_out);
                byte[] file_in_content = File.ReadAllBytes(file_in);
                byte[] file_out_content = File.ReadAllBytes(file_out);
                if (System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(file_in_content, file_out_content))
                    bResult = true;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            File.Delete(file_out);
            return bResult;
        }

        [TestMethod]
        public void TestTexture2D()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Texture2D_1.uasset"));
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Texture2D_2.uasset"));
        }

        [TestMethod]
        public void TestSoundWave()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\SoundWave_1.uasset"));
        }

        [TestMethod]
        public void TestUserDefinedEnum()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\UserDefinedEnum_1.uasset"));
        }

        [TestMethod]
        public void TestDataTable()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\DataTable_1.uasset"));
        }

        [TestMethod]
        public void TestSomething()
        {
            //Assert.IsTrue(OpenSave("C:\\Program Files (x86)\\Steam\\SteamApps\\common\\The Park\\AtlanticIslandPark\\Content\\Maps\\AtlanticIslandPark\\AIP_Gameplay1.umap", true));
            //Assert.IsTrue(OpenSave("C:\\Program Files (x86)\\Steam\\SteamApps\\common\\The Park\\AtlanticIslandPark\\Content\\UI\\MainMenu\\MainMenu.uasset", true));
        }

        [TestMethod]
        public void TestFont()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Font_1.uasset"));
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Font_2.uasset"));
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Font_3.uasset"));
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Font_4.uasset"));
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Font_5.uasset"));
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Font_6.uasset"));
        }
    }
}
