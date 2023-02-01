using SpreadsheetUtilities;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// This file contains the FormulaTests name space and class, which runs a series of tests on the
/// Formula.cs class.  
/// </summary>

namespace FormulaTests {

	[TestClass()]
	public class FormulaTests {

		/// <summary>
		/// Constructs a normal Formula, including a normalizer and variable validity test
		/// </summary>
		[TestMethod()]
		public void ConstructorTest() {
			Formula f = new Formula("5-2", (s) => s.ToLower(), IsValidVariableName);
		}

		/// <summary>
		/// Constructs a normal Formula, with a variable.
		/// </summary>
		[TestMethod()]
		public void ConstructorWithVariableTest() {
			Formula f = new Formula("5+A1", (s) => s.ToLower(), IsValidVariableName);
		}

		/// <summary>
		/// Constructs a normal Formula, with a variable, and without a validity test
		/// </summary>
		[TestMethod()]
		public void ConstructorWithDefaultVariableTest() {
			Formula f = new Formula("5+A1");
		}

		/// <summary>
		/// Attempts to construct a blank Formula.  Should throw an exception.
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void ConstructorBlank() {
			Formula f = new Formula("", (s) => s.ToLower(), IsValidVariableName);
		}

		/// <summary>
		/// Tests out various valid formulae that utilize a number of parentheses
		/// </summary>
		[TestMethod()]
		public void BaiscParenthesisTest() {
			Formula f  = new Formula("(5+A1)");
			Formula f2 = new Formula("((5+A1))");
			Formula f3 = new Formula("((5+A1)*2)");
			Formula f4 = new Formula("((((((((((2-1))))))))))");
		}

		/// <summary>
		/// Attempts to use a variety of imbalanced parenthesis sets
		/// </summary>
		[TestMethod()]
		public void UnbalancedParenthesesTest1() {
			//This method is structured like this so that all test cases
			//(that should each fail) can be put into a single test method.
			//Each block throws an assertion error if the formula didn't
			//throw an exception.

			try {
				Formula f = new Formula("(5+A1");
				Assert.IsTrue(1 == 2);
			} catch(FormulaFormatException) { }

			try {
				Formula f2 = new Formula("(5+A1))");
				Assert.IsTrue(1 == 2);
			}
			catch (FormulaFormatException) { }

			try {
				Formula f3 = new Formula("(5+A1)*2)");
				Assert.IsTrue(1 == 2);
			}
			catch (FormulaFormatException) { }

			try {
				Formula f4 = new Formula("((((((((((2-1)))))))))");
				Assert.IsTrue(1 == 2);
			}
			catch (FormulaFormatException) { }

			try {
				Formula f5 = new Formula(")(");
				Assert.IsTrue(1 == 2);
			}
			catch (FormulaFormatException) { }
		}

		/// <summary>
		/// Attempts to create a Formula that is just "()"
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void EmptyParenthesesTest() {
			Formula f = new Formula("()");
		}

		/// <summary>
		/// Tests out a formula that uses multiple nested parentheses
		/// </summary>
		[TestMethod()]
		public void LongExpressionTest() {
			Formula f = new Formula(
				"((5-3) * (2+A7) / (B4 * Z66) - 2) + 1",
				(s) => s.ToLower(),
				IsValidVariableName
			);
		}

		/// <summary>
		/// Attempts to use an invalid token "&"
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void InvalidTokenTest() {
			Formula f1 = new Formula("5 + 2 & 4");
		}

		/// <summary>
		/// Attempts to violate the Extra Following Rule by having a number
		/// followed by another number
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void ExtraFollowingRuleTest() {
			Formula f1 = new Formula("5 + 2 2");
		}

		/// <summary>
		/// Utilizes scientific notation to show that it is working
		/// </summary>
		[TestMethod()]
		public void ScientificNotationTest() {
			Formula f1 = new Formula("5e-5", (s) => s.ToUpper(), IsValidVariableName);
		}

		/// <summary>
		/// Makes sure that ToString removes whitespace and needless decimal places
		/// </summary>
		[TestMethod()]
		public void ToStringTest() {
			Assert.AreEqual(
				"5+2",
				new Formula("5 + 2").ToString()
			);
			Assert.AreEqual(
				"A22-4",
				new Formula("A22 - 4.0").ToString()
			);
		}

		/// <summary>
		/// Attempts simple evaluation with and without a variable
		/// </summary>
		[TestMethod()]
		public void BasicEvaluateTest() {
			Formula f1 = new Formula("3 / 3");
			double val = (double) f1.Evaluate((v) => 1);
			Assert.IsTrue(val.Equals(1.0));

			Formula f2 = new Formula("A22 * (6 + b2)");
			double val2 = (double)f2.Evaluate((v) => 5);
			Assert.IsTrue(val2.Equals(55));

			Formula f3 = new Formula("3 - 2");
			double val3 = (double)f3.Evaluate((v) => 1);
			Assert.IsTrue(val3.Equals(1.0));
		}

		/// <summary>
		/// Attempts to evaluate 3/0
		/// </summary>
		[TestMethod()]
		public void EvaluateInvalidTest() {
			Formula f1 = new Formula("3 / A2");
			object? val = f1.Evaluate((v) => 0);
			Assert.IsFalse(val is null);
			Assert.IsTrue(val is FormulaError);
		}

		/// <summary>
		/// Attempts to use a variable that doesn't exist
		/// </summary>
		/// <exception cref="ArgumentException">Variable Doesn't Exist exception</exception>
		[TestMethod()]
		public void VariableDoesNotExistTest() {
			Formula f1 = new Formula("A1 / 9");
			object? val = f1.Evaluate((v) => throw new ArgumentException("That doesn't exist, buckaroo!"));
			Assert.IsFalse(val is null);
			Assert.IsTrue(val is FormulaError);
			Assert.IsFalse(val is double);
		}

		/// <summary>
		/// Makes sure ToString removes a lot of whitespace, and even more needless decimal places
		/// </summary>
		[TestMethod()]
		public void ToStringTest2() {
			Assert.AreEqual(
				"5+2",
				new Formula("5 + 2.0").ToString()
			);
			Assert.AreEqual(
				"A22-4",
				new Formula("A22 -       4.00000").ToString()
			);
		}

		/// <summary>
		/// Makes sure that F1.Equals(F2) functions as expected
		/// </summary>
		[TestMethod()]
		public void EqualityTest() {
			Assert.IsTrue(new Formula("5.000000 / 2").Equals(new Formula("5/2")));
			//Rounding checks
			Assert.IsTrue(new Formula("3.00000000000000001 * 3").Equals(new Formula("3*3  ")));
			Assert.IsFalse(new Formula("5 * (4+3)").Equals(null));
			Assert.IsFalse(new Formula("5 * (4+3)").Equals(new Random()));
		}

		/// <summary>
		/// Makes sure that F1 == F2 functions as expected
		/// </summary>
		[TestMethod()]
		public void EqualEqualsTest() {
			Assert.IsTrue(new Formula("5.000000 / 2") == new Formula("5/2"));
			//Rounding checks
			Assert.IsTrue(new Formula("3.00000000000000001 * 3") == new Formula("3*3  "));
		}

		/// <summary>
		/// Makes sure that F1 != F2 functions as expected
		/// </summary>
		[TestMethod()]
		public void NotEqualsTest() {
			Assert.IsFalse(new Formula("5.000000 / 2") != new Formula("5/2"));
			//Rounding checks
			Assert.IsFalse(new Formula("3.00000000000000001 * 3") != new Formula("3*3  "));
		}

		//Makes sure the GetVariables returns the right set
		[TestMethod()]
		public void GetVariablesTest() {
			Formula f1 = new Formula("A1 - A2");
			HashSet<string> vars = (HashSet<string>) f1.GetVariables();
			Assert.IsTrue(vars.Count() == 2);
			Assert.IsTrue(vars.Contains("A1"));
			Assert.IsTrue(vars.Contains("A2"));
		}

		/// <summary>
		/// Makes sure that GetVariables can return a blank correctly
		/// </summary>
		[TestMethod()]
		public void GetVariablesEmptyTest() {
			Formula f1 = new Formula("5 * (3-6) + 3");
			HashSet<string> vars = (HashSet<string>)f1.GetVariables();
			Assert.IsTrue(vars.Count() == 0);
		}

		/// <summary>
		/// Makes sure that duplicates are not re-added to the variables list
		/// </summary>
		[TestMethod()]
		public void GetVariablesDuplicate() {
			Formula f1 = new Formula("A1 + A1 - 4");
			HashSet<string> vars = (HashSet<string>)f1.GetVariables();
			Assert.IsTrue(vars.Count() == 1);
			Assert.IsTrue(vars.Contains("A1"));
		}

		/// <summary>
		/// Stress-tests the GetVariables method by giving it 101 different variables
		/// </summary>
		[TestMethod()]
		public void GetVariablesLongTest() {
			StringBuilder sb = new StringBuilder("B1");
			for (int i = 0; i < 100; i++) {
				sb.Append($" + A{i}");
			}
			Formula f1 = new Formula(sb.ToString());
			HashSet<string> vars = (HashSet<string>) f1.GetVariables();
			Assert.IsTrue(vars.Count() == 101);
			Assert.IsTrue(vars.Contains("B1"));
			for (int i = 0; i < 100; i++) {
				Assert.IsTrue(vars.Contains($"A{i}"));
			}
		}

		/// <summary>
		/// Makes sure that leading negatives are not allowed
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void InvalidSyntaxTest() {
			Formula f1 = new Formula("-2 + 2");
		}




		/// <summary>
		/// This is the validity test from A1.  It is used here as an example function for testing purposes.
		/// 
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
