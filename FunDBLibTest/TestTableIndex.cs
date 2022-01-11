using FunDBLib.Attributes;

namespace FunDBLibTest
{
    public class TestTableIndex
    {
        [FDPrimaryKey]
        public int TestTableIndexID { get; set; }

        public string LineName { get; set; }
    }
}