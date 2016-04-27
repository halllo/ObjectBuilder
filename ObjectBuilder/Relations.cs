using System;
using System.Collections.Generic;

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
			Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc,
			Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc,
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
			Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc,
			Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc,
			Action<TOneModel, Dictionary<TManyId, TManyModel>> setManyModelAction)
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
			Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc,
			Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc,
			Action<Dictionary<TOneId, TOneModel>, TManyModel> setOneModelAction)
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
			Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc,
			Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc,
			Action<Dictionary<TOneId, TOneModel>, TManyModel> setOneModelAction)
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
					mSetOneModelAction(oneEntry, manyModel.Value);
				}
			}
		}

		private readonly Func<TModels, Dictionary<TManyId, TManyModel>> mGetManyEntryFunc;
		private readonly Func<TModels, Dictionary<TOneId, TOneModel>> mGetOneEntryFunc;
		private readonly Action<Dictionary<TOneId, TOneModel>, TManyModel> mSetOneModelAction;
	}


	public class ImplicitRelation<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelation<TModels>
		where TOneId : struct
		where TManyId : struct
	{
		public ImplicitRelation(
			Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc,
			Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc,
			Action<TOneModel, Dictionary<TManyId, TManyModel>> setManyModelAction)
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
					mSetManyModelAction(oneModel.Value, manyEntry);
				}
			}
		}

		private readonly Func<TModels, Dictionary<TManyId, TManyModel>> mGetManyEntryFunc;
		private readonly Func<TModels, Dictionary<TOneId, TOneModel>> mGetOneEntryFunc;
		private readonly Action<TOneModel, Dictionary<TManyId, TManyModel>> mSetManyModelAction;
	}


	public class ForeignKeyRelation<TModels, TOneModel, TOneId, TManyModel, TManyId> : IRelation<TModels>
		where TOneModel : class
		where TOneId : struct
		where TManyId : struct
	{
		public ForeignKeyRelation(
			Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc,
			Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc,
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
						mInitListAction(oneModel.Value);
					}
				}

				foreach (var manyModel in manyEntry)
				{
					var foreignKey = mGetForeignKeyFunc(manyModel.Value);
					var oneModel = oneEntry.GetById(foreignKey);

					if (mSetOneModelAction != null)
					{
						if (foreignKey.HasValue == (oneModel != null))
						{
							mSetOneModelAction(oneModel, manyModel.Value);
						}
					}

					if (oneModel != null && mAddManyModelAction != null)
					{
						mAddManyModelAction(oneModel, manyModel.Value);
					}
				}
			}
		}

		private readonly Action<TOneModel, TManyModel> mAddManyModelAction;

		private readonly Func<TManyModel, TOneId?> mGetForeignKeyFunc;
		private readonly Func<TModels, Dictionary<TManyId, TManyModel>> mGetManyEntryFunc;
		private readonly Func<TModels, Dictionary<TOneId, TOneModel>> mGetOneEntryFunc;
		private readonly Action<TOneModel> mInitListAction;
		private readonly Action<TOneModel, TManyModel> mSetOneModelAction;
	}


	public static class Dictionary
	{
		public static TValue GetById<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey? id) where TKey : struct
		{
			if (!id.HasValue)
			{
				return default(TValue);
			}

			TValue found;
			return dict.TryGetValue(id.Value, out found) ? found : default(TValue);
		}
	}
}
