using FunDBLib.Attributes;

namespace FunDBLibTest
{
    public class TestTableIndex
    {
        [FDPrimaryKey]
        public int TestTableIndexID { get; set; }

        [FDColumnTextAttribute(20)]
        public string LineName { get; set; }
    }
}