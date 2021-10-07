%{
// ATTENTION ATTENTION ATTENTION ATTENTION
// this .CS file is a tool generated file from grammar.y and lexer.l
// DO NOT CHANGE BY HAND!!!!
// YOU HAVE BEEN WARNED !!!!

namespace DevExpress.XtraReports.Import.ReportingServices.Expressions {

using System;
	using System.Collections.Generic;
	using DevExpress.Data.Filtering;

	/// <summary>
	///    The C# Parser
	/// </summary>
	public partial class ExpressionParser {
%}
/* YACC Declarations  Cheops grammar*/
%token CONST
%token IDENTIFIER
%token '(' ')'
%left OP_EQ OP_NE
%left OP_GT OP_LT OP_GE OP_LE
%left OR
%left AND
%right NOT
%left '-' '+' '&'
%left '*' '/' OP_MOD
%nonassoc NEG

%%
formula:
	'\0'								{ $$ = $1; }
	| exp '\0'							{ $$ = $1; }
	;

argumentslist:
	'(' commadelimitedlist ')'			{ $$ = $2; }
	| '(' ')'							{ $$ = new List<CriteriaOperator>(); }
	;

commadelimitedlist:
	exp									{ $$ = new List<CriteriaOperator>() { (CriteriaOperator)$1 }; }
	| commadelimitedlist ',' exp		{ var list = (List<CriteriaOperator>)$1; list.Add((CriteriaOperator)$3); $$ = list; }
	;

property:
	IDENTIFIER							{ $$ = new OperandProperty((string)$1); }
	| property '!' IDENTIFIER			{
		OperandProperty prop1 = (OperandProperty)$1;
		$$ = GetOperandPropertyExclamation(prop1.PropertyName, (string)$3);
	}
	| property	'.'	IDENTIFIER			{ $$ = GetOperandPropertyDot((CriteriaOperator)$1, (string)$3); }
	;

exp:
	  CONST								{ $$ = new ConstantValue($1); }
	| property							{ $$ = $1; }

	| exp '*'   exp						{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Multiply ); }
	| exp '/'   exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Divide ); }
	| exp '+'   exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Plus ); }
    | exp '&'   exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Plus ); }
	| exp '-'   exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Minus ); }
    | exp OP_MOD exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Modulo ); }
	| exp OP_EQ exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Equal ); }
	| exp OP_NE exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.NotEqual ); }
	| exp OP_GT exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Greater ); }
	| exp OP_LT exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Less ); }
	| exp OP_LE exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.LessOrEqual ); }
	| exp OP_GE exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.GreaterOrEqual ); }
	| property argumentslist			{ $$ = GetFunctionOperator((CriteriaOperator)$1, (IList<CriteriaOperator>)$2); }

	| NOT exp							{ $$ = new UnaryOperator(UnaryOperatorType.Not, (CriteriaOperator)$2); }
	| exp AND exp						{ $$ = GroupOperator.And((CriteriaOperator)$1, (CriteriaOperator)$3); }
	| exp OR exp						{ $$ = GroupOperator.Or((CriteriaOperator)$1, (CriteriaOperator)$3); }
	| '(' exp ')'						{ $$ = $2; }
	;
%%
}