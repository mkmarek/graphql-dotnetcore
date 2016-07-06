using System.Linq;

namespace GraphQLCore.Type.Introspection
{
    public class ComplexIntrospectedType : IntrospectedType
    {
        private IIntrospector introspector;
        private IObjectTypeTranslator typeObserver;

        internal ComplexIntrospectedType(IIntrospector introspector, IObjectTypeTranslator typeObserver)
        {
            this.introspector = introspector;
            this.typeObserver = typeObserver;
        }

        public override IntrospectedField[] Fields
        {
            get
            {
                return typeObserver.GetFields()
                    .Select(e => introspector.IntrospectField(e))
                    .ToArray();
            }
        }

        public override IntrospectedType[] Interfaces
        {
            get
            {
                return typeObserver.GetImplementingInterfaces()
                    .Select(e => introspector.Introspect(e))
                    .ToArray();
            }
        }

        public override IntrospectedType[] PossibleTypes
        {
            get
            {
                return typeObserver.GetPossibleTypes()
                    .Select(e => introspector.Introspect(e))
                    .ToArray();
            }
        }
    }
}