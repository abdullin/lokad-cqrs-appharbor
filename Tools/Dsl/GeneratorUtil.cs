using System.CodeDom.Compiler;
using System.IO;
using System.Text;

namespace Hub.Dsl
{
    public static class GeneratorUtil
    {
        public static string Build(string source, TemplatedGenerator generator)
        {
            var builder = new StringBuilder();
            using (var stream = new StringWriter(builder))
            using (var writer = new IndentedTextWriter(stream, "    "))
            {
                generator.Generate(GenerateContext(source), writer);
            }
            return builder.ToString();
        }

        public static Context GenerateContext(string source)
        {
            var context = new MessageContractAssembler().From(source);
            return context;
        }

        public static string ParameterCase(string s)
        {
            return char.ToLowerInvariant(s[0]) + s.Substring(1);
        }

        public static string MemberCase(string s)
        {
            return char.ToUpperInvariant(s[0]) + s.Substring(1);
        }
    }
}