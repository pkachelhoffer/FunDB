namespace FunDBLib
{
    public class HeaderData
    {
        public long LastRecordPosition { get; set; }

        public long FirstRecordPosition { get; set; }

        public HeaderData()
        {
            
        }

        public HeaderData(long lastRecordPosition, long firstRecordPosition)
        {
            LastRecordPosition = lastRecordPosition;
            FirstRecordPosition = firstRecordPosition;
        }
    }
}