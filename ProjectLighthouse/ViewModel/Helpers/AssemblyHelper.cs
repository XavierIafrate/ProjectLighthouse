using ProjectLighthouse.Model;
using ProjectLighthouse.Model.Assembly;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ProjectLighthouse.ViewModel.Helpers
{
    public class AssemblyHelper
    {
        public static List<AssemblyItemExpansion> ExplodeBillOfMaterials(AssemblyItem TargetProduct, int QuantityRequired, List<BillOfMaterialsItem> BOMItems, List<AssemblyItem> Products)
        {
            List<AssemblyItemExpansion> newOrderItems = new();
            newOrderItems.Add(new AssemblyItemExpansion() { Item = TargetProduct, Checked = false, Quantity = QuantityRequired });

            bool complete = false;
            while (!complete)
            {
                List<AssemblyItemExpansion> tmpItems = new();
                foreach (AssemblyItemExpansion x in newOrderItems)
                {
                    complete = true;
                    if (!x.Checked)
                    {
                        complete = false;
                        List<AssemblyItemExpansion> children = AssemblyHelper.GetChildren(x.Item, x.Quantity, BOMItems, Products);
                        tmpItems.AddRange(children);
                        x.Checked = true;
                    };
                }
                newOrderItems.AddRange(tmpItems);
            }

            return newOrderItems;
        }


        private static List<AssemblyItemExpansion> GetChildren(AssemblyItem parent, int parent_quantity, List<BillOfMaterialsItem> BOMItems, List<AssemblyItem> Products)
        {
            List<BillOfMaterialsItem> items = BOMItems.Where(n => n.BOMID == parent.BillOfMaterials).ToList();
            List<AssemblyItemExpansion> result = new();

            if (items.Count > 0)
            {
                foreach (BillOfMaterialsItem i in items)
                {
                    result.Add(new() { Checked = false, Item = Products.SingleOrDefault(n => n.ProductNumber == i.ComponentItem), Quantity = i.Quantity * parent_quantity, Parent = parent.ProductNumber });
                    if (result.Last().Item == null)
                        Debug.WriteLine($"{i.ComponentItem} not found");
                }
            }

            return result;
        }

        public static List<Assembly> CreateAssembliesFromItemList(List<AssemblyOrderItem> items)
        {
            List<Assembly> results = new();

            AssemblyOrderItem root = items.Where(n => string.IsNullOrEmpty(n.ChildOf)).Single();
            items.Remove(root);
            List<AssemblyOrderItem> children = items.Where(n => n.ChildOf == root.ProductName).ToList();

            results.Add(new()
            {
                Parent = root,
                Children = children
            });

            bool complete = false;
            while (!complete)
            {
                List<Assembly> tmp = new();
                complete = true;

                foreach (Assembly a in results)
                {
                    foreach (AssemblyOrderItem child in a.Children)
                    {
                        List<AssemblyOrderItem> NextSeparation = items.Where(x => x.ChildOf == child.ProductName).ToList();
                        if (NextSeparation.Count != 0)
                        {
                            tmp.Add(new()
                            {
                                Parent = child,
                                Children = NextSeparation
                            });
                            foreach (AssemblyOrderItem c in NextSeparation)
                                items.Remove(c);
                        }
                        else
                        {
                            items.Remove(child);
                        }
                    }
                }

                results.AddRange(tmp);
                complete = items.Count == 0;
                Debug.WriteLine($"{items.Count} items remaining in explosion");
            }

            return results;
        }
    }
}
