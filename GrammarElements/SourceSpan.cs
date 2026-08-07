namespace Lexing;

public struct SourceSpan(int start, int length)
{
    public int Start = start;
    public int Length = length;

    public static SourceSpan CombineSpans(params SourceSpan[] spans)
    {
        if (spans.Length == 0)
        {
            return new SourceSpan();
        }

        var start = spans.First().Start;
        var maxEnd = 0;

        foreach (var span in spans)
        {
            if (span.Start < start)
            {
                start = span.Start;
            }

            var currentEnd = span.Start + span.Length;
            if (currentEnd > maxEnd)
            {
                maxEnd = currentEnd;
            }
        }

        return new SourceSpan
        {
            Start = start,
            Length = maxEnd - start
        };
    }
}