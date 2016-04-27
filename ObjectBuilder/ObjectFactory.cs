using System;
using System.Collections.Generic;

namespace ObjectBuilder
{
	public static class ObjectFactory
	{
		public static ObjectFactory<TStates> From<TStates>()
		{
			return new ObjectFactory<TStates>();
		}
	}

	public class ObjectFactory<TStates>
	{
		public ObjectFactory<TStates, TModels> SetupCreation<TModels>(Func<TStates, TModels> graphProcessor)
		{
			return new ObjectFactory<TStates, TModels>(graphProcessor, null);
		}
	}

	public class ObjectFactory<TStates, TModels>
	{
		private readonly Func<TStates, TModels> mGraphProcessor;
		private readonly ObjectComposer<TModels> mComposer;

		public ObjectFactory(Func<TStates, TModels> graphProcessor, ObjectComposer<TModels> composer)
		{
			mGraphProcessor = graphProcessor;
			mComposer = composer;
		}

		public ObjectFactory<TStates, TModels> SetupComposition(Action<ObjectComposer<TModels>> composer)
		{
			var newComposer = new ObjectComposer<TModels>();
			composer(newComposer);

			return new ObjectFactory<TStates, TModels>(mGraphProcessor, newComposer);
		}

		public TModels Create(TStates states)
		{
			var modelGraph = mGraphProcessor(states);

			foreach (var relation in mComposer.Relations)
			{
				relation.Compose(modelGraph);
			}

			return modelGraph;
		}
	}

	public class ObjectComposer<TModels>
	{
		internal ObjectComposer()
		{
			mRelations = new List<IRelation<TModels>>();
		}

		readonly List<IRelation<TModels>> mRelations;

		public IEnumerable<IRelation<TModels>> Relations { get { return mRelations; } }

		public void Add(IRelation<TModels> relation)
		{
			mRelations.Add(relation);
		}
	}
}
