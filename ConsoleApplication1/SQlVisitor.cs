using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using System.IO;

namespace SqlParserSample
{
    class NullVisitor : TSqlFragmentVisitor
    {

    }
    class SQlVisitor : TSqlFragmentVisitor
    {
        StreamWriter sw;
        public SQlVisitor(StreamWriter sw)
        {
            this.sw = sw;
        }

        public override void Visit(TSqlStatement node)
        {
            base.Visit(node);
        }
        public override void Visit(SchemaObjectName node)
        {
            String s = "";
            if (node.DatabaseIdentifier != null)
                s += node.DatabaseIdentifier.Value + ".";
            if (node.BaseIdentifier != null)
                s += node.BaseIdentifier.Value;
            sw.WriteLine("<SchemaObject>" + s + "</SchemaObject>");
        }
        public override void Visit(AlterTableAddTableElementStatement node)
        {
            sw.WriteLine("<AlterTableAddTableElementStatement>");
            node.AcceptChildren(this);

            sw.WriteLine("</AlterTableAddTableElementStatement>");
        }
        public override void Visit(ConstraintDefinition a)
        {

            if (a.ConstraintIdentifier == null)
                sw.WriteLine("<Constraint>");
            else
                sw.WriteLine("<Constraint id='" + a.ConstraintIdentifier.Value + "'>");
            a.AcceptChildren(this);
            sw.WriteLine("</Constraint>");
        }
        public override void Visit(ColumnReferenceExpression col)
        {
            sw.WriteLine("Col" + col);
        }

        public override void Visit(CreateTableStatement node)
        {
            sw.WriteLine("<table>");
            Visit(node.SchemaObjectName);
            foreach (ColumnDefinition coldef in node.Definition.ColumnDefinitions)
                Visit(coldef);
            sw.WriteLine("</table>");
        }
        public void Visit(CreateTableStatement node, IList<TSqlParserToken> TKs)
        {
            sw.WriteLine("<table>");
            Visit(node.SchemaObjectName);
            foreach (ColumnDefinition coldef in node.Definition.ColumnDefinitions)
                Visit(coldef, TKs);
            sw.WriteLine("</table>");
        }
        public override void Visit(ColumnDefinition col)
        {
            String s = "<COL id='" + col.ColumnIdentifier.Value + "'";
            SqlDataTypeReference dt = (SqlDataTypeReference)col.DataType;

            s = s + " type='" + dt.SqlDataTypeOption + "'></COL>";
            sw.WriteLine(s);

            col.Accept(new NullVisitor());

        }
        public void Visit(ColumnDefinition col, IList<TSqlParserToken> TKs)
        {
            String s = "<COL id='" + col.ColumnIdentifier.Value + "'";
            SqlDataTypeReference dt = (SqlDataTypeReference)col.DataType;


            //search 
            int line = col.DataType.StartLine;
            int offset = col.DataType.StartOffset;
            int FragmentLength = col.DataType.FragmentLength;

            StringBuilder R = new StringBuilder("");

            int start = 0;
            for (int k = col.FirstTokenIndex; k<col.LastTokenIndex; k++)
            {
                TSqlParserToken TK = TKs.ElementAt(k);
               if (TK.Line == line)// && TK.Offset >= (offset + FragmentLength))
                {
                    if (TK.TokenType == TSqlTokenType.LeftParenthesis)
                    {
                        start = 1;
                    }
                    if (start == 1)
                    {
                        R.Append(TK.Text);
                    }
                    if (TK.TokenType == TSqlTokenType.RightParenthesis)
                    {
                        break;
                    }
                }
            //    else break; 

            }

            s = s + " type='" + dt.SqlDataTypeOption + R + "'></COL>";
            // TKs[col.LastTokenIndex+1]
            sw.WriteLine(s);

            col.Accept(new NullVisitor());

        }
        public override void Visit(ForeignKeyConstraintDefinition v)
        {
            sw.WriteLine("<COL>");
            String s = "";
            foreach (var col in v.Columns)
                s = s + col.Value + ",";
            s = s.TrimEnd(',');
            sw.WriteLine(s);
            sw.WriteLine("</COL>");
            sw.WriteLine("<Ref>");
            v.ReferenceTableName.Accept(this);

            sw.WriteLine("<COL>");

            s = "";
            foreach (var col in v.ReferencedTableColumns)
                s = s + col.Value + ",";
            s = s.TrimEnd(',');
            sw.WriteLine(s);
            sw.WriteLine("</COL>");

            sw.WriteLine("</Ref>");
        }


    }

}
