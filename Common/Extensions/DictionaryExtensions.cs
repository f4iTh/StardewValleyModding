using System.Collections.Generic;
using System.Linq;

namespace ModCommon.Extensions;

public static class DictionaryExtensions {
  public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IDictionary<TKey, TValue> dictionaryToMerge) {
    return dictionary
      .Concat(dictionaryToMerge)
      .ToLookup(pair => pair.Key, pair => pair.Value)
      .ToDictionary(group => group.Key, group => group.First());
  }
}