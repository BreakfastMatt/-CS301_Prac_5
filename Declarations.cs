
  // This is a skeleton program for developing a parser for Modula-2 declarations
  // Matthew Lewis, Liam Searle, Makungu Chansa 
  using Library;
  using System;
  using System.Text;

  class Token {
    public int kind;
    public string val;

    public Token(int kind, string val) {
      this.kind = kind;
      this.val = val;
    }

  } // Token

  class Declarations {
    
    // +++++++++++++++++++++++++ File Handling and Error handlers ++++++++++++++++++++

    static InFile input;
    static OutFile output;

    static string NewFileName(string oldFileName, string ext) {
    // Creates new file name by changing extension of oldFileName to ext
      int i = oldFileName.LastIndexOf('.');
      if (i < 0) return oldFileName + ext; else return oldFileName.Substring(0, i) + ext;
    } // NewFileName

    static void ReportError(string errorMessage) {
    // Displays errorMessage on standard output and on reflected output
      Console.WriteLine(errorMessage);
      output.WriteLine(errorMessage);
    } // ReportError

    static void Abort(string errorMessage) {
    // Abandons parsing after issuing error message
      ReportError(errorMessage);
      output.Close();
      System.Environment.Exit(1);
    } // Abort

    // +++++++++++++++++++++++  token kinds enumeration +++++++++++++++++++++++++

    const int 
      noSym        =  0,
      EOFSym       =  1,
      identSym     =  2,
      numSym       =  3, 
      lparenSym    =  4, 
      rparenSym    =  5, 
      OFSym        =  6, //"OF"
      TOSym        =  7, //"TO"
      typeSym      =  8, //"TYPE"
      varSym       =  9, //"VAR"
      equalSym     =  10, 
      lsquareSym   =  11,
      rsquareSym   =  12,
      arraySym     =  13, //"ARRAY"
      recordSym    =  14, //"RECORD"
      setSym       =  15, //"SET"
      singleDotSym =  16, //"."
      doubleDotSym =  17, //".."
      commaSym     =  18,
      pointerSym   =  19, //"POINTER"
      semiColonSym =  20, 
      colonSym     =  21,
      lcurveSym    =  22,
      rcurveSym    =  23,
      endSym       =  24;  //"END"

    #region charHandler
    // +++++++++++++++++++++++++++++ Character Handler ++++++++++++++++++++++++++

    const char EOF = '\0';
    static bool atEndOfFile = false;

    // Declaring ch as a global variable is done for expediency - global variables
    // are not always a good thing

    static char ch;    // look ahead character for scanner

    static void GetChar() {
    // Obtains next character ch from input, or CHR(0) if EOF reached
    // Reflect ch to output
      if (atEndOfFile) ch = EOF;
      else {
        ch = input.ReadChar();
        atEndOfFile = (ch == EOF);
        if (!atEndOfFile) output.Write(ch);
      }
    } // GetChar
    #endregion 

    #region Scanner
    // +++++++++++++++++++++++++++++++ Scanner ++++++++++++++++++++++++++++++++++

    // Declaring sym as a global variable is done for expediency - global variables
    // are not always a good thing

    static Token sym;

    static void GetSym() { //we changed stuff here.
    // Scans for next sym from input
      StringBuilder symLex = new StringBuilder();
      int symKind = noSym;
      while (ch > EOF && ch <= ' ') GetChar();
      if (symLex.ToString() == "(*") // skip comment
        do GetChar(); while (symLex.ToString() != "*)");

        if (Char.IsLetter(ch))
        {
            do
            {
                symLex.Append(ch); GetChar();
            } while (Char.IsLetterOrDigit(ch)); // need to change this. - nvm.
            switch (symLex.ToString())
            {
                case "OF":
                    symKind = OFSym;  GetChar();
                    break;
                case "TO":
                    symKind = TOSym;  GetChar();
                    break;
                case "TYPE":
                    symKind = typeSym; GetChar();
                    break;
                case "VAR":
                    symKind = varSym; GetChar();
                    break;
                case "ARRAY":
                    symKind = arraySym; GetChar();
                    break;
                case "RECORD":
                    symKind = recordSym; GetChar();
                    break;
                case "SET":
                    symKind = setSym; GetChar();
                    break;
                case "POINTER":
                    symKind = pointerSym; GetChar();
                    break;
                case "END":
                    symKind = endSym; GetChar();
                    break;
                default: symKind = identSym; break;
            }
        }
        else if (Char.IsDigit(ch))
        {
            do
            {
                symLex.Append(ch); GetChar();
            } while (Char.IsDigit(ch));
            symKind = numSym;
        }
        else
        {
            symLex.Append(ch);
            switch (ch)
            {
                case EOF:
                    symLex = new StringBuilder("EOF"); // special case
                    symKind = EOFSym;
                    break; // no need to GetChar
                case '.':
                    symKind = singleDotSym; GetChar();
                    if (ch == '.')
                    {
                        symLex.Append(ch);
                        symKind = doubleDotSym;
                        GetChar();
                    }
                    break;
                case '=':
                    symKind = equalSym; GetChar();
                    break;
                case '(':
                    GetChar();
                    if (ch == '*'){
                      while(true) 
                      {
                        GetChar();
                        if (ch == '*')
                          GetChar();
                            if (ch == ')'){
                                GetChar();
                                GetSym();
                                return;}
                      }
                    }
                    else
                    {
                        symKind = lparenSym;
                    } 
                    break;
                case ')':
                    symKind = rparenSym; GetChar();
                    break;
                case '[':
                    symKind = lsquareSym; GetChar();
                    break;
                case ']':
                    symKind = rsquareSym; GetChar();
                    break;
                case '{':
                    symKind = lcurveSym; GetChar();
                    break;
                case '}':
                    symKind = rcurveSym; GetChar();
                    break;
                case ':':
                    symKind = colonSym; GetChar();
                    break;
                case ';':
                    symKind = semiColonSym; GetChar();
                    break;
                case ',':
                    symKind = commaSym; GetChar();
                    break;
                default:
                    symKind = noSym; GetChar();
                    break;
            }
        }

        sym = new Token(symKind, symLex.ToString());
    } // GetSym
    #endregion

    #region Parser
    // +++++++++++++++++++++++++++++++ Parser +++++++++++++++++++++++++++++++++++

    static void Accept(int wantedSym, string errorMessage) {
    // Checks that lookahead token is wantedSym
      if (sym.kind == wantedSym) GetSym(); else Abort(errorMessage);
    } // Accept

    static void Accept(IntSet allowedSet, string errorMessage) {
    // Checks that lookahead token is in allowedSet
      if (allowedSet.Contains(sym.kind)) GetSym(); else Abort(errorMessage);
    } // Accept

    static void FieldList() {
        // FieldList = [ IdentList ":" Type ] .
        if (sym.kind == identSym)
        {
            IdentList();
            Accept(colonSym, "Expected a :");
            Type();
        }
        else {
            return;
        }
    }

    static void FieldLists() {
        // FieldLists = FieldList { ";" FieldList } .
        FieldList();
        while (sym.kind == colonSym)
            FieldList(); 
    }

    static void IdentList() {
        //IdentList = identifier { "," identifier }
        Accept(identSym, "Identifier expected");
        while (sym.kind == commaSym)
        {
            GetSym();
            Accept(identSym, "Identifier expected");
        }
    }

    static void Enumeration() {
        // Enumeration = "(" IdentList ")".
        Accept(lparenSym, "Expceted a ("); //GetSym
        IdentList();
        Accept(rparenSym, "Expected a )");
    } 

    static void Constant() {
        // Constant = number | identifier .
        if (sym.kind == numSym || sym.kind == identSym)
            GetSym();
        else
            Abort("Expected a number or an identifier.");
    }

    static void Subrange() {
        // Subrange = "[" Constant ".." Constant "]" .
        Accept(lsquareSym,"Expected a ["); //GetSym
        Constant();
        Accept(doubleDotSym, "Expected a ..");
        Constant();
        Accept(rsquareSym, "Expected a ]");
    }

    static void QualIdent() {
        // QualIdent = identifier { "." identifier } .
        Accept(identSym, "Identifier Expected"); //GetSym 
        while (sym.kind == singleDotSym)
        {
            GetSym();
            Accept(identSym, "Identifier Expected"); 
        }
    } 

    static void SimpleType() {
        // SimpleType = QualIdent [ Subrange ] | Enumeration | Subrange .
        switch (sym.kind)
        {
            case identSym: //QualIdent 
                QualIdent();
                if (sym.kind == lsquareSym)
                    Subrange();
                break;
            case lparenSym: //Enumeration
                Enumeration(); 
                break;
            case lsquareSym: //Subrange
                Subrange();
                break;
            default:
                Abort("Invalid start to SimpleType");
                break;
        }
    }

    static void Type() {
        //Type = SimpleType | ArrayType | RecordType | SetType | PointerType .
        switch (sym.kind)
        {
            case identSym: //SimpleType
            case lparenSym:
            case lsquareSym:
                SimpleType();
                break;
            case arraySym: // ArrayType = "ARRAY" SimpleType { "," SimpleType } "OF" Type.
                GetSym();
                SimpleType();
                while (sym.kind == commaSym)
                    SimpleType();
                Accept(OFSym, "Expected keyword OF");
                Type();
                return;
            case recordSym: // RecordType = "RECORD" FieldLists "END"
                GetSym();
                FieldLists();
                Accept(endSym, "Expected keyword END");
                break;
            case setSym: // SetType = "SET" "OF" SimpleType .
                GetSym();
                Accept(OFSym, "Expected keyword OF");
                SimpleType();
                break;
            case pointerSym: // PointerType = "POINTER" "TO" Type .
                GetSym();
                Accept(TOSym, "Expected keyword TO");
                Type();
                return;
            default:
                Abort("Invalid start to Type");
                break;
        }
    }

    static void VarDecl()
    {
        if (sym.kind == identSym)
        {
            IdentList();
            Accept(colonSym, "Expected a :");
            Type();
            Accept(semiColonSym, "Expected a ;");
        }
        else
        {
            Mod2Decl();
            return;
        }

        if (sym.kind != varSym && sym.kind != typeSym)
            VarDecl();
        /*
        //This does the looping
        if (sym.kind != varSym && sym.kind != typeSym)
            VarDecl();
        else
            Declaration(); */
    }

    static void TypeDecl()
    {
        //do whatever
        Declaration();
    }

    static void Declaration()
    {
        if (sym.kind == typeSym)
        {
            GetSym();
            TypeDecl();

        }
        else if (sym.kind == varSym)
        {
            GetSym();
            VarDecl();
        }
    }

    static void Mod2Decl() {
        if (sym.kind == typeSym || sym.kind == varSym)
            Declaration();
        //doesn't need error checking if not typeSym or varSym since it is optional.
        //Accept(EOFSym,"EOF Expected");
    }
    //DANK
    #endregion

    #region MainDriverFunction
    // +++++++++++++++++++++ Main driver function +++++++++++++++++++++++++++++++

    public static void Main(string[] args) { 
      // Open input and output files from command line arguments
      if (args.Length == 0) {
        Console.WriteLine("Usage: Declarations FileName");
        System.Environment.Exit(1);
      }
      input = new InFile(args[0]);
      output = new OutFile(NewFileName(args[0], ".out"));

      GetChar();                                  // Lookahead character

  //  To test the scanner we can use a loop like the following:
  /*
      do {
        GetSym();                                 // Lookahead symbol
        OutFile.StdOut.Write(sym.kind, 3);
        OutFile.StdOut.WriteLine(" " + sym.val);  // See what we got
      } while (sym.kind != EOFSym);
      */
        //  After the scanner is debugged we shall substitute this code:
      GetSym();                                   // Lookahead symbol 
      Mod2Decl();                                 // Start to parse from the goal symbol
      // if we get back here everything must have been satisfactory
      Console.WriteLine("Parsed correctly");

  
      output.Close();
    } // Main
    #endregion

} // Declarations
