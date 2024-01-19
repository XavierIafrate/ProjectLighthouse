using SQLite;
using System;
using System.Diagnostics;

namespace ProjectLighthouse.Model.Core
{
    public class ApplicationVariable
    {
        [PrimaryKey]
        public string Id { get; set; }

        [NotNull]
        public string DataType { get; set; }

        public string StringValue { get; set; }

        [Ignore]
        public object Data
        {
            get
            {
                return DataType switch
                {
                    "null" => null,
                    "DateTime" => DateTime.Parse(StringValue),
                    "string" => StringValue,
                    "int" => int.Parse(StringValue),
                    "double" => double.Parse(StringValue),
                    "bool" => bool.Parse(StringValue),
                    _ => "Unknown Data"
                };
            }
            set
            {
                if(value == null)
                {
                    DataType = "null";
                    StringValue = null;
                    return;
                }

                DataType = value.GetType().Name;
                Debug.WriteLine(DataType);
                StringValue = value.ToString();
            }
        }

    }
}
