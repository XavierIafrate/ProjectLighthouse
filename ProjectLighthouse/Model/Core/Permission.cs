using SQLite;
using System.Collections.Generic;

namespace ProjectLighthouse.Model.Core
{
    public class Permission : IAutoIncrementPrimaryKey
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [NotNull]
        public int UserId { get; set; }
        [NotNull]
        public PermissionType PermittedAction { get; set; }

        public static Dictionary<PermissionType, string> PermissionNames = new()
        {
             { PermissionType.RaiseRequest, "Raise Request" },
             { PermissionType.ApproveRequest, "Approve Requests" },
             { PermissionType.UpdateOrder, "Update Orders" },
             { PermissionType.EditOrder, "Edit Orders" },
             { PermissionType.CreateDelivery, "Create Deliveries" },
             { PermissionType.EditDelivery, "Edit Deliveries" },
             { PermissionType.CreateSpecial, "Add Special Parts" },
             { PermissionType.ApproveDrawings, "Approve Drawings" },
             { PermissionType.ModifyCalibration, "Modify Calibration" },
             { PermissionType.UpdateCalibration, "Update Calibration" },
             { PermissionType.IssueBar, "Issue Bar Stock" },
             { PermissionType.ConfigureMaintenance, "Configure Maintenance" },
             { PermissionType.ManageProjects, "Manage Development Projects" },
             { PermissionType.ModifyProjects, "Modify Development Projects" },
        };
    }
}
