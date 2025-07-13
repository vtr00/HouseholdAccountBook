namespace HouseholdAccountBook.Dto.KHDbTable
{
    public class CbtNoteDto
    {
        public CbtNoteDto() { }

        public int NOTE_ID { get; set; }
        public string NOTE_NAME { get; set; }
        public int ITEM_ID { get; set; }
        public bool DEL_FLG { get; set; }
    }
}
