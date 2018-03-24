using System;
using System.Collections.Generic;
using ObjectBuilder.Relations;

namespace ObjectBuilder
{
	public class OneFluent<TModels, TId, TModel>
		where TModel : class
		where TId : struct, IComparable<TId>
	{
		internal OneFluent(Func<TModels, IModelGraphEntry<TModel, TId>> getStartEntryFunc)
		{
			_mGetStartEntryFunc = getStartEntryFunc;
		}

		public OneFluent<TModels, TId, TModel> HasOne<TOneModel, TId2>(
			Func<TModels, IModelGraphEntry<TOneModel, TId2>> getOneEntryFunc,
			Func<TModel, TId2?> getForeignKeyFunc,
			Action<TModel, TOneModel> setOneModelAction)
			where TOneModel : class
			where TId2 : struct
		{
			AddForeignKeyManyRelationEnd(_mGetStartEntryFunc, getOneEntryFunc, getForeignKeyFunc, setOneModelAction);
			return this;
		}

		public OneFluent<TModels, TId, TModel> HasMany<TManyModel, TId2>(
			Func<TModels, IModelGraphEntry<TManyModel, TId2>> getManyEntryFunc,
			Func<TModel, TId> getPrimaryKeyFunc,
			Func<TManyModel, TId?> getForeignKeyFunc,
			Action<TModel> initListAction,
			Action<TModel, TManyModel> addManyModelAction)
			where TId2 : struct
		{
			AddForeignKeyOneRelationEnd(_mGetStartEntryFunc, getManyEntryFunc, getPrimaryKeyFunc, getForeignKeyFunc, initListAction, addManyModelAction);
			return this;
		}

		public OneFluent<TModels, TId, TModel> HasImplicit<TImplicitModel, TImplicitId>(
			Func<TModels, IModelGraphEntry<TImplicitModel, TImplicitId>> getImplicitEntryFunc,
			Action<TModel, IModelGraphEntry<TImplicitModel, TImplicitId>> setImplicitModelsAction)
			where TImplicitId : struct
		{
			AddImplicitRelationEnd(_mGetStartEntryFunc, getImplicitEntryFunc, setImplicitModelsAction);
			return this;
		}






		private void AddForeignKeyOneRelationEnd<TOneId, TManyModel, TManyId>(
			Func<TModels, IModelGraphEntry<TModel, TOneId>> getOneEntryFunc,
			Func<TModels, IModelGraphEntry<TManyModel, TManyId>> getManyEntryFunc,
			Func<TModel, TOneId> getPrimaryKeyFunc,
			Func<TManyModel, TOneId?> getForeignKeyFunc,
			Action<TModel> initListAction,
			Action<TModel, TManyModel> addManyModelAction)
			where TOneId : struct, IComparable<TOneId>
			where TManyId : struct
		{
			RelationEnds.Add(new ForeignKeyOneRelationEnd<TModels, TModel, TOneId, TManyModel, TManyId>(
				getOneEntryFunc,
				getManyEntryFunc,
				getPrimaryKeyFunc,
				getForeignKeyFunc,
				initListAction,
				addManyModelAction));
		}

		private void AddForeignKeyManyRelationEnd<TOneModel, TOneId, TManyId>(
			Func<TModels, IModelGraphEntry<TModel, TManyId>> getManyEntryFunc,
			Func<TModels, IModelGraphEntry<TOneModel, TOneId>> getOneEntryFunc,
			Func<TModel, TOneId?> getForeignKeyFunc,
			Action<TModel, TOneModel> setOneModelAction)
			where TOneModel : class
			where TOneId : struct
			where TManyId : struct
		{
			RelationEnds.Add(new ForeignKeyManyRelationEnd<TModels, TOneModel, TOneId, TModel, TManyId>(
				getManyEntryFunc,
				getOneEntryFunc,
				getForeignKeyFunc,
				setOneModelAction));
		}

		private void AddImplicitRelationEnd<TModelId, TImplicitModel, TImplicitId>(
			Func<TModels, IModelGraphEntry<TModel, TModelId>> getEntryFunc,
			Func<TModels, IModelGraphEntry<TImplicitModel, TImplicitId>> getImplicitEntryFunc,
			Action<TModel, IModelGraphEntry<TImplicitModel, TImplicitId>> setImplicitModelsAction)
			where TModelId : struct
			where TImplicitId : struct
		{
			RelationEnds.Add(new ImplicitRelationEnd<TModels, TModel, TModelId, TImplicitModel, TImplicitId>(
				getEntryFunc,
				getImplicitEntryFunc,
				setImplicitModelsAction));
		}

		internal readonly List<IRelationEnd<TModels, TModel>> RelationEnds = new List<IRelationEnd<TModels, TModel>>();
		private readonly Func<TModels, IModelGraphEntry<TModel, TId>> _mGetStartEntryFunc;
	}
}