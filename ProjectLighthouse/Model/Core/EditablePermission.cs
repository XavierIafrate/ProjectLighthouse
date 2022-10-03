using ProjectLighthouse.Model.Core;

namespace Model.Core
{
    public class EditablePermission
    {
        public PermissionType Action { get; set; }
        public bool UserHasPermission { get; set; }
        public string DisplayText { get; set; }
        public bool Inherited { get; set; }

        public EditablePermission(PermissionType action, bool userHasPermission, bool inherited)
        {
            Action = action;
            UserHasPermission = userHasPermission;
            DisplayText = Permission.PermissionNames[action];
            Inherited = inherited;
        }
    }
}
