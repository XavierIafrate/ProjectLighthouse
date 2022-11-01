using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using PdfSharp.Pdf;
using ProjectLighthouse;
using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.ViewModel.Helpers;
using SpreadsheetLight;
using SpreadsheetLight.Drawing;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.IO;
using System.Windows.Media.Imaging;

namespace ViewModel.Helpers
{
    public class ExcelHelper
    {
        public static List<Product> ProductGroups;
        public static void CreateProgrammingPlanner(List<LatheManufactureOrder> orders)
        {
            ProductGroups = DatabaseHelper.Read<Product>();
            SLDocument sl = new();

            AddProgramPlannerHeader(sl);

            AddProgramPlannerContent(sl, orders);

            sl.SaveAs(@"C:\Users\x.iafrate\Desktop\Test_FastExcel.xlsx");
        }

        public static SLDocument AddProgramPlannerHeader(SLDocument doc)
        {
            string[] header = new string[] { "", "Order #", "R&D", "Product Group", "Part Description", "Machine", "Start Date", "Programmer", "Comments", "Program ID" };

            for (int i = 0; i < header.Length; i++)
            {
                AddHeaderCell(doc, i + 1, header[i]);
            }

            return doc;
        }

        public static SLDocument AddProgramPlannerContent(SLDocument doc, List<LatheManufactureOrder> orders)
        {
            doc.SetColumnWidth(1, 9);
            doc.SetColumnWidth(5, 40);
            doc.SetColumnWidth(9, 40);

            for (int i = 0; i < orders.Count; i++)
            {
                AddOrderInfo(doc, i+2, orders[i]);
            }

            return doc;
        }

        public static SLDocument AddOrderInfo(SLDocument doc, int row, LatheManufactureOrder order)
        {
            SLStyle style = doc.CreateStyle();
            style.SetFont("Consolas", 10);
            style.SetVerticalAlignment(VerticalAlignmentValues.Center);
            style.Border.BottomBorder.BorderStyle = BorderStyleValues.Medium;
            doc.SetCellStyle(row, 2, row, 10, style);

            style.Font.FontColor = System.Drawing.Color.FromName("DodgerBlue");

            doc.SetRowHeight(row, 50);

            doc.SetCellValue(row, 2, order.Name);

            doc.SetCellValue(row, 3, order.IsResearch ? "Yes" : "No");
            if (order.IsResearch)
            {
                doc.SetCellStyle(row, 3, style);
            }
            Product? productInfo = GetProductFromOrder(order);

            doc.SetCellValue(row, 4, order.ToolingGroup);
            
            if (productInfo != null)
            {
                doc.SetCellValue(row, 5, productInfo.Description);
                //SLPicture pic = new(productInfo.LocalRenderPath);
                float newResolution;
                using (var imageStream = File.OpenRead(productInfo.LocalRenderPath))
                {
                    const double targetHeight = 60; 
                    var decoder = BitmapDecoder.Create(imageStream, BitmapCreateOptions.IgnoreColorProfile,
                        BitmapCacheOption.Default);
                    var height = decoder.Frames[0].PixelHeight;
                    var res = decoder.Frames[0].DpiY;
                    newResolution = Convert.ToSingle(targetHeight / height * res);

                }


                SLPicture pic = new(productInfo.LocalRenderPath, newResolution, newResolution);
                pic.SetPosition(row-1, 0);
                doc.InsertPicture(pic);
            }

            doc.SetCellValue(row, 6, order.AllocatedMachine);

            if (order.StartDate != DateTime.MinValue)
            {
                doc.SetCellValue(row, 7, order.StartDate.ToString("dd-MMM"));
            }


            return doc;
        }

        private static Product GetProductFromOrder(LatheManufactureOrder order)
        {
            if (string.IsNullOrEmpty(order.ToolingGroup))
            {
                return null;
            }

            return ProductGroups.Find(x => x.Name == order.ToolingGroup[..5]);
        }

        public static SLDocument AddHeaderCell(SLDocument sl, int colNum, string cellContent)
        {
            SLStyle style = sl.CreateStyle();
            style.SetFont("Consolas", 12);
            style.Font.Bold = true;

            style.Border.BottomBorder.BorderStyle = BorderStyleValues.Thick;

            sl.SetCellStyle(1, colNum, style);
            sl.SetCellValue(1, colNum, cellContent);
            sl.AutoFitColumn(colNum);
            return sl;
        }
    }
}
