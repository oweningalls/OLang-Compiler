using AssemblyGeneration.Generation;
using ErrorHelper;

namespace OLangCompiler;

public interface IOLangFrontEnd
{
    public void Compile(string program, IGenerator generator, IErrorHelper errorHelper, string filePath, string fileName);
}