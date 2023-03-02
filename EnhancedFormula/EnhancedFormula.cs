/// Author:		Matthew Williams
/// Partner:	None
/// Date:		01-Mar-2023
/// Finished:	02-Mar-2023
/// Course:		CS 3500 - Software Practice - University of Utah
/// Github ID:	matthew - w56
/// Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
/// Solution:	Spreadsheet
/// Project:	Enhanced Formula
/// Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Work
/// 
/// <summary>
/// 
/// This file contains the Enhanced Formula class.  It extends Formula and allows for special Functions.
/// 
/// It overrides the VerifySyntax and Evaluate functions to continue to act as a normal Formula while
/// allowing for Functions like SUM, AVERAGE, and MEDIAN.
/// 
/// </summary>

using SpreadsheetUtilities;
using System.Diagnostics;

namespace SpreadsheetUtilities {
	
	/// <summary>
	/// This class extends Formula and allows for the parsing and
	/// evaluating of Functions such as SUM, AVERAGE, etc.
	/// </summary>
	public class EnhancedFormula : Formula {

		//List of implemented functions to watch for in the tokens
		protected static HashSet<string> Functions = new HashSet<string>() {
			"SUM",
			"AVERAGE",
			"MEDIAN",
			"COUNTNUM",
			"COUNT"
		};

		//List<string> tokens
		//HashSet<string> uniqueVars
		Dictionary<int, Range> ranges = new();

		/// <summary>
		/// Creates a Formula based on the given string.
		/// 
		/// It does not add any restrictions on variable names, and
		/// does not do any normalization.
		/// </summary>
		/// <param name="formula">The formula string</param>
		public EnhancedFormula(string formula) 
			: this(formula, (s) => s, (s) => true) {  }

		/// <summary>
		/// Creates a Formula based on the given string.  It applies the
		/// given normalize function to all tokens in the formula, and validates
		/// the variables with the given isValid function.
		/// </summary>
		/// <param name="formula">The formula string</param>
		/// <param name="normalize">The method to normalize the tokens (upper case, lower case, etc)</param>
		/// <param name="isValid">The method to validate that a variable name is valid</param>
		public EnhancedFormula(string formula, Func<string, string> normalize, Func<string, bool> isValid) {
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
		/// Makes sure that the formula object has a valid formula.  This version
		/// is different from the parent class in that it looks for and accepts
		/// Functions that are supported by the EvaluateFunction method.
		/// </summary>
		/// <param name="isValid">Method to verify variable names</param>
		/// <exception cref="FormulaFormatException">Thrown when a syntax error exists</exception>
		protected override void VerifySyntax(Func<string, bool> isValid) {
			//Section 1) Verify that tokens is not empty, and that first and last item follow rules

			//Check that the tokens array has items in it.
			if (tokens.Count == 0) throw new FormulaFormatException("Empty expression!  Formula cannot be empty.");

			//Verify first token
			if (
				   tokens[0] != "("
				&& !IsValidVar(tokens[0], isValid)
				&& !IsNumber(tokens[0])
				&& !Functions.Contains(tokens[0].ToUpper())
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

			//Section 2)
			//THIS PART IS NEW FOR ENHANCED FORMULAS - Makes sure that any Functions used are valid
			for (int i = 0; i < tokens.Count(); i++) {
				string token = tokens[i].ToUpper();
				if (Functions.Contains(token)) {
					int start_i = i;

					IncStep(ref i, tokens.Count());
					if (!tokens[i].Equals("("))
						throw new FormulaFormatException("Functions must be followed by the syntax FUNC(var:var).  Missing opening parenthesis.");

					IncStep(ref i, tokens.Count());
					string var1 = tokens[i];
					if (!IsValidVar(tokens[i], isValid))
						throw new FormulaFormatException("Functions must be followed by the syntax FUNC(var:var).  First variable invalid!.");

					IncStep(ref i, tokens.Count());
					if (!tokens[i].Equals(":"))
						throw new FormulaFormatException("Functions must be followed by the syntax FUNC(var:var).  Must separate variables with a : symbol.");

					IncStep(ref i, tokens.Count());
					string var2 = tokens[i];
					if (!IsValidVar(tokens[i], isValid))
						throw new FormulaFormatException("Functions must be followed by the syntax FUNC(var:var).  Second variable invalid!.");

					IncStep(ref i, tokens.Count());
					if (!tokens[i].Equals(")"))
						throw new FormulaFormatException("Functions must be followed by the syntax FUNC(var:var).  Missing closing parenthesis.");

					//Add new range for this function, and all used variables within that range to the uniqueVars array
					ranges.Add(start_i, new Range(var1, var2));
					foreach (string item in ranges[start_i].GetPopulation()) {
						uniqueVars.Add(item);
					}
				}
			}


			//Section 3) Loop through tokens and make sure parentheses match and no invalid tokens exist

			//The number of unresolved left parentheses seen so far
			int LParenths = 0;
			//Previous token
			string last = "";

			//Main loop through tokens in expression
			foreach (string token in tokens) {

				//Count Parentheses
				if (token.Equals("(")) {
					LParenths++;
				} else if (token.Equals(")")) {
					LParenths--;
					if (LParenths < 0)
						throw new FormulaFormatException("Unbalanced Parentheses!  Found right without a corresponding left.");

					//Check to make sure symbol is a valid one (Note: this is part of previous if statement)
				} else if (
							  !(IsNumber(token) || IsValidVar(token, isValid) || IsOperator(token) || IsEnhancedFunction(token) || token.Equals(":"))
							) {
					throw new FormulaFormatException($"{token} is an invalid token!");
				} else if (IsValidVar(token, isValid) && !Functions.Contains(token.ToUpper())) {
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
					&& !Functions.Contains(last.ToUpper())
					&& !token.Equals(":")
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
		/// Evaluates the value of the given function.  Returns either
		/// a double, or a FormulaError (divide by zero, variable not exist,
		/// etc)
		/// </summary>
		/// <param name="lookup">The method used to get values of other cells</param>
		/// <returns>Double or FormulaError</returns>
		public override object Evaluate(Func<string, double> lookup) {

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
				} else {
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
			for (int i = 0; i < tokens.Count(); i++) {
				string token = tokens[i];

				//Get ready to catch a numerical value
				double numVal;
				if (double.TryParse(token, out numVal)) { //Is it a double?
					try { ApplyDouble(numVal); }
					catch (ArgumentException) {
						return new FormulaError("Cannot divide by zero!");
					}

					//If the token is a +/- operator
				} else if (token == "+" || token == "-") {
					//If the operator stack already has one of those
					if (opStack.Count > 0 && (opStack.Peek() == "+" || opStack.Peek() == "-")) {
						//Take care of the previous one now
						try { DoOperationFromStack(); }
						catch (ArgumentException) { return new FormulaError("Cannot divide by zero!"); }
					}
					//Either way, store the current operator token for later
					opStack.Push(token);

					//If the token is a * or / operator             
				} else if (token == "*" || token == "/") {
					//Push it to the stack
					opStack.Push(token);

					//If the token is an opening parentheses
				} else if (token == "(") {
					//Push it to the stack
					opStack.Push(token);

					//If the token is a closing parentheses
				} else if (token == ")") {

					//Stage 1: Deal with any + or - operations in the parentheses
					if (opStack.Count > 0 && (opStack.Peek() == "+" || opStack.Peek() == "-")) DoOperationFromStack();

					//Stage 2: Remove the assumed '(' token from operator stack
					opStack.Pop();

					//Stage 3: Apply any * or / that point to this parentheses group
					if (opStack.Count > 0 && (opStack.Peek() == "*" || opStack.Peek() == "/")) {
						try { DoOperationFromStack(); }
						catch (ArgumentException) { return new FormulaError("Cannot divide by zero!"); }
					}

				//If the token is a Function heading
				} else if (Functions.Contains(token.ToUpper())) {
					//Walk through the items in the function, assuming that it is structured correctly.  That syntax is:
					//		FUNCTION(var1:var2)
					//(in tokens)	Function  (  var1  :  var2  )
					valStack.Push(EvaluateFunction(token.ToUpper(), i, lookup));

					//Push 'i' past the rest of the function declaration
					i += 5;


				//If the token is a variable
				} else {
					//Try to look up the value of the variable, then treat it just like a double
					//No normalization is done here because tokens are normalized upon instantiation
					try {
						double val = lookup(token);
						try {
							ApplyDouble(val);
						}
						catch (ArgumentException) {
							return new FormulaError("Cannot divide by zero!");
						}
					}
					catch (ArgumentException) { return new FormulaError($"{token} does not contain a valid value!"); }

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
		/// Increments the given 'i' variable, and checks to make sure
		/// that it isn't equal to the given max parameter.  Used when
		/// traversing a token list to verify or evaluate Functions
		/// </summary>
		/// <param name="i">Counter variable</param>
		/// <param name="max">Value to throw an exception once i reaches</param>
		/// <exception cref="FormulaFormatException">When i reaches max, it throws this</exception>
		protected void IncStep(ref int i, int max) {
			i++;
			if (i >= max) throw new FormulaFormatException("Functions must be followed by the syntax FUNC(var:var).  Formula not long enough!");
		}

		/// <summary>
		/// Returns whether or not a token is the name of a Function such as
		/// SUM, AVERAGE, etc.
		/// </summary>
		/// <param name="token">token to be looked at</param>
		/// <returns>True or False</returns>
		protected bool IsEnhancedFunction(string token) {
			return Functions.Contains(token.ToUpper());
		}

		/// <summary>
		/// Runs through the computation of a Function and returns it's value.
		/// 
		/// Current counter value (i) is needed as a lookup for what range of
		/// cells to utilize in the function. (stored during VerifySyntax())
		/// </summary>
		/// <param name="func">Name of the Function</param>
		/// <param name="current_i">Current counter value</param>
		/// <param name="lookup">Method to get values of other cells</param>
		/// <returns>double value</returns>
		protected double EvaluateFunction(string func, int current_i, Func<string, double> lookup) {
			double answer = 0.0;
			List<string> population = ranges[current_i].GetPopulation();

			switch (func) {
				case "SUM":
					foreach (string cellName in population) {
						try {
							answer += lookup(cellName);
						}
						catch (ArgumentException) { }
					}
					break;
				case "AVERAGE":
					double total = 0;
					foreach (string cellName in population) {
						try {
							answer += lookup(cellName);
							total++;
						}
						catch (ArgumentException) { }
					}
					if (total > 0) answer /= population.Count();
					break;
				case "MEDIAN":
					List<double> values = new List<double>();
					foreach (string cellName in population) {
						try {
							values.Add(lookup(cellName));
						}
						catch (ArgumentException) { }
					}
					if (values.Count == 0) break;
					values.Sort();
					int index = values.Count / 2;
					answer = values[index];
					break;
				case "COUNTNUM":
					foreach (string cellName in population) {
						try { lookup(cellName); answer++; } catch (ArgumentException) { }
					}
					break;
				case "COUNT":
					foreach (string cellName in population) {
						try {
							lookup(cellName);
							answer++;
						} catch (ArgumentException e) {
							if (e.Message != "") answer++;
						}
					}
					break;
			}

			return answer;
		}
	}


	
	/// <summary>
	/// This class represents a range of cell names (strings).
	/// 
	/// Given a start and end cell, it builds a list of all cells within that range
	/// and can supply them with GetPopulation().
	/// </summary>
	public class Range {

		internal char startL;
		internal int startN;
		internal char endL;
		internal int endN;
		internal List<string> population;

		/// <summary>
		/// Constructs a Range with the given start and end spots.
		/// 
		/// Note: Range.GetStart() returns the top left corner of the
		/// range, not the start parameter given in the constructor.  The
		/// same goes with Range.GetEnd(), which returns the bottom right.
		/// </summary>
		/// <param name="start">One corner of the range bounds</param>
		/// <param name="end">The opposite corner of the range bounds</param>
		public Range(string start, string end) {
			//Parse out the letters and numbers from the two points given by the user
			GetParts(start, out char L1, out int N1);
			GetParts(end, out char L2, out int N2);

			//For iteration ease, the range is seen as a Rectangular section.
			//So whatever two corners the user defined, we store the start spot as
			//the top left and the end spot as the bottom right.
			if (L1 < L2) {
				startL = L1;
				endL = L2;
			} else {
				startL = L2;
				endL = L1;
			}

			if (N1 < N2) {
				startN = N1;
				endN = N2;
			} else {
				startN = N2;
				endN = N1;
			}

			//Build out the list of cells included in this range
			population = new();
			BuildPopulation();
		}

		/// <summary>
		/// Returns a list of all the cells within this Range.
		/// </summary>
		/// <returns>List of strings representing cell names</returns>
		public List<string> GetPopulation() {
			return new List<string>(population);
		}

		/// <summary>
		/// Returns the start cell of this range.
		/// 
		/// Note: This is the top left of the bounds, not
		/// the start parameter given in the constructor for
		/// this Range object.
		/// </summary>
		/// <returns>Cell name</returns>
		public string GetStart() {
			return startL + startN.ToString();
		}

		/// <summary>
		/// Returns the end cell of this range.
		/// 
		/// Note: This is the bottom right of the bounds, not
		/// the end parameter given in the constructor for
		/// this Range object.
		/// </summary>
		/// <returns>Cell name</returns>
		public string GetEnd() {
			return endL + endN.ToString();
		}

		/// <summary>
		/// Called in the constructor for this range object.
		/// Loops through all cell names that lie in this range's
		/// bounds, and adds them to the population list.
		/// </summary>
		protected void BuildPopulation() {
			for (int n = startN; n <= endN; n++) {
				for (char l = startL; l <= endL; l++) {
					population.Add($"{l}{n}");
				}
			}
		}

		/// <summary>
		/// Takes in a string, and outputs it's first character
		/// as a letter, and the rest parsed as an integer.  Used
		/// in cell name parsing.
		/// </summary>
		/// <param name="name">Cell name</param>
		/// <param name="letter">First character as char</param>
		/// <param name="numbers">Rest of name parsed as Integer</param>
		protected void GetParts(string name, out char letter, out int numbers) {
			letter = name[0];
			numbers = int.Parse(name[1..]);
		}
	}
}