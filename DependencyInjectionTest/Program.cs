﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectBuilder;

namespace DependencyInjectionTest
{
	public class Program
	{
		static void Main(string[] args)
		{
			var states = new ModelStates
			{
				Akten = new List<Akte_State>
				{
					new Akte_State { Id = 1, MandantId = 1, Status = Aktenstatus_State.InBearbeitung },
					new Akte_State { Id = 2, MandantId = 1, Status = Aktenstatus_State.Abgeschlossen },
				},
				Personen = new List<Person_State>
				{
					new Person_State { Id = 1, FirstName = "Manuel", LastName = "Naujoks" },
				},
				Einstellungen = new List<Einstellungen_State>
				{
					new Einstellungen_State { Setting1 = true, Setting2 = false },
				}
			};


			var graph = Tests.Instantiate(states);
			var akte = graph.Akten[2];
			akte.RechnungStellen();
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
					new Akte_State { Id = 1, MandantId = 1, Status = Aktenstatus_State.InBearbeitung },
					new Akte_State { Id = 2, MandantId = 1, Status = Aktenstatus_State.Abgeschlossen },
				},
				Personen = new List<Person_State>
				{
					new Person_State { Id = 1, FirstName = "Manuel", LastName = "Naujoks" },
					new Person_State { Id = 2, FirstName = "Manuel", LastName = "Naujoks", ParentId = 1},
				},
				Einstellungen = new List<Einstellungen_State>
				{
					new Einstellungen_State { Setting1 = true, Setting2 = false },
				}
			};


			var graph = Instantiate(states);


			Assert.AreEqual("Manuel", graph.Akten.Values.ElementAt(0).Mandant.FirstName);
			Assert.AreEqual(1, graph.Akten.Values.ElementAt(0).State.Id);
			Assert.AreEqual(2, graph.Akten.Values.ElementAt(1).State.Id);
			Assert.AreEqual(graph.Akten.Values.ElementAt(0).Mandant, graph.Akten.Values.ElementAt(1).Mandant);

			Assert.IsNotNull(graph.Akten.Values.First().Status);

			Assert.AreEqual(true, graph.Akten.Values.ElementAt(0).Einstellungen.Einstellung1);
			Assert.AreEqual(graph.Akten.Values.ElementAt(0).Einstellungen, graph.Akten.Values.ElementAt(1).Einstellungen);

			Assert.IsNull(graph.Personen.Values.ElementAt(0).Eltern);
			Assert.AreEqual(graph.Personen.Values.ElementAt(0), graph.Personen.Values.ElementAt(1).Eltern);
		}

		public static ModelGraph Instantiate(ModelStates modelStates)
		{
			var factory = ObjectFactory.From<ModelStates>()
				.SetupCreation(states => new ModelGraph
				{
					Akten = states.Akten.ToDictionary(s => s.Id, s => new Akte(s)),
					Personen = states.Personen.ToDictionary(s => s.Id, s => new Person(s)),
					Einstellungen = states.Einstellungen.ToDictionary(s => 0, s => new Einstellungen(s)),
					Aktenstatus = new Dictionary<Aktenstatus_State, Aktenstatus>
					{
						{ Aktenstatus_State.potenziell, new Aktenstatus(Aktenstatus_State.potenziell) },
						{ Aktenstatus_State.InBearbeitung, new Aktenstatus(Aktenstatus_State.InBearbeitung) },
						{ Aktenstatus_State.Abgeschlossen, new Aktenstatus(Aktenstatus_State.Abgeschlossen) },
						{ Aktenstatus_State.abgelehnt, new Aktenstatus(Aktenstatus_State.abgelehnt) }
					},
				})
				.SetupComposition(compose =>
				{
					/*
					 * TODO:
					 * 
					 * better assign syntax
					 * 
					 */

					compose.One(g => g.Aktenstatus).HasMany(g => g.Akten, b => b.State.Status)
						.Assign(
							init: null,
							addMany: null,
							setOne: (a, b) => b.Status = a);

					compose.One(g => g.Personen).HasMany(g => g.Akten, b => b.State.MandantId)
						.Assign(
							init: a => a.Akten = new List<Akte>(),
							addMany: (a, b) => a.Akten.Add(b),
							setOne: (a, b) => b.Mandant = a);

					compose.One(g => g.Personen).HasMany(g => g.Personen, b => b.State.ParentId)
						.Assign(
							init: null,
							addMany: null,
							setOne: (a, b) => b.Eltern = a);

					compose.One(g => g.Akten).HasMany(g => g.Einstellungen)
						.Assign((a, b) => a.Einstellungen = b.Values.Single());
				});

			return factory.Create(modelStates);
		}
	}
















	public class Akte
	{
		public Akte_State State { get; private set; }
		public Akte(Akte_State state) { State = state; }

		public Aktenstatus Status { get; set; }
		public Einstellungen Einstellungen { get; set; }
		public Person Mandant { get; set; }

		public void RechnungStellen()
		{
			Console.WriteLine(string.Format("Rechnung an: {0} {1}; Akte ist {2}", Mandant.FirstName, Mandant.LastName, Status.State));
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

		public string FirstName { get { return State.FirstName; } }

		public string LastName { get { return State.LastName; } }

		public List<Akte> Akten { get; set; }

		public Person Eltern { get; set; }
	}

	public class Einstellungen
	{
		public Einstellungen_State State { get; private set; }
		public Einstellungen(Einstellungen_State state) { State = state; }

		public bool Einstellung1 { get { return State.Setting1; } }
		public bool Einstellung2 { get { return State.Setting2; } }
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
		public int? ParentId { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
	}

	[Serializable]
	[DataContract]
	public class Einstellungen_State
	{
		public bool Setting1 { get; set; }
		public bool Setting2 { get; set; }
	}













	public class ModelGraph
	{
		public Dictionary<int, Akte> Akten { get; set; }
		public Dictionary<Aktenstatus_State, Aktenstatus> Aktenstatus { get; set; }
		public Dictionary<int, Person> Personen { get; set; }
		public Dictionary<int, Einstellungen> Einstellungen { get; set; }

	}

	[DataContract]
	public class ModelStates
	{
		[DataMember]
		public List<Akte_State> Akten { get; set; }
		[DataMember]
		public List<Person_State> Personen { get; set; }
		[DataMember]
		public List<Einstellungen_State> Einstellungen { get; set; }
	}
}
