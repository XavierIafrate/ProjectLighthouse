using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.Model
{
    public class TechnicalDrawing : ICloneable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public int Revision { get; set; }
        public string URL { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
        public bool IsArchetype { get; set; }
        public string Customer { get; set; }

        public string DrawingName { get; set; }
        public string DrawingStore { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }
        public DateTime RejectedDate { get; set;}
        public string RejectedBy { get; set; }  
        public string RejectionReason { get; set; }
        public string ApprovedBy { get; set; }  
        public DateTime ApprovedDate { get; set; }  
        public string ProductGroup { get; set; }
        public string ToolingGroup{ get; set; }
        public string MaterialConstraint { get; set; }
        public Type DrawingType { get; set; }
        public Amendment AmendmentType { get; set; }
        public string IssueDetails { get; set; }
        
        [Ignore]
        public bool IsCurrent { get; set; }

        public enum Amendment { A, B, C, D, E, F, G, H }
        public enum Type { Production, Research}


        [Ignore]
        public List<Note> Notes { get; set; }

        public static List<TechnicalDrawing> FindDrawings(List<TechnicalDrawing> drawings, List<LatheManufactureOrderItem> items, string group)
        {
            List<TechnicalDrawing> drawingsList = new();
            for (int i = 0; i < items.Count; i++)
            {
                TechnicalDrawing? d = FindDrawing(drawings, items[i], group);
                if(d != null)
                {
                    drawingsList.Add(d);
                }
            }


            return drawingsList.Distinct().ToList();
        }

        public static TechnicalDrawing FindDrawing(List<TechnicalDrawing> drawings, LatheManufactureOrderItem item, string group)
        {
            if (item.IsSpecialPart)
            {
                List<TechnicalDrawing> matches = drawings.Where(d => d.DrawingName == item.ProductName && !d.IsArchetype).OrderByDescending(d => d.Revision).ToList();
                if (matches.Count > 0)
                {
                    item.DrawingId = matches.First().Id;
                    return matches.First();
                }
                return null;
            }
            else
            {
                List<TechnicalDrawing> matches = drawings.Where(d => d.DrawingName == item.ProductName && !d.IsArchetype).OrderByDescending(d => d.Revision).ToList();
                if (matches.Count == 0)
                {
                    return GetBestDrawingForProduct(
                        family: item.ProductName[..5],
                        group: group,
                        //material: RequiredProduct.Material,
                        drawings: drawings);
                }
                else
                {
                    return matches.First();
                }
            }
        }


#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        private static TechnicalDrawing? GetBestDrawingForProduct(string family, string group, List<TechnicalDrawing> drawings)
#pragma warning restore CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
        {
            List<TechnicalDrawing> matches = drawings.Where(d => d.IsArchetype && d.ProductGroup == family && d.ToolingGroup == group).ToList(); // && d.MaterialConstraint == material
            if (matches.Count > 0)
            {
                matches = matches.OrderByDescending(d => d.Revision).ToList();
                return matches.First();
            }

            matches = drawings.Where(d => d.IsArchetype && d.ProductGroup == family && d.ToolingGroup == group).ToList();
            if (matches.Count > 0)
            {
                matches = matches.OrderByDescending(d => d.Revision).ToList();
                return matches.First();
            }

            matches = drawings.Where(d => d.IsArchetype && d.ProductGroup == family && string.IsNullOrEmpty(d.ToolingGroup)).ToList();
            if (matches.Count > 0)
            {
                matches = matches.OrderByDescending(d => d.Revision).ToList();
                return matches.First();
            }
            return null;
        }

        public object Clone()
        {
            return new TechnicalDrawing()
            {
                Id = Id,
                Revision = Revision,
                URL = URL,
                Created = Created,
                CreatedBy = CreatedBy,
                IsArchetype = IsArchetype,
                Customer = Customer,
                DrawingName = DrawingName,
                DrawingStore = DrawingStore,
                IsApproved = IsApproved,
                IsRejected = IsRejected,
                RejectedDate = RejectedDate,
                RejectionReason = RejectionReason,
                ApprovedBy = ApprovedBy,
                ApprovedDate = ApprovedDate,
                ProductGroup = ProductGroup,
                ToolingGroup = ToolingGroup,
                MaterialConstraint = MaterialConstraint,
                DrawingType = DrawingType,
                AmendmentType = AmendmentType,
                IssueDetails = IssueDetails,
                IsCurrent = IsCurrent,
            };
        }
    }
}
