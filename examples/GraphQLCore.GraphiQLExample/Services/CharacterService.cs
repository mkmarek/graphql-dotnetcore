namespace GraphQLCore.GraphiQLExample.Services
{
    using Data;
    using Models;
    using System.Collections.Generic;
    using System.Linq;
    using System;

    public class CharacterService
    {
        private static readonly Characters characters = new Characters();
        private static readonly List<ICharacter> characterList = GetList().ToList();

        public Droid GetDroidById(string id)
        {
            return characterList.SingleOrDefault(e => e.Id == id) as Droid;
        }

        public Human GetHumanById(string id)
        {
            return characterList.SingleOrDefault(e => e.Id == id) as Human;
        }

        internal Droid CreateDroid(Droid droid)
        {
            var model = new Droid()
            {
                Id = Guid.NewGuid().ToString(),
                AppearsIn = droid.AppearsIn,
                Name = droid.Name,
                PrimaryFunction = droid.PrimaryFunction
            };

            characterList.Add(model);

            return model;
        }

        public IEnumerable<ICharacter> List(Episode episode)
        {
            return characterList.Where(e => e.AppearsIn.Contains(episode));
        }

        private static IEnumerable<ICharacter> GetList()
        {
            return new ICharacter[] {
                characters.Artoo, characters.Han, characters.Leia, characters.Luke,
                characters.Tarkin, characters.Threepio, characters.Vader
            };
        }
    }
}