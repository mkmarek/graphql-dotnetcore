namespace GraphQLCore.Tests.Type
{
    using GraphQLCore;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Complex;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using System.Collections;
    using System.Reflection;
    using System.Collections.Generic;

    [TestFixture]
    public class GraphQLUnionTypeTests
    {
        public class TestUnionType : GraphQLUnionType
        {
            public TestUnionType() : base("TestUnion", "Some union description")
            {
                this.AddPossibleType(typeof(CatSchemaType));
                this.AddPossibleType(typeof(DogSchemaType));
            }

            public override Type ResolveType(object data)
            {
                if (data is Cat)
                    return typeof(Cat);
                else if (data is Dog)
                    return typeof(Dog);

                return null;
            }
        }

        public class Cat
        {
            public int Id {get; set;}
            public string Name {get; set;}
            public string WhatDoesTheCatSay {get; set;}
        }

        public class Dog
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string WhatDoesTheDogSay { get; set; }
        }

        public class Chicken
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string WhatDoesTheChickenSay { get; set; }
        }

        public class CatSchemaType : GraphQLObjectType<Cat>
        {
            public CatSchemaType() : base("Cat", string.Empty)
            {
                this.Field("id", e => e.Id);
                this.Field("name", e => e.Name);
                this.Field("whatDoesTheCatSay", e => e.WhatDoesTheCatSay);
            }
        }

        public class DogSchemaType : GraphQLObjectType<Dog>
        {
            public DogSchemaType() : base("Dog", string.Empty)
            {
                this.Field("id", e => e.Id);
                this.Field("name", e => e.Name);
                this.Field("whatDoesTheDogSay", e => e.WhatDoesTheDogSay);
            }
        }

        public class ChickenSchemaType : GraphQLObjectType<Chicken>
        {
            public ChickenSchemaType() : base("Chicken", string.Empty)
            {
                this.Field("id", e => e.Id);
                this.Field("name", e => e.Name);
                this.Field("whatDoesTheChickenSay", e => e.WhatDoesTheChickenSay);
            }
        }

        public class TestRootType : GraphQLObjectType
        {
            public TestRootType() : base("Query", string.Empty)
            {
                this.Field("animal", () => new Cat()
                {
                    Id = 1,
                    Name = "Mourek",
                    WhatDoesTheCatSay = "Meow"
                }).ResolveWithUnion<TestUnionType>();

                this.Field("unionReturningInvalidType", () => new Chicken() { }).ResolveWithUnion<TestUnionType>(); ;
            }
        }

        public class TestSchema : GraphQLSchema
        {
            public TestSchema()
            {
                var query = new TestRootType();

                this.AddKnownType(new ChickenSchemaType());
                this.AddKnownType(new DogSchemaType());
                this.AddKnownType(new CatSchemaType());
                this.AddKnownType(new TestUnionType());
                this.AddKnownType(query);
                this.Query(query);
            }
        }

        [Test]
        public void CanReturnDataFromResolver()
        {
            var schema = new TestSchema();

            var result = schema.Execute(@"
            {
                animal {
                    __typename
                    ... on Cat {
                        id
                        name
                        whatDoesTheCatSay
                    }
                }
            }
            ");

            Assert.AreEqual("Cat", result.Data.animal.__typename);
            Assert.AreEqual(1, result.Data.animal.id);
            Assert.AreEqual("Mourek", result.Data.animal.name);
            Assert.AreEqual("Meow", result.Data.animal.whatDoesTheCatSay);
        }

        [Test]
        public void ResolvesCatModelForTwoFragments()
        {
            var schema = new TestSchema();

            var result = schema.Execute(@"
            {
                animal {
                    __typename
                    ... on Cat {
                        id
                        name
                        whatDoesTheCatSay
                    }
                    ... on Dog {
                        id
                        name
                        whatDoesTheDogSay
                    }
                }
            }
            ");

            Assert.AreEqual("Cat", result.Data.animal.__typename);
            Assert.AreEqual(1, result.Data.animal.id);
            Assert.AreEqual("Mourek", result.Data.animal.name);
            Assert.AreEqual("Meow", result.Data.animal.whatDoesTheCatSay);
        }

        [Test]
        public void ResolvesUnknownTypeToNull()
        {
            var schema = new TestSchema();

            var result = schema.Execute(@"
            {
                unionReturningInvalidType {
                    __typename
                    ... on Cat {
                        id
                        name
                        whatDoesTheCatSay
                    }
                    ... on Dog {
                        id
                        name
                        whatDoesTheDogSay
                    }
                }
            }
            ");

            Assert.AreEqual(null, result.Data.unionReturningInvalidType);
        }

        [Test]
        public void CorrectlyIntrospectsUnionType()
        {
            /*
             kind must return __TypeKind.UNION.
            name must return a String.
            description may return a String or null.
            possibleTypes returns the list of types that can be represented within this union. They must be object types.
            All other fields must return null.
             */
            var schema = new TestSchema();

            var result = schema.Execute(@"
            {
              __type(name: " + "\"TestUnion\"" + @") {
                name
                description
                kind
                possibleTypes {
                    name
                }
                interfaces {
                    name
                }
                fields {
                    name
                }
                inputFields {
                    name
                }
                ofType {
                    name
                }
              }
            }
            ");

            Assert.AreEqual("TestUnion", result.Data.__type.name);
            Assert.AreEqual("Some union description", result.Data.__type.description);
            Assert.AreEqual("UNION", result.Data.__type.kind);
            Assert.AreEqual("Dog", ((IEnumerable<dynamic>)result.Data.__type.possibleTypes).ElementAt(0).name);
            Assert.AreEqual("Cat", ((IEnumerable<dynamic>)result.Data.__type.possibleTypes).ElementAt(1).name);
            Assert.IsNull(result.Data.__type.interfaces);
            Assert.IsNull(result.Data.__type.fields);
            Assert.IsNull(result.Data.__type.inputFields);
            Assert.IsNull(result.Data.__type.ofType);
        }
    }
}