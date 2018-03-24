using System;

namespace ObjectBuilder
{
	public interface IModelComposer
	{
		TModel Compose<TModel>(TModel model);
		TModel Compose<TModel>(TModel model, Type propertyType, params Type[] propertyTypes);
		int TemporaryId { get; }
	}

	internal class ModelComposer<TStates, TModels> : IModelComposer
	{
		private readonly ObjectBuilder<TStates, TModels> _objectBuilder;
		private readonly TModels _models;

		internal ModelComposer(ObjectBuilder<TStates, TModels> objectBuilder, TModels models)
		{
			_objectBuilder = objectBuilder;
			_models = models;
		}

		public TModel Compose<TModel>(TModel model)
		{
			_objectBuilder.Compose(_models, model);
			return model;
		}

		public TModel Compose<TModel>(TModel model, Type propertyType, params Type[] propertyTypes)
		{
			_objectBuilder.Compose(_models, model, propertyType, propertyTypes);
			return model;
		}

		public int TemporaryId => temporaryId--;
		private int temporaryId = -1;
	}
}