using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles
{
    public class Ensemble
    {
        private readonly IDictionary<Guid, IEnsembleItem> itemsDictionary;
        
        public Ensemble() : this (new Dictionary<Guid, IEnsembleItem>())
        {
        }

        public Ensemble(IDictionary<Guid, IEnsembleItem> itemsDictionary) =>
            this.itemsDictionary = itemsDictionary;

        public bool TryGetById<T>(
                Guid id,
                out T result
            )
            where T : IEnsembleItem
        {
            bool bResult = false;
            result = default;

            if(this.itemsDictionary.TryGetValue(id, out IEnsembleItem tryResult))
            {
                result = (T) tryResult;
                bResult = true;
            }

            return bResult;
        }

        public IEnumerable<T> GetItems<T>()
            where T : IEnsembleItem 
            => this.itemsDictionary.Values.OfType<T>();

        public void AddReplace(IEnsembleItem item)
        {
            bool replacing = this.itemsDictionary.TryGetValue(item.Id, out IEnsembleItem oldItem);
            if (replacing)
                Ensemble.ValidateItemReplacementType(item, oldItem);

            Ensemble.AddReplaceCore(item, this.itemsDictionary, replacing);
        }

        private static void AddReplaceCore(IEnsembleItem item, IDictionary<Guid, IEnsembleItem> itemsDictionary, bool replacing)
        {
            if (replacing)
                itemsDictionary.Remove(item.Id);

            itemsDictionary.Add(item.Id, item);
        }

        public TEnsembleItem Obtain<TEnsembleItem>(TEnsembleItem value)
            where TEnsembleItem : IEnsembleItem
        {
            TEnsembleItem result = default;
            // if not found in ensemble
            if (!this.TryGetById(value.Id, out result))
            {
                this.AddReplace(value);
                result = value;
            }
            return result;
        }

        private static void ValidateItemReplacementType(IEnsembleItem newItem, IEnsembleItem oldItem)
        {
            AssertionConcern.AssertArgumentValid(
                t => t.GetType() == oldItem.GetType(),
                newItem,
                "Item to be replaced must be of the same type as the specified Item.",
                nameof(newItem)
                );
        }

        public void AddReplaceItems(Ensemble ensemble)
        {
            var commonItemsInNewDictionary = ensemble.itemsDictionary.Where(item => this.itemsDictionary.ContainsKey(item.Key)).ToList();
            // validate all common items in specified ensemble
            commonItemsInNewDictionary.ForEach(ci => Ensemble.ValidateItemReplacementType(ci.Value, this.itemsDictionary[ci.Key]));
            ensemble.itemsDictionary.ToList().ForEach(ni => Ensemble.AddReplaceCore(ni.Value, this.itemsDictionary, commonItemsInNewDictionary.Contains(ni)));
        }
    }
}
