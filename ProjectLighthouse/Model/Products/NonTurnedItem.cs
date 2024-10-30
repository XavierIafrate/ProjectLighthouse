using ProjectLighthouse.Model.Core;
using SQLite;

namespace ProjectLighthouse.Model.Products
{
    public class NonTurnedItem : BaseObject, IAutoIncrementPrimaryKey
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }

        private string? exportProductName;

        [Unique]
        [Import("Delivery Name")]
        public string? ExportProductName
        {
            get { return exportProductName; }
            set
            {
                exportProductName = value;
                OnPropertyChanged();
            }
        }


        private bool isSyncing;
        [Import("Is Syncing")]
        public bool IsSyncing
        {
            get { return isSyncing; }
            set
            {
                if (isSyncing == value) return;

                isSyncing = value;
                OnPropertyChanged();
            }
        }

        public string Description { get; set; }
        public int CycleTime { get; set; }
    }
}
