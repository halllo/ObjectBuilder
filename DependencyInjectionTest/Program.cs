using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using DependencyInjector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DependencyInjectionTest
{
	public class Program
	{
		static void Main(string[] args)
		{
		}
	}

	[TestClass]
	public class Tests
	{
		[TestMethod]
		public void AkteMitMandant()
		{
			var states = new ModelStates
			{
				Akten = new List<Akte_State>
				{
					new Akte_State { Id = 1, MandantId = 1, Status = Aktenstatus_State.InBearbeitung }
				},
				Personen = new List<Person_State>
				{
					new Person_State { Id = 1, FirstName = "Manuel", LastName = "Naujoks" }
				},
			};


			var graph = Instantiate(states);


			Assert.AreEqual("Manuel", graph.Akten.First().Mandant.FirstName);
			Assert.IsNotNull(graph.Akten.First().Status);
		}

		private static ModelGraph Instantiate(ModelStates states)
		{
			var mainGraphProcessor = new MainGraphProcessor(new[]
			{
				typeof(AkteComposer),
				typeof(AktenstatusComposer),
				typeof(PersonComposer),
			});
			var graph = mainGraphProcessor.CreateModelsAndCompose(states);
			return graph;
		}
	}
















	public class Akte
	{
		public Akte_State State { get; private set; }
		public Akte(Akte_State state) { State = state; }

		public Aktenstatus Status { get; set; }

		public Person Mandant { get; set; }

		public void RechnungStellen()
		{
			Console.WriteLine($"Rechnung an: {Mandant.FirstName} {Mandant.LastName}; Akte ist {Status.State}");
		}
	}

	public class Aktenstatus
	{
		public Aktenstatus_State State { get; private set; }
		public Aktenstatus(Aktenstatus_State state) { State = state; }
	}

	public class Person
	{
		public Person_State State { get; private set; }
		public Person(Person_State state) { State = state; }

		public string FirstName => State.FirstName;
		public string LastName => State.LastName;

		public List<Akte> Akten { get; set; }
	}

















	[Serializable]
	[DataContract]
	public class Akte_State
	{
		public int Id { get; set; }
		public int MandantId { get; set; }
		public Aktenstatus_State Status { get; set; }
	}

	[DataContract]
	public enum Aktenstatus_State
	{
		[EnumMember]
		potenziell,
		[EnumMember]
		InBearbeitung,
		[EnumMember]
		Abgeschlossen,
		[EnumMember]
		abgelehnt
	}

	[Serializable]
	[DataContract]
	public class Person_State
	{
		public int Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}















	public class ModelGraph
	{
		public ModelGraphEntry<Akte, int> Akten { get; set; }
		public ModelGraphEntry<Aktenstatus, Aktenstatus_State> Aktenstatus { get; set; }
		public ModelGraphEntry<Person, int> Personen { get; set; }

	}

	[DataContract]
	public class ModelStates
	{
		[DataMember]
		public List<Akte_State> Akten { get; set; }
		[DataMember]
		public List<Person_State> Personen { get; set; }
	}
















	public class AktenstatusComposer : Composer
	{
		public override void Init()
		{
			AddForeignKeyRelation(
				g => g.Aktenstatus,
				g => g.Akten,
				b => (Aktenstatus_State?)b.State.Status,
				null,
				null,
				(a, b) => b.Status = a);
		}
	}
	public class AkteComposer : Composer
	{
		public override void Init()
		{
		}
	}
	public class PersonComposer : Composer
	{
		public override void Init()
		{
			AddForeignKeyRelation(
				g => g.Personen,
				g => g.Akten,
				b => (int?)b.State.MandantId,
				a => a.Akten = new List<Akte>(),
				(a, b) => a.Akten.Add(b),
				(a, b) => b.Mandant = a);
		}
	}
}
