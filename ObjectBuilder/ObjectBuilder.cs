using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ObjectBuilder
{
	public class ObjectBuilder<TStates, TModels>
	{
		private readonly Func<TStates, TModels> _statesToModels;
		private readonly List<IComposer<TModels>> _composers;

		internal ObjectBuilder(Func<TStates, TModels> statesToModels, List<IComposer<TModels>> composers)
		{
			_statesToModels = statesToModels;
			_composers = composers;
		}

		public TModels CreateModelsAndCompose(TStates states)
		{
			var modelGraph = CreateModels(states);
			Compose(modelGraph);
			return modelGraph;
		}

		public TModels CreateModels(TStates states)
		{
			var modelGraph = _statesToModels(states);
			return modelGraph;
		}

		public void Compose(TModels modelGraph)
		{
			foreach (var composer in _composers)
			{
				composer.Compose(modelGraph);
			}
		}

		public void Compose<TModel>(TModels modelGraph, TModel model)
		{
			foreach (var composer in _composers)
			{
				if (composer.CanCompose(model))
				{
					composer.Compose(modelGraph, model);
				}
			}
		}

		public void Compose<TModel>(TModels modelGraph, TModel model, Type propertyType, params Type[] propertyTypes)
		{
			foreach (var type in new[] { propertyType }.Concat(propertyTypes))
			{
				foreach (var composer in _composers)
				{
					if (composer.CanCompose(model))
					{
						composer.Compose(modelGraph, model, type);
					}
				}
			}
		}

		public IModelComposer Composer(TModels models)
		{
			return new ModelComposer<TStates, TModels>(this, models);
		}

		public void CopyEntries(TModels from, TModels into, bool overwrite = false)
		{
			foreach (var property in ModelGraphProperties)
			{
				var sourceEntry = property.GetValue(from);
				if (sourceEntry != null)
				{
					var targetEntry = property.GetValue(into);
					if (targetEntry == null || overwrite)
					{
						property.SetValue(into, sourceEntry);
					}
				}
			}
		}

		public void CopyEntryItems(TModels from, TModels into, bool overwrite = false)
		{
			foreach (var property in ModelGraphProperties)
			{
				var sourceEntry = property.GetValue(from);
				if (sourceEntry != null)
				{
					var targetEntry = property.GetValue(into);
					if (targetEntry == null)
					{
						property.SetValue(into, sourceEntry);
					}
					else
					{
						var addEntry = targetEntry.GetType().GetMethod("AddEntry");
						addEntry.Invoke(targetEntry, new[] { sourceEntry, overwrite });
					}
				}
			}
		}

		private static readonly PropertyInfo[] ModelGraphProperties = typeof(TModels).GetProperties(BindingFlags.Public | BindingFlags.Instance);
	}
}