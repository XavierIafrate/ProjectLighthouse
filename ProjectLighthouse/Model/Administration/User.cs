﻿using ProjectLighthouse.Model.Core;
using ProjectLighthouse.ViewModel.Helpers;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model.Administration
{
    public class User
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }

        [CsvHelper.Configuration.Attributes.Ignore]
        public string Password { get; set; }
        public UserRole Role { get; set; }

        public string EmailAddress { get; set; }
        public string computerUsername { get; set; }
        public DateTime LastLogin { get; set; }
        public bool IsBlocked { get; set; }
        public string DefaultView { get; set; }
        public bool ReceivesNotifications { get; set; }
        public string Locale { get; set; }
        public double? DefaultMenuWidth { get; set; }
        public string? Emoji { get; set; }


        public static readonly string[] Emojis = new string[] { "👾", "👹", "💩", "🤖", "👽", "😎", "🥳", "😇", "🦕", "🦎", "🐶", "🐭", "🐷", "🐸", "🐢", "🦞", "🦥", "🦔", "🌞", "🤹", "💎" };

        [Ignore]
        public List<Permission> UserPermissions { get; set; } = new(); 

        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }

        public bool HasPermission(PermissionType action)
        {
            return ExplicitGrantsPermission(action) || RoleGrantsPermission(action);
        }

        public bool ExplicitGrantsPermission(PermissionType action)
        {
            return UserPermissions.Any(x => x.PermittedAction == action);
        }

        public bool RoleGrantsPermission(PermissionType action)
        {
            return Role switch
            {
                UserRole.Viewer => false,
                UserRole.Purchasing => action is PermissionType.RaiseRequest or PermissionType.UpdateOrder,
                UserRole.Production => action is PermissionType.UpdateOrder or PermissionType.CreateDelivery,
                UserRole.Scheduling => action is PermissionType.RaiseRequest or PermissionType.ApproveRequest or PermissionType.EditOrder or PermissionType.UpdateOrder or PermissionType.CreateDelivery,
                UserRole.Administrator => true,
                _ => false
            };
        }

        public bool PermissionInherited(PermissionType action)
        {
            return RoleGrantsPermission(action);
        }

        public override string ToString() => GetFullName();

        public static void PostDefaultMenuWidth(string username, double width)
        {
            string command = $"UPDATE {nameof(User)} SET DefaultMenuWidth={width} WHERE UserName='{username}'";
            DatabaseHelper.ExecuteCommand(command);
        }
    }
}
