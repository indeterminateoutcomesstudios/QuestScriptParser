using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using QuestScript.Interpreter.Helpers;
using QuestScript.Interpreter.InterpreterElements;
using QuestScript.Interpreter.ScriptElements;
using QuestScript.Interpreter.ValidationExceptions;
using QuestScript.Parser;
using static System.Int32;
using Environment = QuestScript.Interpreter.InterpreterElements.Environment;
// ReSharper disable ArgumentsStyleLiteral

namespace QuestScript.Interpreter
{
    public class EnvironmentTreeBuilder : QuestScriptBaseVisitor<bool>
    {
        private Environment _current;
        private readonly TypeInferenceVisitor _typeInferenceVisitor;

        public Environment Root { get; private set; }       

        public List<BaseValidationException> Errors { get; } = new List<BaseValidationException>();
        public List<BaseValidationException> Warnings { get; } = new List<BaseValidationException>();

        public Dictionary<ParserRuleContext, Environment> EnvironmentsByContext { get; } = new Dictionary<ParserRuleContext, Environment>();

        public EnvironmentTreeBuilder()
        {
            _typeInferenceVisitor = new TypeInferenceVisitor(this);
        }

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
     
        public override bool VisitWhileStatement(QuestScriptParser.WhileStatementContext context)
        {
            //if while has a code block, no need for opening a scope
            var shouldOpenNewScope = !context.code.HasDescendantOfType<QuestScriptParser.CodeBlockStatementContext>();
            if (shouldOpenNewScope)
            {
                _current = _current.CreateChild(context); //push
            }
            var success = base.VisitWhileStatement(context);

            if (shouldOpenNewScope)
            {
                _current = _current.Parent; //pop
            }

            return success;
        }

        public override bool VisitForStatement(QuestScriptParser.ForStatementContext context)
        {
            _current = _current.CreateChild(context); //push

            if (!IsVariableDefined(_current, context.iterationVariable.Text))
            {
                //note: we know that iteration variable of "for" is integer because syntax specifies so
                DeclareLocalVariable(context.iterationVariable.Text, context, context, ObjectType.Integer,
                    () => Parse(context.iterationStart.Text),isIterationVariable:true);
            }
            else
            {
                Errors.Add(
                    new ConflictingVariableName(context,context.iterationVariable.Text,
                            "Iteration variable names in 'for' statements must not conflict with already defined variables."));
            }

            var success = base.VisitForStatement(context);
            _current = _current.Parent; //pop
            return success;
        }

        public override bool VisitForEachStatement(QuestScriptParser.ForEachStatementContext context)
        {
            _current = _current.CreateChild(context); //push

            if (!IsVariableDefined(_current, context.iterationVariable.Text))
            {
                //TODO: investigate possibility of using some sort of enumerator for this case
                DeclareLocalVariable(context.iterationVariable.Text,context, context, context.enumerationVariable, isEnumerationVariable:true);
            }
            else
            {
                Errors.Add(
                    new ConflictingVariableName(context,context.iterationVariable.Text,
                        "Iteration variable names in 'foreach' statements must not conflict with already defined variables."));
            }

            var success = base.VisitForEachStatement(context);

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
            var variableDefined = IsVariableDefined(_current,identifier);
            var isMemberAssignment = identifier.Contains(".");
            
            if (!isMemberAssignment && !variableDefined)
            {
                DeclareLocalVariable(identifier, context, context.LVal, valueContext: context.RVal);
            }
            else if(variableDefined && !isMemberAssignment) //do a type check, since we are not declaring but assigning
            { //do type checking, since this is not a declaration but an assignment
                var rValueType = _typeInferenceVisitor.Visit(context.RVal);
                var lValueType = _typeInferenceVisitor.Visit(context.LVal);
                if(lValueType != rValueType && !TypeConverter.CanConvert(rValueType,lValueType))
                    Errors.Add(new UnexpectedTypeException(context,lValueType,rValueType,context.RVal,"Also, couldn't find suitable implicit casting. Perhaps you should consider manually casting to a new type?"));
            }

            return success;
        }

        private void DeclareLocalVariable(string name,ParserRuleContext statementContext, ParserRuleContext variableContext, ObjectType type, Func<object> valueResolver, bool isEnumerationVariable = false, bool isIterationVariable = false)
        {
            _current.LocalVariables.Add(new Variable
            {
                Name = name,
                Type = type,
                ValueResolver = valueResolver,
                IsEnumerationVariable = isEnumerationVariable, 
                IsIterationVariable = isIterationVariable, //if true, prevent changes to the variable
                Context = variableContext
            });
            _current = _current.CreateNextSibling(statementContext);
        }

        private void DeclareLocalVariable(string name, ParserRuleContext statementContext, ParserRuleContext variableContext, ParserRuleContext valueContext, bool isEnumerationVariable = false, bool isIterationVariable = false)
        {
            var type = _typeInferenceVisitor.Visit(valueContext);
            _current.LocalVariables.Add(new Variable
            {
                Name = name,
                Type = type,
                IsEnumerationVariable = isEnumerationVariable, 
                IsIterationVariable = isIterationVariable,
                Context = variableContext,
                ValueResolver = () => throw new NotImplementedException("Instead of this exception, there should be an output of value resolver visitor, which will be used on the parameter")
            });
            _current = _current.CreateNextSibling(statementContext);
        }

        //to be used by TypeInferenceVisitor
        internal Variable GetVariableFromCurrentEnvironment(string name) => GetVariable(_current, name);

        private void RecordErrorIfUndefined(ParserRuleContext context, string variable)
        {
            if (!IsVariableDefined(_current, variable))
                Errors.Add(new UnresolvedVariableException(variable, context));
        }
        
        private Variable GetVariable(Environment environment, string name)
        {
            while (environment != null)
            {
                //first, check in the current environment
                var variable = environment.LocalVariables.FirstOrDefault(v => v.Name.Equals(name));
                if (variable != null)
                    return variable;

                //then, iterate back over siblings and see if it is defined BEFORE the current one
                while(environment.PrevSibling != null) 
                {
                    environment = environment.PrevSibling;
                    variable = environment.LocalVariables.FirstOrDefault(v => v.Name.Equals(name));
                    if (variable != null)
                        return variable;
                }

                environment = environment.Parent;
            }

            return null;
        }
        
        private bool IsVariableDefined(Environment environment, string name)
        {
            return GetVariable(environment, name) != null;
        }
    }
}
