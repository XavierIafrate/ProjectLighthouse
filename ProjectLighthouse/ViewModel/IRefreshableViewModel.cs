namespace ProjectLighthouse.ViewModel
{
    public interface IRefreshableViewModel
    {
        bool StopRefresh { get; set; }
        public void Refresh();
    }
}
