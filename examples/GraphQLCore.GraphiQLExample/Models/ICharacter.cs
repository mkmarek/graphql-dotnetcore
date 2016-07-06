namespace GraphQLCore.GraphiQLExample.Models
{
    using System.Collections.Generic;

    public interface ICharacter
    {
        IEnumerable<Episode> AppearsIn { get; set; }
        IEnumerable<ICharacter> Friends { get; set; }
        string Id { get; set; }
        string Name { get; set; }
        string SecretBackstory { get; set; }
    }
}