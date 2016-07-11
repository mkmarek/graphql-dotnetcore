namespace GraphQLCore.GraphiQLExample.Data
{
    using Models;

    public class Characters
    {
        public readonly Droid Artoo = new Droid()
        {
            Id = "2001",
            AppearsIn = new Episode[] { Episode.EMPIRE, Episode.JEDI, Episode.NEWHOPE },
            Name = "R2-D2",
            PrimaryFunction = "Astromech"
        };

        public readonly Human Han = new Human()
        {
            Id = "1002",
            AppearsIn = new Episode[] { Episode.EMPIRE, Episode.JEDI, Episode.NEWHOPE },
            Name = "Han Solo"
        };

        public readonly Human Leia = new Human()
        {
            Id = "1003",
            AppearsIn = new Episode[] { Episode.EMPIRE, Episode.JEDI, Episode.NEWHOPE },
            Name = "Leia Organa"
        };

        public readonly Human Luke = new Human()
        {
            Id = "1000",
            AppearsIn = new Episode[] { Episode.EMPIRE, Episode.JEDI, Episode.NEWHOPE },
            HomePlanet = "Tatooine",
            Name = "Luke Skywalker"
        };

        public readonly Human Tarkin = new Human()
        {
            Id = "1004",
            AppearsIn = new Episode[] { Episode.EMPIRE },
            Name = "Wilhuff Tarkin"
        };

        public readonly Droid Threepio = new Droid()
        {
            Id = "2000",
            AppearsIn = new Episode[] { Episode.EMPIRE, Episode.JEDI, Episode.NEWHOPE },
            Name = "C-3PO",
            PrimaryFunction = "Protocol"
        };

        public readonly Human Vader = new Human()
        {
            Id = "1001",
            AppearsIn = new Episode[] { Episode.EMPIRE, Episode.JEDI, Episode.NEWHOPE },
            HomePlanet = "Tatooine",
            Name = "Darth Vader"
        };

        public Characters()
        {
            this.Artoo.Friends = new ICharacter[] { Luke, Leia, Han, Threepio };
            this.Threepio.Friends = new ICharacter[] { Luke, Leia, Han };
            this.Tarkin.Friends = new ICharacter[] { Vader };
            this.Leia.Friends = new ICharacter[] { Luke, Han, Artoo, Threepio };
            this.Han.Friends = new ICharacter[] { Luke, Leia, Artoo };
            this.Vader.Friends = new ICharacter[] { Tarkin };
            this.Luke.Friends = new ICharacter[] { Han, Leia, Threepio, Artoo };
        }
    }
}