namespace GraphQLCore.Type
{
    public interface IObjectTypeTranslator
    {
        GraphQLFieldConfig GetField(string fieldName);

        GraphQLFieldConfig[] GetFields();

        GraphQLComplexType[] GetImplementingInterfaces();

        GraphQLComplexType[] GetPossibleTypes();
    }
}