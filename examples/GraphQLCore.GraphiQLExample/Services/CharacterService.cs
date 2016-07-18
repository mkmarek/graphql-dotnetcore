namespace GraphQLCore.GraphiQLExample.Services
{
    using Data;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CharacterService
    {
        private List<ICharacter> characterList;
        private Characters characters = new Characters();

        public CharacterService()
        {
            this.characterList = new ICharacter[] {
                characters.Artoo, characters.Han, characters.Leia, characters.Luke,
                characters.Tarkin, characters.Threepio, characters.Vader
            }.ToList();
        }

        public Droid GetDroidById(string id)
        {
            return characterList.SingleOrDefault(e => e.Id == id) as Droid;
        }

        public Human GetHumanById(string id)
        {
            return characterList.SingleOrDefault(e => e.Id == id) as Human;
        }

        public IEnumerable<ICharacter> List(Episode episode)
        {
            return characterList.Where(e => e.AppearsIn.Contains(episode));
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
    }
}