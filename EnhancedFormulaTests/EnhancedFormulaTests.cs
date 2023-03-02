/// Author:		Matthew Williams
/// Partner:	None
/// Date:		01-Mar-2023
/// Finished:	02-Mar-2023
/// Course:		CS 3500 - Software Practice - University of Utah
/// Github ID:	matthew - w56
/// Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
/// Solution:	Spreadsheet
/// Project:	Enhanced Formula Tests
/// Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Work
/// 
/// <summary>
/// 
/// This file contains tests for the EnhancedFormula.cs class.
/// 
/// Individual methods are not commented with Documentation because they are all
/// self-explanatory and short.
/// 
/// </summary>

using SpreadsheetUtilities;
using System.Diagnostics;
using Range = SpreadsheetUtilities.Range;

namespace DevelopmentTests {
	/// <summary>
	/// This class runs tests on the EnhancedFormula and Range classes,
	/// both of which are found in the EnhancedFormula.cs file.
	/// </summary>
	[TestClass()]
	public class EnhancedFormulaTests {

		[TestMethod()]
		public void RangeConstructorTest() {
			Range r = new Range("A1", "A2");
			Assert.AreEqual(r.GetStart(), "A1");
			Assert.AreEqual(r.GetEnd(), "A2");
		}

		[TestMethod()]
		public void RangeConstructorSwappedTest() {
			Range r = new Range("B1", "A2");
			Assert.AreEqual(r.GetStart(), "A1");
			Assert.AreEqual(r.GetEnd(), "B2");
		}

		[TestMethod()]
		public void LongerNumberRangeTest() {
			Range r = new Range("A1", "A20");
			List<string> population = r.GetPopulation();
			for (int i = 1; i <= 20; i++) {
				Assert.IsTrue(population.Contains($"A{i}"));
			}
		}

		[TestMethod()]
		public void LongerLetterRangeTest() {
			Range r = new Range("A1", "M1");
			List<string> population = r.GetPopulation();
			for (char i = 'A'; i <= 'M'; i++) {
				Assert.IsTrue(population.Contains($"{i}1"));
			}
		}

		[TestMethod()]
		public void TestAverageFunctionTest() {
			EnhancedFormula f1 = new("AVERAGE(B1:A2)", (s)=>s.ToUpper(), (s)=>true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 3.75);
		}

		[TestMethod()]
		public void TestMedianFunctionTest() {
			EnhancedFormula f1 = new("MEDIAN(B1:A2)", (s) => s.ToUpper(), (s) => true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 5.0);
		}

		[TestMethod()]
		public void TestCountNumFunctionTest() {
			EnhancedFormula f1 = new("COUNTNUM(A1:B20)", (s) => s.ToUpper(), (s) => true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 4.0);
		}

		[TestMethod()]
		public void TestSumFunctionTest() {
			EnhancedFormula f1 = new("SUM(B1:A2)", (s) => s.ToUpper(), (s) => true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 15.0);
		}

		[TestMethod()]
		public void FunctionWithinParenthesisTest() {
			EnhancedFormula f1 = new("(AVERAGE(B1:A2))", (s) => s.ToUpper(), (s) => true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 3.75);
		}

		[TestMethod()]
		public void FunctionsAddedTogetherTest() {
			EnhancedFormula f1 = new("AVERAGE(B1:A2) + Median(A1:B2)", (s) => s.ToUpper(), (s) => true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 8.75);
		}

		[TestMethod()]
		public void MoreContextFunctionTest() {
			EnhancedFormula f1 = new("(AVERAGE(B1:A2) + Median(A1:B2)) / 2", (s) => s.ToUpper(), (s) => true);
			Assert.AreEqual(f1.Evaluate(lookupFunc), 4.375);
		}

		/// <summary>
		/// Test lookup method.  One flaw is that is doesn't
		/// act quite as the Spreadsheet does due to not having
		/// cells to work with, so certain Functions are not
		/// tested in the class above.
		/// </summary>
		/// <param name="name">Cell name, hopefully</param>
		/// <returns>double</returns>
		/// <exception cref="ArgumentException">If it can't return a double, throw this</exception>
		protected double lookupFunc(string name) {
			double answer = 0.0;
			switch (name) {
				case "A1":
					answer = 1.0;
					break;
				case "A2":
					answer = 2.0;
					break;
				case "B1":
					answer = 5.0;
					break;
				case "B2":
					answer = 7.0;
					break;
				default:
					throw new ArgumentException("string");
			}
			return answer;
		}

	}
}
