%{
// ATTENTION ATTENTION ATTENTION ATTENTION
// this .CS file is a tool generated file from grammar.y and lexer.l
// DO NOT CHANGE BY HAND!!!!
// YOU HAVE BEEN WARNED !!!!

namespace DevExpress.XtraReports.Design.Import.CrystalFormula {
	using System;
	using System.Globalization;
	using System.Collections;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using DevExpress.Data.Filtering;

	public partial class FormulaParser {
%}
%token GENERIC_STATEMENT END_OF_STATEMENT
%token CONST DIRECTIVE
%token PARAM COLUMN FORMULA
%nonassoc NONELSE
%nonassoc ELSE
%token IF THEN SELECT CASE DEFAULT
%token ':'
%token IDENTIFIER
%token '(' ')'
%left OR
%left AND
%right NOT
%left OP_EQ OP_NE
%left OP_GT OP_LT OP_GE OP_LE
%left '^'
%left '&'
%left '-' '+'
%left '*' '/' OP_MOD '%'
%nonassoc NEG

%%
formula:
	  '\0'								{ result = null; }
	| st0 emptystm '\0'					{ result = (CriteriaOperator)$1; }
	| DIRECTIVE ';' st0 emptystm '\0'	{ directive = GetDirective($1); result = (CriteriaOperator)$3; }
	;

emptystm:
	  ';'
	|
	;

 
st0:
	  IF exp THEN st0 %prec NONELSE		{ $$ = GetIifFormula((CriteriaOperator)$2, (CriteriaOperator)$4, null ); }
	| IF exp THEN st0 ELSE st0			{ $$ = GetIifFormula((CriteriaOperator)$2, (CriteriaOperator)$4, (CriteriaOperator)$6 ); } 
	| SELECT exp cases DEFAULT ':' exp	{ $$ = GetSelectExpression((CriteriaOperator)$2, (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)$3, (CriteriaOperator)$6); }
	| exp								{ $$ = $1; }
	;

cases:
	  cases CASE explist ':' exp		{ var list = (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)$1; list.Add(Tuple.Create((List<CriteriaOperator>)$3, (CriteriaOperator)$5)); $$ = list; }
	|									{ $$ = new List<Tuple<List<CriteriaOperator>, CriteriaOperator>>(); }
	;

explist:
	  exp								{ $$ = new List<CriteriaOperator>() { (CriteriaOperator)$1 }; }
	| explist ',' exp					{ var list = (List<CriteriaOperator>)$1; list.Add((CriteriaOperator)$3); $$ = list; }
	;

exp:
	  CONST								{ $$ = new ConstantValue($1); }
	| PARAM								{ $$ = GetParameter($1); }
	| FORMULA							{ $$ = GetFormula($1); }
	| COLUMN							{ $$ = new OperandProperty((string)$1); }
	
	| exp  '*'    exp					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Multiply ); }
	| exp  '/'	  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Divide ); }
	| exp  '+'	  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Plus ); }
	| exp  '-'	  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Minus ); }
	| exp  '&'	  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Plus ); }
	| exp  OP_MOD exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Modulo ); }
	| exp  OP_EQ  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Equal ); }
	| exp  OP_NE  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.NotEqual ); }
	| exp  OP_GT  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Greater ); }
	| exp  OP_LT  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.Less ); }
	| exp  OP_LE  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.LessOrEqual ); }
	| exp  OP_GE  exp 					{ $$ = new BinaryOperator( (CriteriaOperator)$1, (CriteriaOperator)$3, BinaryOperatorType.GreaterOrEqual ); }
	| exp  '%'  exp						{ $$ = GetPercentExpression((CriteriaOperator)$1, (CriteriaOperator)$3); }
	| exp  '^'  exp						{ $$ = GetPowerExpression((CriteriaOperator)$1, (CriteriaOperator)$3); }
	| IDENTIFIER argumentslist			{ $$ = GetFunctionOperator($1, $2); }
	| IDENTIFIER						{ $$ = GetSpecialField($1); }
	| '-' exp %prec NEG					{ $$ = GetNegativeValue($2); }
	| '+' exp %prec NEG 				{ $$ = new UnaryOperator( UnaryOperatorType.Plus, (CriteriaOperator)$2 ); }
	| NOT exp							{ $$ = new UnaryOperator(UnaryOperatorType.Not, (CriteriaOperator)$2); }
	| exp AND exp						{ $$ = GroupOperator.And((CriteriaOperator)$1, (CriteriaOperator)$3); }
	| exp OR exp						{ $$ = GroupOperator.Or((CriteriaOperator)$1, (CriteriaOperator)$3); }
	| '(' exp ')'						{ $$ = $2; }
	;

argumentslist:
	  '(' commadelimitedlist ')'		{ $$ = $2; }
	| '(' ')'							{ $$ = new List<CriteriaOperator>(); }
	;

commadelimitedlist:
	exp									{ $$ = new List<CriteriaOperator>() { (CriteriaOperator)$1 }; }
	| commadelimitedlist ',' exp		{ var list = (List<CriteriaOperator>)$1; list.Add((CriteriaOperator)$3); $$ = list; }
	;
%%
}