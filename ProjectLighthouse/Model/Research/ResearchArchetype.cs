using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Research
{
    public class ResearchArchetype : BaseObject, IAutoIncrementPrimaryKey, IObjectWithValidation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        private string name;

        public string Name
        {
            get { return name; }
            set { 
                name = value; 
                ValidateProperty(); 
                OnPropertyChanged(); 
            }
        }

        public int ProjectId { get; set; }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Name));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Name))
            {
                ClearErrors(nameof(Name));
                if (string.IsNullOrWhiteSpace(Name))
                {
                    AddError(nameof(Name), "Name cannot be empty");
                    return;
                }

                if (!ValidationHelper.IsValidProductName(Name))
                {
                    AddError(nameof(Name), "Name must be a valid product name");
                }

                if (Name.Length > 25)
                {
                    AddError(nameof(Name), "Name must be less than or equal to 25 charaters long");
                }
            }
        }
    }
}
