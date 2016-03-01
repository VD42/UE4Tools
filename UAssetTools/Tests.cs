using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAssetTools
{
    [TestClass]
    public class Tests
    {
        public bool OpenSave(string file_in)
        {
            bool bResult = false;
            string file_out = System.IO.Path.GetTempFileName();
            try
            {
                PackageReader pr = new PackageReader();
                pr.OpenPackageFile(file_in);
                pr.SavePackageFile(file_out);
                byte[] file_in_content = System.IO.File.ReadAllBytes(file_in);
                byte[] file_out_content = System.IO.File.ReadAllBytes(file_out);
                if (System.Collections.StructuralComparisons.StructuralEqualityComparer.Equals(file_in_content, file_out_content))
                    bResult = true;
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            System.IO.File.Delete(file_out);
            return bResult;
        }

        [TestMethod]
        public void TestTexture2D()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\Texture2D_1.uasset"));
        }

        [TestMethod]
        public void TestDataTable()
        {
            Assert.IsTrue(OpenSave("..\\..\\Tests\\DataTable_1.uasset"));
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
    }
}
