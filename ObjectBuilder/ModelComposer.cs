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
		private readonly ObjectProcessing<TStates, TModels> _objectProcessing;
		private readonly TModels _models;

		internal ModelComposer(ObjectProcessing<TStates, TModels> objectProcessing, TModels models)
		{
			_objectProcessing = objectProcessing;
			_models = models;
		}

		public TModel Compose<TModel>(TModel model)
		{
			_objectProcessing.Compose(_models, model);
			return model;
		}

		public TModel Compose<TModel>(TModel model, Type propertyType, params Type[] propertyTypes)
		{
			_objectProcessing.Compose(_models, model, propertyType, propertyTypes);
			return model;
		}

		public int TemporaryId => temporaryId--;
		private int temporaryId = -1;
	}
}