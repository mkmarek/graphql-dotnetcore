namespace GraphQLCore.Tests.Execution
{
    using GraphQLCore.Exceptions;
    using GraphQLCore.Type;
    using GraphQLCore.Type.Scalar;
    using NUnit.Framework;
    using Schemas;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq;
    using Validation;

    [TestFixture]
    public class ExecutionContext_Variables
    {
        public IGraphQLSchema schema;
        private dynamic variables;

        [Test]
        public void ExecuteMutation_WithBodyVariables_InsertsBodyVariablesCorrectly()
        {
            var query = @"mutation getBodyArgs($stringArgVar: String, $intArgVar: Int!) {
                            
                                insertInputObject(inputObject: 
                                {
                                    nonNullIntField: 0
                                    stringField: $stringArgVar
                                    intField: $intArgVar
                                })
                                {
                                    stringField
                                    intField
                                }
                        }";
            
            var result = this.schema.Execute(query, variables);

            Assert.AreEqual("sample", result.Data.insertInputObject.stringField);
            Assert.AreEqual(3, result.Data.insertInputObject.intField);
        }

        [Test]
        public void Execute_WithBooleanVariable_AcceptsAsBooleanArgument()
        {
            var query = @"query getBooleanArg($booleanArgVar: Boolean) {
                            complicatedArgs {
                                booleanArgField(booleanArg: $booleanArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(true, result.Data.complicatedArgs.booleanArgField);
        }

        [Test]
        public void Execute_WithEnumVariable_AcceptsAsEnumArgument()
        {
            var query = @"query getEnumArg($enumArgVar: FurColor!) {
                            complicatedArgs {
                                enumArgField(enumArg: $enumArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual("BROWN", result.Data.complicatedArgs.enumArgField);
        }

        [Test]
        public void Execute_WithEnumVariableAsString_AcceptsAsEnumArgument()
        {
            var query = @"query getEnumArg($enumArgVarAsString: FurColor!) {
                            complicatedArgs {
                                enumArgField(enumArg: $enumArgVarAsString)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual("BROWN", result.Data.complicatedArgs.enumArgField);
        }

        [Test]
        public void Execute_WithFloatVariable_AcceptsAsFloatArgument()
        {
            var query = @"query getFloatArg($floatArgVar: Float!) {
                            complicatedArgs {
                                floatArgField(floatArg: $floatArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(1.6f, result.Data.complicatedArgs.floatArgField);
        }

        [Test]
        public void Execute_WithIntListVariable_AcceptsAsIntListArgument()
        {
            var query = @"query getStringListArg($intListArgVar: [Int]) {
                            complicatedArgs {
                                intListArgField(intListArg: $intListArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(new int?[] { 1, null, 3 }, result.Data.complicatedArgs.intListArgField);
        }

        [Test]
        public void Execute_WithIntVariable_AcceptsAsIntArgument()
        {
            var query = @"query getIntArg($intArgVar: Int) {
                            complicatedArgs {
                                intArgField(intArg: $intArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(3, result.Data.complicatedArgs.intArgField);
        }

        [Test]
        public void Execute_WithNonNullIntListVariable_AcceptsAsIntListArgument()
        {
            var query = @"query getStringListArg($nonNullIntListArgVar: [Int!]) {
                            complicatedArgs {
                                nonNullIntListArgField(nonNullIntListArg: $nonNullIntListArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(new int[] { 1, 2, 3 }, result.Data.complicatedArgs.nonNullIntListArgField);
        }

        [Test]
        public void Execute_WithNonNullIntVariable_AcceptsAsIntArgument()
        {
            var query = @"query getNonNullIntArg($intArgVar: Int!) {
                            complicatedArgs {
                                nonNullIntArgField(nonNullIntArg: $intArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(3, result.Data.complicatedArgs.nonNullIntArgField);
        }

        [Test]
        public void Execute_WithObjectVariable_ParsesAndReturnsCorrectValues()
        {
            var query = @"query getStringListArg($complicatedObjectArgVar: ComplicatedInputObjectType) {
                            complicatedArgs {
                                complicatedObjectArgField(complicatedObjectArg: $complicatedObjectArgVar) {
                                    intField
                                    nonNullIntField
                                    stringField
                                    booleanField
                                    enumField
                                    floatField
                                    stringListField
                                }
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(1, result.Data.complicatedArgs.complicatedObjectArgField.intField);
            Assert.AreEqual(1, result.Data.complicatedArgs.complicatedObjectArgField.nonNullIntField);
            Assert.AreEqual("sample", result.Data.complicatedArgs.complicatedObjectArgField.stringField);
            Assert.AreEqual(true, result.Data.complicatedArgs.complicatedObjectArgField.booleanField);
            Assert.AreEqual("BROWN", result.Data.complicatedArgs.complicatedObjectArgField.enumField);
            Assert.AreEqual(1.6f, result.Data.complicatedArgs.complicatedObjectArgField.floatField);
            Assert.AreEqual(new string[] { "a", "b", "c" }, result.Data.complicatedArgs.complicatedObjectArgField.stringListField);
        }

        [Test]
        public void Execute_WithObjectVariable_ParsesAndReturnsCorrectValuesForNestedType()
        {
            var query = @"query getStringListArg($complicatedObjectArgVar: ComplicatedInputObjectType) {
                            complicatedArgs {
                                complicatedObjectArgField(complicatedObjectArg: $complicatedObjectArgVar) {
                                    nested {
                                        intField
                                        nonNullIntField
                                        stringField
                                        booleanField
                                        enumField
                                        floatField
                                        stringListField
                                    }
                                }
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(1, result.Data.complicatedArgs.complicatedObjectArgField.nested.intField);
            Assert.AreEqual(1, result.Data.complicatedArgs.complicatedObjectArgField.nested.nonNullIntField);
            Assert.AreEqual("sample", result.Data.complicatedArgs.complicatedObjectArgField.nested.stringField);
            Assert.AreEqual(true, result.Data.complicatedArgs.complicatedObjectArgField.nested.booleanField);
            Assert.AreEqual("BROWN", result.Data.complicatedArgs.complicatedObjectArgField.nested.enumField);
            Assert.AreEqual(1.6f, result.Data.complicatedArgs.complicatedObjectArgField.nested.floatField);
            Assert.AreEqual(new string[] { "a", "b", "c" }, result.Data.complicatedArgs.complicatedObjectArgField.nested.stringListField);
        }

        [Test]
        public void Execute_WithStringListVariable_AcceptsAsStringListArgument()
        {
            var query = @"query getStringListArg($stringListArgVar: [String]) {
                            complicatedArgs {
                                stringListArgField(stringListArg: $stringListArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(new string[] { "a", "b", "c" }, result.Data.complicatedArgs.stringListArgField);
        }

        [Test]
        public void Execute_WithStringVariable_AcceptsAsStringArgument()
        {
            var query = @"query getStringArg($stringArgVar: String) {
                            complicatedArgs {
                                stringArgField(stringArg: $stringArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual("sample", result.Data.complicatedArgs.stringArgField);
        }

        [Test]
        public void Execute_WithObjectListVariable_ParsesAndReturnsCorrectValues()
        {
            var query = @"query getStringListArg($complicatedObjectListArgVar: [ComplicatedInputObjectType]) {
                            complicatedArgs {
                                complicatedObjectListArgField(complicatedObjectListArg: $complicatedObjectListArgVar) {
                                    intField
                                }
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(1, ((IEnumerable<dynamic>)result.Data.complicatedArgs.complicatedObjectListArgField).ElementAt(0).intField);
        }

        [Test]
        public void Execute_WithObjectListVariableWithListField_ParsesAndReturnsCorrectValues()
        {
            var query = @"query getStringListArg($complicatedObjectListArgVar: [ComplicatedInputObjectType]) {
                            complicatedArgs {
                                complicatedObjectListArgField(complicatedObjectListArg: $complicatedObjectListArgVar) {
                                    stringListField
                                }
                            }
                        }";

            var result = this.schema.Execute(query, variables);
            var stringListField = (IEnumerable<object>)((IEnumerable<dynamic>)result.Data.complicatedArgs.complicatedObjectListArgField)
                .ElementAt(0).stringListField;

            Assert.AreEqual("a", stringListField.ElementAt(0));
            Assert.AreEqual("b", stringListField.ElementAt(1));
            Assert.AreEqual("c", stringListField.ElementAt(2));
        }

        [Test]
        public void Execute_WithNonNullObjectVariable_ParsesAndReturnsCorrectValue()
        {
            var query = @"
            mutation getStringListArg($complicatedObjectArgVar: ComplicatedInputObjectType!) {
                insertInputObject(inputObject: $complicatedObjectArgVar) {
                    stringField
                }
            }";

            var result = this.schema.Execute(query, variables);
            var stringField = result.Data.insertInputObject.stringField;

            Assert.AreEqual("sample", stringField);
        }

        [Test]
        public void Execute_WithListVariableInObjectArgs_ParsesAndReturnsCorrectValue()
        {
            var query = @"
            query getStringListField($stringListArgVar: [String]) {
                insertInputObject(inputObject: {
                    nonNullIntField: 0,
                    stringListField: $stringListArgVar
                }) {
                    stringListField
                }
            }";

            var result = this.schema.Execute(query, variables);
            var stringListField = result.Data.insertInputObject.stringListField;

            Assert.AreEqual(new string[] { "a", "b", "c" }, stringListField);
        }

        [Test]
        public void Execute_WithComplexObjectWithArrayProperty_ParsesAndReturnsCorrectValue()
        {
            var query = @"
            query getStringListField($complicatedObjectArgWithArrayVar: ComplicatedInputObjectType!) {
                insertInputObject(inputObject: $complicatedObjectArgWithArrayVar) {
                    complicatedObjectArray {
                        stringField
                    }
                }
            }";

            var result = this.schema.Execute(query, variables);
            var array = (IEnumerable<dynamic>)result.Data.insertInputObject.complicatedObjectArray;

            Assert.AreEqual("sample", array.ElementAt(0).stringField);
            Assert.AreEqual("sample", array.ElementAt(1).stringField);
        }

        [Test]
        public void Execute_WithNullOrEmptyVariables_ParsesAndReturnsCorrectValue()
        {
            var query = @"
            query getIntArg($intArgVar: Int, $intArgVar2: Int) {
                complicatedArgs {
                    intArgField(intArg: $intArgVar)
                    intArgField2 : intArgField(intArg: $intArgVar2)
                }
            }";
            dynamic variables = new ExpandoObject();
            variables.intArgVar2 = null;

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(null, result.Data.complicatedArgs.intArgField);
            Assert.AreEqual(null, result.Data.complicatedArgs.intArgField2);
        }

        [Test]
        public void Execute_WithNonNullEmptyVariable_ThrowsError()
        {
            var query = @"
            query getIntArg($intArgVar: Int!) {
                complicatedArgs {
                    intArgField(intArg: $intArgVar)
                }
            }";
            dynamic variables = new ExpandoObject();

            var result = this.schema.Execute(query, variables);
            var errors = result.Errors as IList<GraphQLException>;
            
            ErrorAssert.AreEqual("Variable \"intArgVar\" of required type \"Int!\" was not provided.", 
                errors.Single(), 2, 29);
        }

        [Test]
        public void Execute_WithVariablesInLists_ParsesAndReturnsCorrectValues()
        {
            var query = @"
            query insertObject($stringArgVar: String, $complicatedObjectArgVar: ComplicatedInputObjectType, $stringListArgVar: [String]) {
                insertInputObject(inputObject: {
                    nonNullIntField: 0,
                    stringListField: [$stringArgVar, ""and"", $stringArgVar],
                    complicatedObjectArray: [
                        $complicatedObjectArgVar,
                        {
                            nonNullIntField: 0,
                            stringListField: $stringListArgVar
                        }
                    ]
                }) {
                    stringListField
                    complicatedObjectArray {
                        stringField
                        stringListField
                    }
                }
            }";

            var result = this.schema.Execute(query, this.variables);

            Assert.AreEqual(new string[] { "sample", "and", "sample" }, result.Data.insertInputObject.stringListField);
            Assert.AreEqual("sample", result.Data.insertInputObject.complicatedObjectArray[0].stringField);
            Assert.AreEqual(new string[] { "a", "b", "c" }, result.Data.insertInputObject.complicatedObjectArray[1].stringListField);
            Assert.AreEqual(null, result.Data.insertInputObject.complicatedObjectArray[1].stringField);
        }

        [Test]
        public void Execute_WithDeeplyNestedVariable_ParsesAndReturnsCorrectValue()
        {
            var query = @"
            query insertObject($complicatedObjectArgVar: ComplicatedInputObjectType) {
                insertInputObject(inputObject: {
                    nonNullIntField: 0,
                    nested: {
                        nonNullIntField: 0,
                        nested: $complicatedObjectArgVar
                    }
                }) {
                    nested {
                        nested {
                            intField
                        }
                    }
                }
            }";

            var result = this.schema.Execute(query, this.variables);

            Assert.AreEqual(1, result.Data.insertInputObject.nested.nested.intField);
        }

        [Test]
        public void Execute_WithNestedJaggedListVariable_ParsesAndReturnsCorrectValue()
        {
            this.variables.one = "a";
            this.variables.two = new string[] { "a", "b" };
            this.variables.three = new List<string[]>()
            {
                new string[] { "a", null },
                new string[] { "c", "d" }
            };

            var query = @"
            query jagged($one: String, $two: [String], $three: [[String]]) {
                jagged(jagged: [
                    [
                        [$one, ""b""],
                        $two
                    ],
                    $three
                ])
            }";

            var result = this.schema.Execute(query, this.variables);

            var expectedResult = new string[][][]
            {
                new string[][]
                {
                    new string[] { "a", "b" },
                    new string[] { "a", "b" }
                },
                new string[][]
                {
                    new string[] { "a", null },
                    new string[] { "c", "d" }
                }
            };

            Assert.AreEqual(expectedResult, result.Data.jagged);
        }

        [Test]
        public void Execute_WithSingleValueListVariable_ParsesAndReturnsCorrectValue()
        {
            var query = @"query getnonNullIntListArg($intArgVar: [Int!]) {
                            complicatedArgs {
                                nonNullIntListArgField(nonNullIntListArg: $intArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(new int[] { 3 }, result.Data.complicatedArgs.nonNullIntListArgField);
        }

        [Test]
        public void Execute_WithNullListVariable_ParsesAndReturnsCorrectValue()
        {
            var query = @"query getnonNullIntListArg($nullArgVar: [Int!]) {
                            complicatedArgs {
                                nonNullIntListArgField(nonNullIntListArg: $nullArgVar)
                            }
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual(null, result.Data.complicatedArgs.nonNullIntListArgField);
        }

        [Test]
        public void Execute_WithNonNullArgument_ParsesAndReturnsCorrectValue()
        {
            var query = @"query getNonNullField($stringArgVar: String!) {
                            nonNullField(a: $stringArgVar)
                        }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual("sample", result.Data.nonNullField);
        }

        [Test]
        public void Execute_WithoutNonNullArgument_ReturnsError()
        {
            var query = @"query getnonNullIntListArg($notExistingVariable: Int!) {
                            complicatedArgs {
                                nonNullIntArgField(nonNullIntArg: $notExistingVariable)
                            }
                        }";

            var result = this.schema.Execute(query, variables);
            var errors = result.Errors as IList<GraphQLException>;
        
            ErrorAssert.AreEqual("Variable \"notExistingVariable\" of required type \"Int!\" was not provided.",
                errors.Single(), 1, 28);
        }

        [Test]
        public void Execute_WithID_ParsesAndReturnsCorrectValue()
        {
            var query = @"
            query insertObject($idArgVar: ID!) {
                idArg(id: $idArgVar)
            }";

            var result = this.schema.Execute(query, variables);

            Assert.AreEqual("123", result.Data.idArg);
        }

        [SetUp]
        public void SetUp()
        {
            this.variables = new ExpandoObject();
            this.variables.intArgVar = 3;
            this.variables.stringArgVar = "sample";
            this.variables.booleanArgVar = true;
            this.variables.enumArgVar = FurColor.BROWN;
            this.variables.enumArgVarAsString = "BROWN";
            this.variables.floatArgVar = 1.6f;
            this.variables.stringListArgVar = new string[] { "a", "b", "c" };
            this.variables.nonNullIntListArgVar = new int[] { 1, 2, 3 };
            this.variables.intListArgVar = new int?[] { 1, null, 3 };
            this.variables.complicatedObjectArgVar = CreateComplicatedDynamicObject();
            this.variables.complicatedObjectListArgVar = new object[] { CreateComplicatedDynamicObject() };
            this.variables.complicatedObjectArgVar.nested = CreateComplicatedDynamicObject();
            this.variables.complicatedObjectArgWithArrayVar = CreateComplicatedDynamicObjectWithArray();
            this.variables.nullArgVar = null;
            this.variables.idArgVar = (ID)123;

            this.schema = new TestSchema();
        }

        private static dynamic CreateComplicatedDynamicObjectWithArray()
        {
            dynamic complicatedObjectArgVar = new ExpandoObject();
            complicatedObjectArgVar.intField = 1;
            complicatedObjectArgVar.nonNullIntField = 1;
            complicatedObjectArgVar.stringField = "sample";
            complicatedObjectArgVar.booleanField = true;
            complicatedObjectArgVar.enumField = FurColor.BROWN;
            complicatedObjectArgVar.floatField = 1.6f;
            complicatedObjectArgVar.stringListField = new string[] { "a", "b", "c" };
            complicatedObjectArgVar.complicatedObjectArray = new List<ExpandoObject>() { CreateComplicatedDynamicObject(), CreateComplicatedDynamicObject() };

            return complicatedObjectArgVar;
        }

        private static dynamic CreateComplicatedDynamicObject()
        {
            dynamic complicatedObjectArgVar = new ExpandoObject();
            complicatedObjectArgVar.intField = 1;
            complicatedObjectArgVar.nonNullIntField = 1;
            complicatedObjectArgVar.stringField = "sample";
            complicatedObjectArgVar.booleanField = true;
            complicatedObjectArgVar.enumField = FurColor.BROWN;
            complicatedObjectArgVar.floatField = 1.6f;
            complicatedObjectArgVar.stringListField = new string[] { "a", "b", "c" };

            return complicatedObjectArgVar;
        }
    }
}