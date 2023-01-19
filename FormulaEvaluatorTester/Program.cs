
using System.Diagnostics;

/// <summary>
/// Author: Matthew Williams
/// Date: 12-Jan-2023
/// Course: 3500 - Software Practice - University of Utah
/// This file is meant to serve as a testing point for the Evaluator.cs in the
/// FormulaEvaluator project.  Tests are constructed and prepared to run.
/// </summary>
/// <sumarry>
/// Placeholder for the working delegate that will be produced later
/// </summary>



static int delegatePlaceholder(string variable_name) { return 7; }

/// <summary>
/// Runs the Evaluator on the given expression and writes out the result,
/// along with the expected result.  A string representation of an exception
/// can also be given to compare whether the expected exception occurs.
/// </summary>
/// <param name="expression">The math formula that is in need of being evaluated</param>
/// <param name="answer">The expected answer to the expression</param>
/// <param name="e">The exception expected.  If none is specified, none is expected</param>
static void Test(string expression, int answer, string? e = null) {

    //If an exception is expected to occur
    if (e != null) {
        try {
            FormulaEvaluator.Evaluator.Evaluate(expression, delegatePlaceholder);
            Console.WriteLine($"Expected an Exception, but none was thrown!\n{expression}\n");
        } catch (Exception) {}
    
    //If no exception is expected
    } else {
        int result = FormulaEvaluator.Evaluator.Evaluate(expression, delegatePlaceholder);
        if (result != answer) Console.WriteLine(
                $"Test Failed!\n{expression}\nExpected: {answer}\nGot: {result}\n");
    }
}


/// <summary>
/// This method is a set of test cases that test the basic operations of the formulas
/// </summary>
static void BasicOperationTests() {
    Test("1 + 2", 3);           //Basic Addition
    Test("2 - 1", 1);           //Basic Subtraction
    Test("2 * 3", 6);           //Basic Multiplication
    Test("6 / 2", 3);           //Basic Division

    Test("5 / 0", 0, "Argument Exception"); //Flags divide by zero when it happens
    Test("0 / 3", 0);                       //Doesn't flag a divide by 0 when 0 is the first value
    Test("5 * 2 + 7 - 1", 16);              //Does a string of operations correctly

    Test("15 - 100 / 4", -10);  //Order of Operations Pt. I
    Test("100 / 4 - 15", 10);   //Order of Operations Pt. II
}

/// <summary>
/// This method is a set of test cases that test various expected exceptions to be thrown
/// </summary>
static void ExceptionTests() {
    Test("-(3*3)+2", 0, "Argument Exception");  //Unary sign should throw error
    Test("+5 - 2", 0, "Argument Exception");    //Unary sign should throw error
    Test("+", 0, "Argument Exception");         //Lone sign
    Test("-", 0, "Argument Exception");         //Lone sign
    Test(" - A - ", 0, "Argument Exception");   //Invalid Expression
}

/// <summary>
/// This method is a set of test cases that test that whitespace makes no difference
/// </summary>
static void SemanticTests() {
    Test("5+3/2*7+1", 13);          //Without Spaces (NOTE: without rounding error it would be 16.5)
    Test("5 + 3 / 2 * 7 + 1", 13);  //With Spaces
    Test("5+ 3/ 2* 7+ 1", 13);      //Mixed Spaces and No Spaces
}

/// <summary>
/// This method is a set of test cases that test various parentheses situations
/// </summary>
static void ParenthesesTests() {
    Test("(1)", 1);                             //One pair of parentheses
    Test("((1))", 1);                           //Two pairs of parentheses
    Test("(((((((1)))))))", 1);                 //Seven pairs
    Test("(((1))", 0, "Argument Exception");    //Imbalanced Parentheses (3 then 2)
    Test("3 * (5+2)", 21);                      //Order of Operations
    Test("(5+2) * 3", 21);                      //Order of Operations
    Test("((5+2) * 3 + 2) / 5", 4);             //Nested Parentheses
    Test("((5+2) * (4-1))", 21);                //Two Nested sets of Parentheses
}

/// <summary>
/// This method is a set of test cases that test various variable-related situations
/// </summary>
static void VariableTests() {
    Test("A2", 7);                              //All variables should be 7
    Test("2A", 0, "Argument Exception");        //Invalid Variable name
    Test("A2A", 0, "Argument Exception");       //Invalid Variable name
    Test("A2b2", 0, "Argument Exception");      //Invalid Variable name
    Test("GBss553t", 0, "Argument Exception");  //Invalid Variable name
    Test("GBss553", 7);                         //Valid, but long name
}


//Run all the Test Case Methods declared above
BasicOperationTests();
ExceptionTests();
SemanticTests();
ParenthesesTests();
VariableTests();
