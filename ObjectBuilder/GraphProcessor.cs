using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectBuilder
{
	public static class GraphProcessor
	{
		public static Dictionary<TId, TModel> AsModels<TModel, TDto, TId>(
			this IEnumerable<TDto> states,
			Func<TDto, TModel> getModelFunc, Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			if (states == null)
			{
				return null;
			}

			return states.Select(getModelFunc).ToDictionary(getIdFunc);
		}

		public static TValue GetById<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey? id) where TKey : struct
		{
			if (!id.HasValue)
			{
				return default(TValue);
			}

			TValue found;
			return dict.TryGetValue(id.Value, out found) ? found : default(TValue);
		}
	}
}