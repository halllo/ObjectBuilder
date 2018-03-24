ObjectBuilder
=============
Loading objects from a persistance layer and efficiently turning them into a mutually referencing domain object graph is a challenge. ObjectBuilder does exactly that for you. It instantiates objects and connects them to each other based on your state objects and their foreign keys. It feels just like an object relational mapper but gives you all the flexibility you need.

All you need to do is to define a mapping from your state objects (those you probably get from a persistance layer or from the client) and your domain model objects (where your logic is).
```csharp
builder = Factory.From<MyModelStates>()
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
...
```

Once you have this mapping, you can use it to turn your flat state objects into deeply connected domain objects.
```csharp
var states = new MyModelStates
{
   Akten = new List<Akte_State>
   {
      new Akte_State { Id = 1, MandantId = 1, Status = Aktenstatus_State.InBearbeitung },
   },
   Personen = new List<Person_State>
   {
      new Person_State { Id = 1, FirstName = "", LastName = "Very First Person" },
      new Person_State { Id = 2, FirstName = "Manuel", LastName = "Naujoks", ParentId = 1},
   },
   Einstellungen = new List<Einstellungen_State>
   {
      new Einstellungen_State { Setting1 = true, Setting2 = false },
   }
};

var graph = builder.CreateModelsAndCompose(states);

var akte = graph.Akten.GetById(1);
...
```