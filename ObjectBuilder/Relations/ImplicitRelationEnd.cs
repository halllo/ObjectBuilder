using System;

namespace ObjectBuilder.Relations
{
	internal class ImplicitRelationEnd<TModels, TModel, TId, TImplicitModel, TImplicitId> : IRelationEnd<TModels, TModel>
		where TId : struct
		where TImplicitId : struct
	{
		public ImplicitRelationEnd(
			Func<TModels, IModelGraphEntry<TModel, TId>> getEntryFunc,
			Func<TModels, IModelGraphEntry<TImplicitModel, TImplicitId>> getImplicitEntryFunc,
			Action<TModel, IModelGraphEntry<TImplicitModel, TImplicitId>> setImplicitModelsAction)
		{
			mGetEntryFunc = getEntryFunc;
			mGetImplicitEntryFunc = getImplicitEntryFunc;
			mSetImplicitModelsAction = setImplicitModelsAction;
		}

		public void Compose(TModels modelGraph)
		{
			var entry = mGetEntryFunc(modelGraph);
			var implicitEntry = mGetImplicitEntryFunc(modelGraph);

			if (entry != null && implicitEntry != null)
			{
				foreach (var model in entry)
				{
					mSetImplicitModelsAction(model, implicitEntry);
				}
			}
		}

		public void Compose(TModels modelGraph, TModel model)
		{
			var implicitEntry = mGetImplicitEntryFunc(modelGraph);

			if (implicitEntry != null)
			{
				mSetImplicitModelsAction(model, implicitEntry);
			}
		}

		public bool CanCompose(TModels modelGraph, Type endType)
		{
			return typeof(TImplicitModel) == endType;
		}

		private readonly Func<TModels, IModelGraphEntry<TModel, TId>> mGetEntryFunc;
		private readonly Func<TModels, IModelGraphEntry<TImplicitModel, TImplicitId>> mGetImplicitEntryFunc;
		private readonly Action<TModel, IModelGraphEntry<TImplicitModel, TImplicitId>> mSetImplicitModelsAction;
	}
}
