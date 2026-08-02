using System.Collections;
using Lexing;
using OLangAst;

namespace OLangTests;

public static class ProgramComparer
{
    public static bool AreEquivalent<T>(T first, T second)
    {
        return GenericAreEquivalent(first, second);
    }

    private static bool GenericAreEquivalent(object? first, object? second, int maxDepth = 100)
    {
        if (maxDepth <= 0)
        {
            throw new Exception("Exceeded recursion limit");
        }
        
        if (first == second)
        {
            return true;
        }
        
        if (first == null || second == null)
        {
            return false;
        }
        
        ValidateComparable(first, second);

        var type = first.GetType();

        if (type.IsPrimitive)
        {
            return first.Equals(second);
        }

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
            if (field.FieldType.IsAssignableTo(typeof(INode)) || field.FieldType.IsAssignableTo(typeof(BaseToken)) || field.FieldType.IsAssignableTo(typeof(IAstNode)))
            {
                if (!GenericAreEquivalent(firstValue, secondValue, maxDepth - 1))
                {
                    return false;
                }
            }
            else
            {
                if (!CompareNonNodes(firstValue, secondValue)) return false;
            }
        }

        return true;
    }

    private static bool CompareNonNodes(object? firstValue, object? secondValue)
    {
        if (firstValue is IEnumerable firstEnumerable && secondValue is IEnumerable secondEnumerable)
        {
            if (!CompareEnumerables(firstEnumerable, secondEnumerable))
            {
                return false;
            }
        }
        else if (!firstValue.Equals(secondValue))
        {
            return false;
        }

        return true;
    }

    private static bool CompareEnumerables(IEnumerable firstEnumerable, IEnumerable secondEnumerable)
    {
        var firstList = firstEnumerable.Cast<object>().ToList();
        var secondList = secondEnumerable.Cast<object>().ToList();

        if (firstList.Count != secondList.Count)
        {
            return false;
        }

        for (var i = 0; i < firstList.Count; i++)
        {
            if (!GenericAreEquivalent(firstList[i], secondList[i]))
            {
                return false;
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