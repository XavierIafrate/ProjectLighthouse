using ProjectLighthouse.Model.Orders;
using ProjectLighthouse.Model.Products;
using ProjectLighthouse.Model.Requests;
using ProjectLighthouse.Model.Scheduling;
using ProjectLighthouse.ViewModel.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Requests
{
    public class RequestsEngine
    {
        public static List<LatheManufactureOrderItem> GetRecommendedOrderItems(List<TurnedProduct> turnedProducts, List<RequestItem> items, TimeSpan? maxRuntime, int numberOfItems = 4)
        {

            maxRuntime ??= new(days: 3, hours: 0, minutes: 0, seconds: 0);
            List<LatheManufactureOrderItem> recommendation = new();

            int groupId = -1;
            int materialId = -1;
            double majorDiameter = -1;

            foreach (RequestItem item in items)
            {
                TurnedProduct requirement = turnedProducts.Find(x => x.Id == item.ItemId)
                ?? throw new Exception("Requests Engine: Requirement not found in product list");
                recommendation.Add(new(requirement, item.QuantityRequired, item.DateRequired ?? DateTime.Today));

                groupId = requirement.GroupId ?? -1;
                materialId = requirement.MaterialId ?? -1;
                majorDiameter = requirement.MajorDiameter;
            }

            if (groupId == -1 || materialId == -1)
            {
                throw new Exception("Requests Engine: no materialId or groupId");
            }


            TimeModel? timeModel = null;
            try
            {
                timeModel = OrderResourceHelper.GetCycleResponse(
                    turnedProducts
                        .Where(x => x.MaterialId == materialId && x.GroupId == groupId)
                        .ToList());
            }
            catch
            {
                timeModel = TimeModel.Default(majorDiameter);
            }

            turnedProducts = turnedProducts
                                .Where(x => !x.Retired
                                         && !x.IsSpecialPart
                                         && !recommendation.Any(r => r.Id != x.Id)
                                         && x.MaterialId == materialId
                                         && x.GroupId == groupId)
                                .OrderByDescending(x => x.GetRecommendedQuantity())
                                .ThenBy(x => x.QuantityInStock)
                                .ToList();

            // try prevent overproduction
            turnedProducts = turnedProducts.Where(x => !x.Overstocked()).ToList();


            foreach (TurnedProduct product in turnedProducts)
            {
                LatheManufactureOrderItem newItem = new(product);
                recommendation.Add(newItem);
            }

            List<LatheManufactureOrderItem> filteredItems = CapQuantitiesForTimeSpan(recommendation, (TimeSpan)maxRuntime, timeModel);

            filteredItems = filteredItems.Take(numberOfItems).ToList();

            return filteredItems;
        }

        public static List<LatheManufactureOrderItem> CapQuantitiesForTimeSpan(List<LatheManufactureOrderItem> items, TimeSpan maxRuntime, TimeModel timeModel)
        {
            List<LatheManufactureOrderItem> cleanedItems = new();
            int permittedSeconds = Convert.ToInt32(maxRuntime.TotalSeconds);
            items = items.OrderByDescending(x => x.RequiredQuantity).ToList();
            items.ForEach(x => x.TargetQuantity = Math.Max(x.TargetQuantity, GetMiniumumOrderQuantity(x)));

            int timeForRequired = items.Sum(x => x.GetTimeToMakeRequired());

            // Requirement will max out runtime
            if (timeForRequired > permittedSeconds)
            {
                cleanedItems = items.Where(x => x.RequiredQuantity > 0).ToList();
                cleanedItems.ForEach(x => x.TargetQuantity = x.RequiredQuantity);
                return cleanedItems;
            }

            int availableTime = permittedSeconds - timeForRequired;

            for (int i = 0; i < items.Count; i++)
            {
                LatheManufactureOrderItem item = items[i];

                int quantityNotRequired = item.TargetQuantity - item.RequiredQuantity;

                int timeToMakeToTarget = quantityNotRequired * (item.PreviousCycleTime ?? timeModel.At(item.MajorLength));

                if (availableTime < timeToMakeToTarget)
                {
                    item.TargetQuantity = Math.Max(
                                                item.RequiredQuantity,
                                                RoundQuantity(item.RequiredQuantity + (int)Math.Floor((double)availableTime / (item.PreviousCycleTime ?? timeModel.At(item.MajorLength))))
                                                );
                    cleanedItems.Add(item);
                    break;
                }

                availableTime -= timeToMakeToTarget;
                cleanedItems.Add(item);
            }

            cleanedItems = cleanedItems.Where(x => x.TargetQuantity > 0).ToList();

            return cleanedItems;
        }

        public static int GetMiniumumOrderQuantity(LatheManufactureOrderItem item)
        {
            return item.MajorDiameter switch
            {
                > 35 => 10,
                > 30 => 50,
                > 25 => 100,
                > 20 => 200,
                > 15 => 300,
                _ => 500,
            };
        }

        public static int GetMiniumumOrderQuantity(TurnedProduct item)
        {
            return item.MajorDiameter switch
            {
                > 35 => 10,
                > 30 => 50,
                > 25 => 100,
                > 20 => 200,
                > 15 => 300,
                _ => 500,
            };
        }

        public static int RoundQuantity(int originalNumber, bool roundUp = false)
        {
            int divisor = originalNumber switch
            {
                < 50 => 10,
                < 100 => 25,
                < 500 => 100,
                < 1000 => 200,
                < 2000 => 500,
                < 10000 => 1000,
                < 20000 => 2000,
                < 30000 => 3000,
                _ => 100,
            };

            if (originalNumber % divisor == 0)
            {
                return originalNumber;
            }

            if (roundUp)
            {
                originalNumber += divisor;
            }

            return originalNumber - originalNumber % divisor;
        }

        public static int EstimateCycleTime(double majorDiameter)
        {
            return majorDiameter switch
            {
                <= 5 => 90,
                <= 7 => 100,
                <= 10 => 120,
                <= 15 => 180,
                <= 20 => 240,
                <= 25 => 270,
                _ => 320
            };
        }
    }
}
