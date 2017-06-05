namespace GraphQLCore.GraphiQLExample.Models
{
    using System.Collections.Generic;
    using Type.Scalar;

    public interface ICharacter
    {
        IEnumerable<Episode> AppearsIn { get; set; }
        IEnumerable<ICharacter> Friends { get; set; }
        ID Id { get; set; }
        string Name { get; set; }
    }
}