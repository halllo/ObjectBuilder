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
		private readonly Func<TModels, IEnumerable<IRelation<TModels>>> mComposer;

		public ObjectFactory(Func<TStates, TModels> graphProcessor, Func<TModels, IEnumerable<IRelation<TModels>>> composer)
		{
			mGraphProcessor = graphProcessor;
			mComposer = composer;
		}

		public ObjectFactory<TStates, TModels> SetupComposition(Func<TModels, IEnumerable<IRelation<TModels>>> composer)
		{
			return new ObjectFactory<TStates, TModels>(mGraphProcessor, composer);
		}

		public TModels Create(TStates states)
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
}
