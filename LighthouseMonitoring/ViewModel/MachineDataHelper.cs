using LighthouseMonitoring.Model;
using ProjectLighthouse.Model.Administration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace LighthouseMonitoring.ViewModel
{
    public static class MachineDataHelper
    {
        private static string IP_ADDRESS;

        public static void Initialise()
        {
            IP_ADDRESS = Settings1.Default.MTConnectHost;
        }

        public static MachineData GetMachineData(Lathe lathe)
        {
            if (string.IsNullOrEmpty(lathe.ControllerReference))
            {
                throw new ArgumentNullException($"{lathe.Id} - Controller reference is null");
            }


            string url = IP_ADDRESS + lathe.ControllerReference + "/current";
            MachineData result = new(lathe);

            url = @"H:\Telecoms & Computers\C01.xml";
            using XmlTextReader reader = new(url);

            MtConnectData parsedData = ParseXmlData(reader);



            result.PushData(parsedData);

            return result;


        }

        private static MtConnectData ParseXmlData(XmlReader reader)
        {
            MtConnectData result = new();

            Dictionary<MtConnectData.FromXml, PropertyInfo> map = MtConnectData.GetPropertyMap();


            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:

                        if (reader.Name == "Header")
                        {
                            result.CreationDate = DateTime.Parse(reader.GetAttribute("creationTime")!);
                            break;
                        }

                        string? name = reader.GetAttribute("name");

                        if (name is not null)
                        {
                            Debug.WriteLine(name);

                            DateTime timeStamp;
                            string strValue;


                            if (name == "system") // system messages, errors etc... 
                            {
                                timeStamp = DateTime.Parse(reader.GetAttribute("timestamp")!);
                                string nativeCode = reader.GetAttribute("nativeCode") ?? "";
                                string type = reader.Name;
                                reader.Read();
                                strValue = reader.Value.ToString();

                                result.SystemMessages.Add(new(nativeCode, timeStamp, type, strValue));

                                break;
                            }

                            MtConnectData.FromXml propAttr;
                            try
                            {
                                propAttr = map.Keys.First(x => x.Name == name);
                            }
                            catch
                            {
                                break;
                            }


                            timeStamp = DateTime.Parse(reader.GetAttribute("timestamp")!);
                            reader.Read();
                            strValue = reader.Value.ToString();

                            dynamic val;

                            try
                            {
                                val = Convert.ChangeType(strValue, propAttr.ValueType);
                            }
                            catch (InvalidCastException)
                            {
                                val = GetDefault(propAttr.ValueType)!;
                            }

                            MtConnectData.XmlElement data = new(name, timeStamp, val);

                            result.GetType().GetProperty(map[propAttr].Name)!.SetValue(result, data);

                            break;


                        }
                        break;

                }
            }

            return result;
        }

        private static object? GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        private static T ConvertObject<T>(object input)
        {
            return (T)Convert.ChangeType(input, typeof(T));
        }


        //private static Dictionary<string, object> ParseXmlElement(XmlReader reader, Dictionary<string, object> result)
        //{
        //    while (reader.MoveToNextAttribute())
        //    {
        //        //dynamic value = 
        //        switch (reader.Name)
        //        {
        //            case "creationTime":
        //                //DateTime.Parse(reader.Value, out DateTime date)
        //                //                    result.DataTime = date;

        //                break;

        //            case "name":
        //                //lastElement = reader.Value;
        //                break;
        //        };
        //    }
        //}

    }
}
