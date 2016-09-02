using System;
using System.Collections.Generic;
using System.Linq;

namespace Lithnet.GoogleApps.ManagedObjects
{
    public static class ManagedObjectExtensions
    {
        public static T GetOrCreatePrimary<T>(this IList<T> items) where T : class, IPrimaryCandidateObject, new()
        {
            T newItem;

            if (items == null)
            {
                newItem = new T() { Primary = true };
                return newItem;
            }

            T existingItem = items.FirstOrDefault(t => t.IsPrimary);

            if (existingItem == null)
            {
                newItem = new T() { Primary = true };
                return newItem;
            }
            else
            {
                return existingItem;
            }
        }

        public static T GetObjectOfTypeOrDefault<T>(this IList<T> items, string type) where T : CustomTypeObject
        {
            if (items == null)
            {
                return default(T);
            }

            return items.FirstOrDefault(t => t.Type == type);
        }

        public static T GetObjectOfTypeOrDefault<T>(this IList<T> items, string type, bool isPrimary) where T : CustomTypeObject, IPrimaryCandidateObject
        {
            if (items == null)
            {
                return default(T);
            }

            return items.FirstOrDefault(t => t.Type == type && t.IsPrimary == isPrimary);
        }

        public static T GetOrCreateObjectOfType<T>(this IList<T> items, string type, bool isPrimary) where T : CustomTypeObject, IPrimaryCandidateObject, new()
        {
            T newItem;

            if (items == null)
            {
                items = new List<T>();
                newItem = new T() { Type = type };
                return newItem;
            }

            T existingItem = items.FirstOrDefault(t => t.Type == type && t.IsPrimary == isPrimary);

            if (existingItem == null)
            {
                newItem = new T() { Type = type };
                return newItem;
            }
            else
            {
                return existingItem;
            }
        }

        public static bool AddOrRemoveIfEmpty<T>(this IList<T> items, T item) where T : IIsEmptyObject
        {
            if (item.IsEmpty())
            {
                if (items.Contains(item))
                {
                    items.Remove(item);
                    return true;
                }
            }
            else
            {
                if (!items.Contains(item))
                {
                    items.Add(item);
                    return true;
                }
            }

            return false;
        }

        public static bool RemoveEmptyItems<T>(this IList<T> items) where T : IIsEmptyObject
        {
            bool updated = false;

            if (items == null)
            {
                return false;
            }

            foreach (T item in items)
            {
                if (item.IsEmpty())
                {
                        items.Remove(item);
                        updated = true;
                }
            }

            return updated;
        }

        public static string ToLowerString(this bool value)
        {
            return value.ToString().ToLower();
        }

        public static bool? ToNullableBool(this string value)
        {
            if (value == null)
            {
                return null;
            }
            else
            {
                return Convert.ToBoolean(value);
            }
        }
    }
}
