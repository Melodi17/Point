namespace Point.Parsing
{
    //public class ASTPrinter : Expr.Visitor<string>
    //{
    //    public string Print(Expr expr)
    //    {
    //        return expr.Accept(this);
    //    }
    //    public string VisitBinaryExpr(Expr.Binary expr)
    //    => Parenthesize(expr.@operator.Raw, expr.left, expr.right);

    //    public string VisitGroupingExpr(Expr.Grouping expr)
    //    => Parenthesize("group", expr.expression);

    //    public string VisitLiteralExpr(Expr.Literal expr)
    //    => expr.value == null ? "null" : Parenthesize(expr.value.ToString());

    //    public string VisitUnaryExpr(Expr.Unary expr)
    //    => Parenthesize(expr.@operator.Raw, expr.right);

    //    public string VisitVariableExpr(Expr.Variable expr)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    private string Parenthesize(string name, params Expr[] exprs)
    //    {
    //        StringBuilder sb = new();
    //        sb.Append("(").Append(name);

    //        foreach (var expr in exprs)
    //        {
    //            sb.Append(" ");
    //            sb.Append(expr.Accept(this));
    //        }

    //        sb.Append(")");

    //        return sb.ToString();
    //    }
    //}
}
