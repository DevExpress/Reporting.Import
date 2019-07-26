using System;
using System.Globalization;
using System.IO;
using System.Text;
using DevExpress.Data.Filtering.Exceptions;

namespace DevExpress.XtraReports.Design.Import.CrystalFormula {
    class FormulaLexer : yyInput {
        readonly TextReader inputReader;
		public int CurrentToken { get; private set; }
		public object CurrentValue { get; private set; }
		public int Line { get; private set; }
        public int Col { get; private set; }
        public int Position { get; private set; }
        public int CurrentTokenPosition { get; private set; }
        bool isAfterColumn = false;
        int currentLine = 0;
		int currentCol = 0;
        bool yyInput.advance() {
			return this.Advance();
		}
		int yyInput.token() {
			return CurrentToken;
		}
		object yyInput.value() {
			return CurrentValue;
		}
		public FormulaLexer(TextReader inputReader) {
            this.inputReader = inputReader;
            Line = -1;
            Col = -1;
            Position = 0;
            CurrentTokenPosition = -1;
		}
		public bool Advance() {
			SkipBlanks();
			Line = currentLine;
			Col = currentCol;
            CurrentTokenPosition = Position;
			CurrentToken = 0;
			CurrentValue = null;
			int nextInt = ReadNextChar();
			if(nextInt == -1) {
				return false;
			}
			char nextChar = (char)nextInt;
			switch(nextChar) {
                case '+':
                case '*':
                case '/':
                case '(':
                case ')':
                case ',':
                case '-':
                case ';':
                case '&':
                case '^':
                case '%':
                    this.CurrentToken = nextChar;
                    this.CurrentValue = null;
                    break;
				case '.':
					DoDotOrNumber();
					break;
                case '$':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					DoNumber(nextChar);
					break;
				case '=':
					this.CurrentToken = Token.OP_EQ;
					if(PeekNextChar() == '=') {
						ReadNextChar();
					}
					break;
				case '<':
					if(PeekNextChar() == '>') {
						ReadNextChar();
						this.CurrentToken = Token.OP_NE;
					} else if(PeekNextChar() == '=') {
						ReadNextChar();
						this.CurrentToken = Token.OP_LE;
					} else {
						this.CurrentToken = Token.OP_LT;
					}
					break;
				case '>':
					if(PeekNextChar() == '=') {
						ReadNextChar();
						this.CurrentToken = Token.OP_GE;
					} else {
						this.CurrentToken = Token.OP_GT;
					}
					break;
                case '{':
                    if(isAfterColumn) {
                        this.CurrentToken = Token.yyErrorCode;
                        this.CurrentValue = nextChar;
                    } else {
                        DoEnclosedColumn();
                    }
                    break;
				case '\'':
                case '"':
					DoString(nextChar);
					break;
                case '#':
                    DoDateTimeConst();
                    break;
                case ':':
                    if(PeekNextChar() == '=') {
                        ReadNextChar();
                        this.CurrentToken = Token.yyErrorCode;
                        this.CurrentValue = nextChar;
                        YYError("Variable assignment is not supported.");
                    } else {
                        this.CurrentToken = nextChar;
                        this.CurrentValue = null;
                    }
                    break;
                default:
					CatchAll(nextChar);
					break;
			}
			isAfterColumn = this.CurrentToken == Token.COLUMN;
			return true;
		}
		char wasChar = '\0';
        bool preread;
        int valuepreread;
        protected int ReadNextChar() {
            int nextInt;
            if(preread) {
                nextInt = valuepreread;
                valuepreread = 0;
                preread = false;
            } else {
                nextInt = inputReader.Read();
            }
			if(nextInt == -1) {
                wasChar = '\0';
			} else if(nextInt == '\n') {
                if(wasChar == '\r') {
                    wasChar = '\0';
                    ++Position;
                } else {
                    wasChar = '\n';
                    ++Position;
                    ++currentLine;
                    currentCol = 0;
                }
			} else if(nextInt == '\r') {
				if(wasChar == '\n') {
                    wasChar = '\0';
                    ++Position;
                } else {
                    wasChar = '\r';
                    ++Position;
                    ++currentLine;
                    currentCol = 0;
                }
            } else {
                ++Position;
                ++currentCol;
            }
			return nextInt;
		}
		protected int PeekNextChar() {
            if(preread)
                return valuepreread;
            else
			    return inputReader.Peek();
		}
        protected int PeekNext2Char() {
            if(!preread) {
                preread = true;
                valuepreread = inputReader.Read();
            }
            System.Diagnostics.Debug.Assert(valuepreread == '/');
            return inputReader.Peek();
        }
        public void SkipBlanks() {
            for(; ; ) {
                int peeked = PeekNextChar();
                if(peeked == '/') {
                    if(PeekNext2Char() == '/') {
                        ReadNextChar();
                        ReadNextChar();
                        for(; ; ) {
                            int nextChar = ReadNextChar();
                            if(nextChar == '\n' || nextChar == -1)
                                break;
                        }
                    } else {
                        return;
                    }
                } else {
                    UnicodeCategory peekedCategory = CharUnicodeInfo.GetUnicodeCategory((char)peeked);
                    if(peekedCategory != UnicodeCategory.SpaceSeparator && peekedCategory != UnicodeCategory.Control)
                        return;
                    ReadNextChar();
                }
            }
        }
        void DoEnclosedColumn() {
            StringBuilder name = GetStringBuilder();
            bool isParameter = false;
            bool isFormula = false;
            bool isFirstChar = true;
            try {
                for(; ; ) {
                    int nextInt = ReadNextChar();
                    if(nextInt == -1) {
                        YYError(FilteringExceptionsText.LexerNonClosedElement, FilteringExceptionsText.LexerElementPropertyName, "}");
                        return;
                    }
                    try {
                        char nextChar = (char)nextInt;
                        if(isFirstChar) {
                            if(nextChar == '?') {
                                isParameter = true;
                                continue;
                            } else if(nextChar == '@') {
                                isFormula = true;
                                continue;
                            }
                        }
                        if(nextChar == '}')
                            return;
                        name.Append(nextChar);
                    }finally {
                        isFirstChar = false;
                    }
                }
            } finally {
                string doneName = DoneWithStringBuilder(name);
                this.CurrentToken = isParameter ? Token.PARAM : isFormula ? Token.FORMULA: Token.COLUMN;
                this.CurrentValue = doneName;
            }
        }
		void DoString(char endChar) {
            this.CurrentToken = Token.CONST;
            StringBuilder str = GetStringBuilder();
			for(; ; ) {
				int nextInt = ReadNextChar();
                if(nextInt == -1) {
                    this.CurrentValue = str;
                    YYError(FilteringExceptionsText.LexerNonClosedElement, FilteringExceptionsText.LexerElementStringLiteral, "'");
                    return;
                }
				char nextChar = (char)nextInt;
				if(nextChar == endChar) {
					if(PeekNextChar() != endChar) {
                        string res = DoneWithStringBuilder(str);
                        this.CurrentValue = res;
						return;
					}
					ReadNextChar();
				}
				str.Append(nextChar);
			}
		}
        string ReadToLoneSharp() {
            var str = GetStringBuilder();
            for(; ; ) {
                int nextInt = ReadNextChar();
                if(nextInt == -1) {
                    var res = DoneWithStringBuilder(str);
                    this.CurrentValue = res;
                    YYError(FilteringExceptionsText.LexerNonClosedElement, FilteringExceptionsText.LexerElementDateTimeOrUserTypeLiteral, "#");
                    return res;
                }
                char nextChar = (char)nextInt;
                if(nextChar == '#') {
                    int peek = PeekNextChar();
                    if(peek == '#')
                        ReadNextChar();
                    else
                        break;
                }
                str.Append(nextChar);
            }
            return DoneWithStringBuilder(str);
        }
        void DoDateTimeConst() {
            this.CurrentToken = Token.CONST;
            string str = ReadToLoneSharp();
            DateTime dt;
            if(DateTime.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dt)) {
                this.CurrentValue = dt;
                return;
            }
            this.CurrentValue = str;
            YYError(FilteringExceptionsText.LexerInvalidElement, FilteringExceptionsText.LexerElementDateTimeLiteral, str);
        }
        void CatchAll(char firstChar) {
            StringBuilder str = GetStringBuilder();
            str.Append(firstChar);
            if(!CanStartToken(firstChar)) {
                this.CurrentToken = Token.yyErrorCode;
                this.CurrentValue = firstChar;
                YYError(FilteringExceptionsText.LexerInvalidInputCharacter, DoneWithStringBuilder(str));
                return;
            }
            for(; ; ) {
                int nextInt = PeekNextChar();
                if(nextInt == -1)
                    break;
                char nextChar = (char)nextInt;
                if(!CanContinueToken(nextChar))
                    break;
                ReadNextChar();
                str.Append(nextChar);
            }
            int currentToken;
            object currentValue;
            ToTokenAndValue(DoneWithStringBuilder(str), out currentToken, out currentValue);
            this.CurrentToken = currentToken;
            this.CurrentValue = currentValue;
        }
        static void ToTokenAndValue(string str, out int currentToken, out object currentValue) {
            currentValue = null;
            switch(str.ToUpperInvariant()) {
                case "AND":
                    currentToken = Token.AND;
                    break;
                case "OR":
                    currentToken = Token.OR;
                    break;
                case "TRUE":
                case "YES":
                    currentToken = Token.CONST;
                    currentValue = true;
                    break;
                case "FALSE":
                case "NO":
                    currentToken = Token.CONST;
                    currentValue = false;
                    break;
                case "NOT":
                    currentToken = Token.NOT;
                    break;
                case "IF":
                    currentToken = Token.IF;
                    break;
                case "THEN":
                    currentToken = Token.THEN;
                    break;
                case "ELSE":
                    currentToken = Token.ELSE;
                    break;
                case "SELECT":
                    currentToken = Token.SELECT;
                    break;
                case "CASE":
                    currentToken = Token.CASE;
                    break;
                case "DEFAULT":
                    currentToken = Token.DEFAULT;
                    break;
                case "WHILEPRINTINGRECORDS":
                    currentToken = Token.DIRECTIVE;
                    currentValue = FormulaCalculationDirective.WhilePrintingRecords;
                    break;
                case "WHILEREADINGRECORDS":
                    currentToken = Token.DIRECTIVE;
                    currentValue = FormulaCalculationDirective.WhileReadingRecords;
                    break;
                case "MOD":
                    currentToken = Token.OP_MOD;
                    break;
                default:
                    currentToken = Token.IDENTIFIER;
                    currentValue = str;
                    break;
            }
        }
		void DoNumber(char firstSymbol) {
            StringBuilder str = GetStringBuilder();
            if(firstSymbol != '$')
			    str.Append(firstSymbol);
			for(; ; ) {
				int nextInt = PeekNextChar();
				char nextChar = (char)nextInt;
				switch(nextChar) {
					case '0':
					case '1':
					case '2':
					case '3':
					case '4':
					case '5':
					case '6':
					case '7':
					case '8':
					case '9':
					case '.':
						ReadNextChar();
						str.Append(nextChar);
						break;
					default:
						this.CurrentToken = Token.CONST;
                        string res = DoneWithStringBuilder(str);
                        object constantValue = res;
						try {
                            constantValue = ExtractNumericValue(res, firstSymbol == '$');
						} catch {
							YYError(FilteringExceptionsText.LexerInvalidElement, FilteringExceptionsText.LexerElementNumberLiteral, res);
						} finally {
                            this.CurrentValue = constantValue;
                        }
                        return;
				}
			}
		}
		object ExtractNumericValue(string str, bool forceDecimal) {
            if(!forceDecimal) {
                int intResult;
                if(int.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out intResult))
                    return intResult;
                long longResult;
                if(long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out longResult))
                    return longResult;
                double doubleResult;
                if(double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out doubleResult))
                    return doubleResult;
            }
            decimal decimalResult;
            if(decimal.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out decimalResult))
                return decimalResult;
            throw new ArgumentException(string.Format("'{0}' is not valid number", str), "str");
		}
		void DoDotOrNumber() {
            var nextChar = (char)PeekNextChar();
			switch(nextChar) {
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					DoNumber('.');
					break;
				default:
					this.CurrentToken = Token.yyErrorCode;
                    this.CurrentValue = nextChar;
					break;
			}
		}
		public static bool CanStartToken(char value) {
			switch(CharUnicodeInfo.GetUnicodeCategory(value)) {
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.ConnectorPunctuation:
					return true;
				default:
					return false;
			}
		}
		public static bool CanContinueToken(char value) {
			switch(CharUnicodeInfo.GetUnicodeCategory(value)) {
				case UnicodeCategory.UppercaseLetter:
				case UnicodeCategory.LowercaseLetter:
				case UnicodeCategory.TitlecaseLetter:
				case UnicodeCategory.ModifierLetter:
				case UnicodeCategory.OtherLetter:
				case UnicodeCategory.ConnectorPunctuation:

				case UnicodeCategory.DecimalDigitNumber:
				case UnicodeCategory.LetterNumber:
				case UnicodeCategory.OtherNumber:
					return true;
				default:
					return false;
			}
		}
        public static bool IsGoodUnescapedName(string fnName) {
            if(fnName == null)
                return false;
            if(fnName.Length < 1)
                return false;
            if(!CanStartToken(fnName[0]))
                return false;
            for(int i = 1; i < fnName.Length; ++i)
                if(!CanContinueToken(fnName[i]))
                    return false;
            int token;
            object value;
            ToTokenAndValue(fnName, out token, out value);
            return token == Token.COLUMN;
        }
        public static bool FailFast { get; set; } = true;
		public virtual void YYError(string message, params object[] args) {
			string fullMessage = string.Format(CultureInfo.InvariantCulture, message, args);
            if(FailFast)
                throw new FormulaParserException(fullMessage);
            else
                XtraPrinting.Tracer.TraceError(XtraPrinting.Native.NativeSR.TraceSource, fullMessage);
        }
        StringBuilder stringBuilder;
        StringBuilder GetStringBuilder() {
            var sb = stringBuilder;
            stringBuilder = null;
            if(sb == null)
                return new StringBuilder();
            else
                return sb;
        }
        string DoneWithStringBuilder(StringBuilder sb) {
            var rv = sb.ToString();
            sb.Clear();
            stringBuilder = sb;
            return rv;
        }
    }
}
