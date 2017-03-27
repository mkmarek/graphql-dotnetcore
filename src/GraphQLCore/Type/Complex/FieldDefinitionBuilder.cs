using System;
using System.Collections.Generic;
using System.Text;

namespace GraphQLCore.Type.Complex
{
    public class FieldDefinitionBuilder
    {
        private GraphQLObjectTypeFieldInfo fieldInfo;

        public FieldDefinitionBuilder(GraphQLObjectTypeFieldInfo fieldInfo)
        {
            this.fieldInfo = fieldInfo;
        }

        public FieldDefinitionBuilder ResolveWithUnion<TUnionType>()
            where TUnionType : GraphQLUnionType
        {
            this.fieldInfo.SystemType = typeof(TUnionType);

            return this;
        }
    }
}
