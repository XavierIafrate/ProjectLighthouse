using SQLite;
using System;
using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model.Administration
{
    public class Machine : BaseObject, IObjectWithValidation
    {
        [PrimaryKey]
        public string Id { get; set; }
        
        [UpdateWatch]
        public string FullName { get; set; }
        
        [UpdateWatch]
        public string SerialNumber { get; set; }
        
        [UpdateWatch]
        public string Make { get; set; }
        
        [UpdateWatch]
        public string Model { get; set; }
        
        [UpdateWatch]
        public bool OutOfService { get; set; }

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; }

        public void ValidateAll()
        {
            ValidateProperty(nameof(Id));
            ValidateProperty(nameof(FullName));
        }

        public void ValidateProperty([CallerMemberName] string propertyName = "")
        {
            throw new NotImplementedException();
        }
    }
}
