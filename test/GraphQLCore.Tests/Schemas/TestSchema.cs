namespace GraphQLCore.Tests.Schemas
{
    using GraphQLCore.Type;
    using GraphQLCore.Type.Directives;
    using GraphQLCore.Type.Scalar;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

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
            this.Field("idArgField", (ID? idArg) => idArg);
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
            this.Field("nested", () => this);
        }
    }

    public class ComplicatedInputObjectType : GraphQLInputObjectType<ComplicatedObject>
    {
        public ComplicatedInputObjectType() : base("ComplicatedInputObjectType", "ComplicatedInputObjectType description")
        {
            this.Field("idField", e => e.IDField);
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
        public ID? IDField { get; set; }
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
            this.Field("idField", e => e.IDField);
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

            this.Field("someBox", () => null as ISomeBox);
            this.Field("connection", () => new ConnectionType());
            this.Field("foo", (int? a, int? b, int? c) => "bar");
            this.Field("bar", (int? a) => "foo");
            this.Field("sum", (int?[] arg) => arg.Sum());
            this.Field("field", (string a, string b, string c) => this);
            this.Field("field2", () => new FieldType());
            this.Field("nonNullField", (NonNullable<string> a) => a);
            this.Field("jagged", (IEnumerable<string[][]> jagged) => jagged);
            this.Field("interfaceObject", () => (ComplicatedInterface)new ComplicatedObject());
            this.Field("interfaceObjectArray", () => new List<ComplicatedInterface>() { new ComplicatedObject() });
            this.Field("complicatedArgs", () => complicatedArgs);
            this.Field("insertInputObject", (ComplicatedObject inputObject) => inputObject);
            this.Field("idArg", (ID id) => id);
        }
    }

    public class TestDirective : GraphQLDirectiveType
    {
        public TestDirective(string name, params DirectiveLocation[] locations) : base(name, "", locations)
        {
        }

        public override LambdaExpression GetResolver(object value, object parentValue)
        {
            Expression<Func<int?, int?, int?, object>> resolver = (a, b, c) => "replacedByDirective";

            return resolver;
        }
    }

    public class MutationRoot : GraphQLObjectType
    {
        public MutationRoot() : base("MutationRoot", "")
        {
            this.Field("insertInputObject", (ComplicatedObject inputObject) => inputObject);
        }
    }

    public class DeeperFieldType : GraphQLObjectType
    {
        public DeeperFieldType() : base("DeeperFieldType", "")
        {
            this.Field("a", () => 1);
            this.Field("y", () => 1);
            this.Field("b", () => 1);
        }
    }

    public class DeepFieldType : GraphQLObjectType
    {
        public DeepFieldType() : base("DeepFieldType", "")
        {
            this.Field("deeperField", () => new DeeperFieldType());
        }
    }

    public class FieldType : GraphQLObjectType
    {
        public FieldType() : base("Type", "")
        {
            this.Field("deepField", () => new DeepFieldType());
        }
    }
    
    public interface ISomeBox
    {
        ISomeBox DeepBox { get; set; }
        string UnrelatedField { get; set; }
    }

    public class StringBox : ISomeBox
    {
        public ISomeBox DeepBox { get; set; }
        public string UnrelatedField { get; set; }
    }

    public class IntBox : ISomeBox
    {
        public ISomeBox DeepBox { get; set; }
        public string UnrelatedField { get; set; }
    }

    public class SomeBoxType : GraphQLInterfaceType<ISomeBox>
    {
        public SomeBoxType() : base("SomeBox", "")
        {
            this.Field("deepBox", e => e.DeepBox);
            this.Field("unrelatedField", e => e.UnrelatedField);
        }
    }

    public class StringBoxType : GraphQLObjectType<StringBox>
    {
        public StringBoxType() : base("StringBox", "")
        {
            this.Field("scalar", e => "");
            this.Field("deepBox", e => e.DeepBox);
            this.Field("unrelatedField", e => e.UnrelatedField);
            this.Field("listStringBox", e => new StringBox[] { });
            this.Field("stringBox", e => new StringBox());
            this.Field("intBox", e => new IntBox());
        }
    }

    public class IntBoxType : GraphQLObjectType<IntBox>
    {
        public IntBoxType() : base("IntBox", "")
        {
            this.Field("scalar", e => 123 as int?);
            this.Field("deepBox", e => e.DeepBox);
            this.Field("unrelatedField", e => e.UnrelatedField);
            this.Field("listStringBox", e => new StringBox[] { });
            this.Field("stringBox", e => new StringBox());
            this.Field("intBox", e => "");
        }
    }

    public interface NonNullStringBox1
    {
        NonNullable<string> Scalar { get; set; }
    }

    public class NonNullStringBox1Impl
    {
        public NonNullable<string> Scalar { get; set; }
        public string UnrelatedField { get; set; }
        public ISomeBox DeepBox { get; set; }
    }

    public class NonNullStringBox1ImplType : GraphQLObjectType<NonNullStringBox1Impl>
    {
        public NonNullStringBox1ImplType() : base("NonNullStringBox1Impl", "")
        {
            this.Field("scalar", e => e.Scalar);
            this.Field("deepBox", e => e.DeepBox);
            this.Field("unrelatedField", e => e.UnrelatedField);
        }
    }

    public interface NonNullStringBox2
    {
        NonNullable<string> Scalar { get; set; }
    }

    public class NonNullStringBox2Impl
    {
        public NonNullable<string> Scalar { get; set; }
        public string UnrelatedField { get; set; }
        public ISomeBox DeepBox { get; set; }
    }

    public class NonNullStringBox2ImplType : GraphQLObjectType<NonNullStringBox2Impl>
    {
        public NonNullStringBox2ImplType() : base("NonNullStringBox2Impl", "")
        {
            this.Field("scalar", e => e.Scalar);
            this.Field("deepBox", e => e.DeepBox);
            this.Field("unrelatedField", e => e.UnrelatedField);
        }
    }

    public class NodeType : GraphQLObjectType
    {
        public NodeType() : base("Node", "")
        {
            this.Field("id", () => "");
            this.Field("name", () => "");
        }
    }

    public class EdgeType : GraphQLObjectType
    {
        public EdgeType() : base("Edge", "")
        {
            this.Field("node", () => new NodeType());
        }
    }

    public class ConnectionType : GraphQLObjectType
    {
        public ConnectionType() : base("Connection", "")
        {
            this.Field("edges", () => new EdgeType[] { });
        }
    }

    public class NonNullStringBox1Type : GraphQLInterfaceType<NonNullStringBox1>
    {
        public NonNullStringBox1Type() : base("NonNullStringBox1", "")
        {
            this.Field("scalar", e => e.Scalar);
        }
    }


    public class TestSchema : GraphQLSchema
    {
        public TestSchema()
        {
            var queryRoot = new QueryRoot();
            var mutationRoot = new MutationRoot();

            this.AddKnownType(queryRoot);
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
            this.AddKnownType(new FieldType());
            this.AddKnownType(new DeepFieldType());
            this.AddKnownType(new DeeperFieldType());
            this.AddKnownType(new IntBoxType());
            this.AddKnownType(new SomeBoxType());
            this.AddKnownType(new StringBoxType());
            this.AddKnownType(new EdgeType());
            this.AddKnownType(new NodeType());
            this.AddKnownType(new ConnectionType());
            this.AddKnownType(new NonNullStringBox1Type());
            this.AddKnownType(new NonNullStringBox1ImplType());

            this.AddOrReplaceDirective(new TestDirective("directive", DirectiveLocation.FIELD));
            this.AddOrReplaceDirective(new TestDirective("directive1", DirectiveLocation.FIELD));
            this.AddOrReplaceDirective(new TestDirective("directive2", DirectiveLocation.FIELD));

            this.AddOrReplaceDirective(new TestDirective("onQuery", DirectiveLocation.QUERY));
            this.AddOrReplaceDirective(new TestDirective("onMutation", DirectiveLocation.MUTATION));
            this.AddOrReplaceDirective(new TestDirective("onSubscription", DirectiveLocation.SUBSCRIPTION));
            this.AddOrReplaceDirective(new TestDirective("onField", DirectiveLocation.FIELD));
            this.AddOrReplaceDirective(new TestDirective("onFragmentDefinition", DirectiveLocation.FRAGMENT_DEFINITION));
            this.AddOrReplaceDirective(new TestDirective("onFragmentSpread", DirectiveLocation.FRAGMENT_SPREAD));
            this.AddOrReplaceDirective(new TestDirective("onInlineFragment", DirectiveLocation.INLINE_FRAGMENT));
            this.AddOrReplaceDirective(new TestDirective("onSchema", DirectiveLocation.SCHEMA));
            this.AddOrReplaceDirective(new TestDirective("onScalar", DirectiveLocation.SCALAR));
            this.AddOrReplaceDirective(new TestDirective("onObject", DirectiveLocation.OBJECT));
            this.AddOrReplaceDirective(new TestDirective("onFieldDefinition", DirectiveLocation.FIELD_DEFINITION));
            this.AddOrReplaceDirective(new TestDirective("onArgumentDefinition", DirectiveLocation.ARGUMENT_DEFINITION));
            this.AddOrReplaceDirective(new TestDirective("onInterface", DirectiveLocation.INTERFACE));
            this.AddOrReplaceDirective(new TestDirective("onUnion", DirectiveLocation.UNION));
            this.AddOrReplaceDirective(new TestDirective("onEnum", DirectiveLocation.ENUM));
            this.AddOrReplaceDirective(new TestDirective("onEnumValue", DirectiveLocation.ENUM_VALUE));
            this.AddOrReplaceDirective(new TestDirective("onInputObject", DirectiveLocation.INPUT_OBJECT));
            this.AddOrReplaceDirective(new TestDirective("onInputFieldDefinition", DirectiveLocation.INPUT_FIELD_DEFINITION));

            this.Query(queryRoot);
            this.Mutation(mutationRoot);
        }
    }
}