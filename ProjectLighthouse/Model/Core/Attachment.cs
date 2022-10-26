using ProjectLighthouse.View.UserControls;
using SQLite;
using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Windows;

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
        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public Attachment()
        {

        }

        public Attachment(string docReference)
        {
            DocumentReference = docReference;
            CreatedAt = DateTime.Now;
            CreatedBy = App.CurrentUser.UserName;
        }

        public bool CopyToStore(string FilePath)
        {
            AttachmentStore = $@"lib\{Path.GetRandomFileName()}";
            Extension = Path.GetExtension(FilePath);
            FileName = Path.GetFileNameWithoutExtension(FilePath);

            try
            {
                File.Copy(FilePath, $"{App.ROOT_PATH}{AttachmentStore}");
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
    }
}
