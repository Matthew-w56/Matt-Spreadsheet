/// <summary>
/// Author: Matthew Williams
/// Date: 12-Jan-2023
/// Course: 3500 - Software Practice - University of Utah
/// This file is meant to serve as a testing point for the Evaluator.cs in the
/// FormulaEvaluator project.  Tests are constructed and prepared to run.
/// </summary>



/// <sumarry>
/// Placeholder for the working delegate that will be produced later
/// </sumarry>
static int delegatePlaceholder(string variable_name) { return 0; }

/// <summary>
/// Runs the Evaluator on the given expression and writes out the result,
/// along with the expected result.  A string representation of an exception
/// can also be given to compare whether the expected exception occurs.
/// </summary>
/// <param name="expression">The math formula that is in need of being evaluated</param>
/// <param name="answer">The expected answer to the expression</param>
/// <param name="e">The exception expected.  If none is specified, none is expected</param>
static void Test(string expression, int answer, string? e = null) {

    Console.WriteLine("Expression: " + expression);

    //If an exception is expected to occur
    if (e != null) {
        Console.WriteLine("Expecting Exception " + e.ToString() + "");
        try {
            FormulaEvaluator.Evaluator.Evaluate(expression, delegatePlaceholder);
            Console.WriteLine("No Exception Thrown!");
        } catch (Exception received_exception) {
            Console.WriteLine("Got Exception: " + received_exception.Message);
        }
    
    //If no exception is expected
    } else {
        int result = FormulaEvaluator.Evaluator.Evaluate(expression, delegatePlaceholder);
        
        Console.WriteLine("Expecting Answer: " + answer);
        Console.WriteLine("Got Answer: " + result);
    }

    //Blank new line for readability in the Console
    Console.WriteLine();
}


Test("5 * 2 + 7 - 1", 16);              //Does a string of operations correctly
Test("5 / 0", 0, "Argument Exception"); //Flags divide by zero when it happens
Test("0 / 3", 0);                       //Doesn't flag a divide by 0 when 0 is the first value
Test("15 - 100 / 4", -10);              //Operations are done in the correct order
Test("5 / 2", 2);                       //What does it do, just cut off any decimals?
Test("5+3/2*7+1", 13);                  //Long expression, without spaces (NOTE: without rounding error it would be 16.5)
Test("(5+2)*3", 21);                    //Does math correctly when there is extra stuff to right
Test("3*(5+2)", 21);                    //Does math correctly when there is extra stuff to left
Test("-(3*3)+2", 0, "Argument Exception"); //No Unary Negatives