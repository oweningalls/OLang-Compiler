using AssemblyGeneration.Generation;
using ErrorHelper;

namespace OLangCompiler;

public interface IOLangFrontEnd
{
    public string Compile(string program, IGenerator generator, IErrorHelper errorHelper);
}