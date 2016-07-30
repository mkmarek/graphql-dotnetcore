namespace GraphQLCore.Tests.Schemas
{
    using GraphQLCore.Type;

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
            this.Field("booleanArgField", (bool booleanArg) => booleanArg);
            this.Field("enumArgField", (FurColor enumArg) => enumArg);
            this.Field("floatArgField", (float floatArg) => floatArg);
            this.Field("stringListArgField", (string[] stringListArg) => stringListArg);
            this.Field("nonNullIntListArgField", (int[] nonNullIntListArg) => nonNullIntListArg);
            this.Field("intListArgField", (int?[] intListArg) => intListArg);
            this.Field("complicatedObjectArgField", (ComplicatedObject complicatedObjectArg) => complicatedObjectArg);
            this.Field("complicatedObjectListArgField", (ComplicatedObject[] complicatedObjectListArg) => complicatedObjectListArg);
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
        }
    }

    public interface ComplicatedParentInteface
    {
        int? IntField { get; set; }
    }

    public interface SimpleInterface
    {
        bool BooleanField { get; set; }
    }

    public interface ComplicatedInteface : ComplicatedParentInteface
    {
        bool BooleanField { get; set; }
        FurColor EnumField { get; set; }
        float FloatField { get; set; }
        ComplicatedObject Nested { get; set; }
        int NonNullIntField { get; set; }
        string StringField { get; set; }
        string[] StringListField { get; set; }
    }

    public class ComplicatedObject : ComplicatedInteface
    {
        public bool BooleanField { get; set; }
        public FurColor EnumField { get; set; }
        public float FloatField { get; set; }
        public int? IntField { get; set; }
        public ComplicatedObject Nested { get; set; }
        public int NonNullIntField { get; set; }
        public string StringField { get; set; }
        public string[] StringListField { get; set; }
    }

    public class SimpleObject : SimpleInterface
    {
        public bool BooleanField { get; set; }
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
        }
    }

    public class SimpleObjectType : GraphQLObjectType<SimpleObject>
    {
        public SimpleObjectType() : base("SimpleObjectType", "")
        {
            this.Field("booleanField", e => e.BooleanField);
        }
    }

    public class SimpleInterfaceType : GraphQLInterfaceType<SimpleInterface>
    {
        public SimpleInterfaceType() : base("SimpleInterfaceType", "")
        {
            this.Field("booleanField", e => e.BooleanField);
        }
    }

    public class ComplicatedParentInterfaceType : GraphQLInterfaceType<ComplicatedParentInteface>
    {
        public ComplicatedParentInterfaceType() : base("ComplicatedParentInterfaceType", "")
        {
            this.Field("intField", e => e.IntField);
        }
    }

    public class ComplicatedInterfaceType : GraphQLInterfaceType<ComplicatedInteface>
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

            this.Field("interfaceObject", () => (ComplicatedInteface)new ComplicatedObject());
            this.Field("complicatedArgs", () => complicatedArgs);
        }
    }

    public class TestSchema : GraphQLSchema
    {
        public TestSchema()
        {
            var queryRoot = new QueryRoot();

            this.AddKnownType(queryRoot);
            this.AddKnownType(new SimpleInterfaceType());
            this.AddKnownType(new FurColorEnum());
            this.AddKnownType(new SimpleObjectType());
            this.AddKnownType(new ComplicatedInterfaceType());
            this.AddKnownType(new ComplicatedParentInterfaceType());
            this.AddKnownType(new ComplicatedInputObjectType());
            this.AddKnownType(new ComplicatedObjectType());
            this.AddKnownType(new ComplicatedArgs());
            this.Query(queryRoot);
        }
    }
}