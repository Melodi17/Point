using System.Text;
using Point.Interpreting.Features;
using Point.Lexing;
using Point.Parsing;
using Point.Tools;

namespace Point.Interpreting
{
    public class Interpreter : Expr.Visitor<Object>, Stmt.Visitor<Object>
    {
        public readonly Scope Globals;
        private Scope Scope = new();
        private StringBuilder TopLevel = new();
        private string Attachment = "";

        //private readonly Dictionary<Expr, int> Locals = new();
        public Interpreter()
        {
            Globals = new();
            Scope = new(Globals);

            //Globals.Define("print", new ExternalFunction((i, j) =>
            //{
            //    Console.WriteLine(j.First());
            //    return null;
            //}, 1));
        }

        public string Export()
        {
            string[] splt = Attachment.Split("$&", 2);
            string first = splt.Length > 0 ? splt[0] : "";
            string last = splt.Length > 1 ? splt[1] : "";
            return $"{first}{TopLevel.ToString()}{last}";
        }

        public void Interpret(List<Stmt> statements)
        {
            try
            {
                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Global.RuntimeError(error);
            }
        }
        public void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }
        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }
        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            if (obj is double) return (double)obj > 0;
            if (obj is List<object>) return ((List<object>)obj).Any();
            return true;
        }
        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;

            return left.Equals(right);
        }
        private void CheckIntergerOperand(Token token, object obj)
        {
            if (obj is double) return;

            throw new RuntimeError(token, "Operand must be an interger");
        }
        private void CheckIntergerOperands(Token token, object left, object right)
        {
            if (left is double && right is double) return;

            throw new RuntimeError(token, "Operands must be an intergers");
        }
        public static string Stringify(object obj)
        {
            if (obj == null) return "null";
            if (obj is double)
            {
                string text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text[..^2];
                }
                return text;
            }
            if (obj is bool) return obj.ToString().ToLower();
            if (obj is List<object>)
            {
                return string.Join(", ", ((List<object>)obj).Select(x => Stringify(x)));
            }

            return obj.ToString();
        }
        public object DeclareStmt(Token name, Expr initializer, VariableType type)
        {
            object value = null;
            value = initializer;

            Scope.Set(name, value);
            return value;
        }
        public void ExecuteBlock(List<Stmt> statements, Scope scope)
        {
            Scope previous = Scope;
            try
            {
                Scope = scope;

                foreach (Stmt statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                Scope = previous;
            }
        }

        #region VisitExpr
        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            Scope.Set(expr.name, value);
            return value;
            //return DeclareStmt(expr.name, expr.value, VariableType.Normal);
        }
        public object VisitVariableExpr(Expr.Variable expr)
        {
            object obj = Scope.Get(expr.name);
            if (obj == null)
                return null;

            return obj;
        }
        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.@operator.Type)
            {
                case TokenType.Plus:
                    if (left is double && right is double)
                        return (double)left + (double)right;
                    else if (left is string && right is string)
                        return (string)left + (string)right;
                    else if (left is string || right is string)
                        return left.ToString() + right.ToString();
                    else
                        throw new RuntimeError(expr.@operator, "Operands must be two intergers or two strings");
                    break;

                case TokenType.Minus:
                    CheckIntergerOperands(expr.@operator, left, right);
                    return (double)left - (double)right;

                case TokenType.Star:
                    if (left is double && right is double)
                        return (double)left * (double)right;
                    else if (left is string && right is double)
                        return ((string)left).Multiply(((double)right).ToInt());
                    else if (left is double && right is string)
                        return ((string)right).Multiply(((double)left).ToInt());
                    else
                        throw new RuntimeError(expr.@operator, "Operands must be two intergers or a string and an interger");
                    break;

                case TokenType.Slash:
                    CheckIntergerOperands(expr.@operator, left, right);
                    return (double)left / (double)right;

                case TokenType.Right_Angle:
                    CheckIntergerOperands(expr.@operator, left, right);
                    return (double)left > (double)right;
                case TokenType.Greater_Equal:
                    CheckIntergerOperands(expr.@operator, left, right);
                    return (double)left >= (double)right;
                case TokenType.Left_Angle:
                    CheckIntergerOperands(expr.@operator, left, right);
                    return (double)left < (double)right;
                case TokenType.Less_Equal:
                    CheckIntergerOperands(expr.@operator, left, right);
                    return (double)left <= (double)right;

                case TokenType.Equal_Equal:
                    return IsEqual(left, right);
                case TokenType.Not_Equal:
                    return !IsEqual(left, right);
            }

            return null;
        }
        public object VisitElementExpr(Expr.Element expr)
        {
            Element element = new(expr.tag,
                expr.id,
                expr.classes,
                expr.attributes
                    .Select(x => (x.Key, Stringify(Evaluate(x.Value))))
                    .ToDictionary(x => x.Key, x => x.Item2),
                expr.content
                    .Select(x => Stringify(Evaluate(x)))
                    .ToList());

            if (expr.toplevel)
                TopLevel.Append(element.ToString());

            return element.ToString();
        }
        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }
        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }
        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.@operator.Type)
            {
                case TokenType.Not:
                    return !IsTruthy(right);
                case TokenType.Minus:
                    CheckIntergerOperand(expr.@operator, right);
                    return -(double)right;
            }

            return null;
        }
        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.@operator.Type == TokenType.Pipe_Pipe)
                if (IsTruthy(left)) return left;
                else
                if (!IsTruthy(left)) return left;

            return Evaluate(expr.right);
        }
        #endregion

        #region VisitStmt
        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new Scope(Scope));
            return null;
        }
        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);
            return null;
        }
        public object VisitImportStmt(Stmt.Import stmt)
        {
            Dictionary<string, string> dict = new();

            string file = stmt.name.Literal.ToString();
            string ext = Path.GetExtension(file).ToLower();

            string rel = ext switch
            {
                ".css" => "stylesheet",
                ".ico" => "icon",
                ".html" => "preload",
                ".pt" => "point",
                ".js" => "javascript",
                _ => ""
            };

            if (stmt.type != null)
                rel = stmt.type.Literal.ToString();

            if (rel != "point")
            {
                bool isJs = rel == "javascript";

                dict[isJs ? "src" : "href"] = file;

                if (rel.Length > 0 && !isJs)
                    dict["rel"] = rel;

                Element link = new(isJs ? "script" : "link", "", new(), dict, new());
                TopLevel.Append(link.ToString());
            }
            else
            {
                string exactPath = Path.Join(Global.CurrentPath, file);
                string exportPath = Path.Join(Global.CurrentOutputPath, Path.ChangeExtension(file, ".html"));
                if (!Global.BuiltFiles.Contains(exactPath))
                {
                    Global.BuiltFiles.Add(exactPath);
                    Global.ToBuild.Add((exactPath, exportPath));
                }
            }

            return null;
        }

        public object VisitAttachStmt(Stmt.Attach stmt)
        {
            Global.Output(ConsoleColor.DarkYellow, "[WARN] Attach statements allow for raw injection from external sources, this can be a security risk");
            string path = stmt.name.Literal.ToString();
            if (File.Exists(path))
                Attachment = File.ReadAllText(path);
            else
                Global.Output(ConsoleColor.DarkYellow, "[WARN] Specified file to attach was unavailable");

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            TopLevel.Append(Stringify(Evaluate(stmt.data)));

            return null;
        }

        #endregion
    }
}