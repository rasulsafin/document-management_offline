using System;
using System.Collections.Generic;
using System.Linq;
using MRS.DocumentManagement.Database;
using MRS.DocumentManagement.Synchronizer.Models;

namespace MRS.DocumentManagement.Synchronizer.Utils
{
    public static class TuplesUtils
    {
        internal static List<SynchronizingTuple<T>> CreateSynchronizingTuples<T>(
                IEnumerable<T> dbList,
                IEnumerable<T> remoteList,
                Func<T, SynchronizingTuple<T>, bool> isEqualsFunc)
                where T : ISynchronizable<T>
        {
            var result = new List<SynchronizingTuple<T>>();

            void AddToList(IEnumerable<T> list, Action<SynchronizingTuple<T>, T> setUnsynchronized)
            {
                foreach (var element in list)
                {
                    if (string.IsNullOrEmpty(element.ExternalID))
                    {
                        result.Add(new SynchronizingTuple<T>(local: element));
                        continue;
                    }

                    var containsItem = result.FirstOrDefault(x => isEqualsFunc(element, x));
                    if (containsItem == null)
                        result.Add(containsItem = new SynchronizingTuple<T>(element.ExternalID));
                    if (element.IsSynchronized)
                        containsItem.Synchronized = element;
                    else
                        setUnsynchronized(containsItem, element);
                }
            }

            AddToList(dbList, (tuple, item) => tuple.Local = item);
            AddToList(remoteList, (tuple, item) => tuple.Remote = item);

            return result;
        }
    }
}
