namespace FunDBLib
{
    public class HeaderData
    {
        public long FirstRecordPosition { get; set; }

        public long LastRecordPosition { get; set; }

        public HeaderData()
        {
            
        }

        public HeaderData(long firstRecordPosition, long lastRecordPosition)
        {
            FirstRecordPosition = firstRecordPosition;
            LastRecordPosition = lastRecordPosition;
        }
    }
}