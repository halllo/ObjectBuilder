using System;
using System.Collections.Generic;

namespace ObjectBuilder
{
	public static class InModels
	{
		public static InModelsOne<TModels, TOneModel, TOneId> One<TModels, TOneModel, TOneId>(this ObjectComposer<TModels> composer, Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc)
			where TOneModel : class
			where TOneId : struct
		{
			return new InModelsOne<TModels, TOneModel, TOneId>(composer, getOneEntryFunc);
		}
	}

	public class InModelsOne<TModels, TOneModel, TOneId>
		where TOneModel : class
		where TOneId : struct
	{
		private readonly ObjectComposer<TModels> mComposer;
		private readonly Func<TModels, Dictionary<TOneId, TOneModel>> mGetOneEntryFunc;

		public InModelsOne(ObjectComposer<TModels> composer, Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc)
		{
			mComposer = composer;
			mGetOneEntryFunc = getOneEntryFunc;
		}

		public InModelsOneHasManyForeignKey<TModels, TOneModel, TOneId, TManyModel, TManyId> HasMany<TManyModel, TManyId>(Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc, Func<TManyModel, TOneId?> getForeignKeyFunc)
			where TManyId : struct
		{
			return new InModelsOneHasManyForeignKey<TModels, TOneModel, TOneId, TManyModel, TManyId>(mComposer, mGetOneEntryFunc, getManyEntryFunc, getForeignKeyFunc);
		}

		public InModelsOneHasMany<TModels, TOneModel, TOneId, TManyModel, TManyId> HasMany<TManyModel, TManyId>(Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc)
			where TManyId : struct
		{
			return new InModelsOneHasMany<TModels, TOneModel, TOneId, TManyModel, TManyId>(mComposer, mGetOneEntryFunc, getManyEntryFunc);
		}
	}

	public class InModelsOneHasMany<TModels, TOneModel, TOneId, TManyModel, TManyId>
		where TOneModel : class
		where TManyId : struct
		where TOneId : struct
	{
		private readonly ObjectComposer<TModels> mComposer;
		private readonly Func<TModels, Dictionary<TOneId, TOneModel>> mGetOneEntryFunc;
		private readonly Func<TModels, Dictionary<TManyId, TManyModel>> mGetManyEntryFunc;

		public InModelsOneHasMany(ObjectComposer<TModels> composer, Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc, Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc)
		{
			mComposer = composer;
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
		}

		public ObjectComposer<TModels> Assign(
			Action<TOneModel, Dictionary<TManyId, TManyModel>> setMany)
		{
			return Composer.ImplicitRelation(mComposer, mGetOneEntryFunc, mGetManyEntryFunc, setMany);
		}
	}

	public class InModelsOneHasManyForeignKey<TModels, TOneModel, TOneId, TManyModel, TManyId>
		where TOneModel : class
		where TOneId : struct
		where TManyId : struct
	{
		private readonly ObjectComposer<TModels> mComposer;
		private readonly Func<TModels, Dictionary<TOneId, TOneModel>> mGetOneEntryFunc;
		private readonly Func<TModels, Dictionary<TManyId, TManyModel>> mGetManyEntryFunc;
		private readonly Func<TManyModel, TOneId?> mGetForeignKeyFunc;

		public InModelsOneHasManyForeignKey(ObjectComposer<TModels> composer, Func<TModels, Dictionary<TOneId, TOneModel>> getOneEntryFunc, Func<TModels, Dictionary<TManyId, TManyModel>> getManyEntryFunc, Func<TManyModel, TOneId?> getForeignKeyFunc)
		{
			mComposer = composer;
			mGetOneEntryFunc = getOneEntryFunc;
			mGetManyEntryFunc = getManyEntryFunc;
			mGetForeignKeyFunc = getForeignKeyFunc;
		}

		public ObjectComposer<TModels> Assign(
			Action<TOneModel> init,
			Action<TOneModel, TManyModel> addMany,
			Action<TOneModel, TManyModel> setOne)
		{
			return Composer.ForeignKeyRelation(mComposer, mGetOneEntryFunc, mGetManyEntryFunc, mGetForeignKeyFunc, init, addMany, setOne);
		}
	}
}