using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Programs
{
    public class NcProgram : BaseObject, IObjectWithValidation, ICloneable, IAutoIncrementPrimaryKey
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

        private string? notepad;
        public string? Notepad
        {
            get { return notepad; }
            set
            {
                notepad = value;
                OnPropertyChanged();
            }
        }

        public bool Inactive { get; set; }
        private string? schedulingProgramName;

        public string? SchedulingProgramName
        {
            get { return schedulingProgramName; }
            set
            {
                schedulingProgramName = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }


        private string? products;
        public string? Products
        {
            get { return products; }
            set
            {
                products = value;
                OnPropertyChanged();
            }
        }

        public List<string> ProductStringIds
        {
            get
            {
                if (Products is null) return new();
                return Products.Split(";").ToList();
            }
        }

        private string? groups;
        public string? Groups
        {
            get { return groups; }
            set
            {
                groups = value;
                OnPropertyChanged();
            }
        }

        public List<string> GroupStringIds
        {
            get
            {
                if (Groups is null) return new();
                return Groups.Split(";").ToList();
            }
        }

        private string? materials;
        public string? Materials
        {
            get { return materials; }
            set
            {
                materials = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public List<string> MaterialsList
        {
            get
            {
                if (Materials is null) return new();
                return Materials.Split(";").OrderBy(x => x).ToList();
            }
            set
            {
                if (value.Count > 0)
                {
                    Materials = string.Join(";", value);
                    OnPropertyChanged();
                    return;
                }
                Materials = null;
                OnPropertyChanged();
            }
        }

        private string? machines;
        public string? Machines
        {
            get { return machines; }
            set
            {
                machines = value;
                OnPropertyChanged();
            }
        }

        [Ignore]
        public List<string> MachinesList
        {
            get
            {
                if (Machines is null) return new();
                return Machines.Split(";").OrderBy(x => x).ToList();
            }
            set
            {
                if (value.Count > 0)
                {
                    Machines = string.Join(";", value);
                    OnPropertyChanged();
                    return;
                }
                Machines = null;
                OnPropertyChanged();
            }
        }

        private string? tags;
        public string? Tags
        {
            get { return tags; }
            set
            {
                tags = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        public string SearchableTags
        {
            get
            {
                if (Tags is null) return "";
                return Tags?.Replace(" ", "").ToLowerInvariant();
            }
        }

        [Ignore]
        public List<string> TagsList
        {
            get
            {
                if (Tags is null) return new();
                return Tags.Split(";").OrderBy(x => x).ToList();
            }
            set
            {
                if (value.Count > 0)
                {
                    Tags = string.Join(";", value);
                    OnPropertyChanged();
                    return;
                }
                Tags = null;
                OnPropertyChanged();
            }
        }


        private DateTime? fileLastModified = null;
        [Ignore]
        public DateTime? FileLastModified
        {
            get { return fileLastModified; }
            set { fileLastModified = value; OnPropertyChanged(); }
        }


        private bool? fileExists = null;
        [Ignore]
        public bool? FileExists
        {
            get { return fileExists; }
            set { fileExists = value; OnPropertyChanged(); }
        }


        public string Path
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Name)) return null;

                return $"{App.Constants.BaseProgramPath}{Name}.PRG";
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
            ValidateProperty(nameof(SchedulingProgramName));
        }

        public void ValidateTags()
        {
            ValidateProperty(nameof(TagsList));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (propertyName == nameof(Name))
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Name))
                {
                    AddError(propertyName, "Name cannot be empty");
                    return;
                }

                if (!ValidationHelper.StringIsNumbers(Name))
                {
                    AddError(propertyName, "Name must only contain numeric characters");
                }

                return;
            }
            else if (propertyName == nameof(SchedulingProgramName))
            {
                ClearErrors(propertyName);

                if (SchedulingProgramName == null) return;

                if (!ValidationHelper.StringIsNumbers(SchedulingProgramName))
                {
                    AddError(propertyName, "Scheduling Program Name must only contain numeric characters");
                }

                return;
            }
            else if (propertyName == nameof(Tags) || propertyName == nameof(TagsList))
            {
                ClearErrors(propertyName);

                List<string> tags = TagsList.Distinct().ToList();
                tags.ForEach(x => x = x.ToLower());

                if (tags.Count != TagsList.Count)
                {
                    AddError(propertyName, "Tags contains duplicates");
                }

                if (tags.Count > 10)
                {
                    AddError(propertyName, "Maximum 10 tags");
                }

                if (tags.Any(x => x.Length > 24))
                {
                    AddError(propertyName, "Maximum tag length is 24 characters");
                }

                if (tags.Any(x => x.Trim() != x))
                {
                    AddError(propertyName, "One or more tags has leading or trailing whitespace");
                }

                if (tags.Any(x => string.IsNullOrWhiteSpace(x)))
                {
                    AddError(propertyName, "One or more tags is empty");
                }

                return;
            }

            throw new NotImplementedException();
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<NcProgram>(serialised);
        }

        [Ignore]
        public MonacoProgram ProgramContent { get; set; }
        public class MonacoProgram
        {
            public string Header { get; set; }
            public string OriginalDollarOneCode { get; set; }
            public string DollarOneCode { get; set; }
            public string OriginalDollarTwoCode { get; set; }
            public string DollarTwoCode { get; set; }
            public string DollarZeroCode { get; set; }

            public string Pack()
            {
                return $"$1\n{DollarOneCode}\n\n$2\n{DollarTwoCode}\n\n$0\n{DollarZeroCode}";
            }
        }
    }
}
