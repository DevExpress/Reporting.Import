
#line 2 "ExpressionGrammar.y"
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
#line default
  int yyMax;

  Object yyparse (yyInput yyLex) {
    if (yyMax <= 0) yyMax = 256;			// initial size
    int yyState = 0;                                   // state stack ptr
    int [] yyStates = new int[yyMax];	                // state stack 
    Object yyVal = null;                               // value stack ptr
    Object [] yyVals = new Object[yyMax];	        // value stack
    int yyToken = -1;					// current input
    int yyErrorFlag = 0;				// #tks to shift

    int yyTop = 0;
    goto skip;
    yyLoop:
    yyTop++;
    skip:
    for(;;) {  //++yyTop) {
      if(yyTop >= yyStates.Length) {			// dynamically increase
        int[] i = new int[yyStates.Length + yyMax];
        yyStates.CopyTo(i, 0);
        yyStates = i;
        Object[] o = new Object[yyVals.Length + yyMax];
        yyVals.CopyTo(o, 0);
        yyVals = o;
      }
      yyStates[yyTop] = yyState;
      yyVals[yyTop] = yyVal;

      yyDiscarded:	// discarding a token does not change stack
      for(;;) {
        int yyN;
        if ((yyN = yyDefRed[yyState]) == 0) {	// else [default] reduce (yyN)
          if(yyToken < 0)
            yyToken = yyLex.advance() ? yyLex.token() : 0;
          if((yyN = yySindex[yyState]) != 0 && ((yyN += yyToken) >= 0)
              && (yyN < yyTable.Length) && (yyCheck[yyN] == yyToken)) {
            yyState = yyTable[yyN];		// shift to yyN
            yyVal = yyLex.value();
            yyToken = -1;
            if (yyErrorFlag > 0) -- yyErrorFlag;
            goto yyLoop;
          }
          if((yyN = yyRindex[yyState]) != 0 && (yyN += yyToken) >= 0
              && yyN < yyTable.Length && yyCheck[yyN] == yyToken)
            yyN = yyTable[yyN];			// reduce (yyN)
          else
            switch(yyErrorFlag) {
  
            case 0:
              yyerror("syntax error");
              goto case 1;
            case 1: case 2:
              yyErrorFlag = 3;
              do {
                if((yyN = yySindex[yyStates[yyTop]]) != 0
                    && (yyN += Token.yyErrorCode) >= 0 && yyN < yyTable.Length
                    && yyCheck[yyN] == Token.yyErrorCode) {
                  yyState = yyTable[yyN];
                  yyVal = yyLex.value();
                  goto yyLoop;
                }
              } while (--yyTop >= 0);
              yyerror("irrecoverable syntax error");
              goto yyDiscarded;
  
            case 3:
              if (yyToken == 0)
                yyerror("irrecoverable syntax error at end-of-file");
              yyToken = -1;
              goto yyDiscarded;		// leave stack alone
            }
        }
        int yyV = yyTop + 1 - yyLen[yyN];
        yyVal = yyV > yyTop ? null : yyVals[yyV];
        switch(yyN) {
case 1:
#line 33 "ExpressionGrammar.y"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 2:
#line 34 "ExpressionGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 3:
#line 38 "ExpressionGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 4:
#line 39 "ExpressionGrammar.y"
  { yyVal = new List<CriteriaOperator>(); }
  break;
case 5:
#line 43 "ExpressionGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 6:
#line 44 "ExpressionGrammar.y"
  { var list = (List<CriteriaOperator>)yyVals[-2+yyTop]; list.Add((CriteriaOperator)yyVals[0+yyTop]); yyVal = list; }
  break;
case 7:
#line 48 "ExpressionGrammar.y"
  { yyVal = new OperandProperty((string)yyVals[0+yyTop]); }
  break;
case 8:
#line 49 "ExpressionGrammar.y"
  {
		OperandProperty prop1 = (OperandProperty)yyVals[-2+yyTop];
		yyVal = GetOperandPropertyExclamation(prop1.PropertyName, (string)yyVals[0+yyTop]);
	}
  break;
case 9:
#line 53 "ExpressionGrammar.y"
  { yyVal = GetOperandPropertyDot((CriteriaOperator)yyVals[-2+yyTop], (string)yyVals[0+yyTop]); }
  break;
case 10:
#line 57 "ExpressionGrammar.y"
  { yyVal = new ConstantValue(yyVals[0+yyTop]); }
  break;
case 11:
#line 58 "ExpressionGrammar.y"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 12:
#line 60 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Multiply ); }
  break;
case 13:
#line 61 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Divide ); }
  break;
case 14:
#line 62 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 15:
#line 63 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 16:
#line 64 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Minus ); }
  break;
case 17:
#line 65 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Equal ); }
  break;
case 18:
#line 66 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.NotEqual ); }
  break;
case 19:
#line 67 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Greater ); }
  break;
case 20:
#line 68 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Less ); }
  break;
case 21:
#line 69 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.LessOrEqual ); }
  break;
case 22:
#line 70 "ExpressionGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.GreaterOrEqual ); }
  break;
case 23:
#line 71 "ExpressionGrammar.y"
  { yyVal = GetFunctionOperator((CriteriaOperator)yyVals[-1+yyTop], (IList<CriteriaOperator>)yyVals[0+yyTop]); }
  break;
case 24:
#line 73 "ExpressionGrammar.y"
  { yyVal = new UnaryOperator(UnaryOperatorType.Not, (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 25:
#line 74 "ExpressionGrammar.y"
  { yyVal = GroupOperator.And((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 26:
#line 75 "ExpressionGrammar.y"
  { yyVal = GroupOperator.Or((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 27:
#line 76 "ExpressionGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
#line default
        }
        yyTop -= yyLen[yyN];
        yyState = yyStates[yyTop];
        int yyM = yyLhs[yyN];
        if(yyState == 0 && yyM == 0) {
          yyState = yyFinal;
          if(yyToken < 0)
            yyToken = yyLex.advance() ? yyLex.token() : 0;
          if(yyToken == 0)
            return yyVal;
          goto yyLoop;
        }
        if(((yyN = yyGindex[yyM]) != 0) && ((yyN += yyState) >= 0)
            && (yyN < yyTable.Length) && (yyCheck[yyN] == yyState))
          yyState = yyTable[yyN];
        else
          yyState = yyDgoto[yyM];
	 goto yyLoop;
      }
    }
  }

   static  short [] yyLhs  = {              -1,
    0,    0,    2,    2,    3,    3,    4,    4,    4,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,
  };
   static  short [] yyLen = {           2,
    1,    2,    3,    2,    1,    3,    1,    3,    3,    1,
    1,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    2,    2,    3,    3,    3,
  };
   static  short [] yyDefRed = {            0,
   10,    7,    0,    0,    1,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    2,    0,    0,    0,   23,   27,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   12,   13,    4,    0,    0,    8,    9,    3,    0,    0,
  };
  protected static  short [] yyDgoto  = {             6,
    7,   28,   45,    8,
  };
  protected static  int yyFinal = 6;
  protected static  short [] yySindex = {            1,
    0,    0,  -10,  -10,    0,    0,   18,   -1,  -38,  133,
  -10,  -10,  -10,  -10,  -10,  -10,  -10,  -10,  -10,  -10,
  -10,  -10,  -10,    0,  112, -244, -239,    0,    0,  -22,
  -22,  -14,  -14,  -14,  -14,  102,  133,  -36,  -36,  -36,
    0,    0,    0,  -30,   -6,    0,    0,    0,  -10,  -30,
  };
  protected static  short [] yyRindex = {            0,
    0,    0,    0,    0,    0,    0,    0,   10,    0,   50,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,   95,
   97,   66,   73,   81,   88,    2,   58,   26,   34,   42,
    0,    0,    0,   -4,    0,    0,    0,    0,    0,    3,
  };
  protected static  short [] yyGindex = {            0,
  147,    0,    0,    0,
  };
  protected static  short [] yyTable = {            21,
    5,   26,   29,   22,   20,   22,   19,   21,   23,   11,
   23,   22,   20,   46,   19,   21,   23,   24,   47,   22,
   20,    0,   19,   21,   23,   16,    0,   22,   20,    3,
   19,   26,   23,   14,   48,    0,    5,   49,   25,    5,
    3,   15,   26,    6,   27,   26,    6,   11,    0,   24,
   11,   11,   11,   11,   11,   21,   11,   25,    0,   22,
   20,    0,   19,   16,   23,   19,   16,    0,   16,   16,
   16,   14,   20,    0,   14,    0,   14,   14,   14,   15,
   22,    0,   15,    0,   15,   15,   15,   21,    0,    0,
   24,    0,    0,   24,   17,    0,   18,    0,   25,    0,
    0,   25,    0,    0,    0,    0,   19,    0,    0,   19,
    0,    0,    0,   20,    0,    0,   20,    0,    0,    0,
    0,   22,    0,    0,   22,    0,    0,    0,   21,    0,
    0,   21,    0,    0,    0,   17,    0,   18,   17,   21,
   18,    0,    0,   22,   20,    0,   19,    0,   23,    9,
   10,    3,   43,    0,    0,    0,    0,   30,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   21,   44,    0,    0,   22,   20,    0,   19,    0,   23,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   50,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   11,   12,   13,   14,   15,   16,   17,   18,   11,   12,
   13,   14,   15,   16,   17,   18,    0,    0,   13,   14,
   15,   16,   17,   18,    0,    0,    1,    2,    0,    0,
   17,   18,    0,    0,    0,    0,    4,    1,    2,    0,
   26,   26,   26,   26,   26,   26,   26,    4,   11,   11,
   11,   11,   11,   11,   11,   11,   11,   12,   13,   14,
   15,   16,   17,   18,   16,   16,   16,   16,   16,   16,
   16,   16,   14,   14,   14,   14,   14,   14,   14,   14,
   15,   15,   15,   15,   15,   15,   15,   15,   24,   24,
   24,   24,   24,   24,   24,   24,   25,   25,   25,   25,
   25,   25,   25,   25,   19,   19,   19,   19,   19,   19,
    0,   20,   20,   20,   20,   20,   20,    0,    0,   22,
   22,   22,   22,   22,   22,    0,   21,   21,   21,   21,
   21,   21,    0,   17,   17,   18,   18,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   18,    1,    2,
    0,    0,    0,    0,    0,    0,    0,    0,    4,
  };
  protected static  short [] yyCheck = {            38,
    0,    0,   41,   42,   43,   42,   45,   38,   47,    0,
   47,   42,   43,  258,   45,   38,   47,    0,  258,   42,
   43,   -1,   45,   38,   47,    0,   -1,   42,   43,   40,
   45,   33,   47,    0,   41,   -1,   41,   44,   40,   44,
   40,    0,   41,   41,   46,   44,   44,   38,   -1,    0,
   41,   42,   43,   44,   45,   38,   47,    0,   -1,   42,
   43,   -1,   45,   38,   47,    0,   41,   -1,   43,   44,
   45,   38,    0,   -1,   41,   -1,   43,   44,   45,   38,
    0,   -1,   41,   -1,   43,   44,   45,    0,   -1,   -1,
   41,   -1,   -1,   44,    0,   -1,    0,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   -1,   41,   -1,   -1,   44,
   -1,   -1,   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,
   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   41,   -1,   41,   44,   38,
   44,   -1,   -1,   42,   43,   -1,   45,   -1,   47,    3,
    4,   40,   41,   -1,   -1,   -1,   -1,   11,   12,   13,
   14,   15,   16,   17,   18,   19,   20,   21,   22,   23,
   38,   25,   -1,   -1,   42,   43,   -1,   45,   -1,   47,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   49,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
  259,  260,  261,  262,  263,  264,  265,  266,  259,  260,
  261,  262,  263,  264,  265,  266,   -1,   -1,  261,  262,
  263,  264,  265,  266,   -1,   -1,  257,  258,   -1,   -1,
  265,  266,   -1,   -1,   -1,   -1,  267,  257,  258,   -1,
  259,  260,  261,  262,  263,  264,  265,  267,  259,  260,
  261,  262,  263,  264,  265,  266,  259,  260,  261,  262,
  263,  264,  265,  266,  259,  260,  261,  262,  263,  264,
  265,  266,  259,  260,  261,  262,  263,  264,  265,  266,
  259,  260,  261,  262,  263,  264,  265,  266,  259,  260,
  261,  262,  263,  264,  265,  266,  259,  260,  261,  262,
  263,  264,  265,  266,  259,  260,  261,  262,  263,  264,
   -1,  259,  260,  261,  262,  263,  264,   -1,   -1,  259,
  260,  261,  262,  263,  264,   -1,  259,  260,  261,  262,
  263,  264,   -1,  259,  260,  259,  260,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  266,  257,  258,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  267,
  };

#line 79 "ExpressionGrammar.y"
}
#line default
 class Token {
  public const int CONST = 257;
  public const int IDENTIFIER = 258;
  public const int OP_EQ = 259;
  public const int OP_NE = 260;
  public const int OP_GT = 261;
  public const int OP_LT = 262;
  public const int OP_GE = 263;
  public const int OP_LE = 264;
  public const int OR = 265;
  public const int AND = 266;
  public const int NOT = 267;
  public const int NEG = 268;
  public const int yyErrorCode = 256;
 }
 interface yyInput {
   bool advance ();
   int token ();
   Object value ();
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
