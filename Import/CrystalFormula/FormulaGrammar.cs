
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
#line 39 "FormulaGrammar.y"
  { result = null; }
  break;
case 2:
#line 40 "FormulaGrammar.y"
  { result = (CriteriaOperator)yyVals[-2+yyTop]; }
  break;
case 3:
#line 41 "FormulaGrammar.y"
  { directive = GetDirective(yyVals[-4+yyTop]); result = (CriteriaOperator)yyVals[-2+yyTop]; }
  break;
case 6:
#line 50 "FormulaGrammar.y"
  { yyVal = new ConstantValue(yyVals[0+yyTop]); }
  break;
case 7:
#line 51 "FormulaGrammar.y"
  { yyVal = GetParameter(yyVals[0+yyTop]); }
  break;
case 8:
#line 52 "FormulaGrammar.y"
  { yyVal = GetFormula(yyVals[0+yyTop]); }
  break;
case 9:
#line 53 "FormulaGrammar.y"
  { yyVal = new OperandProperty((string)yyVals[0+yyTop]); }
  break;
case 10:
#line 55 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Multiply ); }
  break;
case 11:
#line 56 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Divide ); }
  break;
case 12:
#line 57 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 13:
#line 58 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Minus ); }
  break;
case 14:
#line 59 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Plus ); }
  break;
case 15:
#line 60 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Modulo ); }
  break;
case 16:
#line 61 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Equal ); }
  break;
case 17:
#line 62 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.NotEqual ); }
  break;
case 18:
#line 63 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Greater ); }
  break;
case 19:
#line 64 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.Less ); }
  break;
case 20:
#line 65 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.LessOrEqual ); }
  break;
case 21:
#line 66 "FormulaGrammar.y"
  { yyVal = new BinaryOperator( (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], BinaryOperatorType.GreaterOrEqual ); }
  break;
case 22:
#line 67 "FormulaGrammar.y"
  { yyVal = GetPercentExpression((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 23:
#line 68 "FormulaGrammar.y"
  { yyVal = GetPowerExpression((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 24:
#line 69 "FormulaGrammar.y"
  { yyVal = GetFunctionOperator(yyVals[-1+yyTop], yyVals[0+yyTop]); }
  break;
case 25:
#line 70 "FormulaGrammar.y"
  { yyVal = GetSpecialField(yyVals[0+yyTop]); }
  break;
case 26:
#line 71 "FormulaGrammar.y"
  { yyVal = GetNegativeValue(yyVals[0+yyTop]); }
  break;
case 27:
#line 72 "FormulaGrammar.y"
  { yyVal = new UnaryOperator( UnaryOperatorType.Plus, (CriteriaOperator)yyVals[0+yyTop] ); }
  break;
case 28:
#line 73 "FormulaGrammar.y"
  { yyVal = new UnaryOperator(UnaryOperatorType.Not, (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 29:
#line 74 "FormulaGrammar.y"
  { yyVal = GroupOperator.And((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 30:
#line 75 "FormulaGrammar.y"
  { yyVal = GroupOperator.Or((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 31:
#line 76 "FormulaGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 32:
#line 77 "FormulaGrammar.y"
  { yyVal = GetIifFormula((CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop], null ); }
  break;
case 33:
#line 78 "FormulaGrammar.y"
  { yyVal = GetIifFormula((CriteriaOperator)yyVals[-4+yyTop], (CriteriaOperator)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop] ); }
  break;
case 34:
#line 79 "FormulaGrammar.y"
  { yyVal = GetSelectExpression((CriteriaOperator)yyVals[-4+yyTop], (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)yyVals[-3+yyTop], (CriteriaOperator)yyVals[0+yyTop]); }
  break;
case 35:
#line 83 "FormulaGrammar.y"
  { var list = (List<Tuple<List<CriteriaOperator>, CriteriaOperator>>)yyVals[-4+yyTop]; list.Add(Tuple.Create((List<CriteriaOperator>)yyVals[-2+yyTop], (CriteriaOperator)yyVals[0+yyTop])); yyVal = list; }
  break;
case 36:
#line 84 "FormulaGrammar.y"
  { yyVal = new List<Tuple<List<CriteriaOperator>, CriteriaOperator>>(); }
  break;
case 37:
#line 88 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 38:
#line 89 "FormulaGrammar.y"
  { var list = (List<CriteriaOperator>)yyVals[-2+yyTop]; list.Add((CriteriaOperator)yyVals[0+yyTop]); yyVal = list; }
  break;
case 39:
#line 93 "FormulaGrammar.y"
  { yyVal = yyVals[-1+yyTop]; }
  break;
case 40:
#line 94 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>(); }
  break;
case 41:
#line 98 "FormulaGrammar.y"
  { yyVal = new List<CriteriaOperator>() { (CriteriaOperator)yyVals[0+yyTop] }; }
  break;
case 42:
#line 99 "FormulaGrammar.y"
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
    0,    0,    0,    2,    2,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    1,    1,    1,    1,    1,    1,
    1,    1,    1,    1,    4,    4,    5,    5,    3,    3,
    6,    6,
  };
   static  short [] yyLen = {           2,
    1,    3,    5,    1,    0,    1,    1,    1,    1,    3,
    3,    3,    3,    3,    3,    3,    3,    3,    3,    3,
    3,    3,    3,    2,    1,    2,    2,    2,    3,    3,
    3,    4,    6,    6,    5,    0,    1,    3,    3,    2,
    1,    3,
  };
   static  short [] yyDefRed = {            0,
    6,    0,    7,    9,    8,    0,    0,    0,    0,    0,
    0,    0,    1,    0,    0,    0,    0,    0,    0,   24,
    0,    0,   26,   27,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    4,    0,    0,    0,    0,   40,    0,    0,   31,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   10,   11,   15,   22,    2,    0,    0,    0,    0,
   39,    0,    3,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,
  };
  protected static  short [] yyDgoto  = {            14,
   15,   42,   20,   45,   76,   48,
  };
  protected static  int yyFinal = 14;
  protected static  short [] yySindex = {          212,
    0,  -55,    0,    0,    0,  413,  413,  -31,  413,  413,
  413,  413,    0,    0,  487,  413,  502,  550,  436,    0,
  513,  572,    0,    0,  413,  413,  413,  413,  413,  413,
  413,  413,  413,  413,  413,  413,  413,  413,  413,  413,
    0,   16,  487,  413, -215,    0,  550,   -7,    0,  561,
  572,  -19,  -19,  -32,  -32,  -32,  -32,   95,  -12,  -35,
  -35,    0,    0,    0,    0,    0,   21,  530,  413,  -29,
    0,  413,    0,  413,  550,  -44,  413,  550,  550,  413,
  413,  550,  550,  550,
  };
  protected static  short [] yyRindex = {            0,
    0,    0,    0,    0,    0,    0,    0,    1,    0,    0,
    0,    0,    0,    0,   27,    0,    0, -213,    0,    0,
    0,  247,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   27,    0,    0,    0,    8,    0,    0,  447,
  313,  179,  192,  108,  128,  147,  163,   86,   66,   20,
   47,    0,    0,    0,    0,    0,    0,   40,    0,    0,
    0,    0,    0,    0,  -41,    0,    0,    9,  463,    0,
    0,  469, -202,  -36,
  };
  protected static  short [] yyGindex = {            0,
  848,  -11,    0,    0,    0,    0,
  };
  protected static  short [] yyTable = {            81,
   25,   40,   37,   16,   40,   34,   37,   38,   19,   37,
   36,   38,   35,   80,   38,   66,   37,   40,   34,   13,
   73,   38,   37,   36,   40,   35,    5,   38,   77,   37,
   36,   67,   35,   71,   38,    0,   72,   25,   25,   32,
    0,   25,   25,   25,   25,   25,   12,   25,   41,   42,
    0,   41,   42,   69,   70,   36,   36,   13,   25,   25,
   13,   33,   13,   13,   13,   14,   35,   35,    0,    0,
    0,    0,    0,    0,   33,    0,    0,   13,   13,    0,
   32,    0,    0,   32,   12,   23,    0,   12,    0,   12,
   12,   12,    0,    0,   25,    0,    0,   32,   32,    0,
    0,    0,    0,   14,   12,   12,   14,   18,    0,   14,
    0,    0,    0,   13,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   14,   14,    0,   23,   19,    0,   23,
    0,   40,   34,    0,    0,    0,   37,   36,    0,   35,
   12,   38,    0,   23,   23,    0,   21,    0,   18,    0,
    0,   18,    0,    0,    0,    0,    0,    0,    0,   14,
    0,    0,   20,    0,    0,   18,   18,    0,   19,    0,
    0,   19,    0,    0,    0,    0,    0,    0,   16,   23,
    0,    0,    0,    0,    0,   19,   19,   21,    0,    0,
   21,   17,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   20,   21,   21,   20,    0,    0,    0,
    0,   13,    0,    0,    0,    0,    0,    0,    0,   16,
   20,   20,   16,    0,    0,    0,    0,    0,    0,    0,
    0,    0,   17,    0,    0,   17,   16,   16,    0,    0,
    0,    0,    0,    0,    0,   39,   28,    0,   39,   17,
   17,    9,    0,    0,   12,    0,   11,   29,   30,   31,
   32,   39,    0,    0,    0,   25,    0,   25,   39,   25,
   25,    0,   25,   25,    0,   25,   25,   25,   25,   25,
   25,   25,    0,    0,   13,    0,   13,   28,   13,   13,
   28,   13,   13,    0,   13,   13,   13,   13,   13,   13,
    0,    0,    0,    0,   28,   28,   32,    0,   32,   32,
    0,   12,   29,   12,    0,   12,   12,    0,   12,   12,
    0,   12,   12,   12,   12,   12,   12,    0,    0,    0,
   14,    0,   14,    0,   14,   14,    0,   14,   14,    0,
   14,   14,   14,   14,   14,   14,    0,    0,    0,    0,
   23,    0,   23,   29,   23,   23,   29,   23,   23,    0,
   23,   23,   23,   23,   23,   23,    0,    0,    0,    0,
   29,   29,   18,    0,   18,   39,   18,   18,    0,   18,
   18,    0,   18,   18,   18,   18,   18,   18,    0,    0,
    0,    0,   19,    0,   19,    0,   19,   19,    0,   19,
   19,    0,   19,   19,   19,   19,   19,   19,    0,    0,
    0,   21,    0,   21,    0,   21,   21,    0,   21,   21,
    0,   21,   21,   21,   21,   21,   21,   20,    0,   20,
    0,   20,   20,    0,   20,   20,    0,   20,   20,   20,
   20,   20,   20,   16,    0,   16,   30,   16,   16,    0,
   16,   16,    9,   16,   16,   12,   17,   11,   17,    0,
   17,   17,   33,   17,   17,    0,   17,   17,   34,    0,
    1,    2,    3,    4,    5,    9,   46,    6,   12,    7,
   11,    0,    8,    0,    0,   10,    0,   30,    0,    0,
   30,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   33,   30,   30,   33,    0,    0,   34,
    0,   28,   34,   28,    0,   28,   28,    0,   28,   28,
   33,   33,    0,   40,   34,    0,   34,   34,   37,   36,
    0,   35,    0,   38,    0,    0,    0,    0,   40,   34,
    0,    0,    0,   37,   36,   41,   35,    0,   38,   40,
   34,    0,    0,   49,   37,   36,    0,   35,    0,   38,
    0,    0,    0,    0,    0,    0,   40,   34,    0,    0,
    0,   37,   36,    0,   35,    0,   38,   29,    0,   29,
   33,   29,   29,    0,   29,   29,   40,   34,    0,    0,
    0,   37,   36,    0,   35,   33,   38,   40,   34,    0,
    0,    0,   37,   36,    0,   35,   33,   38,   40,   34,
    0,    0,    0,   37,   36,    0,   35,    0,   38,    0,
    0,    0,    0,   33,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,   33,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,   33,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,   33,    0,    0,    0,    0,
    0,    1,    0,    3,    4,    5,    0,    0,    6,    0,
    7,    0,    0,    8,    0,    0,   10,    0,    0,    0,
    0,    0,    0,    0,    1,    0,    3,    4,    5,    0,
    0,    6,    0,    7,    0,    0,    8,    0,    0,   10,
    0,   30,    0,   30,    0,   30,   30,    0,   30,    0,
    0,    0,    0,    0,    0,    0,    0,   33,    0,   33,
    0,   33,   33,   34,    0,   34,    0,   34,   34,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,   25,   26,
    0,   27,   28,   29,   30,   31,   32,   39,   44,    0,
    0,    0,    0,   25,   26,    0,   27,   28,   29,   30,
   31,   32,   39,    0,   25,   26,    0,   27,   28,   29,
   30,   31,   32,   39,   74,    0,    0,    0,    0,    0,
    0,   25,   26,    0,   27,   28,   29,   30,   31,   32,
   39,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,   25,   26,    0,   27,   28,   29,   30,   31,   32,
   39,    0,    0,   26,    0,   27,   28,   29,   30,   31,
   32,   39,    0,    0,    0,    0,   27,   28,   29,   30,
   31,   32,   39,   17,   18,    0,   21,   22,   23,   24,
    0,    0,    0,   43,    0,    0,   47,    0,    0,    0,
    0,    0,   50,   51,   52,   53,   54,   55,   56,   57,
   58,   59,   60,   61,   62,   63,   64,   65,    0,    0,
    0,   68,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,    0,    0,    0,    0,
    0,    0,    0,    0,    0,    0,   75,    0,    0,   78,
    0,   79,    0,    0,   82,    0,    0,   83,   84,
  };
  protected static  short [] yyCheck = {            44,
    0,   37,   44,   59,   37,   38,   42,   44,   40,   42,
   43,   47,   45,   58,   47,    0,   58,   37,   38,    0,
    0,   58,   42,   43,   37,   45,    0,   47,   58,   42,
   43,   43,   45,   41,   47,   -1,   44,   37,   38,    0,
   -1,   41,   42,   43,   44,   45,    0,   47,   41,   41,
   -1,   44,   44,  269,  270,  269,  270,   38,   58,   59,
   41,   94,   43,   44,   45,    0,  269,  270,   -1,   -1,
   -1,   -1,   -1,   -1,   94,   -1,   -1,   58,   59,   -1,
   41,   -1,   -1,   44,   38,    0,   -1,   41,   -1,   43,
   44,   45,   -1,   -1,   94,   -1,   -1,   58,   59,   -1,
   -1,   -1,   -1,   38,   58,   59,   41,    0,   -1,   44,
   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   58,   59,   -1,   41,    0,   -1,   44,
   -1,   37,   38,   -1,   -1,   -1,   42,   43,   -1,   45,
   94,   47,   -1,   58,   59,   -1,    0,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   94,
   -1,   -1,    0,   -1,   -1,   58,   59,   -1,   41,   -1,
   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,    0,   94,
   -1,   -1,   -1,   -1,   -1,   58,   59,   41,   -1,   -1,
   44,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   41,   58,   59,   44,   -1,   -1,   -1,
   -1,    0,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   41,
   58,   59,   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   41,   -1,   -1,   44,   58,   59,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,  281,    0,   -1,  281,   58,
   59,   40,   -1,   -1,   43,   -1,   45,  277,  278,  279,
  280,  281,   -1,   -1,   -1,  265,   -1,  267,  281,  269,
  270,   -1,  272,  273,   -1,  275,  276,  277,  278,  279,
  280,  281,   -1,   -1,  265,   -1,  267,   41,  269,  270,
   44,  272,  273,   -1,  275,  276,  277,  278,  279,  280,
   -1,   -1,   -1,   -1,   58,   59,  267,   -1,  269,  270,
   -1,  265,    0,  267,   -1,  269,  270,   -1,  272,  273,
   -1,  275,  276,  277,  278,  279,  280,   -1,   -1,   -1,
  265,   -1,  267,   -1,  269,  270,   -1,  272,  273,   -1,
  275,  276,  277,  278,  279,  280,   -1,   -1,   -1,   -1,
  265,   -1,  267,   41,  269,  270,   44,  272,  273,   -1,
  275,  276,  277,  278,  279,  280,   -1,   -1,   -1,   -1,
   58,   59,  265,   -1,  267,  281,  269,  270,   -1,  272,
  273,   -1,  275,  276,  277,  278,  279,  280,   -1,   -1,
   -1,   -1,  265,   -1,  267,   -1,  269,  270,   -1,  272,
  273,   -1,  275,  276,  277,  278,  279,  280,   -1,   -1,
   -1,  265,   -1,  267,   -1,  269,  270,   -1,  272,  273,
   -1,  275,  276,  277,  278,  279,  280,  265,   -1,  267,
   -1,  269,  270,   -1,  272,  273,   -1,  275,  276,  277,
  278,  279,  280,  265,   -1,  267,    0,  269,  270,   -1,
  272,  273,   40,  275,  276,   43,  265,   45,  267,   -1,
  269,  270,    0,  272,  273,   -1,  275,  276,    0,   -1,
  259,  260,  261,  262,  263,   40,   41,  266,   43,  268,
   45,   -1,  271,   -1,   -1,  274,   -1,   41,   -1,   -1,
   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   41,   58,   59,   44,   -1,   -1,   41,
   -1,  265,   44,  267,   -1,  269,  270,   -1,  272,  273,
   58,   59,   -1,   37,   38,   -1,   58,   59,   42,   43,
   -1,   45,   -1,   47,   -1,   -1,   -1,   -1,   37,   38,
   -1,   -1,   -1,   42,   43,   59,   45,   -1,   47,   37,
   38,   -1,   -1,   41,   42,   43,   -1,   45,   -1,   47,
   -1,   -1,   -1,   -1,   -1,   -1,   37,   38,   -1,   -1,
   -1,   42,   43,   -1,   45,   -1,   47,  265,   -1,  267,
   94,  269,  270,   -1,  272,  273,   37,   38,   -1,   -1,
   -1,   42,   43,   -1,   45,   94,   47,   37,   38,   -1,
   -1,   -1,   42,   43,   -1,   45,   94,   47,   37,   38,
   -1,   -1,   -1,   42,   43,   -1,   45,   -1,   47,   -1,
   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   94,   -1,   -1,   -1,   -1,
   -1,  259,   -1,  261,  262,  263,   -1,   -1,  266,   -1,
  268,   -1,   -1,  271,   -1,   -1,  274,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,  259,   -1,  261,  262,  263,   -1,
   -1,  266,   -1,  268,   -1,   -1,  271,   -1,   -1,  274,
   -1,  265,   -1,  267,   -1,  269,  270,   -1,  272,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,  265,   -1,  267,
   -1,  269,  270,  265,   -1,  267,   -1,  269,  270,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,  272,  273,
   -1,  275,  276,  277,  278,  279,  280,  281,  267,   -1,
   -1,   -1,   -1,  272,  273,   -1,  275,  276,  277,  278,
  279,  280,  281,   -1,  272,  273,   -1,  275,  276,  277,
  278,  279,  280,  281,  265,   -1,   -1,   -1,   -1,   -1,
   -1,  272,  273,   -1,  275,  276,  277,  278,  279,  280,
  281,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,  272,  273,   -1,  275,  276,  277,  278,  279,  280,
  281,   -1,   -1,  273,   -1,  275,  276,  277,  278,  279,
  280,  281,   -1,   -1,   -1,   -1,  275,  276,  277,  278,
  279,  280,  281,    6,    7,   -1,    9,   10,   11,   12,
   -1,   -1,   -1,   16,   -1,   -1,   19,   -1,   -1,   -1,
   -1,   -1,   25,   26,   27,   28,   29,   30,   31,   32,
   33,   34,   35,   36,   37,   38,   39,   40,   -1,   -1,
   -1,   44,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,   -1,
   -1,   -1,   -1,   -1,   -1,   -1,   69,   -1,   -1,   72,
   -1,   74,   -1,   -1,   77,   -1,   -1,   80,   81,
  };

#line 102 "FormulaGrammar.y"
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
