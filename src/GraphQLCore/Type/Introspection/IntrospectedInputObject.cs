namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Introspection;
    using System.Linq;

    public class IntrospectedInputObject : IntrospectedType
    {
        public override IntrospectedInputValue[] InputFields
        {
            get
            {
                return typeObserver.GetFields()
                    .Select(e => introspector.IntrospectInputValue(e))
                    .ToArray();
            }
        }

        private IIntrospector introspector;
        private IObjectTypeTranslator typeObserver;

        internal IntrospectedInputObject(IIntrospector introspector, IObjectTypeTranslator typeObserver)
        {
            this.introspector = introspector;
            this.typeObserver = typeObserver;
        }
    }
}