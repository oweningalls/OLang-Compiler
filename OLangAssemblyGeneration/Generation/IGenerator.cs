using ErrorHelper;
using OLangAst;

namespace AssemblyGeneration.Generation;

public interface IGenerator
{
    public string GenerateProgram(Program program);
}