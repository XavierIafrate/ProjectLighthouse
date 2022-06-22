using System;
using System.Reflection;

namespace ProjectLighthouse.Model.Reporting.Internal
{
    // This some wizard shit right here

    public class LogoImage
    {
        private const string LogoResourceName = "Resources.Lighthouse_Mono_L_Embedded.png";
        public string GetMigraDocFileName()
        {
            return ConvertToMigraDocFileName(LoadResource(LogoResourceName));
        }

        private string ConvertToMigraDocFileName(byte[] image)
        {
            return $"base64:{Convert.ToBase64String(image)}";
        }

        private byte[] LoadResource(string name)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string fullName = $"{assembly.GetName().Name}.{name}";
            using System.IO.Stream stream = assembly.GetManifestResourceStream(fullName);
            if (stream == null)
            {
                throw new ArgumentException($"No resource with name {name}");
            }

            int count = (int)stream.Length;
            byte[] data = new byte[count];
            stream.Read(data, 0, count);
            return data;
        }
    }
}
