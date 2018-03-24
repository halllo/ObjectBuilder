using System;

namespace ObjectBuilder.Relations
{
	internal interface IRelationEnd<in TModels, in TModel>
	{
		void Compose(TModels modelGraph);
		void Compose(TModels modelGraph, TModel model);
		bool CanCompose(TModels modelGraph, Type endType);
	}
}