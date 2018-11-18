using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using QuestScript.Interpreter.InterpreterElements;
using QuestScript.Interpreter.ValidationExceptions;
using QuestScript.Parser;
using Environment = QuestScript.Interpreter.InterpreterElements.Environment;

namespace QuestScript.Interpreter
{
    public class EnvironmentTreeBuilder : QuestScriptBaseListener
    {
        private Environment _current;
        private readonly TypeInferenceVisitor _typeInferencer = new TypeInferenceVisitor();

        //things that rely on fully built environment tree - like checking whether variables are used BEFORE they are declared
        private readonly List<Action> _deferredToTheEndOfParsingActions = new List<Action>();
        public Environment Root { get; private set; }

        public List<Exception> Errors { get; } = new List<Exception>();

        public Dictionary<ParserRuleContext, Environment> EnvironmentsByContext { get; } = new Dictionary<ParserRuleContext, Environment>();

        public override void EnterScript(QuestScriptParser.ScriptContext context)
        {
            Root = new Environment
            {
                Context = context
            };
            _current = Root;
            base.EnterScript(context);
        }

        public override void ExitScript(QuestScriptParser.ScriptContext context)
        {
            base.ExitScript(context);
            foreach (var action in _deferredToTheEndOfParsingActions)
                action();
        }

        public override void EnterCodeBlockStatement(QuestScriptParser.CodeBlockStatementContext context)
        {          
            //code block start opens a new level of environments
            _current = _current.CreateChild(context);
            base.EnterCodeBlockStatement(context);
        }

        public override void ExitCodeBlockStatement(QuestScriptParser.CodeBlockStatementContext context)
        {
            base.ExitCodeBlockStatement(context);
            _current = _current.Parent;
        }

        public override void EnterStatement(QuestScriptParser.StatementContext context)
        {
            //assign relevant statement to environment, so we can later resolve variables
            //and check if they are used before they are defined
            _current.Statements.Add(context);
            EnvironmentsByContext.Add(context,_current);
            base.EnterStatement(context);
        }

        public override void ExitVariableOperand(QuestScriptParser.VariableOperandContext context)
        {
            base.ExitVariableOperand(context);
            var env = _current;
            var variable = context.GetText();
            if (!IsVariableDefined(env, variable))
            {
                RecordErrorIfUndefined(context, variable);
            }
        }

        private void RecordErrorIfUndefined(ParserRuleContext context, string variable)
        {
            if (!IsVariableDefined(_current, variable))
                Errors.Add(new UnresolvedVariableException(variable, context));
        }

        public override void ExitAssignmentStatement(QuestScriptParser.AssignmentStatementContext context)
        {
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

            if(context.RVal.HasDescendantOfType<QuestScriptParser.IdentifierOperandContext>() && !IsVariableDefined(_current,context.RVal.GetText()))
                RecordErrorIfUndefined(context,context.RVal.GetText());
            else if(context.RVal.HasDescendantOfType<QuestScriptParser.MemberFieldOperandContext>())
            {
                //TODO : add resolution to object members as well                
            }

            base.ExitAssignmentStatement(context);
        }

        public bool IsVariableDefined(Environment environment, string name)
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
