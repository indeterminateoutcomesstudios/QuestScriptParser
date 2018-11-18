using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using QuestScript.Interpreter.InterpreterElements;
using QuestScript.Interpreter.ValidationExceptions;
using QuestScript.Parser;
using Environment = QuestScript.Interpreter.InterpreterElements.Environment;

namespace QuestScript.Interpreter
{
    public class EnvironmentTreeBuilder : QuestScriptBaseVisitor<bool>
    {
        private Environment _current;
        private readonly TypeInferenceVisitor _typeInferencer = new TypeInferenceVisitor();

        public Environment Root { get; private set; }

        public List<Exception> Errors { get; } = new List<Exception>();

        public Dictionary<ParserRuleContext, Environment> EnvironmentsByContext { get; } = new Dictionary<ParserRuleContext, Environment>();

        public override bool Visit(IParseTree tree)
        {
            base.Visit(tree);
            return Errors.Count == 0;
        }

        public override bool VisitScript(QuestScriptParser.ScriptContext context)
        {
            Root = new Environment
            {
                Context = context
            };
            _current = Root;
            return base.VisitScript(context);
        }

        public override bool VisitCodeBlockStatement(QuestScriptParser.CodeBlockStatementContext context)
        {
            _current = _current.CreateChild(context); //push
            var success = base.VisitCodeBlockStatement(context);
            _current = _current.Parent; //pop
            return success;
        }

        public override bool VisitStatement(QuestScriptParser.StatementContext context)
        {
            //assign relevant statement to environment, so we can later resolve variables
            //and check if they are used before they are defined
            _current.Statements.Add(context);
            EnvironmentsByContext.Add(context,_current);
            return base.VisitStatement(context);
        }
       

        //TODO : add resolution to object members as well                
        public override bool VisitIdentifierOperand(QuestScriptParser.IdentifierOperandContext context)
        {
            //if the parent is RValue -> it is an identifier that is part of an expression, and not a statement
            if(context.HasParentOfType<QuestScriptParser.RValueContext>() && !IsVariableDefined(_current,context.GetText()))
                RecordErrorIfUndefined(context,context.GetText());

            return base.VisitIdentifierOperand(context);
        }

        public override bool VisitAssignmentStatement(QuestScriptParser.AssignmentStatementContext context)
        {
            var success = base.VisitAssignmentStatement(context);

            var identifier = context.LVal.GetText();
            if (!identifier.Contains(".") && //member assignment is not a local variable...
                !IsVariableDefined(_current,identifier))
            {
                var type = _typeInferencer.Visit(context.RVal);
                _current.LocalVariables.Add(new Variable
                {
                    Name = identifier,
                    Type = type,
                    Value = () => context.RVal
                });
                _current = _current.CreateNextSibling(context);
            }

            return success;
        }

        private void RecordErrorIfUndefined(ParserRuleContext context, string variable)
        {
            if (!IsVariableDefined(_current, variable))
                Errors.Add(new UnresolvedVariableException(variable, context));
        }

        private bool IsVariableDefined(Environment environment, string name)
        {
            while (environment != null)
            {
                //first, check in the current environment
                if (environment.LocalVariables.Any(v => v.Name.Equals(name)))
                    return true;

                //then, iterate back over siblings and see if it is defined BEFORE the current one
                while(environment.PrevSibling != null) 
                {
                    environment = environment.PrevSibling;
                    if (environment.LocalVariables.Any(v => v.Name.Equals(name)))
                        return true;
                }

                environment = environment.Parent;
            }

            return false;
        }
    }
}
