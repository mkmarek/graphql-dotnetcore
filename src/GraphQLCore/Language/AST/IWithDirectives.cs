using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GraphQLCore.Language.AST
{
    public interface IWithDirectives
    {
        IEnumerable<GraphQLDirective> Directives { get; set; }
    }
}
