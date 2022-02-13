using System.Timers;

namespace ProjectLighthouse.ViewModel
{
    public interface IRefreshableViewModel
    {
        public void Refresh(bool silent = false);
    }
}
