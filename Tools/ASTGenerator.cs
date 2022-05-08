using System.IO;

namespace Point.Tools
{
    public static class ASTGenerator
    {
        public static void DefineAst(string outputPath, string baseName, List<string> types)
        {
            StreamWriter sw = new(outputPath);
            sw.AutoFlush = true;

            sw.WriteLines(new string[]
            {
                "using Point.Lexing;",
                "using Point.Interpreting;",
                "",
                $"namespace Point.Parsing",
                "{",
               $"   [Serializable] public abstract class {baseName}",
                "   {",
            });

            sw.WriteLine("     public abstract T Accept<T>(Visitor<T> visitor);");

            DefineVisitor(sw, baseName, types);

            foreach (string type in types)
            {
                string className = type.Split(":")[0].Trim();
                string classFields = type.Split(":")[1].Trim();
                DefineType(sw, baseName, className, classFields);

                sw.WriteLine();
            }
            sw.WriteLine("  }");

            sw.WriteLine("}");
            sw.Close();
        }

        private static void DefineVisitor(StreamWriter sw, string baseName, List<string> types)
        {
            sw.WriteLine("     public interface Visitor<T>");
            sw.WriteLine("     {");

            foreach (string type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                sw.WriteLine($"         public T Visit{typeName}{baseName}({typeName} {baseName.ToLower()});");
            }

            sw.WriteLine("     }");
        }

        private static void DefineType(StreamWriter sw, string baseName, string className, string classFieldsStr)
        {
            sw.WriteLine($"     [Serializable] public class {className} : {baseName}");
            sw.WriteLine("      {");

            string[] classFields = classFieldsStr.Split(", ");

            foreach (string field in classFields)
                sw.WriteLine($"     public readonly {field};");

            sw.WriteLine();
            sw.WriteLine($"         public {className}({classFieldsStr})");
            sw.WriteLine("          {");
            foreach (string field in classFields)
            {
                string fieldName = field.Split(" ")[1];
                sw.WriteLine($"             this.{fieldName} = {fieldName};");
            }
            sw.WriteLine("          }");

            sw.WriteLine();
            sw.WriteLine("          public override T Accept<T>(Visitor<T> visitor)");
            sw.WriteLine("          {");
            sw.WriteLine($"             return visitor.Visit{className}{baseName}(this);");
            sw.WriteLine("          }");

            sw.WriteLine("      }");
        }
    }
}
