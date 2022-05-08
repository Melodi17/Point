using System.Diagnostics;
using Melodi.Efficient;
using Point.Interpreting;
using Point.Lexing;
using Point.Parsing;
using Point.Tools;

public static class Global
{
    public static Interpreter Interpreter = new();
    public static bool ErrorOccured = false;
    public static bool RuntimeErrorOccured = false;
    public static string DataPath;
    public static string LibDataPath;
    public static string CurrentPath;
    public static string CurrentOutputPath;

    public static List<string> BuiltFiles = new();
    public static List<(string, string)> ToBuild = new();

    public static void Main(string[] args)
    {
        DataPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Point");
        LibDataPath = Path.Join(DataPath, "Libraries");

        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(LibDataPath);

        if (args.Any(x => x == "UPDATE_AST"))
            UpdateAST();
        else if (args.Any())
        {
            string[] arr = Box.IOUtils.ConnectStrings(args.Join(" "), false);
            if (arr.Length == 1)
            {
                string pth = Path.Join(Path.GetTempPath(), $"{{{Guid.NewGuid()}}}");
                Directory.CreateDirectory(pth);

                string fl = Path.Join(pth, Path.ChangeExtension(Path.GetFileName(arr[0]), "html"));
                RunFile(arr[0], fl);
                new Process
                {
                    StartInfo = new ProcessStartInfo(fl)
                    {
                        UseShellExecute = true
                    }
                }.Start();
            }
            else
            {
                string kw = arr[0];
                if (kw == "build")
                {
                    if (arr.Length == 2)
                    {
                        string htmlPath = Path.ChangeExtension(arr[1], "html");
                        RunFile(arr[1], htmlPath);
                    }
                    else if (arr.Length == 3)
                        RunFile(arr[1], arr[2]);
                }
                else if (kw == "help")
                    Help();
            }

        }
        else
            Help();
    }

    public static void RunFile(string path, string outputPath)
    {
        CurrentPath = (Path.GetDirectoryName(path));
        CurrentOutputPath = (Path.GetDirectoryName(outputPath));
        Run(File.ReadAllText(path));

        if (ErrorOccured) Environment.Exit(65);
        if (RuntimeErrorOccured) Environment.Exit(70);

        Output(ConsoleColor.Blue, $"[INFO] Transpiling...");
        File.WriteAllText(outputPath, Interpreter.Export());
        Output(ConsoleColor.Green, $"[INFO] Successfully transpiled '{path}' to '{outputPath}'");

        if (ToBuild.Any())
        {
            Interpreter = new();
            var fst = ToBuild.First();
            ToBuild.RemoveAt(0);
            RunFile(fst.Item1, fst.Item2);
        }
    }

    private static void RunPrompt()
    {
        while (true)
        {
            Console.Write("> ");
            string line = Console.ReadLine();
            if (line == null) break;
            Run(line);
            ErrorOccured = false;
        }
    }

    public static void Run(string source)
    {
        Output(ConsoleColor.Blue, $"[INFO] Lexing...");
        Lexer lexer = new(source);
        List<Token> tokens = lexer.Lex();

        Output(ConsoleColor.Blue, $"[INFO] Parsing...");
        Parser parser = new(tokens);
        List<Stmt> statements = parser.Parse();

        if (ErrorOccured) return;

        try
        {
            Output(ConsoleColor.Blue, $"[INFO] Interpreting...");
            Interpreter.Interpret(statements);
        }
        catch (Exception) { RuntimeErrorOccured = true; }
    }

    private static void UpdateAST()
    {
        ASTGenerator.DefineAst(Path.Join("..", "..", "..", "Parsing", "Expr.cs"), "Expr", new string[]
        {
            "Assign   : Token name, Expr value",
            "Binary   : Expr left, Token @operator, Expr right",
            "Grouping : Expr expression",
            "Literal  : Object value",
            "Logical  : Expr left, Token @operator, Expr right",
            "Element  : string tag, string id, List<string> classes, Dictionary<string,Expr> attributes, List<Expr> content, bool toplevel",
            "Unary    : Token @operator, Expr right",
            "Variable : Token name"
        }.ToList());

        ASTGenerator.DefineAst(Path.Join("..", "..", "..", "Interpreting", "Stmt.cs"), "Stmt", new string[]
        {
            "Block      : List<Stmt> statements",
            "Expression : Expr expression",
            "Import     : Token name, Token type",
            "Attach     : Token name",
            "Print      : Expr data",
        }.ToList());

        Console.WriteLine("[ASTGenerator] AST nodes Expr and Stmt have been updated");
        Environment.Exit(0);
    }

    private static void Help()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine(" point help");
        Console.WriteLine(" point build <file>");
        Console.WriteLine(" point build <file> <output>");
        Console.WriteLine(" point <file>");
        Console.WriteLine();
        Console.WriteLine("Syntax:");
        Console.WriteLine(" Commands:");
        Console.WriteLine("  import <file>           Links file to current document");
        Console.WriteLine("  import <file> as <type> Links file to current document as specific type");
        Console.WriteLine("  attach <file>           Attaches specific file to header and footer of result");
        Console.WriteLine("  print <data>            Write raw data to result");
        Console.WriteLine();
        Console.WriteLine(" Variables:");
        Console.WriteLine("  $<addr> = <value>       Set the value of a varaible");
        Console.WriteLine("  $<addr>                 Get the value of a varaible");
        Console.WriteLine();
        Console.WriteLine(" Definitions:");
        Console.WriteLine("  <tag>.<class>#<id> {");
        Console.WriteLine("   <attribKey*>: <attribValue>;");
        Console.WriteLine("   <value*>;");
        Console.WriteLine("  }");

        Environment.Exit(0);
    }


    public static void Error(int line, string msg)
    {
        Output(ConsoleColor.Red, $"[ERR ] {line}: {msg}");
        ErrorOccured = true;
    }

    public static void Error(Token token, string msg)
    {
        Output(ConsoleColor.Red, $"[ERR ] {token.Line}, {token.Raw}: {msg}");
        ErrorOccured = true;
    }

    public static void RuntimeError(RuntimeError error)
    {
        Output(ConsoleColor.Red, $"[RERR] {error.Token.Line}, {error.Token.Raw}: {error.Message}");
        RuntimeErrorOccured = true;
    }

    public static void Output(ConsoleColor color, string text)
    {
        Console.ForegroundColor = color;
        Console.Write("█ ");
        Console.ResetColor();
        Console.WriteLine(text);
    }
}