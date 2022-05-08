using Point.Lexing;
using Point.Interpreting;

namespace Point.Parsing
{
   [Serializable] public abstract class Stmt
   {
     public abstract T Accept<T>(Visitor<T> visitor);
     public interface Visitor<T>
     {
         public T VisitBlockStmt(Block stmt);
         public T VisitExpressionStmt(Expression stmt);
         public T VisitImportStmt(Import stmt);
         public T VisitAttachStmt(Attach stmt);
         public T VisitPrintStmt(Print stmt);
     }
     [Serializable] public class Block : Stmt
      {
     public readonly List<Stmt> statements;

         public Block(List<Stmt> statements)
          {
             this.statements = statements;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitBlockStmt(this);
          }
      }

     [Serializable] public class Expression : Stmt
      {
     public readonly Expr expression;

         public Expression(Expr expression)
          {
             this.expression = expression;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitExpressionStmt(this);
          }
      }

     [Serializable] public class Import : Stmt
      {
     public readonly Token name;
     public readonly Token type;

         public Import(Token name, Token type)
          {
             this.name = name;
             this.type = type;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitImportStmt(this);
          }
      }

     [Serializable] public class Attach : Stmt
      {
     public readonly Token name;

         public Attach(Token name)
          {
             this.name = name;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitAttachStmt(this);
          }
      }

     [Serializable] public class Print : Stmt
      {
     public readonly Expr data;

         public Print(Expr data)
          {
             this.data = data;
          }

          public override T Accept<T>(Visitor<T> visitor)
          {
             return visitor.VisitPrintStmt(this);
          }
      }

  }
}
