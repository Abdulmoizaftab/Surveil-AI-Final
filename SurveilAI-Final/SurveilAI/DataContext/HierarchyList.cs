using System;
using System.Collections.Generic;
using System.Linq;

namespace SurveilAI.Models
{
    public static class HierarchyList
    {
        public static IEnumerable<TreeNode<Hierarchy, string>> GenrateTree(IEnumerable<Hierarchy> hierarchies)
        {
            //Generating Tree
            List<Hierarchy> parents = new List<Hierarchy>();
            parents = hierarchies.Where(a => !a.HierId.Contains('.')).OrderBy(a => a.HierId).ToList();
            var childs = hierarchies.Where(a => a.HierId.Contains('.')).OrderBy(a => a.HierId).ToList();
            int maxLvl = 0;
            foreach (var item in childs)
            {
                maxLvl = item.HierId.Split('.').Length;
            }

            foreach (var item in parents)
            {
                for (int i = 0; i < parents.Count; i++)
                {
                    if (item.HierId == parents[i].HierId)
                    {
                        //parents[i].ChildHier = childs.Where(a => a.HierId.StartsWith(item.HierId)).OrderBy(a => a.HierId).ToList();
                        parents[i].ChildHier = childs.Where(a => a.HierId.StartsWith(item.HierId + ".")).OrderBy(a => a.HierId).ToList();
                    }
                }
            }

            List<Hierarchy> HL = new List<Hierarchy>();
            string prevHierId = "";
            Hierarchy tempobj = new Hierarchy();
            foreach (var item in parents)
            {
                prevHierId = item.HierId;
                HL.Add(new Hierarchy { HierId = item.HierId, HierName = item.HierName, ParentId = "0", Hierlevel = item.Hierlevel });
                foreach (var chld in item.ChildHier)
                {

                    string parentId = chld.HierId;
                    var ps = parentId.LastIndexOf('.');
                    if (ps >= 0)
                        parentId = parentId.Substring(0, ps);
                    tempobj = new Hierarchy { HierId = chld.HierId, HierName = chld.HierName, ParentId = parentId, Hierlevel = chld.Hierlevel };
                    HL.Add(tempobj);


                    prevHierId = chld.HierId;
                }
            }
            var tree = HL.ToTree(item => item.HierId, item => item.ParentId);
            return tree;
        }
    }

    public sealed class TreeNode<T, TKey>
    {
        public T Item { get; set; }
        public TKey ParentId { get; set; }

        public IEnumerable<TreeNode<T, TKey>> Children { get; set; }
    }

    public static class EnumerableExtensions
    {
        public static IEnumerable<TreeNode<T, TKey>> ToTree<T, TKey>(
            this IList<T> collection,
            Func<T, TKey> itemIdSelector,
            Func<T, TKey> parentIdSelector)
        {
            var rootNodes = new List<TreeNode<T, TKey>>();
            var collectionHash = collection.ToLookup(parentIdSelector);

            //find root nodes
            var parentIds = collection.Select(parentIdSelector);
            var itemIds = collection.Select(itemIdSelector);
            var rootIds = parentIds.Except(itemIds);

            foreach (var rootId in rootIds)
            {
                rootNodes.AddRange(
                    GetTreeNodes(
                        itemIdSelector,
                        collectionHash,
                        rootId)
                    );
            }

            return rootNodes;
        }

        private static IEnumerable<TreeNode<T, TKey>> GetTreeNodes<T, TKey>(
            Func<T, TKey> itemIdSelector,
            ILookup<TKey, T> collectionHash,
            TKey parentId)
        {
            return collectionHash[parentId].Select(collectionItem => new TreeNode<T, TKey>
            {
                ParentId = parentId,
                Item = collectionItem,
                Children = GetTreeNodes(
                    itemIdSelector,
                    collectionHash,
                    itemIdSelector(collectionItem))
            });
        }
    }


}