
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
#line 42 "FormulaGrammar.y"
  { result = null; }
  break;
case 2:
#line 43 "FormulaGrammar.y"
  { result = (CriteriaOperator)yyVals[-2+yyTop]; }
  break;
case 3:
#line 44 "FormulaGrammar.y"
  { directive = GetDirective(yyVals[-4+yyTop]); result = (CriteriaOperator)yyVals[-2+yyTop]; }
  break;
case 6:
#line 54 "FormulaGrammar.y"
  { yyVal = GetIifFormula((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], null ); }
  break;
case 7:
#line 55 "FormulaGrammar.y"
  { yyVal = GetIifFormula((CriteriaOperator)yyVals[-4+yyTop], (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop] ); }
  break;
case 8:
#line 56 "FormulaGrammar.y"
  { yyVal = GetSelectExpression((CriteriaOperator)yyVals[-4+yyTop], (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)yyVals[-3+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 9:
#line 57 "FormulaGrammar.y"
  { yyVal = yyVals[0+yyTop]; }
  break;
case 10:
#line 61 "FormulaGrammar.y"
  { var list = (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)yyVals[-4+yyTop]; list.Add(Tuple.Create((List<CriteriaOperator>)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop])); yyVal = list; }
  break;
case 11:
#line 62 "FormulaGrammar.y"
  { yyVal = new List<Tuple<List<CriteriaOperator>, CriteriaOperator>>(); }
  break;
case 12:
#line 66 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 13:
#line 67 "FormulaGrammar.y"
  { var list = (List<CriteriaOperator>)yyVals[-2+yyTop]; list.Add((CriteriaOperator)yyVals[0+yyTop]); yyVal = list; }
  break;
case 14:
#line 71 "FormulaGrammar.y"
  { yyVal = new ConstantValue(yyVals[0+yyTop]); }
  break;
case 15:
#line 72 "FormulaGrammar.y"
  { yyVal = GetParameter(yyVals[0+yyTop]); }
  break;
case 16:
#line 73 "FormulaGrammar.y"
  { yyVal = GetFormula(yyVals[0+yyTop]); }
  break;
case 17:
#line 74 "FormulaGrammar.y"
  { yyVal = new OperandProperty((string)yyVals[0+yyTop]); }
  break;
case 18:
#line 76 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Multiply ); }
  break;
case 19:
#line 77 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Divide ); }
  break;
case 20:
#line 78 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 21:
#line 79 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Minus ); }
  break;
case 22:
#line 80 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 23:
#line 81 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Modulo ); }
  break;
case 24:
#line 82 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Equal ); }
  break;
case 25:
#line 83 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.NotEqual ); }
  break;
case 26:
#line 84 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Greater ); }
  break;
case 27:
#line 85 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Less ); }
  break;
case 28:
#line 86 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.LessOrEqual ); }
  break;
case 29:
#line 87 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.GreaterOrEqual ); }
  break;
case 30:
#line 88 "FormulaGrammar.y"
  { yyVal = GetPercentExpression((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 31:
#line 89 "FormulaGrammar.y"
  { yyVal = GetPowerExpression((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 32:
#line 90 "FormulaGrammar.y"
  { yyVal = GetFunctionOperator(yyVals[-1+yyTop], yyVals[0+yyTop]); }
  break;
case 33:
#line 91 "FormulaGrammar.y"
  { yyVal = GetNegativeValue(yyVals[0+yyTop]); }
  break;
case 34:
#line 92 "FormulaGrammar.y"
  { yyVal = new UnaryOperator( UnaryOperatorType.Plus, (CriteriaOperator)yyVals[0+yyTop] ); }
  break;
case 35:
#line 93 "FormulaGrammar.y"
  { yyVal = new UnaryOperator(UnaryOperatorType.Not, (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 36:
#line 94 "FormulaGrammar.y"
  { yyVal = GroupOperator.And((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 37:
#line 95 "FormulaGrammar.y"
  { yyVal = GroupOperator.Or((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 38:
#line 96 "FormulaGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 39:
#line 100 "FormulaGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 40:
#line 101 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>(); }
  break;
case 41:
#line 105 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 42:
#line 106 "FormulaGrammar.y"
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
    3,    3,    3,    3,    3,    3,    3,    3,    6,    6,
    7,    7,
  };
   static  short [] yyLen = {           2,
    1,    3,    5,    1,    0,    4,    6,    6,    1,    5,
    0,    1,    3,    1,    1,    1,    1,    3,    3,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    2,    2,    2,    2,    3,    3,    3,    3,    2,
    1,    3,
  };
   static  short [] yyDefRed = {            0,
   14,    0,   15,   17,   16,    0,    0,    0,    0,    0,
    0,    0,    1,    0,    0,    0,    0,    0,    0,    0,
   32,    0,    0,   33,   34,    4,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   40,    0,    0,   38,
    2,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   18,   19,   23,   30,    0,    0,    0,
    0,   39,    0,    3,    0,    0,    0,    0,    0,    7,
    0,    0,    0,    0,    0,
  };
  protected static  short [] yyDgoto  = {            14,
   15,   27,   16,   46,   77,   21,   49,
  };
  protected static  int yyFinal = 14;
  protected static  short [] yySindex = {          191,
    0,  -53,    0,    0,    0,  415,  415,  -22,  415,  415,
  415,  415,    0,    0,  -34,  463,  345,  207,  463,  366,
    0,  452,  497,    0,    0,    0,   27,  415,  415,  415,
  415,  415,  415,  415,  415,  415,  415,  415,  415,  415,
  415,  415,  415,  -34,  345, -233,    0,  463,  -41,    0,
    0,  486,  497,  511,  511,  -26,  -26,  -26,  -26,  -14,
   30,  -33,  -33,    0,    0,    0,    0,   38, -235,  415,
  -11,    0,  415,    0,  345,  463,  -36,  415,  463,    0,
  415,  415,  463,  463,  463,
  };
  protected static  short [] yyRindex = {            0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   53,    7,    0,    0, -220,    0,
    0,    0,  199,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   53,    0,    0,    0,  -39,    0,    0,
    0,  309,  247,  159,  171,   82,  104,  124,  143,   62,
   43,    1,   20,    0,    0,    0,    0,    0,   15,    0,
    0,    0,    0,    0,    0,  -18,    0,    0,   -9,    0,
    0,    0,   10, -218,  -10,
  };
  protected static  short [] yyGindex = {            0,
   -4,   11,  624,    0,    0,    0,    0,
  };
  protected static  short [] yyTable = {            72,
   21,   41,   73,   43,   41,   17,    9,   82,   40,    8,
   43,   37,   44,   41,    6,   40,   39,   20,   38,   20,
   41,   81,   43,   37,   26,   12,   51,   40,   39,   75,
   38,   42,   41,   13,   42,   70,   71,   74,   21,   12,
   69,   21,   22,   21,   21,   21,   78,   13,   11,   11,
   10,   10,    5,    0,   68,    0,    0,   20,   21,   21,
   20,   31,   20,   20,   20,    9,   43,   36,    8,    0,
   80,   40,   39,    6,   38,    0,   41,   20,   20,    0,
   22,   26,    0,   22,    0,    0,   22,    0,    0,    0,
    0,    0,    0,    0,   21,    0,    0,    0,    0,    0,
   22,   22,   31,   27,    0,   31,    0,    0,    0,    0,
    0,    0,    0,   20,    0,    0,    0,    0,    0,   31,
   31,    0,   26,   29,    0,   26,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   22,    0,    0,   26,
   26,    0,   28,    0,   27,    0,    0,   27,    0,    0,
    0,    0,    0,    0,    0,   31,    0,    0,   24,    0,
    0,   27,   27,    0,   29,    0,    0,   29,    0,    0,
   25,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   29,   29,   28,    0,    0,   28,    0,    0,    0,
   13,    0,    0,    0,    0,    0,    0,    0,   35,   24,
   28,   28,   24,    0,    0,    0,    0,    0,    0,    0,
    0,   25,    0,    0,   25,    0,   24,   24,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   25,   25,
    9,    0,    0,   12,    0,   11,    0,    0,    0,   35,
    0,    0,   35,   43,   37,    0,   36,   42,   40,   39,
    0,   38,    0,   41,   42,    0,   35,   35,    0,    0,
    0,    0,    0,    0,    0,   21,   42,   21,    0,   21,
   21,    9,   21,   21,    8,   21,   21,   21,   21,   21,
   21,    0,    0,    0,   20,    0,   20,   36,   20,   20,
   36,   20,   20,    0,   20,   20,   20,   20,   20,   20,
   36,    0,    0,    0,   36,   36,    0,   22,   37,   22,
   42,   22,   22,    0,   22,   22,    0,   22,   22,   22,
   22,   22,   22,    0,    0,    0,   31,    0,   31,    0,
   31,   31,    0,   31,   31,    0,   31,   31,   31,   31,
   31,   31,    0,    0,    0,    0,   26,    0,   26,   37,
   26,   26,   37,   26,   26,    0,   26,   26,   26,   26,
   26,   26,    0,    0,    0,    0,   37,   37,   27,    0,
   27,    0,   27,   27,    0,   27,   27,    0,   27,   27,
   27,   27,   27,   27,    9,    0,    0,   12,   29,   11,
   29,    0,   29,   29,    0,   29,   29,    0,   29,   29,
   29,   29,   29,   29,    0,    9,   47,   28,   12,   28,
   11,   28,   28,    0,   28,   28,    0,   28,   28,   28,
   28,   28,   28,   24,    0,   24,    0,   24,   24,    0,
   24,   24,    0,   24,   24,   25,    0,   25,    0,   25,
   25,    0,   25,   25,    0,   25,   25,    0,    0,    1,
    2,    3,    4,    5,    9,    0,    6,   12,    7,   11,
    0,    8,    0,   35,   10,   35,    0,   35,   35,    0,
   35,   35,    0,   45,    0,    0,    0,    0,   28,   29,
    0,   30,   31,   32,   33,   34,   35,   42,   43,   37,
    0,    0,   50,   40,   39,    0,   38,    0,   41,   43,
   37,    0,    0,    0,   40,   39,    0,   38,    0,   41,
    0,   36,    0,   36,    0,   36,   36,    0,   36,   36,
    0,    0,   43,   37,    0,    0,    0,   40,   39,    0,
   38,    0,   41,   43,   37,    0,    0,    0,   40,   39,
    0,   38,    0,   41,    0,   36,    0,   43,   37,    0,
    0,    0,   40,   39,    0,   38,   36,   41,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   37,    0,   37,    0,   37,   37,   36,
   37,    0,    0,    0,    0,    0,    0,    0,    0,    0,
   36,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    1,   36,    3,    4,    5,    0,    0,
    6,    0,    7,    0,    0,    8,    0,    0,   10,    0,
    0,    0,    0,    0,    1,    0,    3,    4,    5,   18,
   19,    0,   22,   23,   24,   25,    8,    0,    0,   10,
    0,    0,    0,   48,    0,    0,    0,    0,    0,    0,
    0,   52,   53,   54,   55,   56,   57,   58,   59,   60,
   61,   62,   63,   64,   65,   66,   67,    0,    0,    0,
    0,    0,    0,    1,    0,    3,    4,    5,    0,    0,
    0,    0,    0,    0,    0,    8,    0,    0,   10,    0,
    0,    0,    0,   76,    0,    0,   79,    0,    0,    0,
    0,   83,    0,    0,   84,   85,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   28,   29,    0,   30,   31,   32,   33,
   34,   35,   42,    0,   28,   29,    0,   30,   31,   32,
   33,   34,   35,   42,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   29,    0,
   30,   31,   32,   33,   34,   35,   42,    0,    0,    0,
    0,   30,   31,   32,   33,   34,   35,   42,    0,    0,
    0,    0,    0,    0,    0,    0,    0,   32,   33,   34,
   35,   42,
  };
  protected static  short [] yyCheck = {            41,
    0,   41,   44,   37,   44,   59,    0,   44,   42,    0,
   37,   38,   17,   47,    0,   42,   43,   40,   45,    0,
   47,   58,   37,   38,   59,   44,    0,   42,   43,  265,
   45,   41,   47,   44,   44,  269,  270,    0,   38,   58,
   45,   41,    0,   43,   44,   45,   58,   58,  269,  270,
  269,  270,    0,   -1,   44,   -1,   -1,   38,   58,   59,
   41,    0,   43,   44,   45,   59,   37,   94,   59,   -1,
   75,   42,   43,   59,   45,   -1,   47,   58,   59,   -1,
   38,    0,   -1,   41,   -1,   -1,   44,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,
   58,   59,   41,    0,   -1,   44,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,   58,
   59,   -1,   41,    0,   -1,   44,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   94,   -1,   -1,   58,
   59,   -1,    0,   -1,   41,   -1,   -1,   44,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   94,   -1,   -1,    0,   -1,
   -1,   58,   59,   -1,   41,   -1,   -1,   44,   -1,   -1,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   58,   59,   41,   -1,   -1,   44,   -1,   -1,   -1,
    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,    0,   41,
   58,   59,   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   41,   -1,   -1,   44,   -1,   58,   59,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   58,   59,
   40,   -1,   -1,   43,   -1,   45,   -1,   -1,   -1,   41,
   -1,   -1,   44,   37,   38,   -1,    0,  281,   42,   43,
   -1,   45,   -1,   47,  281,   -1,   58,   59,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  265,  281,  267,   -1,  269,
  270,  265,  272,  273,  265,  275,  276,  277,  278,  279,
  280,   -1,   -1,   -1,  265,   -1,  267,   41,  269,  270,
   44,  272,  273,   -1,  275,  276,  277,  278,  279,  280,
   94,   -1,   -1,   -1,   58,   59,   -1,  265,    0,  267,
  281,  269,  270,   -1,  272,  273,   -1,  275,  276,  277,
  278,  279,  280,   -1,   -1,   -1,  265,   -1,  267,   -1,
  269,  270,   -1,  272,  273,   -1,  275,  276,  277,  278,
  279,  280,   -1,   -1,   -1,   -1,  265,   -1,  267,   41,
  269,  270,   44,  272,  273,   -1,  275,  276,  277,  278,
  279,  280,   -1,   -1,   -1,   -1,   58,   59,  265,   -1,
  267,   -1,  269,  270,   -1,  272,  273,   -1,  275,  276,
  277,  278,  279,  280,   40,   -1,   -1,   43,  265,   45,
  267,   -1,  269,  270,   -1,  272,  273,   -1,  275,  276,
  277,  278,  279,  280,   -1,   40,   41,  265,   43,  267,
   45,  269,  270,   -1,  272,  273,   -1,  275,  276,  277,
  278,  279,  280,  265,   -1,  267,   -1,  269,  270,   -1,
  272,  273,   -1,  275,  276,  265,   -1,  267,   -1,  269,
  270,   -1,  272,  273,   -1,  275,  276,   -1,   -1,  259,
  260,  261,  262,  263,   40,   -1,  266,   43,  268,   45,
   -1,  271,   -1,  265,  274,  267,   -1,  269,  270,   -1,
  272,  273,   -1,  267,   -1,   -1,   -1,   -1,  272,  273,
   -1,  275,  276,  277,  278,  279,  280,  281,   37,   38,
   -1,   -1,   41,   42,   43,   -1,   45,   -1,   47,   37,
   38,   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,
   -1,  265,   -1,  267,   -1,  269,  270,   -1,  272,  273,
   -1,   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,
   45,   -1,   47,   37,   38,   -1,   -1,   -1,   42,   43,
   -1,   45,   -1,   47,   -1,   94,   -1,   37,   38,   -1,
   -1,   -1,   42,   43,   -1,   45,   94,   47,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  265,   -1,  267,   -1,  269,  270,   94,
  272,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   94,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  259,   94,  261,  262,  263,   -1,   -1,
  266,   -1,  268,   -1,   -1,  271,   -1,   -1,  274,   -1,
   -1,   -1,   -1,   -1,  259,   -1,  261,  262,  263,    6,
    7,   -1,    9,   10,   11,   12,  271,   -1,   -1,  274,
   -1,   -1,   -1,   20,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   28,   29,   30,   31,   32,   33,   34,   35,   36,
   37,   38,   39,   40,   41,   42,   43,   -1,   -1,   -1,
   -1,   -1,   -1,  259,   -1,  261,  262,  263,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  271,   -1,   -1,  274,   -1,
   -1,   -1,   -1,   70,   -1,   -1,   73,   -1,   -1,   -1,
   -1,   78,   -1,   -1,   81,   82,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,  272,  273,   -1,  275,  276,  277,  278,
  279,  280,  281,   -1,  272,  273,   -1,  275,  276,  277,
  278,  279,  280,  281,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  273,   -1,
  275,  276,  277,  278,  279,  280,  281,   -1,   -1,   -1,
   -1,  275,  276,  277,  278,  279,  280,  281,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  277,  278,  279,
  280,  281,
  };

#line 109 "FormulaGrammar.y"
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
  public const int FUNCTION = 271;
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
