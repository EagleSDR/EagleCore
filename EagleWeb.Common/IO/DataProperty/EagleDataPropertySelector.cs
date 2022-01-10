using System;
using System.Collections.Generic;
using System.Text;

namespace EagleWeb.Common.IO.DataProperty
{
    /// <summary>
    /// A wrapper for an EagleDataPropertyWritable that allows for selecting items by their ID
    /// </summary>
    public class EagleDataPropertySelector<T> : IEagleDataPropertyWritable<T> where T : IEagleIdProvider
    {
        public EagleDataPropertySelector(IEagleDataPropertyWritable<string> underlying, ICollection<T> collection)
        {
            this.underlying = underlying;
            this.collection = collection;
            underlying.OnWebSet += Underlying_OnWebSet;
        }

        private IEagleDataPropertyWritable<string> underlying;
        private T selected = default(T);
        private ICollection<T> collection;

        public string Id => underlying.Id;

        public T Value {
            get => selected;
            set
            {
                //If it's not null, make sure it exists in the collection
                if (value != null && !collection.Contains(value))
                    throw new Exception("Item doesn't exist in selector collection.");

                //Set
                selected = value;
                underlying.Value = selected == null ? null : selected.Id;
            }
        }

        public event EagleDataPropertyWritable_OnWebSetEventArgs<T> OnWebSet;

        public IEagleDataPropertyWritable<T> RequirePermission(string permission)
        {
            underlying.RequirePermission(permission);
            return this;
        }

        private bool TryFindItemById(string id, out T result)
        {
            result = default(T);
            foreach (T itm in collection)
            {
                if (itm.Id == id)
                {
                    result = itm;
                    return true;
                }
            }
            return false;
        }

        private void Underlying_OnWebSet(IEagleDataPropertyWritable<string> property, string value)
        {
            //Search for an item with this ID
            if (TryFindItemById(value, out T result))
            {
                OnWebSet?.Invoke(this, result);
            } else
            {
                throw new Exception($"\"{value}\" is not a valid item.");
            }
        }
    }
}
