using Lexing;

namespace OLangTests;

public static class ProgramComparer
{
    public static bool AreEquivalent<T>(T first, T second) where T : INode
    {
        return AreEquivalent((object)first, second);
    }

    private static bool AreEquivalent(object first, object second, int maxDepth = 100)
    {
        if (maxDepth <= 0)
        {
            throw new Exception("Exceeded recursion limit");
        }
        
        ValidateComparable(first, second);

        var type = first.GetType();

        var fields = type.GetFields();

        var isToken = type.IsAssignableTo(typeof(BaseToken));
        if (isToken)
        {
            var excludedFields = new List<string>
            {
                nameof(BaseToken.LineNumber),
                nameof(BaseToken.AbsoluteEndCharNumber),
                nameof(BaseToken.AbsoluteStartCharNumber),
                nameof(BaseToken.RelativeEndCharNumber),
                nameof(BaseToken.RelativeStartCharNumber),
            };
            fields = fields.Where(x => !excludedFields.Contains(x.Name)).ToArray();
        }

        foreach (var field in fields)
        {
            var firstValue = field.GetValue(first);
            var secondValue = field.GetValue(second);
            if (field.FieldType.IsAssignableTo(typeof(INode)) || field.FieldType.IsAssignableTo(typeof(BaseToken)))
            {
                if (!AreEquivalent(firstValue, secondValue, maxDepth - 1))
                {
                    return false;
                }
            }
            else
            {
                if (!firstValue.Equals(secondValue))
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static void ValidateComparable(object first, object second)
    {
        var type = first.GetType();

        if (type != second.GetType())
        {
            throw new Exception($"Objects of different types {type.Name} and {second.GetType().Name} cannot be equivalent");
        }
    }

}