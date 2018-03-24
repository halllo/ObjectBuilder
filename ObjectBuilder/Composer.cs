using System;
using System.Collections.Generic;
using ObjectBuilder.Relations;

namespace ObjectBuilder
{
	internal interface IComposer<in TModels, in TModel> : IComposer<TModels>
	{
		void Compose(TModels modelGraph, TModel model);
		void Compose(TModels modelGraph, TModel model, Type propertyType);
	}

	internal interface IComposer<in TModels>
	{
		void Compose(TModels modelGraph);
		void Compose(TModels modelGraph, object model);
		void Compose(TModels modelGraph, object model, Type propertyType);
		bool CanCompose(object model);
	}

	internal class Composer<TModels, TModel> : IComposer<TModels, TModel>
	{
		private readonly List<IRelationEnd<TModels, TModel>> _relationEnds;

		public Composer(List<IRelationEnd<TModels, TModel>> relationEnds)
		{
			_relationEnds = relationEnds;
		}

		public void Compose(TModels modelGraph)
		{
			foreach (var relationEnd in _relationEnds)
			{
				relationEnd.Compose(modelGraph);
			}
		}

		public void Compose(TModels modelGraph, TModel model)
		{
			foreach (var relationEnd in _relationEnds)
			{
				relationEnd.Compose(modelGraph, model);
			}
		}

		public void Compose(TModels modelGraph, TModel model, Type propertyType)
		{
			foreach (var relationEnd in _relationEnds)
			{
				if (relationEnd.CanCompose(modelGraph, propertyType))
				{
					relationEnd.Compose(modelGraph, model);
				}
			}
		}

		bool IComposer<TModels>.CanCompose(object model) => model is TModel;
		void IComposer<TModels>.Compose(TModels modelGraph, object model) => Compose(modelGraph, (TModel)model);
		void IComposer<TModels>.Compose(TModels modelGraph, object model, Type propertyType) => Compose(modelGraph, (TModel)model, propertyType);
	}
}
