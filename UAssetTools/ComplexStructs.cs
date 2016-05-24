using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UAssetTools
{
    public class FCompositeFont : UStruct
    {
        public UProperty<FTypeface> DefaultTypeface;
        //public UProperty<List<FCompositeSubFont>> SubTypefaces;

        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);
        }
    }

    public class FTypeface : UStruct
    {
        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);
        }
    }

    public class FFontData : UStruct
    {
        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);
        }
    }

    public class FFontImportOptionsData : UStruct
    {
        public override void Serialize(FArchive ar)
        {
            base.Serialize(ar);
        }
    }
}
