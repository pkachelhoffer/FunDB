namespace FunDBLib
{
    internal class IndexMaintainInstruction<TTableDefinition>
    {
        public TTableDefinition Row { get; set; }
        public RowAction RowAction { get; set; }
        public long Address { get; set; }

        public IndexMaintainInstruction(TTableDefinition row, RowAction rowAction, long address)
        {
            Row = row;
            RowAction = rowAction;
            Address = address;
        }
    }
}