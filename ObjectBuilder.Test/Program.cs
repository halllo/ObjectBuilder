using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ObjectBuilder.Test
{
	public class Program
	{
		static void Main(string[] args)
		{
			var states = new MyModelStates
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


			var graph = ObjectGraph.Builder.CreateModelsAndCompose(states);
			var akte = graph.Akten.GetById(2);
		}
	}




	[TestClass]
	public class Tests
	{
		[TestMethod]
		public void AkteMitMandant()
		{
			var states = new MyModelStates
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


			var graph = ObjectGraph.Builder.CreateModelsAndCompose(states);


			Assert.AreEqual("Manuel", graph.Akten.GetById(1).Mandant.FirstName);
			Assert.AreEqual(1, graph.Akten.GetById(1).State.Id);
			Assert.AreEqual(2, graph.Akten.GetById(2).State.Id);
			Assert.AreEqual(graph.Akten.GetById(1).Mandant, graph.Akten.GetById(2).Mandant);

			Assert.IsNotNull(graph.Akten.ModelsById.First().Value.Status);

			Assert.AreEqual(true, graph.Akten.GetById(1).Einstellungen.Einstellung1);
			Assert.AreEqual(graph.Akten.GetById(1).Einstellungen, graph.Akten.GetById(2).Einstellungen);

			Assert.IsNull(graph.Personen.GetById(1).Eltern);
			Assert.AreEqual(graph.Personen.GetById(1), graph.Personen.GetById(2).Eltern);
		}

		[TestMethod]
		public void AkteErzeugtRechnung()
		{
			var states = new MyModelStates
			{
				Akten = new List<Akte_State>
				{
					new Akte_State { Id = 1, MandantId = 1, Status = Aktenstatus_State.InBearbeitung },
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


			var graph = ObjectGraph.Builder.CreateModelsAndCompose(states);

			var akte = graph.Akten.GetById(1);
			var rechnungen = akte.NeueRechnungen(ObjectGraph.Builder.Composer(graph)).ToList();


			Assert.IsNotNull(rechnungen[0].Einstellungen);
			Assert.IsNotNull(rechnungen[1].Einstellungen);
			Assert.IsNotNull(rechnungen[2].Einstellungen);
			Assert.AreEqual(akte, rechnungen[0].Akte);
			Assert.AreEqual(akte, rechnungen[1].Akte);
			Assert.AreEqual(akte, rechnungen[2].Akte);
		}
	}






































	public class Akte
	{
		public Akte_State State { get; private set; }
		public Akte(Akte_State state) { State = state; }

		public Aktenstatus Status { get; set; }
		public Einstellungen Einstellungen { get; set; }
		public Person Mandant { get; set; }
		public List<Rechnung> Rechnungen { get; set; }

		public IEnumerable<Rechnung> NeueRechnungen(IModelComposer composer)
		{
			yield return composer.Compose(new Rechnung(new Rechnung_State { Id = composer.TemporaryId, AkteId = State.Id }));
			yield return composer.Compose(new Rechnung(new Rechnung_State { Id = composer.TemporaryId, AkteId = State.Id }));
			yield return composer.Compose(new Rechnung(new Rechnung_State { Id = composer.TemporaryId, AkteId = State.Id }));
		}
	}

	public class Rechnung
	{
		public Rechnung_State State { get; private set; }
		public Rechnung(Rechnung_State state) { State = state; }

		public Akte Akte { get; set; }
		public Einstellungen Einstellungen { get; set; }
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

		public Person Eltern { get; set; }
	}

	public class Einstellungen
	{
		public Einstellungen_State State { get; private set; }
		public Einstellungen(Einstellungen_State state) { State = state; }

		public bool Einstellung1 => State.Setting1;
		public bool Einstellung2 => State.Setting2;
	}

















	[Serializable]
	[DataContract]
	public class Akte_State
	{
		public int Id { get; set; }
		public int MandantId { get; set; }
		public Aktenstatus_State Status { get; set; }
	}

	[Serializable]
	[DataContract]
	public class Rechnung_State
	{
		public int Id { get; set; }
		public int AkteId { get; set; }
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





























	public class MyModels
	{
		public IModelGraphEntry<Akte, int> Akten { get; set; }
		public IModelGraphEntry<Rechnung, int> Rechnungen { get; set; }
		public IModelGraphEntry<Aktenstatus, Aktenstatus_State> Aktenstatus { get; set; }
		public IModelGraphEntry<Person, int> Personen { get; set; }
		public IModelGraphEntry<Einstellungen, int> Einstellungen { get; set; }
	}

	[DataContract]
	public class MyModelStates
	{
		[DataMember]
		public List<Akte_State> Akten { get; set; }
		[DataMember]
		public List<Rechnung_State> Rechnungen { get; set; }
		[DataMember]
		public List<Person_State> Personen { get; set; }
		[DataMember]
		public List<Einstellungen_State> Einstellungen { get; set; }
	}


	public class ObjectGraph
	{
		public static ObjectBuilder<MyModelStates, MyModels> Builder { get; }
		public static IModelGraphEntry<Aktenstatus, Aktenstatus_State> Aktenstatus { get; }

		static ObjectGraph()
		{
			Aktenstatus = Factory.Constant(new Dictionary<Aktenstatus_State, Aktenstatus>
			{
				{Aktenstatus_State.potenziell, new Aktenstatus(Aktenstatus_State.potenziell)},
				{Aktenstatus_State.InBearbeitung, new Aktenstatus(Aktenstatus_State.InBearbeitung)},
				{Aktenstatus_State.Abgeschlossen, new Aktenstatus(Aktenstatus_State.Abgeschlossen)},
				{Aktenstatus_State.abgelehnt, new Aktenstatus(Aktenstatus_State.abgelehnt)}
			}, m => m.State);

			Builder = Factory.From<MyModelStates>()
				.Create(states =>
				{
					return new MyModels
					{
						Aktenstatus = Aktenstatus,
						Akten = Factory.Entry(states.Akten, s => new Akte(s), m => m.State.Id),
						Rechnungen = Factory.Entry(states.Rechnungen, s => new Rechnung(s), m => m.State.Id),
						Personen = Factory.Entry(states.Personen, s => new Person(s), m => m.State.Id),
						Einstellungen = Factory.Entry(states.Einstellungen, s => new Einstellungen(s), m => 0),
					};
				})
				.Compose(compose =>
				{
					compose.One(g => g.Akten)
						.HasMany(g => g.Rechnungen,
							a => a.State.Id,
							r => r.State.AkteId,
							a => a.Rechnungen = new List<Rechnung>(),
							(a, r) => a.Rechnungen.Add(r))
						.HasOne(g => g.Aktenstatus,
							s => (Aktenstatus_State?)s.State.Status,
							(a, s) => a.Status = s)
						.HasOne(g => g.Personen,
							p => (int?)p.State.MandantId,
							(a, p) => a.Mandant = p)
						.HasImplicit(g => g.Einstellungen,
							(a, e) => a.Einstellungen = e.Single());

					compose.One(g => g.Rechnungen)
						.HasOne(g => g.Akten,
							r => (int?)r.State.AkteId,
							(r, a) => r.Akte = a)
						.HasImplicit(g => g.Einstellungen,
							(r, e) => r.Einstellungen = e.Single());

					compose.One(g => g.Personen)
						.HasMany(g => g.Akten,
							p => p.State.Id,
							a => a.State.MandantId,
							p => p.Akten = new List<Akte>(),
							(p, a) => p.Akten.Add(a))
						.HasOne(g => g.Personen,
							p => (int?)p.State.ParentId,
							(p, p2) => p.Eltern = p2);
				});
		}
	}
}
