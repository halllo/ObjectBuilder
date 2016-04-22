using System;
using System.Collections;
using System.Collections.Generic;

namespace DependencyInjector
{
	public static class ObjectBuilder
	{
		public static ObjectBuilder<TStates> From<TStates>()
		{
			return new ObjectBuilder<TStates>();
		}
	}

	public class ObjectBuilder<TStates>
	{
		public GraphProcesor<TStates, TModels> Setup<TModels>(Func<TStates, TModels> graphProcessor, Func<TModels, IEnumerable<IRelation<TModels>>> composer)
		{
			return new GraphProcesor<TStates, TModels>(graphProcessor, composer);
		}
	}

	public class GraphProcesor<TStates, TModels>
	{
		private readonly Func<TStates, TModels> mGraphProcessor;
		private readonly Func<TModels, IEnumerable<IRelation<TModels>>> mComposer;

		public GraphProcesor(Func<TStates, TModels> graphProcessor, Func<TModels, IEnumerable<IRelation<TModels>>> composer)
		{
			mGraphProcessor = graphProcessor;
			mComposer = composer;
		}

		public TModels CreateModelsAndCompose(TStates states)
		{
			var modelGraph = mGraphProcessor(states);
			var relations = mComposer(modelGraph);

			foreach (var relation in relations)
			{
				relation.Compose(modelGraph);
			}

			return modelGraph;
		}
	}





	public static class ConstantModelGraphProcessor
	{
		public static ModelGraphEntry<TModel, TId> CreateConstantEntry<TModel, TId>(Dictionary<TId, TModel> modelsById,
			Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			var entry = new ModelGraphEntry<TModel, TId>(getIdFunc);
			entry.Set(modelsById);
			return entry;
		}
	}


	public static class VariableModelGraphProcessor
	{
		public static ModelGraphEntry<TModel, TId> CreateEntry<TModel, TDto, TId>(IEnumerable<TDto> dtos,
			Func<TDto, TModel> getModelFunc, Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			if (dtos == null)
			{
				return null;
			}

			var entry = new ModelGraphEntry<TModel, TId>(getIdFunc);
			foreach (var dto in dtos)
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