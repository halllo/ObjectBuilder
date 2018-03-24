using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObjectBuilder
{
	public interface IModelGraphEntry<TModel, TId> : IEnumerable<TModel> where TId : struct
	{
		void Set(Dictionary<TId, TModel> modelsById);
		void Add(TModel model);
		void AddRange(IEnumerable<TModel> models);
		void AddEntry(IModelGraphEntry<TModel, TId> entry, bool overwrite);
		Dictionary<TId, TModel> ModelsById { get; }
		TModel GetById(TId? id);
		List<TModel> GetByIds(IEnumerable<TId?> ids);
		List<TModel> GetByIds(IEnumerable<TId> ids);
		List<TResult> ConvertAll<TResult>(Func<TModel, TResult> converter);
		IModelGraphEntry<TDerivedModel, TId> OfType<TDerivedModel>() where TDerivedModel : TModel;
	}

	internal class ModelGraphEntry<TModel, TId> : IModelGraphEntry<TModel, TId> where TId : struct
	{
		public ModelGraphEntry(Func<TModel, TId> getIdFunc)
		{
			mGetIdFunc = getIdFunc;
		}

		public Dictionary<TId, TModel> ModelsById { get; private set; } = new Dictionary<TId, TModel>();

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
				ModelsById.Add(mGetIdFunc(model), model);
			}
		}

		public void AddEntry(IModelGraphEntry<TModel, TId> entry, bool overwrite)
		{
			if (entry == this)
				return;

			foreach (var itemInEntry in entry)
			{
				if (!ModelsById.ContainsKey(mGetIdFunc(itemInEntry)))
				{
					Add(itemInEntry);
				}
				else if (overwrite)
				{
					ModelsById[mGetIdFunc(itemInEntry)] = itemInEntry;
				}
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

		public List<TModel> GetByIds(IEnumerable<TId?> ids)
		{
			return ids
				.Where(id => id.HasValue)
				.Select(id => id.Value)
				.Select(id => GetById(id))
				.ToList();
		}

		public List<TModel> GetByIds(IEnumerable<TId> ids)
		{
			return ids
				.Select(id => GetById(id))
				.ToList();
		}

		public List<TResult> ConvertAll<TResult>(Func<TModel, TResult> converter)
		{
			return ModelsById.Values.Select(converter).ToList();
		}

		public IModelGraphEntry<TDerivedModel, TId> OfType<TDerivedModel>() where TDerivedModel : TModel
		{
			var entryOfDerivedTypes = new ModelGraphEntry<TDerivedModel, TId>(m => mGetIdFunc(m));
			entryOfDerivedTypes.ModelsById = ModelsById.Where(kvp => kvp.Value is TDerivedModel).ToDictionary(kvp => kvp.Key, kvp => (TDerivedModel)kvp.Value);

			return entryOfDerivedTypes;
		}

		public IEnumerator<TModel> GetEnumerator()
		{
			return ModelsById.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ModelsById.Values.GetEnumerator();
		}

		private readonly Func<TModel, TId> mGetIdFunc;
	}
}
