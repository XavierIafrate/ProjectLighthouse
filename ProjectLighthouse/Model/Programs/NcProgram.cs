using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Programs
{
    public class NcProgram : BaseObject, IObjectWithValidation
    {
        [AutoIncrement, PrimaryKey]
        public int Id { get; set; }

        private string name;
        [Unique]
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        [Unique]
        public string FilePath { get; set; }
        public string? Description { get; set; }

        private string tags = "";

        public string Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }


        public List<string> TagsList
        {
            get
            {
                return Tags.Split(";").ToList();
            }
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Name) ? Id.ToString("0") : Name;
        }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(Tags));
            ValidateProperty(nameof(TagsList));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Name))
            {
                ClearErrors(propertyName);

                if (!ValidationHelper.StringIsNumbers(Name))
                {
                    AddError(propertyName, "Name must only contain numeric characters");
                }

                return;
            }
            else if (propertyName == nameof(Tags) || propertyName == nameof(TagsList))
            {
                ClearErrors(propertyName);

                List<string> tags = TagsList;
                tags.ForEach(x => x = x.ToLower());
                if (tags.Distinct().Count() != tags.Count)
                {
                    AddError(propertyName, "Tags contains duplicates");
                }

                if (tags.Distinct().Count() > 10)
                {
                    AddError(propertyName, "Maximum 10 tags");
                }

                return;
            }

            throw new System.NotImplementedException();
        }
    }
}
