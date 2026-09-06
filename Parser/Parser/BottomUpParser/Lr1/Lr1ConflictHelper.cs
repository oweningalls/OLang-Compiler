using Lexing;

namespace Parser.Parser.BottomUpParser.Lr1;

public static class Lr1ConflictHelper
{
    public static void CheckForConflicts(HashSet<Lr1Configuration> state)
    {
        var reduceLookaheadGroups = state.Where(x => x.IsReduce()).GroupBy(x => x.GetLookAhead()).ToList();
        
        // there aren't any conflicts if there aren't any reduces
        if (reduceLookaheadGroups.Count == 0) return;

        if (reduceLookaheadGroups.FirstOrDefault(x => x.Count() > 1) is {} group)
        {
            var firstReduce = group.First();
            var secondReduce = group.Last();
            
            throw new Exception($"Reduce/reduce conflict: `{firstReduce}`, `{secondReduce}`");
        }

        var reduces = reduceLookaheadGroups.SelectMany(x => x.AsEnumerable()).ToHashSet();
        var shifts = state.Where(x => x.IsShift()).ToList();
        var shiftsOnTerminals = shifts.Where(x => x.GetElementAfterBookmark().IsAssignableTo(typeof(BaseToken))).ToList();

        var reduceLookaheads = reduces.Select(y => y.GetLookAhead());
        var conflictingShift = shiftsOnTerminals.FirstOrDefault(x => reduceLookaheads.Contains(x.GetElementAfterBookmark()));

        if (conflictingShift == null) return;
        
        var conflictingTerminal = conflictingShift.GetElementAfterBookmark();
        var conflictingReduce = reduces.First(x => x.GetLookAhead() == conflictingTerminal);
        
        throw new Exception($"Shift/reduce conflict: `{conflictingShift}`, `{conflictingReduce}`");
    }
}