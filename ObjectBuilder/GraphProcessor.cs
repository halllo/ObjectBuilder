using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObjectBuilder
{
	public static class GraphProcessor
	{
		public static ModelGraphEntry<TModel, TId> AsModels<TModel, TId>(
			this IEnumerable<TModel> models,
			Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			var entry = new ModelGraphEntry<TModel, TId>(getIdFunc);
			entry.Set(models.ToDictionary(getIdFunc));
			return entry;
		}

		public static ModelGraphEntry<TModel, TId> AsModels<TModel, TDto, TId>(
			this IEnumerable<TDto> states,
			Func<TDto, TModel> getModelFunc, Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			if (states == null)
			{
				return null;
			}

			var entry = new ModelGraphEntry<TModel, TId>(getIdFunc);
			foreach (var dto in states)
			{
				entry.Add(getModelFunc(dto));
			}

			return entry;
		}
	}


	public class ModelGraphEntry<TModel, TId> : IEnumerable<TModel> where TId : struct
	{
		private readonly Func<TModel, TId> mGetIdFunc;

		public ModelGraphEntry(Func<TModel, TId> getIdFunc)
		{
			mGetIdFunc = getIdFunc;
			ModelsById = new Dictionary<TId, TModel>();
		}

		public Dictionary<TId, TModel> ModelsById { get; private set; }

		public IEnumerator<TModel> GetEnumerator()
		{
			return ModelsById.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ModelsById.Values.GetEnumerator();
		}

		public void Set(Dictionary<TId, TModel> modelsById)
		{
			ModelsById = modelsById;
		}

		public void Add(TModel model)
		{
			ModelsById.Add(mGetIdFunc(model), model);
		}

		public void AddRange(IEnumerable<TModel> models)
		{
			foreach (var model in models)
			{
				Add(model);
			}
		}

		public TModel GetById(TId? id)
		{
			if (!id.HasValue)
			{
				return default(TModel);
			}

			TModel found;
			return ModelsById.TryGetValue(id.Value, out found) ? found : default(TModel);
		}
	}
}