using System;

namespace ObjectBuilder.Relations
{
	internal class ForeignKeyOneRelationEnd<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelationEnd<TModels, TOneModel>
		where TOneModel : class
		where TOneId : struct, IComparable<TOneId>
		where TManyId : struct
	{
		public ForeignKeyOneRelationEnd(
			Func<TModels, IModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, IModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Func<TOneModel, TOneId> getPrimaryKeyFunc,
			Func<TManyModel, TOneId?> getForeignKeyFunc,
			Action<TOneModel> initListAction,
			Action<TOneModel, TManyModel> addManyModelAction
			)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mGetPrimaryKeyFunc = getPrimaryKeyFunc;
			mGetForeignKeyFunc = getForeignKeyFunc;
			mInitListAction = initListAction;
			mAddManyModelAction = addManyModelAction;
		}

		public void Compose(TModels modelGraph)
		{
			var oneEntry = mGetOneEntryFunc(modelGraph);
			var manyEntry = mGetManyEntryFunc(modelGraph);

			if (oneEntry != null && manyEntry != null)
			{
				if (mInitListAction != null)
				{
					foreach (var oneModel in oneEntry)
					{
						mInitListAction(oneModel);
					}
				}

				foreach (var manyModel in manyEntry)
				{
					var foreignKey = mGetForeignKeyFunc(manyModel);
					var oneModel = oneEntry.GetById(foreignKey);

					if (oneModel != null)
					{
						mAddManyModelAction?.Invoke(oneModel, manyModel);
					}
				}
			}
		}

		public void Compose(TModels modelGraph, TOneModel oneModel)
		{
			var manyEntry = mGetManyEntryFunc(modelGraph);

			if (manyEntry != null)
			{
				mInitListAction?.Invoke(oneModel);
				var primaryKey = mGetPrimaryKeyFunc(oneModel);

				foreach (var manyModel in manyEntry)
				{
					var foreignKey = mGetForeignKeyFunc(manyModel);
					if (foreignKey.HasValue && foreignKey.Value.CompareTo(primaryKey) == 0)
					{
						mAddManyModelAction(oneModel, manyModel);
					}
				}
			}
		}

		public bool CanCompose(TModels modelGraph, Type endType)
		{
			return typeof(TManyModel) == endType;
		}

		private readonly Func<TModels, IModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Func<TModels, IModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<TOneModel, TOneId> mGetPrimaryKeyFunc;
		private readonly Func<TManyModel, TOneId?> mGetForeignKeyFunc;
		private readonly Action<TOneModel> mInitListAction;
		private readonly Action<TOneModel, TManyModel> mAddManyModelAction;
	}
}
