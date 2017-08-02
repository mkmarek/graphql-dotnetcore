namespace GraphQLCore.Validation.Rules
{
    using Exceptions;
    using Language.AST;
    using System.Collections.Generic;
    using System.Linq;
    using Type;
    using Type.Complex;

    public class OverlappingFieldsCanBeMergedVisitor : ValidationASTVisitor
    {
        private List<string> comparedFragments = new List<string>();

        public OverlappingFieldsCanBeMergedVisitor(IGraphQLSchema schema) : base(schema)
        {
            this.Errors = new List<GraphQLException>();
        }

        public List<GraphQLException> Errors { get; private set; }

        public override GraphQLSelectionSet BeginVisitSelectionSet(GraphQLSelectionSet node)
        {
            var conflicts = this.FindCnflictsWithinSelectionSet(node);

            foreach (var conflict in conflicts)
            {
                this.Errors.Add(new GraphQLException(this.FieldsConflictMessage(conflict),
                    conflict.Field1.Concat(conflict.Field2)));
            }

            return base.BeginVisitSelectionSet(node);
        }

        private static bool DoParentTypesNotMatch(NodeAndDefinitions field1, NodeAndDefinitions field2)
        {
            if (field1.ParentType != null && field2.ParentType != null)
            {
                return field1.ParentType.Name != field2.ParentType.Name;
            }

            if (field1.PresumedParentName != null || field2.PresumedParentName != null)
            {
                return field1.PresumedParentName != field2.PresumedParentName;
            }

            return false;
        }

        private string FieldsConflictMessage(Conflict conflict)
        {
            return $"Fields \"{conflict.ResponseName}\" conflict because {this.ProcessReason(conflict)}" +
                ". Use different aliases on the fields to fetch both if this was " +
                "intentional.";
        }

        private string ProcessReason(Conflict conflict)
        {
            if (conflict.Subreasons?.Count() > 0)
            {
                return string.Join(" and ",
                    conflict.Subreasons.Select(
                        e => $"subfields \"{e.ResponseName}\" conflict because {this.ProcessReason(e)}"));
            }

            return conflict.Reason;
        }

        private IEnumerable<Conflict> FindCnflictsWithinSelectionSet(GraphQLSelectionSet node)
        {
            var fieldMap = new Dictionary<string, ICollection<NodeAndDefinitions>>();
            var fragmentNames = new List<string>();
            var conflicts = new List<Conflict>();
            var parentObject = this.GetLastType();

            this.GetFieldsAndFragmentNames(
                node,
                fieldMap,
                fragmentNames,
                parentObject);

            conflicts.AddRange(this.CollectConflictsWithin(node, fieldMap));

            for (var i = 0; i < fragmentNames.Count; i++)
            {
                conflicts.AddRange(this.CollectConflictsBetweenFieldsAndFragment(
                    fieldMap, fragmentNames.ElementAt(i), false));

                for (var j = i + 1; j < fragmentNames.Count; j++)
                {
                    conflicts.AddRange(this.CollectConflictsBetweenFragments(
                        fragmentNames.ElementAt(i), fragmentNames.ElementAt(j), false));
                }
            }

            return conflicts;
        }

        private IEnumerable<Conflict> CollectConflictsBetweenFragments(
            string fragmentName, string fragmentName2, bool areMutuallyExclusive)
        {
            var conflicts = new List<Conflict>();

            if (string.IsNullOrWhiteSpace(fragmentName) || string.IsNullOrWhiteSpace(fragmentName2))
            {
                return conflicts;
            }

            // No need to compare a fragment to itself.
            if (fragmentName == fragmentName2)
            {
                return conflicts;
            }

            if (!this.Fragments.ContainsKey(fragmentName) || !this.Fragments.ContainsKey(fragmentName2))
                return conflicts;

            if (this.comparedFragments.Contains($"{fragmentName}-{fragmentName2}-{areMutuallyExclusive}"))
                return conflicts;

            this.comparedFragments.Add($"{fragmentName}-{fragmentName2}-{areMutuallyExclusive}");
            this.comparedFragments.Add($"{fragmentName2}-{fragmentName}-{areMutuallyExclusive}");

            var fragment1 = this.Fragments[fragmentName];
            var fragment2 = this.Fragments[fragmentName2];

            var fieldMap1 = new Dictionary<string, ICollection<NodeAndDefinitions>>();
            var fragmentNames1 = new List<string>();

            this.GetReferencedFieldsAndFragmentNames(fragment1, fieldMap1, fragmentNames1);

            var fieldMap2 = new Dictionary<string, ICollection<NodeAndDefinitions>>();
            var fragmentNames2 = new List<string>();

            this.GetReferencedFieldsAndFragmentNames(fragment2, fieldMap2, fragmentNames2);

            conflicts.AddRange(this.CollectConflictsBetween(areMutuallyExclusive, fieldMap1, fieldMap2));

            foreach (var frag in fragmentNames2)
            {
                conflicts.AddRange(this.CollectConflictsBetweenFragments(
                    fragmentName,
                    frag,
                    areMutuallyExclusive));
            }

            foreach (var frag in fragmentNames1)
            {
                conflicts.AddRange(this.CollectConflictsBetweenFragments(
                    frag,
                    fragmentName2,
                    areMutuallyExclusive));
            }

            return conflicts;
        }

        private IEnumerable<Conflict> CollectConflictsBetweenFieldsAndFragment(
            IDictionary<string, ICollection<NodeAndDefinitions>> fieldMap,
            string fragmentName,
            bool areMutuallyExclusive,
            string[] visitedFragmentsChain = null)
        {
            var conflicts = new List<Conflict>();
            var visitedFragments = (visitedFragmentsChain ?? new string[] { }).ToList();

            if (!this.Fragments.ContainsKey(fragmentName))
                return conflicts;

            if (visitedFragments.Contains(fragmentName))
                return conflicts;

            var fragment = this.Fragments[fragmentName];
            visitedFragments.Add(fragmentName);

            var fieldMap2 = new Dictionary<string, ICollection<NodeAndDefinitions>>();
            var fragmentNames2 = new List<string>();

            this.GetReferencedFieldsAndFragmentNames(fragment, fieldMap2, fragmentNames2);

            conflicts.AddRange(this.CollectConflictsBetween(
                areMutuallyExclusive,
                fieldMap,
                fieldMap2));

            foreach (var fragmentName2 in fragmentNames2)
            {
                conflicts.AddRange(this.CollectConflictsBetweenFieldsAndFragment(
                    fieldMap, fragmentName2, areMutuallyExclusive, visitedFragments.ToArray()));
            }

            return conflicts;
        }

        private IEnumerable<Conflict> CollectConflictsBetween(
            bool areMutuallyExclusive,
            IDictionary<string, ICollection<NodeAndDefinitions>> fieldMap,
            Dictionary<string, ICollection<NodeAndDefinitions>> fieldMap2)
        {
            foreach (var pair in fieldMap)
            {
                if (!fieldMap2.ContainsKey(pair.Key))
                    continue;

                var fields2 = fieldMap2[pair.Key];
                var fields1 = pair.Value;

                for (var i = 0; i < fields1.Count; i++)
                {
                    for (var j = 0; j < fields2.Count; j++)
                    {
                        var conflict = this.FindConflict(
                            areMutuallyExclusive, // within one collection is never mutually exclusive
                            pair.Key,
                            fields1.ElementAt(i),
                            fields2.ElementAt(j));

                        if (conflict != null)
                        {
                            yield return conflict;
                        }
                    }
                }
            }
        }

        private void GetReferencedFieldsAndFragmentNames(
            GraphQLFragmentDefinition fragment,
            Dictionary<string, ICollection<NodeAndDefinitions>> fieldMap2,
            List<string> fragmentNames2)
        {
            var type = this.SchemaRepository.GetSchemaOutputTypeByName(
                fragment.TypeCondition.Name.Value);

            this.GetFieldsAndFragmentNames(
                fragment.SelectionSet,
                fieldMap2,
                fragmentNames2,
                type);
        }

        private IEnumerable<Conflict> CollectConflictsWithin(
            GraphQLSelectionSet node,
            IDictionary<string, ICollection<NodeAndDefinitions>> fieldMap)
        {
            foreach (var pair in fieldMap)
            {
                var fields = pair.Value;

                if (fields.Count > 1)
                {
                    for (var i = 0; i < fields.Count; i++)
                    {
                        for (var j = i + 1; j < fields.Count; j++)
                        {
                            var conflict = this.FindConflict(
                                false, // within one collection is never mutually exclusive
                                pair.Key,
                                fields.ElementAt(i),
                                fields.ElementAt(j));

                            if (conflict != null)
                            {
                                yield return conflict;
                            }
                        }
                    }
                }
            }
        }

        private Conflict FindConflict(
            bool parentFieldsAreMutuallyExclusive,
            string responseName,
            NodeAndDefinitions field1,
            NodeAndDefinitions field2)
        {
            var areMutuallyExclusive =
                parentFieldsAreMutuallyExclusive ||
                DoParentTypesNotMatch(field1, field2);

            var name1 = field1.Selection.Name.Value;
            var name2 = field2.Selection.Name.Value;
            GraphQLBaseType type1 = null;
            GraphQLBaseType type2 = null;

            if (field1.ParentType is GraphQLComplexType)
            {
                var complexType = field1.ParentType as GraphQLComplexType;
                type1 = complexType.GetFieldInfo(name1)?.GetGraphQLType(this.SchemaRepository);
            }

            if (field2.ParentType is GraphQLComplexType)
            {
                var complexType = field2.ParentType as GraphQLComplexType;
                type2 = complexType.GetFieldInfo(name2)?.GetGraphQLType(this.SchemaRepository);
            }

            if (!areMutuallyExclusive)
            {
                if (name1 != name2)
                {
                    return new Conflict()
                    {
                        ResponseName = responseName,
                        Reason = $"{name1} and {name2} are different fields",
                        Field1 = new[] { field1.Selection },
                        Field2 = new[] { field2.Selection }
                    };
                }

                if (!this.HaveSameArguments(field1.Selection, field2.Selection))
                {
                    return new Conflict()
                    {
                        ResponseName = responseName,
                        Reason = "they have differing arguments",
                        Field1 = new[] { field1.Selection },
                        Field2 = new[] { field2.Selection }
                    };
                }
            }

            if (type1 != null && type2 != null && this.DoTypesConflict(type1, type2))
            {
                return new Conflict()
                {
                    ResponseName = responseName,
                    Reason = $"they return conflicting types {type1} and {type2}",
                    Field1 = new[] { field1.Selection },
                    Field2 = new[] { field2.Selection }
                };
            }

            var selectionSet1 = field1.Selection?.SelectionSet;
            var selectionSet2 = field2.Selection?.SelectionSet;

            if (selectionSet1 != null && selectionSet2 != null)
            {
                var conflicts = this.FindConflictsBetweenSubSelectionSets(
                    type1,
                    type2,
                    selectionSet1,
                    selectionSet2,
                    areMutuallyExclusive);

                if (conflicts.Any())
                    return this.SubfieldConflicts(conflicts, responseName, field1, field2);
            }

            return null;
        }

        private Conflict SubfieldConflicts(
            IEnumerable<Conflict> conflicts,
            string responseName,
            NodeAndDefinitions field1,
            NodeAndDefinitions field2)
        {
            var field1List = new List<GraphQLFieldSelection> { field1.Selection };
            field1List.AddRange(conflicts.SelectMany(e => e.Field1));

            var field2List = new List<GraphQLFieldSelection> { field2.Selection };
            field2List.AddRange(conflicts.SelectMany(e => e.Field2));

            var fields = new Dictionary<string, Conflict>();

            foreach (var conflict in conflicts)
            {
                if (!fields.ContainsKey(conflict.ResponseName))
                {
                    fields.Add(conflict.ResponseName, conflict);
                }
            }

            return new Conflict()
            {
                Field1 = field1List.ToArray(),
                Field2 = field2List.ToArray(),
                ResponseName = responseName,
                Subreasons = fields.Values.ToArray()
            };
        }

        private IEnumerable<Conflict> FindConflictsBetweenSubSelectionSets(
            GraphQLBaseType type1,
            GraphQLBaseType type2,
            GraphQLSelectionSet selectionSet1,
            GraphQLSelectionSet selectionSet2,
            bool areMutuallyExclusive)
        {
            var conflicts = new List<Conflict>();

            var fieldMap1 = new Dictionary<string, ICollection<NodeAndDefinitions>>();
            var fragmentNames1 = new List<string>();

            this.GetFieldsAndFragmentNames(
                selectionSet1, fieldMap1, fragmentNames1, type1);

            var fieldMap2 = new Dictionary<string, ICollection<NodeAndDefinitions>>();
            var fragmentNames2 = new List<string>();

            this.GetFieldsAndFragmentNames(
                selectionSet2, fieldMap2, fragmentNames2, type2);

            conflicts.AddRange(this.CollectConflictsBetween(areMutuallyExclusive, fieldMap1, fieldMap2));

            foreach (var frag in fragmentNames2)
            {
                conflicts.AddRange(this.CollectConflictsBetweenFieldsAndFragment(
                    fieldMap1,
                    frag,
                    areMutuallyExclusive));
            }

            foreach (var frag in fragmentNames1)
            {
                conflicts.AddRange(this.CollectConflictsBetweenFieldsAndFragment(
                    fieldMap2,
                    frag,
                    areMutuallyExclusive));
            }

            foreach (var frag1 in fragmentNames1)
            {
                foreach (var frag2 in fragmentNames2)
                {
                    conflicts.AddRange(this.CollectConflictsBetweenFragments(
                        frag1,
                        frag2,
                        areMutuallyExclusive));
                }
            }

            return conflicts;
        }

        private bool DoTypesConflict(
            GraphQLBaseType type1,
            GraphQLBaseType type2)
        {
            if (type1 is GraphQLList)
            {
                return type2 is GraphQLList
                    ? this.DoTypesConflict(
                        ((GraphQLList)type1).MemberType,
                        ((GraphQLList)type2).MemberType)
                    : true;
            }

            if (type2 is GraphQLList)
            {
                return type1 is GraphQLList
                    ? this.DoTypesConflict(
                        ((GraphQLList)type1).MemberType,
                        ((GraphQLList)type2).MemberType)
                    : true;
            }

            if (type1 is GraphQLNonNull)
            {
                return type2 is GraphQLNonNull
                    ? this.DoTypesConflict(
                        ((GraphQLNonNull)type1).UnderlyingNullableType,
                        ((GraphQLNonNull)type2).UnderlyingNullableType)
                    : true;
            }

            if (type2 is GraphQLNonNull)
            {
                return type1 is GraphQLNonNull
                    ? this.DoTypesConflict(
                        ((GraphQLNonNull)type1).UnderlyingNullableType,
                        ((GraphQLNonNull)type2).UnderlyingNullableType)
                    : true;
            }

            if (type1.IsLeafType || type2.IsLeafType)
            {
                return type1.Name != type2.Name;
            }

            return false;
        }

        private bool HaveSameArguments(GraphQLFieldSelection selection1, GraphQLFieldSelection selection2)
        {
            if (selection1.Arguments.Count() != selection2.Arguments.Count())
                return false;

            return selection1.Arguments
            .All(arg1 => selection2.Arguments
                .Any(arg2 => arg1.Name.Value == arg2.Name.Value
                     && arg1.Value.ToString() == arg2.Value.ToString()));
        }

        private void GetFieldsAndFragmentNames(
                GraphQLSelectionSet node,
                Dictionary<string, ICollection<NodeAndDefinitions>> nodeDefinitions,
                ICollection<string> fragmentNames,
                GraphQLBaseType parentType,
                string presumedParentName = null)
        {
            foreach (var selection in node.Selections)
            {
                switch (selection.Kind)
                {
                    case ASTNodeKind.Field:
                        var fieldSelection = selection as GraphQLFieldSelection;
                        var fieldName = fieldSelection.Name.Value;
                        GraphQLObjectTypeFieldInfo fieldDefinition = null;

                        if (parentType is GraphQLComplexType)
                        {
                            fieldDefinition = ((GraphQLComplexType)parentType).GetFieldInfo(fieldName);
                        }

                        var responseName = fieldSelection.Alias != null
                            ? fieldSelection.Alias.Value
                            : fieldName;

                        if (!nodeDefinitions.ContainsKey(responseName))
                            nodeDefinitions.Add(responseName, new List<NodeAndDefinitions>());

                        nodeDefinitions[responseName].Add(new NodeAndDefinitions()
                        {
                            PresumedParentName = presumedParentName,
                            ParentType = parentType,
                            Selection = fieldSelection,
                            FieldDefinition = fieldDefinition
                        });
                        break;

                    case ASTNodeKind.FragmentSpread:
                        fragmentNames.Add(((GraphQLFragmentSpread)selection).Name.Value);
                        break;

                    case ASTNodeKind.InlineFragment:
                        var inlineFragment = selection as GraphQLInlineFragment;
                        var typeCondition = inlineFragment.TypeCondition;
                        var inlineFragmentType = typeCondition != null
                            ? this.SchemaRepository.GetSchemaOutputTypeByName(typeCondition.Name.Value)
                            : parentType;

                        this.GetFieldsAndFragmentNames(
                            inlineFragment.SelectionSet,
                            nodeDefinitions,
                            fragmentNames,
                            inlineFragmentType,
                            typeCondition?.Name?.Value);
                        break;
                }
            }
        }
    }
}