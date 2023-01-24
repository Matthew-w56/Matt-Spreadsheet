using System.Text.RegularExpressions;

/// <summary>
/// Author: Matthew Williams
/// Date: 12-Jan-2023
/// Course: 3500 - Software Practice - University of Utah
/// 
/// I, Matthew Williams, certify that I wrote this code myself and did not steal ideas or
/// copy the work of another.
/// 
/// This Library provides the Evaluator that takes in a string representation of a formula
/// and returns the answer to that formula.
/// </summary>

namespace FormulaEvaluator
{
    /// <summary>
    /// Contains the static method Evaluate() that looks through the given math expression, and
    /// returns the result of the operations.
    /// </summary>
    public class Evaluator
    {
        /// <summary>
        /// Serves as a delegate for a function that returns variable values, given their name
        /// </summary>
        /// <param name="variable_name">The name of the variable</param>
        /// <returns>The value of the variable</returns>
        public delegate int Lookup(String variable_name);

        /// <summary>
        /// Reads and interprets mathematical expressions and returns the answer to that formula.
        /// When an error occurs in the math or in the interpretation of the expression, an
        /// ArgumentException is thrown.
        /// </summary>
        /// <param name="expression">The mathematical expression to be evaluated</param>
        /// <param name="variableEvaluator">A Lookup containing the variables used in the formula</param>
        /// <returns>The result of the expression's math</returns>
        public static int Evaluate( String expression, Lookup variableEvaluator )
        {
            //Obtain a list of tokens present in the expression
            string[] substrings =
            Regex.Split(expression, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");

            Stack<int> valStack = new();
            Stack<string> opStack = new();

            /// <summary>
            /// Helper Method to assist in taking care of an incoming Integer.
            /// Put into a method because both integers and variables reuse this block of code.
            /// </summary>
            /// <param name="val">The value of the integer to be applied</param>
            void ApplyInt(int val) {
                if (opStack.Count > 0 && (opStack.Peek() == "*" || opStack.Peek() == "/")) {
                    if (valStack.Count == 0) throw new ArgumentException("Invalid Expression!");
                    int prevVal = valStack.Pop();
                    int result = ApplyOperation(prevVal, opStack.Pop(), val);
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
                if (valStack.Count < 2) throw new ArgumentException("Invalid Expression!");
                int val2 = valStack.Pop();
                int val1 = valStack.Pop();
                string op = opStack.Pop();
                int result = ApplyOperation(val1, op, val2);
                valStack.Push(result);
            }


            //Loop through the tokens in the expression
            foreach (string t in substrings) {
                //Save the trimmed version rather than trimming it every time it is referenced
                string trimmed = t.Trim();
                //Forget it if it's blank
                if (trimmed.Length == 0) continue;

                //Get ready to catch a numerical value
                int numVal;
                if (int.TryParse(trimmed, out numVal)) { //Is it an Integer?
                    ApplyInt(numVal);
                
                //If the token is a +/- operator
                } else if (trimmed == "+" || trimmed == "-") {
                    //If the operator stack already has one of those
                    if (opStack.Count > 0 && (opStack.Peek() == "+" || opStack.Peek() == "-")) {
                        //Take care of the previous one now
                        DoOperationFromStack();
                    }
                    //Either way, store the current operator token for later
                    opStack.Push(trimmed);
                
                //If the token is a * or / operator             
                } else if (trimmed == "*" || trimmed == "/") {
                    //Push it to the stack
                    opStack.Push(trimmed);

                //If the token is an opening parentheses
                } else if (trimmed == "(") {
                    //Push it to the stack
                    opStack.Push(trimmed);

                //If the token is a closing parentheses
                } else if (trimmed == ")") {

                    //Stage 1: Deal with any + or - operations in the parentheses
                    if (opStack.Count > 0 && (opStack.Peek() == "+" || opStack.Peek() == "-")) DoOperationFromStack();

                    //Stage 2: Expected '(' removal
                    if (opStack.Count == 0 || opStack.Pop() != "(") throw new ArgumentException("Improper Parentheses!");

                    //Stage 3: Apply any * or / that point to this parentheses group
                    if (opStack.Count > 0 && (opStack.Peek() == "*" || opStack.Peek() == "/")) DoOperationFromStack();

                //If the token is a variable
                } else {
                    //Double check the variable is valid
                    if (!IsValidVariableName(trimmed)) throw new ArgumentException($"Invalid Token ( {trimmed} )!");
                    //Look up the value of the variable, then treat it just like an Integer
                    ApplyInt(variableEvaluator(t));
                }
            }

            //Now that we are done processing the tokens, resolve any remaining operations (usually +/-)
            if (opStack.Count > 0) {
                if (opStack.Count != 1 || valStack.Count != 2) throw new ArgumentException("Invalid Expression!");
                DoOperationFromStack();
            }

            //At this point we should just have one value left: our final answer
            if (valStack.Count != 1) throw new ArgumentException("Invalid Expression!");

            //Return the answer
            return valStack.Pop();
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
        static int ApplyOperation(int val1, string op, int val2) {
            //Check that there is no Divide by Zero problem
            if (op == "/" && val2 == 0) throw new ArgumentException("Cannot Divide by Zero!");

            //Return the result of the math
            return op switch
            {
                "*" => val1 * val2,
                "/" => val1 / val2,
                "+" => val1 + val2,
                "-" => val1 - val2,
                _ => throw new ArgumentException("Operator Not Recognized! ( " + op + " )")
            };
        }

        /// <summary>
        /// Returns whether or not a variable name is of the correct form (1+ letters followed by 1+ numbers)
        /// </summary>
        /// <param name="name">The name of the variable</param>
        /// <returns>True or False.</returns>
        static bool IsValidVariableName(string name) {
            if (Regex.Match(name, @"[^a-zA-Z0-9]").Success) return false;       //Fail if name contains anything other than letters and numbers
            if (Regex.Match(name, @"[0-9](?=[a-zA-Z])").Success) return false;  //Fail if name contains a number followed by a letter
            if (!Regex.Match(name, @"[a-zA-Z]").Success) return false;      //Fail if name doesn't contain a letter
            if (!Regex.Match(name, @"[0-9]").Success) return false;         //Fail if name doesn't contain a number
            //Return true if nothing has failed by now
            return true;
        }
    }
}