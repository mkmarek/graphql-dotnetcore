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
                return this.typeObserver.GetFields()
                    .Select(e => this.introspector.IntrospectField(e))
                    .ToArray();
            }
        }

        public override IntrospectedType[] Interfaces
        {
            get
            {
                return this.typeObserver.GetImplementingInterfaces()
                    .Select(e => this.introspector.Introspect(e))
                    .ToArray();
            }
        }

        public override IntrospectedType[] PossibleTypes
        {
            get
            {
                return this.typeObserver.GetPossibleTypes()
                    .Select(e => this.introspector.Introspect(e))
                    .ToArray();
            }
        }
    }
}