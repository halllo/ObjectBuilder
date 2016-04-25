using System;

namespace ObjectBuilder
{
	public interface IRelation<in TModels>
	{
		void Compose(TModels modelGraph);
	}


	public static class Composer
	{
		public static ObjectComposer<TModels> ForeignKeyRelation<TModels, TOneModel, TOneId, TManyModel, TManyId>(
			this ObjectComposer<TModels> composer,
			Func<TModels, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Func<TManyModel, TOneId?> getForeignKeyFunc,
			Action<TOneModel> initListAction,
			Action<TOneModel, TManyModel> addManyModelAction,
			Action<TOneModel, TManyModel> setOneModelAction)
			where TOneModel : class
			where TOneId : struct
			where TManyId : struct
		{
			composer.Add(new ForeignKeyRelation<TModels, TOneModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				getForeignKeyFunc,
				initListAction,
				addManyModelAction,
				setOneModelAction));
			return composer;
		}

		public static ObjectComposer<TModels> ImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId>(
			this ObjectComposer<TModels> composer,
			Func<TModels, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<TOneModel, ModelGraphEntry<TManyModel, TManyId>> setManyModelAction)
			where TOneId : struct
			where TManyId : struct
		{
			composer.Add(new ImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				setManyModelAction));
			return composer;
		}

		public static ObjectComposer<TModels> InverseImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId>(
			this ObjectComposer<TModels> composer,
			Func<TModels, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<ModelGraphEntry<TOneModel, TOneId>, TManyModel> setOneModelAction)
			where TOneId : struct
			where TManyId : struct
		{
			composer.Add(new InverseImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				setOneModelAction));
			return composer;
		}
	}


	public class InverseImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelation<TModels>
		where TOneId : struct
		where TManyId : struct
	{
		public InverseImplicitRelation(
			Func<TModels, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<ModelGraphEntry<TOneModel, TOneId>, TManyModel> setOneModelAction)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
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
					mSetOneModelAction(oneEntry, manyModel);
				}
			}
		}

		private readonly Func<TModels, ModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<TModels, ModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Action<ModelGraphEntry<TOneModel, TOneId>, TManyModel> mSetOneModelAction;
	}


	public class ImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelation<TModels>
		where TOneId : struct
		where TManyId : struct
	{
		public ImplicitRelation(
			Func<TModels, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<TOneModel, ModelGraphEntry<TManyModel, TManyId>> setManyModelAction)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mSetManyModelAction = setManyModelAction;
		}

		public void Compose(TModels modelGraph)
		{
			var oneEntry = mGetOneEntryFunc(modelGraph);
			var manyEntry = mGetManyEntryFunc(modelGraph);

			if (oneEntry != null && manyEntry != null)
			{
				foreach (var oneModel in oneEntry)
				{
					mSetManyModelAction(oneModel, manyEntry);
				}
			}
		}

		private readonly Func<TModels, ModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<TModels, ModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Action<TOneModel, ModelGraphEntry<TManyModel, TManyId>> mSetManyModelAction;
	}


	public class ForeignKeyRelation<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelation<TModels>
		where TOneModel : class
		where TOneId : struct
		where TManyId : struct
	{
		public ForeignKeyRelation(
			Func<TModels, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModels, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Func<TManyModel, TOneId?> getForeignKeyFunc,
			Action<TOneModel> initListAction,
			Action<TOneModel, TManyModel> addManyModelAction,
			Action<TOneModel, TManyModel> setOneModelAction
			)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mGetForeignKeyFunc = getForeignKeyFunc;
			mInitListAction = initListAction;
			mAddManyModelAction = addManyModelAction;
			mSetOneModelAction = setOneModelAction;
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

					if (mSetOneModelAction != null)
					{
						if (foreignKey.HasValue == (oneModel != null))
						{
							mSetOneModelAction(oneModel, manyModel);
						}
					}

					if (oneModel != null && mAddManyModelAction != null)
					{
						mAddManyModelAction(oneModel, manyModel);
					}
				}
			}
		}

		private readonly Action<TOneModel, TManyModel> mAddManyModelAction;

		private readonly Func<TManyModel, TOneId?> mGetForeignKeyFunc;
		private readonly Func<TModels, ModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<TModels, ModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Action<TOneModel> mInitListAction;
		private readonly Action<TOneModel, TManyModel> mSetOneModelAction;
	}
}
