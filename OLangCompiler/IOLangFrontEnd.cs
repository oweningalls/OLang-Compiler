using AssemblyGeneration.Generation;
using AstHelpers;
using ErrorHelper;

namespace OLangCompiler;

public interface IOLangFrontEnd
{
    public void Compile(string program, IGenerator generator, IErrorHelper errorHelper, TypeHelper typeHelper, string filePath, string fileName);
}