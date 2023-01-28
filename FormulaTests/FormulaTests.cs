using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace FormulaTests {

	[TestClass()]
	public class FormulaTests {

		[TestMethod()]
		public void ConstructorTest() {
			Formula f = new Formula("5-2", (s) => s.ToLower(), IsValidVariableName);
		}

		[TestMethod()]
		public void ConstructorWithVariableTest() {
			Formula f = new Formula("5+A1", (s) => s.ToLower(), IsValidVariableName);
		}

		[TestMethod()]
		public void ConstructorWithDefaultVariableTest() {
			Formula f = new Formula("5+A1");
		}

		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void ConstructorBlank() {
			Formula f = new Formula("", (s) => s.ToLower(), IsValidVariableName);
		}

		[TestMethod()]
		public void BaiscParenthesisTest() {
			Formula f  = new Formula("(5+A1)");
			Formula f2 = new Formula("((5+A1))");
			Formula f3 = new Formula("((5+A1)*2)");
			Formula f4 = new Formula("((((((((((2-1))))))))))");
		}

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

		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void EmptyParenthesesTest() {
			Formula f = new Formula("()");
		}

		[TestMethod()]
		public void LongExpressionTest() {
			Formula f = new Formula(
				"((5-3) * (2+A7) / (B4 * Z66) - 2) + 1",
				(s) => s.ToLower(),
				IsValidVariableName
			);
		}

		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void InvalidTokenTest() {
			Formula f1 = new Formula("5 + 2 & 4");
		}

		[TestMethod()]
		[ExpectedException(typeof(FormulaFormatException))]
		public void ExtraFollowingRuleTest() {
			Formula f1 = new Formula("5 + 2 2");
		}

		[TestMethod()]
		public void ScientificNotationTest() {
			Formula f1 = new Formula("5e-5", (s) => s.ToUpper(), IsValidVariableName);
		}

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

		[TestMethod()]
		public void BasicEvaluateTest() {
			Formula f1 = new Formula("3 - 1");
			double val = (double) f1.Evaluate((v) => 1);
			Assert.IsTrue(val.Equals(2.0));

			Formula f2 = new Formula("A22 * (6 + b2)");
			double val2 = (double)f2.Evaluate((v) => 5);
			Assert.IsTrue(val2.Equals(55));
		}

		[TestMethod()]
		public void EvaluateInvalidTest() {
			Formula f1 = new Formula("3 / A2");
			object? val = f1.Evaluate((v) => 0);
			Assert.IsFalse(val is null);
			Assert.IsTrue(val is FormulaError);
		}

		[TestMethod()]
		public void VariableDoesNotExistTest() {
			Formula f1 = new Formula("A1 / 9");
			object? val = f1.Evaluate((v) => throw new ArgumentException("That doesn't exist, buckaroo!"));
			Assert.IsFalse(val is null);
			Assert.IsTrue(val is FormulaError);
			Assert.IsFalse(val is double);
		}

		[TestMethod()]
		public void PiazzaTest() {
			Formula f1 = new Formula("(1)-2/2");
			object? val1 = f1.Evaluate((v) => 1);
			Assert.IsTrue(val1 is not null);
			double answer = (double)val1;
			Assert.IsTrue(answer.Equals(0.0));
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
