namespace ProjectLighthouse.Model.Reporting
{
    public interface IReport
    {
        void Export(string path, PerformanceReportData data);
        void Export(string path, OrderPrintoutData data);
    }
}
