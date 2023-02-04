// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens

/// <summary>
/// This file holds a class that represents a Formula.  It is immutable, and
/// is checked for valid syntax upon instantiation.
/// 
/// Co-Author: Matthew Williams
/// </summary>


using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities {

	/// <summary>
	/// Represents formulas written in standard infix notation using standard precedence
	/// rules.  The allowed symbols are non-negative numbers written using double-precision 
	/// floating-point syntax (without unary preceding '-' or '+'); 
	/// variables that consist of a letter or underscore followed by 
	/// zero or more letters, underscores, or digits; parentheses; and the four operator 
	/// symbols +, -, *, and /.  
	/// 
	/// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
	/// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
	/// and "x 23" consists of a variable "x" and a number "23".
	/// 
	/// Associated with every formula are two delegates:  a normalizer and a validator.  The
	/// normalizer is used to convert variables into a canonical form, and the validator is used
	/// to add extra restrictions on the validity of a variable (beyond the standard requirement 
	/// that it consist of a letter or underscore followed by zero or more letters, underscores,
	/// or digits.)  Their use is described in detail in the constructor and method comments.
	/// </summary>
	public class Formula {

		private List<string> tokens;
		private HashSet<string> uniqueVars = new();

		/// <summary>
		/// Creates a Formula from a string that consists of an infix expression written as
		/// described in the class comment.  If the expression is syntactically invalid,
		/// throws a FormulaFormatException with an explanatory Message.
		/// 
		/// The associated normalizer is the identity function, and the associated validator
		/// maps every string to true.  
		/// </summary>
		public Formula(String formula) :
			this(formula, s => s, s => true) { }

		/// <summary>
		/// Creates a Formula from a string that consists of an infix expression written as
		/// described in the class comment.  If the expression is syntactically incorrect,
		/// throws a FormulaFormatException with an explanatory Message.
		/// 
		/// The associated normalizer and validator are the second and third parameters,
		/// respectively.  
		/// 
		/// If the formula contains a variable v such that normalize(v) is not a legal variable, 
		/// throws a FormulaFormatException with an explanatory message. 
		/// 
		/// If the formula contains a variable v such that isValid(normalize(v)) is false,
		/// throws a FormulaFormatException with an explanatory message.
		/// 
		/// Suppose that N is a method that converts all the letters in a string to upper case, and
		/// that V is a method that returns true only if a string consists of one letter followed
		/// by one digit.  Then:
		/// 
		/// new Formula("x2+y3", N, V) should succeed
		/// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
		/// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
		/// </summary>
		public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid) {

			//Split up the expression into tokens
			IEnumerable<string> tokens_temp = GetTokens(formula);

			tokens = new List<string>();
			//Normalize each token
			foreach (string token in tokens_temp) {
				tokens.Add(normalize(token));
			}

			//Verify that the expression is valid
			VerifySyntax(isValid);
		}

		/// <summary>
		/// Verifies whether or not the syntax of the formula's expression is valid.
		/// This method assumes that this.tokens is already set and regularized
		/// </summary>
		/// <returns>Boolean.  Whether this expression is valid</returns>
		private void VerifySyntax(Func<string, bool> isValid) {
			//Section 1) Verify that tokens is not empty, and that first and last item follow rules

			//Check that the tokens array has items in it.
			if (tokens.Count == 0) throw new FormulaFormatException("Empty expression!  Formula cannot be empty.");

			//Verify first token
			if (
				   tokens[0] != "("
				&& !IsValidVar(tokens[0], isValid)
				&& !IsNumber(tokens[0])
			   )
				//If the symbol isn't +,-,*,/,(, and isn't variable and isn't number
				throw new FormulaFormatException("Formula must start with a valid operator [+,-,*,(,/], a variable, or a number!");

			//Verify last token
			int maxIndex = tokens.Count - 1;
			if (
				!IsNumber(tokens[maxIndex])
				&& !tokens[maxIndex].Equals(")")
				&& !IsValidVar(tokens[maxIndex], isValid)
			   )
				//If the last symbol isn't a number, a variable, or a closing parentheses
				throw new FormulaFormatException("Formula must end with either a number, a variable, or a closing parentheses!");

			//Section 2) Loop through tokens and make sure parentheses match and no invalid tokens exist

			//The number of unresolved left parentheses seen so far
			int LParenths = 0;
			//Previous token
			string last = "";

			//Main loop through tokens in expression
			foreach (string token in tokens) {

				//Count Parentheses
				if (token.Equals("(")) {
					LParenths++;
				}
				else if (token.Equals(")")) {
					LParenths--;
					if (LParenths < 0)
						throw new FormulaFormatException("Unbalanced Parentheses!  Found right without a corresponding left.");

					//Check to make sure symbol is a valid one (Note: this is part of previous if statement)
				}
				else if (
							!(IsNumber(token) || IsValidVar(token, isValid) || IsOperator(token))
						  ) {
					throw new FormulaFormatException($"{token} is an invalid token!");
				}
				else if (IsValidVar(token, isValid) && !uniqueVars.Contains(token)) {
					//Log every unique variable for later (see getVariables())
					uniqueVars.Add(token);
				}

				//Section 3) Check for any following rule violations (Still within foreach loop)

				//Check for any Following Rule violations
				if (
					!last.Equals("") && (IsOperator(last) || last.Equals("("))
					&& !(IsNumber(token) || IsValidVar(token, isValid) || token.Equals("("))
				   )
					throw new FormulaFormatException($"Invalid Token Sequence!  {token} cannot follow {last}!");

				//Check for any Extra Following Rule violations
				if (
					!last.Equals("")
					&& (IsNumber(last) || IsValidVar(last, isValid) || last.Equals(")"))
					&& !IsOperator(token)
					&& !token.Equals(")")
				   )
					throw new FormulaFormatException($"Invalid Token Sequence!  {token} cannot follow {last}!");

				//Section 4) Prep for next iteration

				//Update last token
				last = token;
			}
			//Finalize that the number of left and right parentheses match
			if (LParenths != 0) throw new FormulaFormatException("Unbalanced Parentheses!  Too many left parentheses.");
		}

		/// <summary>
		/// Evaluates this Formula, using the lookup delegate to determine the values of
		/// variables.  When a variable symbol v needs to be determined, it should be looked up
		/// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
		/// the constructor.)
		/// 
		/// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
		/// in a string to upper case:
		/// 
		/// new Formula("x+7", N, s => true).Evaluate(L) is 11
		/// new Formula("x+7").Evaluate(L) is 9
		/// 
		/// Given a variable symbol as its parameter, lookup returns the variable's value 
		/// (if it has one) or throws an ArgumentException (otherwise).
		/// 
		/// If no undefined variables or divisions by zero are encountered when evaluating 
		/// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
		/// The Reason property of the FormulaError should have a meaningful explanation.
		///
		/// This method should never throw an exception.
		/// </summary>
		public object Evaluate(Func<string, double> lookup) {

			Stack<double> valStack = new();
			Stack<string> opStack = new();

			/// <summary>
			/// Helper Method to assist in taking care of an incoming double.
			/// Put this into a method because both doubles and variables reuse this block of code.
			/// </summary>
			/// <param name="val">The value of the double to be applied</param>
			void ApplyDouble(double val) {
				if (opStack.Count > 0 && (opStack.Peek() == "*" || opStack.Peek() == "/")) {
					double prevVal = valStack.Pop();
					double result = ApplyOperation(prevVal, opStack.Pop(), val);
					valStack.Push(result);
				}
				else {
					valStack.Push(val);
				}
			}

			/// <summary>
			/// Takes the two most recent values and most recent operator token in the stacks, and
			/// applies the math to them and pushes the result back to the value stack.
			/// </summary>
			void DoOperationFromStack() {
				double val2 = valStack.Pop();
				double val1 = valStack.Pop();
				string op = opStack.Pop();
				double result = ApplyOperation(val1, op, val2);
				valStack.Push(result);
			}

			//Loop through the tokens in the expression
			foreach (string token in tokens) {

				//Get ready to catch a numerical value
				double numVal;
				if (double.TryParse(token, out numVal)) { //Is it a double?
					ApplyDouble(numVal);

					//If the token is a +/- operator
				}
				else if (token == "+" || token == "-") {
					//If the operator stack already has one of those
					if (opStack.Count > 0 && (opStack.Peek() == "+" || opStack.Peek() == "-")) {
						//Take care of the previous one now
						DoOperationFromStack();
					}
					//Either way, store the current operator token for later
					opStack.Push(token);

					//If the token is a * or / operator             
				}
				else if (token == "*" || token == "/") {
					//Push it to the stack
					opStack.Push(token);

					//If the token is an opening parentheses
				}
				else if (token == "(") {
					//Push it to the stack
					opStack.Push(token);

					//If the token is a closing parentheses
				}
				else if (token == ")") {

					//Stage 1: Deal with any + or - operations in the parentheses
					if (opStack.Count > 0 && (opStack.Peek() == "+" || opStack.Peek() == "-")) DoOperationFromStack();

					//Stage 2: Remove the assumed '(' token from operator stack
					opStack.Pop();

					//Stage 3: Apply any * or / that point to this parentheses group
					if (opStack.Count > 0 && (opStack.Peek() == "*" || opStack.Peek() == "/")) {
						try { DoOperationFromStack(); }
						catch (ArgumentException) { return new FormulaError("Cannot divide by zero!"); }
					}

					//If the token is a variable
				}
				else {
					//Try to look up the value of the variable, then treat it just like a double
					//No normalization is done here because tokens are normalized upon instantiation
					try { ApplyDouble(lookup(token)); }
					catch (ArgumentException) { return new FormulaError($"No value exists for variable {token}!"); }

				}
			}

			//Now that we are done processing the tokens, resolve any remaining operations (usually +/-)
			if (opStack.Count > 0) {
				try { DoOperationFromStack(); }
				catch (ArgumentException) { return new FormulaError("Cannot divide by zero!"); }
			}

			//Return the answer
			return valStack.Pop();
		}

		/// <summary>
		/// Enumerates the normalized versions of all of the variables that occur in this 
		/// formula.  No normalization may appear more than once in the enumeration, even 
		/// if it appears more than once in this Formula.
		/// 
		/// For example, if N is a method that converts all the letters in a string to upper case:
		/// 
		/// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
		/// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
		/// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
		/// </summary>
		public IEnumerable<String> GetVariables() {
			return new HashSet<string>(uniqueVars);
		}

		/// <summary>
		/// Returns a string containing no spaces which, if passed to the Formula
		/// constructor, will produce a Formula f such that this.Equals(f).  All of the
		/// variables in the string should be normalized.
		/// 
		/// For example, if N is a method that converts all the letters in a string to upper case:
		/// 
		/// new Formula("x + y", N, s => true).ToString() should return "X+Y"
		/// new Formula("x + Y").ToString() should return "x+Y"
		/// </summary>
		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			foreach (string token in tokens) {
				double val;
				if (Double.TryParse(token, out val)) sb.Append(val.ToString());
				else sb.Append(token);
			}
			return sb.ToString();
		}

		/// <summary>
		///  <change> make object nullable </change>
		///
		/// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
		/// whether or not this Formula and obj are equal.
		/// 
		/// Two Formulae are considered equal if they consist of the same tokens in the
		/// same order.  To determine token equality, all tokens are compared as strings 
		/// except for numeric tokens and variable tokens.
		/// Numeric tokens are considered equal if they are equal after being "normalized" 
		/// by C#'s standard conversion from string to double, then back to string. This 
		/// eliminates any inconsistencies due to limited floating point precision.
		/// Variable tokens are considered equal if their normalized forms are equal, as 
		/// defined by the provided normalizer.
		/// 
		/// For example, if N is a method that converts all the letters in a string to upper case:
		///  
		/// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
		/// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
		/// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
		/// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
		/// </summary>
		public override bool Equals(object? obj) {
			if (obj is null || obj.GetType() != typeof(Formula)) return false;
			return ToString().Equals(obj.ToString());
		}

		/// <summary>
		///   <change> We are now using Non-Nullable objects.  Thus neither f1 nor f2 can be null!</change>
		/// Reports whether f1 == f2, using the notion of equality from the Equals method.
		/// 
		/// </summary>
		public static bool operator ==(Formula f1, Formula f2) {
			return f1.Equals(f2);
		}

		/// <summary>
		///   <change> We are now using Non-Nullable objects.  Thus neither f1 nor f2 can be null!</change>
		///   <change> Note: != should almost always be not ==, if you get my meaning </change>
		///   Reports whether f1 != f2, using the notion of equality from the Equals method.
		/// </summary>
		public static bool operator !=(Formula f1, Formula f2) {
			return !(f1 == f2);
		}

		/// <summary>
		/// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
		/// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
		/// randomly-generated unequal Formulae have the same hash code should be extremely small.
		/// </summary>
		public override int GetHashCode() {
			return ToString().GetHashCode();
		}

		/// <summary>
		/// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
		/// right paren; one of the four operator symbols; a string consisting of a letter or underscore
		/// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
		/// match one of those patterns.  There are no empty tokens, and no token contains white space.
		/// </summary>
		private static IEnumerable<string> GetTokens(String formula) {
			// Patterns for individual tokens
			String lpPattern = @"\(";
			String rpPattern = @"\)";
			String opPattern = @"[\+\-*/]";
			String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
			String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
			String spacePattern = @"\s+";

			// Overall pattern
			String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
											lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

			// Enumerate matching tokens that don't consist solely of white space.
			foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace)) {
				if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline)) {
					yield return s;
				}
			}
		}

		/// <summary>
		/// Returns whether or not the given string represents a number.
		/// 
		/// This is done by attempting to parse it's value as a Double.
		/// </summary>
		/// <param name="s">Value to be checked for if it's a number</param>
		/// <returns>Boolean.  If the given value was a number</returns>
		private static bool IsNumber(string s) { return Double.TryParse(s, out _); }

		/// <summary>
		/// This returns whether or not the given string is whether +, -, *, or /.
		/// 
		/// Rather than checking each case separately, it is checked whether or not
		/// the given string is contained within a whitespace-separated string of these
		/// operators.  Because these tokens are stripped of whitespace before being
		/// checked, it can be assumed that whitespace won't exist, and that any
		/// success returned will be by a single character.
		/// </summary>
		/// <param name="s">The string to be checked</param>
		/// <returns>Boolean.  Whether or not the string is representing an operator</returns>
		private static bool IsOperator(string s) { return "+ - * /".Contains(s); }

		/// <summary>
		/// Returns whether the variable is valid by first making sure that it
		/// starts with either a letter or an underscore (_), and then making
		/// sure that it matches with the given isValid method from the user
		/// </summary>
		private static bool IsValidVar(string s, Func<string, bool> isValid) {
			if (!Regex.Match(s.Substring(0, 1), "[a-zA-Z]|_").Success) return false;
			return isValid(s);
		}

		/// <summary>
		/// Applies the given operation to the two given values in the form:
		///     [val1] (op) [val2]
		/// </summary>
		/// <param name="val1">The first value in the operation</param>
		/// <param name="op">The operation being done (*,/,+,-)</param>
		/// <param name="val2">The second value in the operation</param>
		/// <returns>The result of the operation</returns>
		/// <exception cref="ArgumentException">Divide by Zero, and Invalid Operator</exception>
		private static double ApplyOperation(double val1, string op, double val2) {
			//Check that there is no Divide by Zero problem
			if (op == "/" && val2 == 0) throw new ArgumentException("Cannot Divide by Zero!");

			//Return the result of the math
			return op switch
			{
				"*" => val1 * val2,
				"/" => val1 / val2,
				"+" => val1 + val2,
				"-" => val1 - val2,
				_ => val1
			};
		}
	}

	/// <summary>
	/// Used to report syntactic errors in the argument to the Formula constructor.
	/// </summary>
	public class FormulaFormatException: Exception {
		/// <summary>
		/// Constructs a FormulaFormatException containing the explanatory message.
		/// </summary>
		public FormulaFormatException(String message)
			: base(message) {
		}
	}

	/// <summary>
	/// Used as a possible return value of the Formula.Evaluate method.
	/// </summary>
	public struct FormulaError {
		/// <summary>
		/// Constructs a FormulaError containing the explanatory reason.
		/// </summary>
		/// <param name="reason"></param>
		public FormulaError(String reason)
			: this() {
			Reason = reason;
		}

		/// <summary>
		///  The reason why this FormulaError was created.
		/// </summary>
		public string Reason { get; private set; }
	}
}

// <change>
//   If you are using Extension methods to deal with common stack operations (e.g., checking for
//   an empty stack before peeking) you will find that the Non-Nullable checking is "biting" you.
//
//   To fix this, you have to use a little special syntax like the following:
//
//       public static bool OnTop<T>(this Stack<T> stack, T element1, T element2) where T : notnull
//
//   Notice that the "where T : notnull" tells the compiler that the Stack can contain any object
//   as long as it doesn't allow nulls!
// </change>
