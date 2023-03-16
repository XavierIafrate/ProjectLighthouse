using ProjectLighthouse.Model.Drawings;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Products
{
    public class Product : BaseObject, IObjectWithValidation, ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private string name;
        [Unique, NotNull]
        public string  Name
        { 
            get { return name; } 
            set
            {
                name = value;
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string description;
        [NotNull]
        public string Description
        {
            get { return description; }
            set 
            { 
                description = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string webUrl = "";
        public string WebUrl
        {
            get { return webUrl; }
            set 
            { 
                webUrl = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string imageUrl = "";
        public string ImageUrl
        {
            get { return imageUrl; }
            set 
            { 
                imageUrl = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }

        private string webImageUrl = "";
        public string WebImageUrl
        {
            get { return webImageUrl; }
            set 
            { 
                webImageUrl = value; 
                ValidateProperty();
                OnPropertyChanged();
            }
        }


        [Ignore]
        public string LocalRenderPath
        {
            get { 
                return (string.IsNullOrEmpty(ImageUrl) || string.IsNullOrEmpty(App.AppDataDirectory)) 
                    ? null 
                    : $@"{App.AppDataDirectory}lib\renders\{ImageUrl}"; 
            }
        }

        [Ignore]
        public List<ProductGroup> Archetypes { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public object Clone()
        {
            string serialised = Newtonsoft.Json.JsonConvert.SerializeObject(this);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Product>(serialised);
        }

        public void ValidateAll()
        {
            if (Id == -1) // Unassigned
            {
                return;
            }

            ValidateProperty(nameof(Name));
            ValidateProperty(nameof(Description));
            ValidateProperty(nameof(WebUrl));
            ValidateProperty(nameof(WebImageUrl));
            ValidateProperty(nameof(ImageUrl));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            if (Id == -1) // Unassigned
            {
                return;
            }

            if (propertyName == "Name")
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Name))
                {
                    AddError(propertyName, "Name cannot be empty");
                    return;
                }

                if(!ValidationHelper.IsValidProductName(Name))
                {
                    string invalidChars = ValidationHelper.GetInvalidProductNameChars(Name);
                    AddError(propertyName, $"Name must be a valid product code (invalid characters: {invalidChars})");
                    return;
                }

                return;
            }
            else if (propertyName == "Description")
            {
                ClearErrors(propertyName);

                if (string.IsNullOrWhiteSpace(Description)) 
                {
                    AddError(propertyName, "Description cannot be empty");
                    return;
                }

                if (Description.Trim() != Description)
                {
                    AddError(propertyName, "Description has leading or trailing whitespace");
                    return;
                }

                if (Description.Length > 30)
                {
                    AddError(propertyName, $"Description has a max length of 30 characters (currently {Description.Length})");
                    return;
                }

                return;
            }
            else if (propertyName == "WebUrl")
            {
                ClearErrors(propertyName);

                // Allow empty value
                if (string.IsNullOrEmpty(WebUrl))
                {
                    return;
                }

                bool result = Uri.TryCreate(WebUrl, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    AddError(propertyName, "Failed to assert the address as a valid HTTP/HTTPS URI");
                    return;
                }

                return;
            }
            else if (propertyName == "WebImageUrl")
            {
                ClearErrors(propertyName);

                // Allow empty value
                if (string.IsNullOrEmpty(WebImageUrl))
                {
                    return;
                }

                bool result = Uri.TryCreate(WebImageUrl, UriKind.Absolute, out Uri uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);

                if (!result)
                {
                    AddError(propertyName, "Failed to assert the address as a valid HTTP/HTTPS URI");
                    return;
                }

                return;
            }
            else if (propertyName == "ImageUrl")
            {
                ClearErrors(propertyName);

                return;
            }

            throw new NotImplementedException();
        }
    }
}
