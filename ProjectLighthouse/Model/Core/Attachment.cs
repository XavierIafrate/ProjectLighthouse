using SQLite;

namespace ProjectLighthouse.Model.Core
{
    public class Attachment
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string DocumentReference { get; set; }
        public string AttachmentStore { get; set; }
        public string Extension { get; set; }
        public string FileName { get; set; }
        public string Remark { get; set; }
    }
}
