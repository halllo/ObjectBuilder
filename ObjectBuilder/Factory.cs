using System;
using System.Collections.Generic;

namespace ObjectBuilder
{
	public static class Factory
	{
		public static ObjectCreation<TStates> From<TStates>() where TStates : new()
		{
			return new ObjectCreation<TStates>();
		}

		public static IModelGraphEntry<TModel, TId> Entry<TModel, TDto, TId>(IEnumerable<TDto> dtos, Func<TDto, TModel> getModelFunc, Func<TModel, TId> getIdFunc) where TId : struct
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

		public static IModelGraphEntry<TModel, TId> Constant<TModel, TId>(Dictionary<TId, TModel> modelsById, Func<TModel, TId> getIdFunc)
			where TId : struct
		{
			var entry = new ModelGraphEntry<TModel, TId>(getIdFunc);
			entry.Set(modelsById);
			return entry;
		}
	}







	public class ObjectCreation<TStates>
	{
		internal ObjectCreation()
		{
		}

		public ObjectComposing<TStates, TModels> Create<TModels>(Func<TStates, TModels> statesToModels)
		{
			return new ObjectComposing<TStates, TModels>(statesToModels);
		}

	}







	public class ObjectComposing<TStates, TModels>
	{
		private readonly Func<TStates, TModels> _statesToModels;

		internal ObjectComposing(Func<TStates, TModels> statesToModels)
		{
			_statesToModels = statesToModels;
		}

		public ObjectProcessing<TStates, TModels> Compose(Action<ObjectComposer> composer)
		{
			var newComposer = new ObjectComposer();
			composer(newComposer);

			return new ObjectProcessing<TStates, TModels>(_statesToModels, newComposer.Composers);
		}

		public class ObjectComposer
		{
			internal readonly List<IComposer<TModels>> Composers = new List<IComposer<TModels>>();

			internal ObjectComposer()
			{
			}

			public OneFluent<TModels, TId, TModel> One<TId, TModel>(Func<TModels, IModelGraphEntry<TModel, TId>> getStartEntryFunc)
				where TId : struct, IComparable<TId>
				where TModel : class
			{
				var oneFluent = new OneFluent<TModels, TId, TModel>(getStartEntryFunc);
				Composers.Add(new Composer<TModels, TModel>(oneFluent.RelationEnds));
				return oneFluent;
			}
		}
	}
}
