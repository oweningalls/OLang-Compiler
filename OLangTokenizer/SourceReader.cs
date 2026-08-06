using Lexing;

namespace OLangLexing;

public class SourceReader
{
    public SourceReader(string sourceText)
    {
        _sourceText = sourceText;
        _lineStarts = [0];

        for (var i = 0; i < sourceText.Length; i++)
        {
            if (sourceText[i] == '\n')
            {
                _lineStarts.Add(i + 1);
            }
        }
    }

    public (int, int) GetLineAndRelativeCharacterNumber(SourceSpan span)
    {
        return GetLineAndRelativeCharacterNumber(span.Start);
    }

    public (int, int) GetLineAndRelativeCharacterNumber(int absoluteCharacterNumber)
    {
        var lineNumber = _lineStarts.BinarySearch(absoluteCharacterNumber);
        if (lineNumber < 0)
        {
            lineNumber = ~lineNumber - 1;
        }
        return (lineNumber, absoluteCharacterNumber - _lineStarts[lineNumber]);
    }

    public string GetLine(int lineNumber)
    {
        int length;
        var begin = _lineStarts[lineNumber];
        length = lineNumber < _lineStarts.Count - 1 ? _lineStarts[lineNumber + 1] - begin : _sourceText.Length - begin;

        return _sourceText.Substring(begin, length);
    }

    private readonly string _sourceText;
    private readonly List<int> _lineStarts;
}