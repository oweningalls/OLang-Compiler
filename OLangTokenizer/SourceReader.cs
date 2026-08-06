namespace OLangLexing;

public class SourceReader
{
    public SourceReader(string sourceText)
    {
        SourceText = sourceText;
        LineStarts = [0];

        for (var i = 0; i < sourceText.Length; i++)
        {
            if (sourceText[i] == '\n')
            {
                LineStarts.Add(i + 1);
            }
        }
    }

    public (int, int) GetLineAndRelativeCharacterNumber(int absoluteCharacterNumber)
    {
        var lineNumber = LineStarts.BinarySearch(absoluteCharacterNumber);
        if (lineNumber < 0)
        {
            lineNumber = ~lineNumber - 1;
        }
        return (lineNumber, absoluteCharacterNumber - LineStarts[lineNumber]);
    }

    private string SourceText;
    private List<int> LineStarts;
}