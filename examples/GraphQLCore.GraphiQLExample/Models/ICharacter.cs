namespace GraphQLCore.GraphiQLExample.Models
{
    using System.Collections.Generic;

    public interface ICharacter
    {
        string Id { get; set; }
        string Name { get; set; }
        IEnumerable<ICharacter> Friends { get; set; }
        IEnumerable<Episode> AppearsIn { get; set; }
        string SecretBackstory { get; set; }
    }
}
