using System.Runtime.CompilerServices;

namespace ProjectLighthouse.Model
{
    public interface IObjectWithValidation
    {
        public void ValidateAll();

        public void ValidateProperty([CallerMemberName] string propertyName = "");
    }
}
