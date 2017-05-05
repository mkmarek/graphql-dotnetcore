namespace GraphQLCore.Tests.Schemas
{
    using GraphQLCore.Type;
    using System.Collections.Generic;
    using System.Linq;

    public enum FurColor
    {
        BROWN = 0,
        BLACK = 1,
        TAN = 2,
        SPOTTED = 3
    }

    public class ComplicatedArgs : GraphQLObjectType
    {
        public ComplicatedArgs() : base("ComplicatedArgs", "")
        {
            this.Field("intArgField", (int? intArg) => intArg);
            this.Field("multipleArgsField", (int? arg1, int? arg2) => arg1 + arg2);
            this.Field("nonNullIntArgField", (int nonNullIntArg) => nonNullIntArg);
            this.Field("nonNullIntMultipleArgsField", (int arg1, int arg2) => arg1 + arg2);
            this.Field("stringArgField", (string stringArg) => stringArg);
            this.Field("booleanArgField", (bool? booleanArg) => booleanArg);
            this.Field("enumArgField", (FurColor enumArg) => enumArg);
            this.Field("floatArgField", (float floatArg) => floatArg);
            this.Field("stringListArgField", (string[] stringListArg) => stringListArg);
            this.Field("nonNullIntListArgField", (int[] nonNullIntListArg) => nonNullIntListArg);
            this.Field("intListArgField", (int?[] intListArg) => intListArg);
            this.Field("complicatedObjectArgField", (ComplicatedObject complicatedObjectArg) => complicatedObjectArg);
            this.Field("complicatedObjectListArgField", (ComplicatedObject[] complicatedObjectListArg) => complicatedObjectListArg);
            this.Field("insertInputObject", (ComplicatedObject inputObject) => inputObject);
        }
    }

    public class ComplicatedInputObjectType : GraphQLInputObjectType<ComplicatedObject>
    {
        public ComplicatedInputObjectType() : base("ComplicatedInputObjectType", "ComplicatedInputObjectType description")
        {
            this.Field("intField", e => e.IntField);
            this.Field("nonNullIntField", e => e.NonNullIntField);
            this.Field("stringField", e => e.StringField);
            this.Field("booleanField", e => e.BooleanField);
            this.Field("enumField", e => e.EnumField);
            this.Field("floatField", e => e.FloatField);
            this.Field("stringListField", e => e.StringListField);
            this.Field("nested", e => e.Nested);
            this.Field("complicatedObjectArray", e => e.ComplicatedObjectArray);
        }
    }

    public interface ComplicatedParentInterface
    {
        int? IntField { get; set; }
    }

    public interface SimpleInterface
    {
        bool? BooleanField { get; set; }
    }

    public interface ComplicatedInterface : ComplicatedParentInterface
    {
        bool? BooleanField { get; set; }
        FurColor? EnumField { get; set; }
        float? FloatField { get; set; }
        ComplicatedObject Nested { get; set; }
        int NonNullIntField { get; set; }
        string StringField { get; set; }
        string[] StringListField { get; set; }
    }

    public class ComplicatedObject : ComplicatedInterface
    {
        public bool? BooleanField { get; set; }
        public FurColor? EnumField { get; set; }
        public float? FloatField { get; set; }
        public int? IntField { get; set; }
        public ComplicatedObject Nested { get; set; }
        public int NonNullIntField { get; set; }
        public string StringField { get; set; }
        public string[] StringListField { get; set; }
        public ComplicatedObject[] ComplicatedObjectArray { get; set; }
    }

    public class SimpleObject : SimpleInterface
    {
        public bool? BooleanField { get; set; }
    }

    public class AnotherSimpleObject : SimpleInterface
    {
        public bool? BooleanField { get; set; }
    }

    public class ComplicatedObjectType : GraphQLObjectType<ComplicatedObject>
    {
        public ComplicatedObjectType() : base("ComplicatedObjectType", "")
        {
            this.Field("intField", e => e.IntField);
            this.Field("nonNullIntField", e => e.NonNullIntField);
            this.Field("stringField", e => e.StringField);
            this.Field("booleanField", e => e.BooleanField);
            this.Field("enumField", e => e.EnumField);
            this.Field("floatField", e => e.FloatField);
            this.Field("stringListField", e => e.StringListField);
            this.Field("nested", e => e.Nested);
            this.Field("complicatedObjectArray", e => e.ComplicatedObjectArray);
        }
    }

    public class SimpleObjectType : GraphQLObjectType<SimpleObject>
    {
        public SimpleObjectType() : base("SimpleObjectType", "")
        {
            this.Field("booleanField", e => e.BooleanField);
            this.Field("notInterfaceField", e => "");
            this.Field("simple", e => "simple");
        }
    }

    public class AnotherSimpleObjectType : GraphQLObjectType<AnotherSimpleObject>
    {
        public AnotherSimpleObjectType() : base("AnotherSimpleObjectType", "")
        {
            this.Field("booleanField", e => e.BooleanField);
            this.Field("boolField", e => false);
            this.Field("notInterfaceField", e => "");
            this.Field("sample", e => "sample");
        }
    }

    public class SimpleSampleUnionType : GraphQLUnionType
    {
        public SimpleSampleUnionType()
            : base("SimpleSampleUnionType", "")
        {
            this.AddPossibleType(typeof(SimpleObject));
            this.AddPossibleType(typeof(AnotherSimpleObject));
        }

        public override System.Type ResolveType(object data)
        {
            if (data is SimpleObject)
                return typeof(SimpleObject);
            else if (data is AnotherSimpleObject)
                return typeof(AnotherSimpleObject);

            return null;
        }
    }

    public class SimpleInterfaceType : GraphQLInterfaceType<SimpleInterface>
    {
        public SimpleInterfaceType() : base("SimpleInterfaceType", "")
        {
            this.Field("booleanField", e => e.BooleanField);
        }
    }

    public class ComplicatedParentInterfaceType : GraphQLInterfaceType<ComplicatedParentInterface>
    {
        public ComplicatedParentInterfaceType() : base("ComplicatedParentInterfaceType", "")
        {
            this.Field("intField", e => e.IntField);
        }
    }

    public class SampleInputObject
    {
        public int Id { get; set; }
        public bool? Field { get; set; }
        public string Field1 { get; set; }
        public string Field2 { get; set; }
        public string Field3 { get; set; }
        public SampleInputObject Deep { get; set; }
    }

    public class SampleInputObjectType : GraphQLInputObjectType<SampleInputObject>
    {
        public SampleInputObjectType() : base("SampleInputObjectType", "SampleInputObjectType")
        {
            this.Field("id", e => e.Id);
            this.Field("f", e => e.Field);
            this.Field("f1", e => e.Field1);
            this.Field("f2", e => e.Field2);
            this.Field("f3", e => e.Field3);
            this.Field("deep", e => e.Deep);
        }
    }

    public class ComplicatedInterfaceType : GraphQLInterfaceType<ComplicatedInterface>
    {
        public ComplicatedInterfaceType() : base("ComplicatedInterfaceType", "")
        {
            this.Field("intField", e => e.IntField);
            this.Field("nonNullIntField", e => e.NonNullIntField);
            this.Field("stringField", e => e.StringField);
            this.Field("booleanField", e => e.BooleanField);
            this.Field("enumField", e => e.EnumField);
            this.Field("floatField", e => e.FloatField);
            this.Field("stringListField", e => e.StringListField);
            this.Field("nested", e => e.Nested);
        }
    }

    public class FurColorEnum : GraphQLEnumType<FurColor>
    {
        public FurColorEnum() : base("FurColor", "")
        {
        }
    }

    public class QueryRoot : GraphQLObjectType
    {
        public QueryRoot() : base("QueryRoot", "")
        {
            var complicatedArgs = new ComplicatedArgs();

            this.Field("foo", (int? a, int? b, int? c) => "bar");
            this.Field("bar", (int? a) => "foo");
            this.Field("sum", (int?[] arg) => arg.Sum());
            this.Field("field", (string a, string b, string c) => this);
            this.Field("nonNullField", (NonNullable<string> a) => a);
            this.Field("jagged", (IEnumerable<string[][]> jagged) => jagged);
            this.Field("interfaceObject", () => (ComplicatedInterface)new ComplicatedObject());
            this.Field("complicatedArgs", () => complicatedArgs);
            this.Field("insertInputObject", (ComplicatedObject inputObject) => inputObject);
        }
    }

    public class MutationRoot : GraphQLObjectType
    {
        public MutationRoot() : base("MutationRoot", "")
        {
            this.Field("insertInputObject", (ComplicatedObject inputObject) => inputObject);
        }
    }

    public class TestSchema : GraphQLSchema
    {
        public TestSchema()
        {
            var queryRoot = new QueryRoot();
            var mutationRoot = new MutationRoot();

            this.AddKnownType(queryRoot);
            this.AddKnownType(new SampleInputObjectType());
            this.AddKnownType(new SimpleInterfaceType());
            this.AddKnownType(new FurColorEnum());
            this.AddKnownType(new SimpleObjectType());
            this.AddKnownType(new AnotherSimpleObjectType());
            this.AddKnownType(new ComplicatedInterfaceType());
            this.AddKnownType(new ComplicatedParentInterfaceType());
            this.AddKnownType(new ComplicatedInputObjectType());
            this.AddKnownType(new ComplicatedObjectType());
            this.AddKnownType(new ComplicatedArgs());
            this.AddKnownType(new SimpleSampleUnionType());
            this.Query(queryRoot);
            this.Mutation(mutationRoot);
        }
    }
}