namespace GraphQLCore.Tests.Utils
{
    using GraphQLCore.Events;
    using GraphQLCore.Language.AST;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Complex;
    using GraphQLCore.Type.Scalar;
    using GraphQLCore.Type.Translation;
    using GraphQLCore.Utils;
    using NUnit.Framework;
    using System;
    using System.Collections.Generic;

    [TestFixture]
    public class PrintSchemaTests
    {
        private GraphQLSchema schema;
        private GraphQLObjectType root;

        [Test]
        public void PrintSchema_PrintsStringField()
        {
            var result = this.PrintSingleFieldSchema<string>();

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField: String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringListField()
        {
            var result = this.PrintSingleFieldSchema<List<string>>();

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField: [String]
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsNonNullStringField()
        {
            var result = this.PrintSingleFieldSchema<NonNullable<string>>();

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField: String!
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringNonNullListField()
        {
            var result = this.PrintSingleFieldSchema<NonNullable<List<string>>>();

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField: [String]!
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsNonNullStringListField()
        {
            var result = this.PrintSingleFieldSchema<List<NonNullable<string>>>();

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField: [String!]
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsNonNullStringNonNullListField()
        {
            var result = this.PrintSingleFieldSchema<NonNullable<List<NonNullable<string>>>>();

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField: [String!]!
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsObjectField()
        {
            new TestObjectType<Foo>("Foo", this.schema)
                .Field("str", () => default(string));
            this.root
                .Field("foo", () => default(Foo));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Foo {
              str: String
            }

            type Root {
              foo: Foo
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithIntArg()
        {
            this.root
                .Field("singleField", (int? argOne) => default(string));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithIntArgWithDefault()
        {
            this.root
                .Field("singleField", (int? argOne) => default(string))
                .WithDefaultValue("argOne", 2);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int = 2): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithIntArgWithDefaultNull()
        {
            this.root
                .Field("singleField", (int? argOne) => default(string))
                .WithDefaultValue("argOne", null);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int = null): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithNonNullIntArg()
        {
            this.root
                .Field("singleField", (int argOne) => default(string));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int!): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithMultipleArgs()
        {
            this.root
                .Field("singleField", (int? argOne, string argTwo) => default(string));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int, argTwo: String): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithMultipleArgsFirstIsDefault()
        {
            this.root
                .Field("singleField", (int? argOne, string argTwo, bool? argThree) => default(string))
                .WithDefaultValue("argOne", 1);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int = 1, argTwo: String, argThree: Boolean): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithMultipleArgsSecondIsDefault()
        {
            this.root
                .Field("singleField", (int? argOne, string argTwo, bool? argThree) => default(string))
                .WithDefaultValue("argTwo", "foo");

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int, argTwo: String = ""foo"", argThree: Boolean): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsStringFieldWithMultipleArgsLastIsDefault()
        {
            this.root
                .Field("singleField", (int? argOne, string argTwo, bool? argThree) => default(string))
                .WithDefaultValue("argThree", false);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Root {
              singleField(argOne: Int, argTwo: String, argThree: Boolean = false): String
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsInterface()
        {
            new TestInterfaceType<IFoo>("Foo", this.schema)
                .Field("str", e => default(string));
            new TestObjectType<BarIFoo>("Bar", this.schema)
                .Field("str", () => default(string));
            this.root
                .Field("bar", () => default(BarIFoo));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Bar implements Foo {
              str: String
            }

            interface Foo {
              str: String
            }

            type Root {
              bar: Bar
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsMultipleInterfaces()
        {
            new TestInterfaceType<IFoo>("Foo", this.schema)
                .Field("str", e => default(string));
            new TestInterfaceType<IBaaz>("Baaz", this.schema)
                .Field("int", e => default(int?));
            var bar = new TestObjectType<BarIFooIBaaz>("Bar", this.schema);
            bar.Field("str", () => default(string));
            bar.Field("int", () => default(int?));
            this.root.Field("bar", () => default(BarIFooIBaaz));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            interface Baaz {
              int: Int
            }

            type Bar implements Foo, Baaz {
              str: String
              int: Int
            }

            interface Foo {
              str: String
            }

            type Root {
              bar: Bar
            }
            ", result);
        }

        [Test]
        public void PrintSchema_PrintsUnions()
        {
            new TestObjectType<Foo>("Foo", this.schema)
                .Field("bool", () => default(bool?));
            new TestObjectType<Bar>("Bar", this.schema)
                .Field("str", () => default(string));
            this.schema
                .AddKnownType(new TestSingleUnionType());
            this.schema
                .AddKnownType(new TestMultipleUnionType());
            this.root
                .Field("single", () => default(object)).ResolveWithUnion<TestSingleUnionType>();
            this.root
                .Field("multiple", () => default(object)).ResolveWithUnion<TestMultipleUnionType>();

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            type Bar {
              str: String
            }

            type Foo {
              bool: Boolean
            }

            union MultipleUnion = Foo | Bar

            type Root {
              single: SingleUnion
              multiple: MultipleUnion
            }

            union SingleUnion = Foo
            ", result);
        }

        [Test]
        public void PrintSchema_PrintsInputType()
        {
            new TestInputObjectType<Foo>("InputType", this.schema)
                .Field("int", e => default(int?));
            this.root
                .Field("str", (Foo argOne) => default(string));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            input InputType {
              int: Int
            }

            type Root {
              str(argOne: InputType): String
            }
            ", result);
        }

        [Test]
        public void PrintSchema_PrintsCustomScalar()
        {
            new TestCustomScalarType(this.schema);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }
 
            scalar Odd
 
            type Root {

            }
            ", result);
        }

        [Test]
        public void PrintSchema_PrintsEnum()
        {
            new TestEnumType<RGBType>("RGB", this.schema);
            this.root
                .Field("rgb", () => default(RGBType?));

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            enum RGB {
              RED
              GREEN
              BLUE
            }

            type Root {
              rgb: RGB
            }
            ", result);
        }

        [Test]
        public void PrintSchema_PrintsIntrospectionSchema()
        {
            this.root
                .Field("onlyField", () => default(string));

            var result = SchemaUtils.PrintIntrospectionSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }

            # Directs the executor to include this field or fragment only when the `if` argument is true.
            directive @include(
              # Included when true.
              if: Boolean!
            ) on FIELD | FRAGMENT_SPREAD | INLINE_FRAGMENT

            # Directs the executor to skip this field or fragment when the `if` argument is true.
            directive @skip(
              # Skipped when true.
              if: Boolean!
            ) on FIELD | FRAGMENT_SPREAD | INLINE_FRAGMENT

            # A Directive provides a way to describe alternate runtime execution and type validation behavior in a GraphQL document.
            #
            # In some cases, you need to provide options to alter GraphQL's execution behavior
            # in ways field arguments will not suffice, such as conditionally including or
            # skipping a field. Directives provide this by describing additional information
            # to the executor.
            type __Directive {
              name: String!
              description: String
              locations: [__DirectiveLocation!]!
              args: [__InputValue!]!
            }

            # A Directive can be adjacent to many parts of the GraphQL language, a
            # __DirectiveLocation describes one such possible adjacencies.
            enum __DirectiveLocation {
              # Location adjacent to a query operation.
              QUERY

              # Location adjacent to a mutation operation.
              MUTATION

              # Location adjacent to a subscription operation.
              SUBSCRIPTION

              # Location adjacent to a field.
              FIELD

              # Location adjacent to a fragment definition.
              FRAGMENT_DEFINITION

              # Location adjacent to a fragment spread.
              FRAGMENT_SPREAD

              # Location adjacent to an inline fragment.
              INLINE_FRAGMENT

              # Location adjacent to a schema definition.
              SCHEMA

              # Location adjacent to a scalar definition.
              SCALAR

              # Location adjacent to an object type definition.
              OBJECT

              # Location adjacent to a field definition.
              FIELD_DEFINITION

              # Location adjacent to an argument definition.
              ARGUMENT_DEFINITION

              # Location adjacent to an interface definition.
              INTERFACE

              # Location adjacent to a union definition.
              UNION

              # Location adjacent to an enum definition.
              ENUM

              # Location adjacent to an enum value definition.
              ENUM_VALUE

              # Location adjacent to an input object type definition.
              INPUT_OBJECT

              # Location adjacent to an input object field definition.
              INPUT_FIELD_DEFINITION
            }

            # One possible value for a given Enum. Enum values are unique values, not a
            # placeholder for a string or numeric value. However an Enum value is returned in
            # a JSON response as a string.
            type __EnumValue {
              name: String!
              description: String
              isDeprecated: Boolean!
              deprecationReason: String
            }

            # Object and Interface types are described by a list of Fields, each of which has
            # a name, potentially a list of arguments, and a return type.
            type __Field {
              name: String!
              description: String
              args: [__InputValue!]!
              type: __Type!
              isDeprecated: Boolean!
              deprecationReason: String
            }

            # Arguments provided to Fields or Directives and the input fields of an
            # InputObject are represented as Input Values which describe their type and
            # optionally a default value.
            type __InputValue {
              name: String!
              description: String
              type: __Type!

              # A GraphQL-formatted string representing the default value for this input value.
              defaultValue: String
            }

            # A GraphQL Schema defines the capabilities of a GraphQL server. It exposes all
            # available types and directives on the server, as well as the entry points for
            # query, mutation, and subscription operations.
            type __Schema {
              # A list of all types supported by this server.
              types: [__Type!]!

              # The type that query operations will be rooted at.
              queryType: __Type!

              # If this server supports mutation, the type that mutation operations will be rooted at.
              mutationType: __Type

              # If this server support subscription, the type that subscription operations will be rooted at.
              subscriptionType: __Type

              # A list of all directives supported by this server.
              directives: [__Directive!]!
            }

            # The fundamental unit of any GraphQL Schema is the type. There are many kinds of
            # types in GraphQL as represented by the `__TypeKind` enum.
            #
            # Depending on the kind of a type, certain fields describe information about that
            # type. Scalar types provide no information beyond a name and description, while
            # Enum types provide their values. Object and Interface types provide the fields
            # they describe. Abstract types, Union and Interface, provide the Object types
            # possible at runtime. List and NonNull types compose other types.
            type __Type {
              kind: __TypeKind!
              name: String
              description: String
              fields(includeDeprecated: Boolean = false): [__Field!]
              interfaces: [__Type!]
              possibleTypes: [__Type!]
              enumValues(includeDeprecated: Boolean = false): [__EnumValue!]
              inputFields: [__InputValue!]
              ofType: __Type
            }

            # An enum describing what kind of type a given `__Type` is.
            enum __TypeKind {
              # Indicates this type is a scalar.
              SCALAR

              # Indicates this type is an object. `fields` and `interfaces` are valid fields.
              OBJECT

              # Indicates this type is an interface. `fields` and `possibleTypes` are valid fields.
              INTERFACE

              # Indicates this type is a union. `possibleTypes` is a valid field.
              UNION

              # Indicates this type is an enum. `enumValues` is a valid field.
              ENUM

              # Indicates this type is an input object. `inputFields` is a valid field.
              INPUT_OBJECT

              # Indicates this type is a list. `ofType` is a valid field.
              LIST

              # Indicates this type is a non-null. `ofType` is a valid field.
              NON_NULL
            }
            ", result);
        }

        [Test]
        public void PrintSchema_PrintsOperationDefinitions()
        {
            this.schema = new GraphQLSchema();

            var query = new TestObjectType("QueryRoot", this.schema);
            query.Field("foo", () => default(string));
            this.schema.Query(query);

            var mutation = new TestMutationType("MutationRoot", this.schema);
            mutation.Field("foo", () => default(int));
            this.schema.Mutation(mutation);

            var subscription = new TestSubscriptionType("SubscriptionRoot", this.schema);
            subscription.Field("foo", () => default(ID));
            this.schema.Subscription(subscription);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: QueryRoot
              mutation: MutationRoot
              subscription: SubscriptionRoot
            }

            type MutationRoot {
              foo: Int!
            }

            type QueryRoot {
              foo: String
            }

            type SubscriptionRoot {
              foo: ID!
            }", result);
        }

        [Test]
        public void PrintSchema_DoesntPrintSchemaOfCommonNames()
        {
            this.schema = new GraphQLSchema();
            
            var query = new TestObjectType("Query", this.schema);
            query.Field("foo", () => default(string));
            this.schema.Query(query);

            var mutation = new TestMutationType("Mutation", this.schema);
            mutation.Field("foo", () => default(int));
            this.schema.Mutation(mutation);

            var subscription = new TestSubscriptionType("Subscription", this.schema);
            subscription.Field("foo", () => default(ID));
            this.schema.Subscription(subscription);

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            type Mutation {
              foo: Int!
            }

            type Query {
              foo: String
            }

            type Subscription {
              foo: ID!
            }", result);
        }

        [Test]
        public void PrintSchema_PrintsDeprecated()
        {
            new TestInputObjectType<Foo>("Input", this.schema)
                .Field("int", e => default(int?))
                .IsDeprecated("because reasons");
            this.root
                .Field("dep1", () => default(string))
                .IsDeprecated();
            this.root
                .Field("dep2", () => default(string))
                .IsDeprecated("");

            var result = SchemaUtils.PrintSchema(this.schema);

            this.AreEqual(@"
            schema {
              query: Root
            }
            
            input Input {
              int: Int @deprecated(reason: ""because reasons"")
            }

            type Root {
              dep1: String @deprecated
              dep2: String @deprecated
            }", result);
        }

        [SetUp]
        public void SetUp()
        {
            this.schema = new GraphQLSchema();
            this.root = new QueryRootType(this.schema);
        }

        private void AreEqual(string expected, string actual)
        {
            Assert.AreEqual(StringUtils.Dedent(expected), actual);
        }

        private string PrintSingleFieldSchema<T>()
        {
            this.root.Field("singleField", () => default(T));

            return SchemaUtils.PrintSchema(this.schema);
        }

        private class TestObjectType : GraphQLObjectType
        {
            public TestObjectType(string name, GraphQLSchema schema) : base(name, null)
            {
                schema.AddKnownType(this);
            }
        }

        private class TestObjectType<T> : GraphQLObjectType<T>
        {
            public TestObjectType(string name, GraphQLSchema schema) : base(name, null)
            {
                schema.AddKnownType(this);
            }
        }

        private class TestInputObjectType<T> : GraphQLInputObjectType<T>
            where T : class, new()
        {
            public TestInputObjectType(string name, GraphQLSchema schema) : base(name, null)
            {
                schema.AddKnownType(this);
            }
        }

        private class TestInterfaceType<T> : GraphQLInterfaceType<T>
            where T : class
        {
            public TestInterfaceType(string name, GraphQLSchema schema) : base(name, null)
            {
                schema.AddKnownType(this);
            }
        }

        private class TestSingleUnionType : GraphQLUnionType
        {
            public TestSingleUnionType() : base("SingleUnion", null)
            {
                this.AddPossibleType(typeof(Foo));
            }

            public override Type ResolveType(object data)
            {
                return null;
            }
        }

        private class TestMultipleUnionType : GraphQLUnionType
        {
            public TestMultipleUnionType() : base("MultipleUnion", null)
            {
                this.AddPossibleType(typeof(Foo));
                this.AddPossibleType(typeof(Bar));
            }

            public override Type ResolveType(object data)
            {
                return null;
            }
        }

        private class TestCustomScalarType : GraphQLScalarType
        {
            public TestCustomScalarType(GraphQLSchema schema) : base("Odd", null)
            {
                schema.AddKnownType(this);
            }

            public override Result GetValueFromAst(GraphQLValue astValue, ISchemaRepository schemaRepository)
            {
                return null;
            }

            protected override GraphQLValue GetAst(object value, ISchemaRepository schemaRepository)
            {
                return null;
            }
        }

        private class TestEnumType<T> : GraphQLEnumType<T>
            where T : struct, IConvertible
        {
            public TestEnumType(string name, GraphQLSchema schema) : base(name, null)
            {
                schema.AddKnownType(this);
            }
        }

        private class TestMutationType : GraphQLObjectType
        {
            public TestMutationType(string name, GraphQLSchema schema) : base(name, null)
            {
                schema.AddKnownType(this);
            }
        }

        private class TestSubscriptionType : GraphQLSubscriptionType
        {
            public TestSubscriptionType(string name, GraphQLSchema schema) : base(name, null, new InMemoryEventBus())
            {
                schema.AddKnownType(this);
            }
        }

        private class QueryRootType : GraphQLObjectType
        {
            public QueryRootType(GraphQLSchema schema) : base("Root", "")
            {
                schema.AddKnownType(this);
                schema.Query(this);
            }
        }

        private interface IFoo { }
        private interface IBaaz { }

        private class Foo { }
        private class Bar { }
        private class BarIFoo : IFoo { }
        private class BarIFooIBaaz : IFoo, IBaaz { }

        private enum RGBType { RED, GREEN, BLUE }
    }
}

