namespace GraphQLCore.Type
{
    using GraphQLCore.Type.Introspection;
    using System.Linq;

    public class IntrospectedInputObject : IntrospectedType
    {
        private IIntrospector introspector;

        private IObjectTypeTranslator typeObserver;

        internal IntrospectedInputObject(IIntrospector introspector, IObjectTypeTranslator typeObserver)
        {
            this.introspector = introspector;
            this.typeObserver = typeObserver;
        }

        public override IntrospectedInputValue[] InputFields
        {
            get
            {
                return this.typeObserver.GetFields()
                    .Select(e => this.introspector.IntrospectInputValue(e))
                    .ToArray();
            }
        }
    }
}