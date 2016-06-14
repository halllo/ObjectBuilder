using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObjectBuilder
{
	public static class ObjectFactory
	{
		public static ObjectFactory<TStates, TStates> From<TStates>()
		{
			return new ObjectFactory<TStates, TStates>(s => s, null);
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

		public ObjectFactory<TStates, TModelsNext> SetupCreation<TModelsNext>(Func<TStates, TModelsNext> graphProcessor)
		{
			return new ObjectFactory<TStates, TModelsNext>(graphProcessor, null);
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

			AssignFactory(modelGraph, new Factory<TModels>(modelGraph, Compose));

			Compose(modelGraph);

			return modelGraph;
		}

		private void Compose(TModels modelGraph)
		{
			foreach (var relation in mComposer.Relations)
			{
				relation.Compose(modelGraph);
			}
		}

		private void Compose(TModels modelGraph, Type relevantType)
		{
			var relevantRelations = mComposer.Relations.Where(r =>
			{
				var genericTypes = r.GetType().GetGenericArguments();
				return genericTypes[1].IsAssignableFrom(relevantType) || genericTypes[3].IsAssignableFrom(relevantType);
			});

			foreach (var relation in relevantRelations)
			{
				relation.Compose(modelGraph);
			}
		}

		private static void AssignFactory(TModels modelGraph, IFactory factory)
		{
			var modelGraphType = modelGraph.GetType();

			var objects = modelGraphType.GetProperties().SelectMany(p =>
			{
				var modelGraphEntry = p.GetValue(modelGraph);
				if (modelGraphEntry != null)
				{
					var modelGraphEntryValues = p.PropertyType.GetProperty("Values").GetValue(modelGraphEntry);
					return ((IEnumerable)modelGraphEntryValues).OfType<object>();
				}
				else
				{
					return Enumerable.Empty<object>();
				}
			}).ToList();

			foreach (var o in objects)
			{
				var modelFactoryProperty = o.GetType().GetProperties().FirstOrDefault(p => p.PropertyType == typeof(IFactory));
				if (modelFactoryProperty != null)
				{
					modelFactoryProperty.SetValue(o, factory);
				}
			}
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
