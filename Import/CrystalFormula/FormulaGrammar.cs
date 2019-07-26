
#line 2 "FormulaGrammar.y"
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
#line 40 "FormulaGrammar.y"
  { result = null; }
  break;
case 2:
#line 41 "FormulaGrammar.y"
  { result = (CriteriaOperator)yyVals[-2+yyTop]; }
  break;
case 3:
#line 42 "FormulaGrammar.y"
  { directive = GetDirective(yyVals[-4+yyTop]); result = (CriteriaOperator)yyVals[-2+yyTop]; }
  break;
case 6:
#line 52 "FormulaGrammar.y"
  { yyVal = GetIifFormula((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], null ); }
  break;
case 7:
#line 53 "FormulaGrammar.y"
  { yyVal = GetIifFormula((CriteriaOperator)yyVals[-4+yyTop], (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop] ); }
  break;
case 8:
#line 54 "FormulaGrammar.y"
  { yyVal = GetSelectExpression((CriteriaOperator)yyVals[-4+yyTop], (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)yyVals[-3+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 9:
#line 55 "FormulaGrammar.y"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 10:
#line 59 "FormulaGrammar.y"
  { var list = (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)yyVals[-4+yyTop]; list.Add(Tuple.Create((List<CriteriaOperator>)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop])); yyVal = list; }
  break;
case 11:
#line 60 "FormulaGrammar.y"
  { yyVal = new List<Tuple<List<CriteriaOperator>, CriteriaOperator>>(); }
  break;
case 12:
#line 64 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 13:
#line 65 "FormulaGrammar.y"
  { var list = (List<CriteriaOperator>)yyVals[-2+yyTop]; list.Add((CriteriaOperator)yyVals[0+yyTop]); yyVal = list; }
  break;
case 14:
#line 69 "FormulaGrammar.y"
  { yyVal = new ConstantValue(yyVals[0+yyTop]); }
  break;
case 15:
#line 70 "FormulaGrammar.y"
  { yyVal = GetParameter(yyVals[0+yyTop]); }
  break;
case 16:
#line 71 "FormulaGrammar.y"
  { yyVal = GetFormula(yyVals[0+yyTop]); }
  break;
case 17:
#line 72 "FormulaGrammar.y"
  { yyVal = new OperandProperty((string)yyVals[0+yyTop]); }
  break;
case 18:
#line 74 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Multiply ); }
  break;
case 19:
#line 75 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Divide ); }
  break;
case 20:
#line 76 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 21:
#line 77 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Minus ); }
  break;
case 22:
#line 78 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 23:
#line 79 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Modulo ); }
  break;
case 24:
#line 80 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Equal ); }
  break;
case 25:
#line 81 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.NotEqual ); }
  break;
case 26:
#line 82 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Greater ); }
  break;
case 27:
#line 83 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Less ); }
  break;
case 28:
#line 84 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.LessOrEqual ); }
  break;
case 29:
#line 85 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.GreaterOrEqual ); }
  break;
case 30:
#line 86 "FormulaGrammar.y"
  { yyVal = GetPercentExpression((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 31:
#line 87 "FormulaGrammar.y"
  { yyVal = GetPowerExpression((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 32:
#line 88 "FormulaGrammar.y"
  { yyVal = GetFunctionOperator(yyVals[-1+yyTop], yyVals[0+yyTop]); }
  break;
case 33:
#line 89 "FormulaGrammar.y"
  { yyVal = GetSpecialField(yyVals[0+yyTop]); }
  break;
case 34:
#line 90 "FormulaGrammar.y"
  { yyVal = GetNegativeValue(yyVals[0+yyTop]); }
  break;
case 35:
#line 91 "FormulaGrammar.y"
  { yyVal = new UnaryOperator( UnaryOperatorType.Plus, (CriteriaOperator)yyVals[0+yyTop] ); }
  break;
case 36:
#line 92 "FormulaGrammar.y"
  { yyVal = new UnaryOperator(UnaryOperatorType.Not, (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 37:
#line 93 "FormulaGrammar.y"
  { yyVal = GroupOperator.And((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 38:
#line 94 "FormulaGrammar.y"
  { yyVal = GroupOperator.Or((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 39:
#line 95 "FormulaGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 40:
#line 99 "FormulaGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 41:
#line 100 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>(); }
  break;
case 42:
#line 104 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 43:
#line 105 "FormulaGrammar.y"
  { var list = (List<CriteriaOperator>)yyVals[-2+yyTop]; list.Add((CriteriaOperator)yyVals[0+yyTop]); yyVal = list; }
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
    0,    0,    0,    2,    2,    1,    1,    1,    1,    4,
    4,    5,    5,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    6,
    6,    7,    7,
  };
   static  short [] yyLen = {           2,
    1,    3,    5,    1,    0,    4,    6,    6,    1,    5,
    0,    1,    3,    1,    1,    1,    1,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    2,    1,    2,    2,    2,    3,    3,    3,    3,
    2,    1,    3,
  };
   static  short [] yyDefRed = {            0,
   14,    0,   15,   17,   16,    0,    0,    0,    0,    0,
    0,    0,    1,    0,    0,    0,    0,    0,    0,    0,
   32,    0,    0,   34,   35,    4,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   41,    0,    0,   39,
    2,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   18,   19,   23,   30,    0,    0,    0,
    0,   40,    0,    3,    0,    0,    0,    0,    0,    7,
    0,    0,    0,    0,    0,
  };
  protected static  short [] yyDgoto  = {            14,
   15,   27,   16,   46,   77,   21,   49,
  };
  protected static  int yyFinal = 14;
  protected static  short [] yySindex = {          212,
    0,  -55,    0,    0,    0,  370,  370,  -34,  370,  370,
  370,  370,    0,    0,  -29,  485,  349,  455,  485,  -32,
    0,  466,  508,    0,    0,    0,   15,  370,  370,  370,
  370,  370,  370,  370,  370,  370,  370,  370,  370,  370,
  370,  370,  370,  -29,  349, -220,    0,  485,   -9,    0,
    0,  496,  508,  519,  519,  -21,  -21,  -21,  -21,   56,
  -14,  -35,  -35,    0,    0,    0,    0,   36, -225,  370,
   -4,    0,  370,    0,  349,  485,  -44,  370,  485,    0,
  370,  370,  485,  485,  485,
  };
  protected static  short [] yyRindex = {            0,
    0,    0,    0,    0,    0,    0,    0,    1,    0,    0,
    0,    0,    0,    0,   51,   10,    0,    0, -213,    0,
    0,    0,  247,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   51,    0,    0,    0,   -7,    0,    0,
    0,  447,  313,  179,  192,  108,  128,  147,  163,   86,
   66,   20,   47,    0,    0,    0,    0,    0,    3,    0,
    0,    0,    0,    0,    0,  -39,    0,    0,   11,    0,
    0,    0,   18, -202,  -17,
  };
  protected static  short [] yyGindex = {            0,
    8,   26,  628,    0,    0,    0,    0,
  };
  protected static  short [] yyTable = {            82,
   33,   43,    6,   17,   12,   20,   40,    9,   47,    9,
   12,   41,   11,   81,   51,   43,   37,    8,   12,   21,
   40,   39,   43,   38,   44,   41,   13,   40,   39,   26,
   38,   72,   41,   42,   73,   74,   42,   33,   33,   75,
   13,   33,   33,   33,   33,   33,   20,   33,   70,   71,
    5,   43,   69,   78,   43,   11,   11,   21,   33,   33,
   21,    6,   21,   21,   21,   22,   10,   10,    9,   68,
    0,    0,   36,    0,    0,    0,    8,   21,   21,    0,
    0,    0,   80,    0,   20,   31,    0,   20,    0,   20,
   20,   20,   43,   37,   33,    0,    0,   40,   39,    0,
   38,    0,   41,   22,   20,   20,   22,   26,    0,   22,
    0,    0,    0,   21,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   22,   22,    0,   31,   27,    0,   31,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   20,    0,    0,   31,   31,    0,   29,    0,   26,    0,
    0,   26,    0,    0,    0,    0,    0,    0,    0,   22,
    0,    0,   28,    0,    0,   26,   26,    0,   27,    0,
    0,   27,    0,    0,    0,    0,    0,    0,   24,   31,
    0,    0,    0,    0,    0,   27,   27,   29,    0,    0,
   29,   25,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   28,   29,   29,   28,    0,    0,    0,
    0,   13,    0,    0,    0,    0,    0,    0,    0,   24,
   28,   28,   24,    0,    0,    0,    1,    0,    3,    4,
    5,    0,   25,    0,    0,   25,   24,   24,    8,    0,
    0,   10,    0,    0,    0,   42,   36,    0,    0,   25,
   25,    9,    0,    0,   12,    0,   11,    0,    0,   42,
    0,    0,    0,    0,    0,   33,   42,   33,    0,   33,
   33,    0,   33,   33,    9,   33,   33,   33,   33,   33,
   33,   33,    8,    0,   21,    0,   21,   36,   21,   21,
   36,   21,   21,    0,   21,   21,   21,   21,   21,   21,
    0,    0,    0,    0,   36,   36,    0,    0,    0,    0,
    0,   20,   37,   20,    0,   20,   20,    0,   20,   20,
    0,   20,   20,   20,   20,   20,   20,    0,    0,    0,
   22,    0,   22,    0,   22,   22,   42,   22,   22,    0,
   22,   22,   22,   22,   22,   22,    0,    0,    0,    0,
   31,    0,   31,   37,   31,   31,   37,   31,   31,    0,
   31,   31,   31,   31,   31,   31,    0,    0,    0,    0,
   37,   37,   26,    0,   26,    0,   26,   26,    0,   26,
   26,    0,   26,   26,   26,   26,   26,   26,    9,    0,
    0,   12,   27,   11,   27,    0,   27,   27,    0,   27,
   27,    0,   27,   27,   27,   27,   27,   27,    0,    9,
    0,   29,   12,   29,   11,   29,   29,    0,   29,   29,
    0,   29,   29,   29,   29,   29,   29,   28,    0,   28,
    0,   28,   28,    0,   28,   28,    0,   28,   28,   28,
   28,   28,   28,   24,    0,   24,   38,   24,   24,    0,
   24,   24,    0,   24,   24,    0,   25,    0,   25,    0,
   25,   25,    0,   25,   25,    0,   25,   25,    0,    0,
    1,    2,    3,    4,    5,    0,    0,    6,    0,    7,
    0,    0,    8,    0,    0,   10,    0,   38,    0,    0,
   38,   43,   37,    0,    0,    0,   40,   39,    0,   38,
    0,   41,   43,   37,   38,   38,   50,   40,   39,    0,
   38,   36,   41,   36,    0,   36,   36,    0,   36,   36,
    0,   43,   37,    0,    0,    0,   40,   39,    0,   38,
    0,   41,   43,   37,    0,    0,    0,   40,   39,    0,
   38,    0,   41,    0,   43,   37,    0,    0,   36,   40,
   39,    0,   38,    0,   41,   43,   37,    0,    0,   36,
   40,   39,    0,   38,    0,   41,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   37,   36,   37,
    0,   37,   37,    0,   37,   37,    0,    0,    0,   36,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   36,    0,    0,    0,    0,    0,    1,    0,    3,
    4,    5,   36,    0,    6,    0,    7,    0,    0,    8,
    0,    0,   10,    0,    0,    0,    0,    0,    1,    0,
    3,    4,    5,   18,   19,    0,   22,   23,   24,   25,
    8,    0,    0,   10,    0,    0,    0,   48,    0,    0,
    0,    0,    0,    0,    0,   52,   53,   54,   55,   56,
   57,   58,   59,   60,   61,   62,   63,   64,   65,   66,
   67,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   76,    0,    0,
   79,    0,    0,    0,    0,   83,    0,    0,   84,   85,
    0,   38,    0,   38,    0,   38,   38,    0,   38,    0,
    0,   45,    0,    0,    0,    0,   28,   29,    0,   30,
   31,   32,   33,   34,   35,   42,    0,   28,   29,    0,
   30,   31,   32,   33,   34,   35,   42,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   28,   29,    0,   30,
   31,   32,   33,   34,   35,   42,    0,    0,   29,    0,
   30,   31,   32,   33,   34,   35,   42,    0,    0,    0,
    0,    0,   30,   31,   32,   33,   34,   35,   42,    0,
    0,    0,    0,    0,    0,   32,   33,   34,   35,   42,
  };
  protected static  short [] yyCheck = {            44,
    0,   37,    0,   59,   44,   40,   42,   40,   41,    0,
   43,   47,   45,   58,    0,   37,   38,    0,   58,    0,
   42,   43,   37,   45,   17,   47,   44,   42,   43,   59,
   45,   41,   47,   41,   44,    0,   44,   37,   38,  265,
   58,   41,   42,   43,   44,   45,    0,   47,  269,  270,
    0,   41,   45,   58,   44,  269,  270,   38,   58,   59,
   41,   59,   43,   44,   45,    0,  269,  270,   59,   44,
   -1,   -1,   94,   -1,   -1,   -1,   59,   58,   59,   -1,
   -1,   -1,   75,   -1,   38,    0,   -1,   41,   -1,   43,
   44,   45,   37,   38,   94,   -1,   -1,   42,   43,   -1,
   45,   -1,   47,   38,   58,   59,   41,    0,   -1,   44,
   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   58,   59,   -1,   41,    0,   -1,   44,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   94,   -1,   -1,   58,   59,   -1,    0,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   94,
   -1,   -1,    0,   -1,   -1,   58,   59,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,    0,   94,
   -1,   -1,   -1,   -1,   -1,   58,   59,   41,   -1,   -1,
   44,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   41,   58,   59,   44,   -1,   -1,   -1,
   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,
   58,   59,   44,   -1,   -1,   -1,  259,   -1,  261,  262,
  263,   -1,   41,   -1,   -1,   44,   58,   59,  271,   -1,
   -1,  274,   -1,   -1,   -1,  281,    0,   -1,   -1,   58,
   59,   40,   -1,   -1,   43,   -1,   45,   -1,   -1,  281,
   -1,   -1,   -1,   -1,   -1,  265,  281,  267,   -1,  269,
  270,   -1,  272,  273,  265,  275,  276,  277,  278,  279,
  280,  281,  265,   -1,  265,   -1,  267,   41,  269,  270,
   44,  272,  273,   -1,  275,  276,  277,  278,  279,  280,
   -1,   -1,   -1,   -1,   58,   59,   -1,   -1,   -1,   -1,
   -1,  265,    0,  267,   -1,  269,  270,   -1,  272,  273,
   -1,  275,  276,  277,  278,  279,  280,   -1,   -1,   -1,
  265,   -1,  267,   -1,  269,  270,  281,  272,  273,   -1,
  275,  276,  277,  278,  279,  280,   -1,   -1,   -1,   -1,
  265,   -1,  267,   41,  269,  270,   44,  272,  273,   -1,
  275,  276,  277,  278,  279,  280,   -1,   -1,   -1,   -1,
   58,   59,  265,   -1,  267,   -1,  269,  270,   -1,  272,
  273,   -1,  275,  276,  277,  278,  279,  280,   40,   -1,
   -1,   43,  265,   45,  267,   -1,  269,  270,   -1,  272,
  273,   -1,  275,  276,  277,  278,  279,  280,   -1,   40,
   -1,  265,   43,  267,   45,  269,  270,   -1,  272,  273,
   -1,  275,  276,  277,  278,  279,  280,  265,   -1,  267,
   -1,  269,  270,   -1,  272,  273,   -1,  275,  276,  277,
  278,  279,  280,  265,   -1,  267,    0,  269,  270,   -1,
  272,  273,   -1,  275,  276,   -1,  265,   -1,  267,   -1,
  269,  270,   -1,  272,  273,   -1,  275,  276,   -1,   -1,
  259,  260,  261,  262,  263,   -1,   -1,  266,   -1,  268,
   -1,   -1,  271,   -1,   -1,  274,   -1,   41,   -1,   -1,
   44,   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,
   -1,   47,   37,   38,   58,   59,   41,   42,   43,   -1,
   45,  265,   47,  267,   -1,  269,  270,   -1,  272,  273,
   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,
   -1,   47,   37,   38,   -1,   -1,   -1,   42,   43,   -1,
   45,   -1,   47,   -1,   37,   38,   -1,   -1,   94,   42,
   43,   -1,   45,   -1,   47,   37,   38,   -1,   -1,   94,
   42,   43,   -1,   45,   -1,   47,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  265,   94,  267,
   -1,  269,  270,   -1,  272,  273,   -1,   -1,   -1,   94,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   94,   -1,   -1,   -1,   -1,   -1,  259,   -1,  261,
  262,  263,   94,   -1,  266,   -1,  268,   -1,   -1,  271,
   -1,   -1,  274,   -1,   -1,   -1,   -1,   -1,  259,   -1,
  261,  262,  263,    6,    7,   -1,    9,   10,   11,   12,
  271,   -1,   -1,  274,   -1,   -1,   -1,   20,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   28,   29,   30,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   41,   42,
   43,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   70,   -1,   -1,
   73,   -1,   -1,   -1,   -1,   78,   -1,   -1,   81,   82,
   -1,  265,   -1,  267,   -1,  269,  270,   -1,  272,   -1,
   -1,  267,   -1,   -1,   -1,   -1,  272,  273,   -1,  275,
  276,  277,  278,  279,  280,  281,   -1,  272,  273,   -1,
  275,  276,  277,  278,  279,  280,  281,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,  272,  273,   -1,  275,
  276,  277,  278,  279,  280,  281,   -1,   -1,  273,   -1,
  275,  276,  277,  278,  279,  280,  281,   -1,   -1,   -1,
   -1,   -1,  275,  276,  277,  278,  279,  280,  281,   -1,
   -1,   -1,   -1,   -1,   -1,  277,  278,  279,  280,  281,
  };

#line 108 "FormulaGrammar.y"
}
#line default
 class Token {
  public const int GENERIC_STATEMENT = 257;
  public const int END_OF_STATEMENT = 258;
  public const int CONST = 259;
  public const int DIRECTIVE = 260;
  public const int PARAM = 261;
  public const int COLUMN = 262;
  public const int FORMULA = 263;
  public const int NONELSE = 264;
  public const int ELSE = 265;
  public const int IF = 266;
  public const int THEN = 267;
  public const int SELECT = 268;
  public const int CASE = 269;
  public const int DEFAULT = 270;
  public const int IDENTIFIER = 271;
  public const int OR = 272;
  public const int AND = 273;
  public const int NOT = 274;
  public const int OP_EQ = 275;
  public const int OP_NE = 276;
  public const int OP_GT = 277;
  public const int OP_LT = 278;
  public const int OP_GE = 279;
  public const int OP_LE = 280;
  public const int OP_MOD = 281;
  public const int NEG = 282;
  public const int yyErrorCode = 256;
 }
 interface yyInput {
   bool advance ();
   int token ();
   Object value ();
 }
} // close outermost namespace, that MUST HAVE BEEN opened in the prolog
