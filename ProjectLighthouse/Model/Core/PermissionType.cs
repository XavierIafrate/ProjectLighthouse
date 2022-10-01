namespace ProjectLighthouse.Model.Core
{
    /*
    * 
    * Enum Allocations:
    * 
    * 1xx = Request Permission
    * 2xx = Orders & Delivery Permission
    * 3xx = Drawings Permission
    * 4xx = Quality Permissions
    * 
    */

    public enum PermissionType
    {
        RaiseRequest = 101,
        ApproveRequest = 102,
        CreateSpecial = 103,
        UpdateOrder = 201,
        EditOrder = 202,
        IssueBar = 203,
        CreateDelivery = 204,
        EditDelivery = 205,
        ApproveDrawings = 301,
        ModifyCalibration = 401,
        UpdateCalibration = 402,
    }
}
