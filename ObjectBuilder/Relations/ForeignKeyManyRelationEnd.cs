using System;

namespace ObjectBuilder.Relations
{
	internal class ForeignKeyManyRelationEnd<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelationEnd<TModels, TManyModel>
		where TOneModel : class
		where TOneId : struct
		where TManyId : struct
	{
		public ForeignKeyManyRelationEnd(
			Func<TModels, IModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Func<TModels, IModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TManyModel, TOneId?> getForeignKeyFunc,
			Action<TManyModel, TOneModel> setOneModelAction
			)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mGetForeignKeyFunc = getForeignKeyFunc;
			mSetOneModelAction = setOneModelAction;
		}

		public void Compose(TModels modelGraph)
		{
			var oneEntry = mGetOneEntryFunc(modelGraph);
			var manyEntry = mGetManyEntryFunc(modelGraph);

			if (oneEntry != null && manyEntry != null)
			{
				foreach (var manyModel in manyEntry)
				{
					var foreignKey = mGetForeignKeyFunc(manyModel);
					var oneModel = oneEntry.GetById(foreignKey);

					if (foreignKey.HasValue == (oneModel != null))
					{
						mSetOneModelAction(manyModel, oneModel);
					}
				}
			}
		}

		public void Compose(TModels modelGraph, TManyModel manyModel)
		{
			var oneEntry = mGetOneEntryFunc(modelGraph);

			if (oneEntry != null)
			{
				var foreignKey = mGetForeignKeyFunc(manyModel);
				var oneModel = oneEntry.GetById(foreignKey);

				if (foreignKey.HasValue == (oneModel != null))
				{
					mSetOneModelAction(manyModel, oneModel);
				}
			}
		}

		public bool CanCompose(TModels modelGraph, Type endType)
		{
			return typeof(TOneModel) == endType;
		}

		private readonly Func<TModels, IModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Func<TModels, IModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<TManyModel, TOneId?> mGetForeignKeyFunc;
		private readonly Action<TManyModel, TOneModel> mSetOneModelAction;
	}
}
