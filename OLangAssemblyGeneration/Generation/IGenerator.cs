using ErrorHelper;
using OLangAst;

namespace AssemblyGeneration.Generation;

public interface IGenerator
{
    public void GenerateProgram(Program program, string filePath, string fileName);
}