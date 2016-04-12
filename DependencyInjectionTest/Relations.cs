using System;
using DependencyInjectionTest;

namespace DependencyInjector
{
	public interface IRelation
	{
		void Compose(ModelGraph modelGraph);
	}


	public static class Compose
	{
		public static IRelation ForeignKeyRelation<TOneModel, TOneId, TManyModel, TManyId>(
			Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Func<TManyModel, TOneId?> getForeignKeyFunc,
			Action<TOneModel> initListAction,
			Action<TOneModel, TManyModel> addManyModelAction,
			Action<TOneModel, TManyModel> setOneModelAction)
			where TOneModel : class
			where TOneId : struct
			where TManyId : struct
		{
			return new ForeignKeyRelation<TOneModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				getForeignKeyFunc,
				initListAction,
				addManyModelAction,
				setOneModelAction);
		}

		public static IRelation ImplicitRelation<TOneModel, TOneId, TManyModel, TManyId>(
			Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<TOneModel, ModelGraphEntry<TManyModel, TManyId>> setManyModelAction)
			where TOneId : struct
			where TManyId : struct
		{
			return new ImplicitRelation<TOneModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				setManyModelAction);
		}

		public static IRelation InverseImplicitRelation<TOneModel, TOneId, TManyModel, TManyId>(
			Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<ModelGraphEntry<TOneModel, TOneId>, TManyModel> setOneModelAction)
			where TOneId : struct
			where TManyId : struct
		{
			return new InverseImplicitRelation<TOneModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				setOneModelAction);
		}
	}



	public class InverseImplicitRelation<TOneModel, TOneId, TManyModel, TManyId> : IRelation
	where TOneId : struct
	where TManyId : struct
	{
		public InverseImplicitRelation(
			Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<ModelGraphEntry<TOneModel, TOneId>, TManyModel> setOneModelAction)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mSetOneModelAction = setOneModelAction;
		}

		public void Compose(ModelGraph modelGraph)
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

		private readonly Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Action<ModelGraphEntry<TOneModel, TOneId>, TManyModel> mSetOneModelAction;
	}


	public class ImplicitRelation<TOneModel, TOneId, TManyModel, TManyId> : IRelation
	where TOneId : struct
	where TManyId : struct
	{
		public ImplicitRelation(
			Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Action<TOneModel, ModelGraphEntry<TManyModel, TManyId>> setManyModelAction)
		{
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mSetManyModelAction = setManyModelAction;
		}

		public void Compose(ModelGraph modelGraph)
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

		private readonly Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Action<TOneModel, ModelGraphEntry<TManyModel, TManyId>> mSetManyModelAction;
	}


	public class ForeignKeyRelation<TOneModel, TOneId, TManyModel, TManyId> : IRelation
		where TOneModel : class
		where TOneId : struct
		where TManyId : struct
	{
		public ForeignKeyRelation(
			Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
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

		public void Compose(ModelGraph modelGraph)
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
		private readonly Func<ModelGraph, ModelGraphEntry<TManyModel, TManyId>> mGetManyEntryFunc;
		private readonly Func<ModelGraph, ModelGraphEntry<TOneModel, TOneId>> mGetOneEntryFunc;
		private readonly Action<TOneModel> mInitListAction;
		private readonly Action<TOneModel, TManyModel> mSetOneModelAction;
	}
}
