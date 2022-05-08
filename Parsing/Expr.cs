using Point.Lexing;
using Point.Interpreting;

namespace Point.Parsing
{
   [Serializable] public abstract class Expr
   {
     public abstract T Accept<T>(Visitor<T> visitor);
     public interface Visitor<T>
     {
         public T VisitAssignExpr(Assign expr);
         public T VisitBinaryExpr(Binary expr);
         public T VisitGroupingExpr(Grouping expr);
         public T VisitLiteralExpr(Literal expr);
         public T VisitLogicalExpr(Logical expr);
         public T VisitElementExpr(Element expr);
         public T VisitUnaryExpr(Unary expr);
         public T VisitVariableExpr(Variable expr);
     }
     [Serializable] public class Assign : Expr
      {
     public readonly Token name;
     public readonly Expr value;

         public Assign(Token name, Expr value)
          {
             this.name = name;
             this.value = value;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitAssignExpr(this);
          }
      }

     [Serializable] public class Binary : Expr
      {
     public readonly Expr left;
     public readonly Token @operator;
     public readonly Expr right;

         public Binary(Expr left, Token @operator, Expr right)
          {
             this.left = left;
             this.@operator = @operator;
             this.right = right;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitBinaryExpr(this);
          }
      }

     [Serializable] public class Grouping : Expr
      {
     public readonly Expr expression;

         public Grouping(Expr expression)
          {
             this.expression = expression;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitGroupingExpr(this);
          }
      }

     [Serializable] public class Literal : Expr
      {
     public readonly Object value;

         public Literal(Object value)
          {
             this.value = value;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitLiteralExpr(this);
          }
      }

     [Serializable] public class Logical : Expr
      {
     public readonly Expr left;
     public readonly Token @operator;
     public readonly Expr right;

         public Logical(Expr left, Token @operator, Expr right)
          {
             this.left = left;
             this.@operator = @operator;
             this.right = right;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitLogicalExpr(this);
          }
      }

     [Serializable] public class Element : Expr
      {
     public readonly string tag;
     public readonly string id;
     public readonly List<string> classes;
     public readonly Dictionary<string,Expr> attributes;
     public readonly List<Expr> content;
     public readonly bool toplevel;

         public Element(string tag, string id, List<string> classes, Dictionary<string,Expr> attributes, List<Expr> content, bool toplevel)
          {
             this.tag = tag;
             this.id = id;
             this.classes = classes;
             this.attributes = attributes;
             this.content = content;
             this.toplevel = toplevel;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitElementExpr(this);
          }
      }

     [Serializable] public class Unary : Expr
      {
     public readonly Token @operator;
     public readonly Expr right;

         public Unary(Token @operator, Expr right)
          {
             this.@operator = @operator;
             this.right = right;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitUnaryExpr(this);
          }
      }

     [Serializable] public class Variable : Expr
      {
     public readonly Token name;

         public Variable(Token name)
          {
             this.name = name;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitVariableExpr(this);
          }
      }

  }
}
