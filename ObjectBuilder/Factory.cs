using System;
using System.Linq;

namespace ObjectBuilder
{
	public interface IFactory
	{
		T New<T>(object id, params object[] args);
	}

	internal class Factory<TModels> : IFactory
	{
		private readonly TModels _modelGraph;
		private readonly Type _modelGraphType;
		private readonly Action<TModels, Type> _compose;

		public Factory(TModels modelGraph, Action<TModels, Type> compose)
		{
			_modelGraph = modelGraph;
			_modelGraphType = modelGraph.GetType();
			_compose = compose;
		}

		public T New<T>(object id, params object[] args)
		{
			var newModelType = typeof(T);
			var newModel = (T)Activator.CreateInstance(newModelType, args);

			var newModelEntryProperties = from property in _modelGraphType.GetProperties()
										  where property.PropertyType.GetGenericArguments()[1].IsAssignableFrom(newModelType)
										  select property;

			var newModelEntryProperty = newModelEntryProperties.Single();
			var newModelEntry = newModelEntryProperty.GetValue(_modelGraph);
			//TODO: if null create entry dictionary

			var newModelEntryAdd = newModelEntry.GetType().GetMethod("Add");
			newModelEntryAdd.Invoke(newModelEntry, new object[] { id, newModel });

			_compose(_modelGraph, newModelType);

			return newModel;
		}
	}
}