namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Type;
    using NUnit.Framework;
    using System.Collections.Generic;
    using System.Linq;
    using GraphQLCore.Execution;

    [TestFixture]
    public class ExecutionContext_NonNull
    {
        private GraphQLSchema schema;

        private enum EnumBasedModel { ONE, TWO, THREE }

        [Test]
        public void Introspection_ClassBasedModelProperty_IsObject()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("OBJECT", GetField(result, "ClassBasedModel").type.kind);
        }

        [Test]
        public void Introspection_EnumTypeProperty_IsNonNull()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("NON_NULL", GetField(result, "EnumTypeProperty").type.kind);
        }

        [Test]
        public void Introspection_EnumTypeProperty_OfTypeIsEnum()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("ENUM", GetField(result, "EnumTypeProperty").type.ofType.kind);
        }

        [Test]
        public void Introspection_IntProperty_IsNonNull()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("NON_NULL", GetField(result, "IntProperty").type.kind);
        }

        [Test]
        public void Introspection_IntProperty_OfTypeIsInt()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("Int", GetField(result, "IntProperty").type.ofType.name);
        }

        [Test]
        public void Introspection_IntProperty_OfTypeIsScalar()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("SCALAR", GetField(result, "IntProperty").type.ofType.kind);
        }

        [Test]
        public void Introspection_NullableIntProperty_IsInt()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("Int", GetField(result, "NullableIntProperty").type.name);
        }

        [Test]
        public void Introspection_NullableIntProperty_IsScalar()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("SCALAR", GetField(result, "NullableIntProperty").type.kind);
        }

        [Test]
        public void Introspection_StringProperty_IsScalar()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("SCALAR", GetField(result, "StringProperty").type.kind);
        }

        [Test]
        public void Introspection_StructBasedModel_IsNonNull()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("NON_NULL", GetField(result, "StructBasedModel").type.kind);
        }

        [Test]
        public void Introspection_StructBasedModel_OfTypeIsObject()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("OBJECT", GetField(result, "StructBasedModel").type.ofType.kind);
        }

        [Test]
        public void Introspection_StructBasedModel_OfTypeNameIsStructBasedModel()
        {
            var result = this.schema.Execute(this.GetIntrospectionQuery());

            Assert.AreEqual("StructBasedModel", GetField(result, "StructBasedModel").type.ofType.name);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            var graphQLClassBasedModel = new GraphQLClassBasedModel();
            var graphQLStructBasedModel = new GraphQLStructBasedModel();
            var graphQLEnumBasedModel = new GraphQLEnumBasedModel();

            this.schema.AddKnownType(graphQLClassBasedModel);
            this.schema.AddKnownType(graphQLStructBasedModel);
            this.schema.AddKnownType(graphQLEnumBasedModel);

            schema.Query(graphQLClassBasedModel);
        }

        private static dynamic GetField(ExecutionResult result, string name)
        {
            return ((IEnumerable<dynamic>)result.Data.__type.fields).SingleOrDefault(e => e.name == name);
        }

        private string GetIntrospectionQuery()
        {
            return "{ __type(name: \"ClassBasedModel\") { fields { name type { name kind ofType { kind name } } } } }";
        }

        private struct StructBasedModel { }

        private class ClassBasedModel
        {
            public int IntProperty { get; set; }
            public int? NullableIntProperty { get; set; }
            public string StringProperty { get; set; }
        }

        private class GraphQLClassBasedModel : GraphQLObjectType<ClassBasedModel>
        {
            public GraphQLClassBasedModel()
                : base("ClassBasedModel", "")
            {
                this.Field("IntProperty", e => e.IntProperty);
                this.Field("NullableIntProperty", e => e.NullableIntProperty);
                this.Field("ClassBasedModel", () => new ClassBasedModel());
                this.Field("StructBasedModel", () => new StructBasedModel());
                this.Field("StringProperty", e => e.StringProperty);
                this.Field("EnumTypeProperty", e => EnumBasedModel.ONE);
            }
        }

        private class GraphQLEnumBasedModel : GraphQLEnumType<EnumBasedModel>
        {
            public GraphQLEnumBasedModel()
                : base("EnumBasedModel", "")
            {
            }
        }

        private class GraphQLStructBasedModel : GraphQLObjectType<StructBasedModel>
        {
            public GraphQLStructBasedModel()
                : base("StructBasedModel", "")
            {
            }
        }
    }
}