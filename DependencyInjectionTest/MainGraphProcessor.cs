using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DependencyInjectionTest;

namespace DependencyInjector
{
	public class MainGraphProcessor
	{
		private readonly ConstantModelGraphProcessor mConstantModelGraphProcessor;
		private readonly VariableModelGraphProcessor mVariableModelGraphProcessor;
		private readonly MainComposer mMainComposer;

		public MainGraphProcessor(IEnumerable<Type> compoerTypes)
		{
			mConstantModelGraphProcessor = new ConstantModelGraphProcessor();
			mVariableModelGraphProcessor = new VariableModelGraphProcessor();
			mMainComposer = new MainComposer(compoerTypes);
		}

		public ModelGraph CreateModelsAndCompose(ModelStates states)
		{
			var modelGraph = CreateModels(states);
			Compose(modelGraph);
			return modelGraph;
		}

		private ModelGraph CreateModels(ModelStates states)
		{
			var modelGraph = new ModelGraph();

			mConstantModelGraphProcessor.CreateEntries(modelGraph,);
			mVariableModelGraphProcessor.CreateEntries(modelGraph, states);

			return modelGraph;
		}

		private void Compose(ModelGraph modelGraph)
		{
			mMainComposer.Init();
			mMainComposer.Compose(modelGraph);
		}
	}






	public class ConstantModelGraphProcessor
	{
		private readonly Dictionary<Aktenstatus_State, Aktenstatus> AktenstatusModelsNachId = new List<Aktenstatus>
		{
			new Aktenstatus(Aktenstatus_State.potenziell),
			new Aktenstatus(Aktenstatus_State.InBearbeitung),
			new Aktenstatus(Aktenstatus_State.Abgeschlossen),
			new Aktenstatus(Aktenstatus_State.abgelehnt)
		}.ToDictionary(m => m.State);

		public void CreateEntries(ModelGraph modelGraph)
		{
			modelGraph.Aktenstatus = CreateConstantEntry(AktenstatusModelsNachId, m => m.State);
		}

		private ModelGraphEntry<TModel, TId> CreateConstantEntry<TModel, TId>(Dictionary<TId, TModel> modelsById,
			Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			var entry = new ModelGraphEntry<TModel, TId>(getIdFunc);
			entry.Set(modelsById);
			return entry;
		}
	}


	public class VariableModelGraphProcessor
	{
		public void CreateEntries(ModelGraph modelGraph, ModelStates modelStates)
		{
			modelGraph.Akten = CreateEntry(modelStates.Akten, state => new Akte(state), m => m.State.Id);
			modelGraph.Personen = CreateEntry(modelStates.Personen, state => new Person(state), m => m.State.Id);
		}

		private ModelGraphEntry<TModel, TId> CreateEntry<TModel, TDto, TId>(IEnumerable<TDto> dtos,
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
		}

		public Dictionary<TId, TModel> ModelsById { get; private set; } = new Dictionary<TId, TModel>();

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