/// Author:		Matthew Williams
/// Partner:	None
/// Date:		03-16 Feb 2023
/// Course:		CS 3500 - Software Practice - University of Utah
/// Github ID:	matthew - w56
/// Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
/// Solution:	Spreadsheet
/// Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Work
/// 
/// <summary>
/// 
/// This file contains tests for the Spreadsheet.cs class.
/// Tests range from constructor tests to dependency tracking tests, and
/// go to data replacement, Formula handling, and data retrieval
/// 
/// Author: Matthew Williams	February 2023
/// 
/// </summary>

using SpreadsheetUtilities;
using SS;
using System.Diagnostics;

namespace SpreadsheetTests {

	/// <summary>
	/// Tests the Spreadsheet class, which is an implementation of
	/// the AbstractSpreadsheet abstract class.
	/// </summary>
	[TestClass]
	public class SpreadsheetTests {

		//----------[ A5 Specific Tests ]----------

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		public void TestSpreadsheetSave() {
			Spreadsheet s = new Spreadsheet();
			s.SetContentsOfCell("A1", "9.0");
			s.SetContentsOfCell("A2", "Hey man");
			s.Save("myfile.xml");

			Assert.AreEqual("default", s.GetSavedVersion("myfile.xml"));
		}

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		public void TestSpreadsheetLoad() {
			Spreadsheet s = new Spreadsheet();
			s.SetContentsOfCell("A1", "4.0");
			s.SetContentsOfCell("A2", "Hey man");
			s.Save("myfile.xml");
			Spreadsheet s2 = new Spreadsheet("myfile.xml", (s)=>true, (s)=>s, "default");
			Assert.AreEqual(s2.GetCellContents("A1").ToString(), "4");
		}

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SpreadsheetReadWriteException))]
		public void SavedSheetInvalidFileNameTest() {
			Spreadsheet s = new Spreadsheet("bogusSpreadsheetName.xml", (s) => true, (s) => s, "FaultyVersion");
		}

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SpreadsheetReadWriteException))]
		public void SpreadsheetInvalidPathTest() {
			Spreadsheet s = new("/some/random/path.xml", (s)=>true, (s)=>s, "default");
		}

		/// <summary>
		/// Evaluates a formula that uses no variables
		/// </summary>
		[TestMethod()]
		public void EvaluateGroundLevelTest() {
			Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=10*(7-3)");
			double cellVal = (double) s.GetCellValue("A1");
			Assert.AreEqual(40, cellVal);
		}

		/// <summary>
		/// Evaluates a formula that uses cells that contain doubles as variables
		/// </summary>
		[TestMethod()]
		public void EvaluateSingleLevelTest() {
			Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=B1+C1");
			s.SetContentsOfCell("B1", "7");
			s.SetContentsOfCell("C1", "4");
			double cellVal = (double) s.GetCellValue("A1");
			Assert.AreEqual(11, cellVal);
		}

		/// <summary>
		/// Evaluates a formula that contains a variable that is, itself, a formula
		/// </summary>
		[TestMethod()]
		public void EvaluateMultipleLevelTest() {
			Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=B1+C1");
			s.SetContentsOfCell("B1", "7");
			s.SetContentsOfCell("C1", "=B1*D1");
			s.SetContentsOfCell("D1", "13");
			double cellVal = (double) s.GetCellValue("A1");
			Assert.AreEqual(98, cellVal);
		}

		/// <summary>
		/// Makes sure that cells revert to old content after circular dependency is found
		/// </summary>
		[TestMethod()]
		public void CircularExceptionOnExistingCellTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=B1 - 3");
			s.SetContentsOfCell("B1", "=C1+D1");
			try {
				s.SetContentsOfCell("B1", "=A1 + 3");
			} catch (CircularException) {
				Assert.AreEqual("C1+D1", s.GetCellContents("B1"));
			}
		}

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		public void GetEmptyCellValueTest() {
			Spreadsheet s = new();
			Assert.AreEqual("", s.GetCellValue("A1"));
		}

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SpreadsheetReadWriteException))]
		public void GetSavedVersionInvalidPathTest() {
			new Spreadsheet().GetSavedVersion("/some/random/path/dude.xml");
		}

		/// <summary>
		/// See Title.
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SpreadsheetReadWriteException))]
		public void SavedSheetVersionMismatchTest() {
			Spreadsheet s = new Spreadsheet();
			s.Save("tempSheet.xml");

			Spreadsheet s2 = new Spreadsheet("tempSheet.xml", (s)=>true, (s)=>s, "FaultyVersion");
		}

		/// <summary>
		/// Runs through some steps such as
		///		Creating a Spreadsheet
		///		Setting various cells' values
		///		Saving that sheet
		///		Loading that sheet
		///		Checking the values
		///	
		/// A lot so that we can see it's general performance
		/// </summary>
		[TestMethod()]
		public void PerformanceStressTest() {
			int iterations = 100;
			int itemsPerIteration = 50;
			for (int i = 0; i < iterations; i++) {

				Spreadsheet s = new Spreadsheet();
				for (int j = 0; j < itemsPerIteration; j++) {
					s.SetContentsOfCell($"F{j}", $"{j} is a great number!");
				}
				for (int j = 0; j < itemsPerIteration; j+=2) {
					s.SetContentsOfCell($"F{j}", "");
				}
				for (int j = 1; j < itemsPerIteration; j+=4) {
					s.SetContentsOfCell($"F{j}", $"{j} is even better, though!");
				}
				s.Save("tempSheet.xml");


				Spreadsheet ns = new Spreadsheet("tempSheet.xml", (s) => true, (s) => s, "default");
				for (int j = 1; j < itemsPerIteration; j += 4) {
					Assert.AreEqual($"{j} is even better, though!", ns.GetCellContents($"F{j}"));
				}
			}
		}





		//----------[ Converted A4 Tests ]----------

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void TestConstructor() {
			SS.Spreadsheet _ = new();
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void SetCellContentsTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "Hello, world!");
			s.SetContentsOfCell("A2", "3.99");
			s.SetContentsOfCell("A3", "=(3+4)*7");
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void GetCellContentsTest() {
			SS.Spreadsheet s = new();
			//Checking for each contents type
			s.SetContentsOfCell("A1", "Hello, world!");
			s.SetContentsOfCell("A2", "3.2");
			s.SetContentsOfCell("A3", "=(3+4)*7");

			Assert.IsTrue("Hello, world!".Equals((string) s.GetCellContents("A1")));
			Assert.AreEqual(3.20, (double) s.GetCellContents("A2"));
			Assert.IsTrue(new Formula("(3+4)*7").Equals((Formula) s.GetCellContents("A3")));
		}

		/// <summary>
		/// Tries to get an Empty cell's contents
		/// </summary>
		[TestMethod()]
		public void GetEmptyCellContentsTest() {
			SS.Spreadsheet s = new();
			Assert.IsTrue("".Equals((string) s.GetCellContents("A1")));
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void GetNonEmptyCellNamesTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "Hello, world!");
			s.SetContentsOfCell("A2", "3.20");
			s.SetContentsOfCell("A3", "=(3+4)*7");
			List<string> names = (List<string>) s.GetNamesOfAllNonemptyCells();

			Assert.AreEqual(3, names.Count);
			Assert.IsTrue(names.Contains("A1"));
			Assert.IsTrue(names.Contains("A2"));
			Assert.IsTrue(names.Contains("A3"));
		}

		/// <summary>
		/// Set's the content of a cell that already has
		/// something else in it.
		/// </summary>
		[TestMethod()]
		public void SetExistingCellContentsTest() {
			//Initialize the sheet
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "Hello, world!");

			//Reset the contents
			s.SetContentsOfCell("A1", "Goodbye");
			Assert.AreEqual("Goodbye", (string) s.GetCellContents("A1"));

			s.SetContentsOfCell("A1", "3.3");
			Assert.AreEqual(3.3, (double) s.GetCellContents("A1"));

			s.SetContentsOfCell("A1", "=3+3");
			Assert.AreEqual(new Formula("3+3"), s.GetCellContents("A1"));
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void BasicDependencyTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=B1 + C1");
			List<string> cellsToRecalculate =
				(List<string>) s.SetContentsOfCell("C1", "9.2");

			Assert.AreEqual(2, cellsToRecalculate.Count());
			Assert.IsTrue(cellsToRecalculate.Contains("A1"));
			Assert.IsTrue(cellsToRecalculate.Contains("C1"));
		}

		/// <summary>
		/// Replaces the contents of one Formula that creates
		/// dependencies with another that create a different
		/// set of dependencies.  Should cleanly end with only
		/// the new set of dependencies stored in the sheet.
		/// </summary>
		[TestMethod()]
		public void ReplaceDependenciesTest() {
			SS.Spreadsheet s = new();
			Assert.AreEqual(0, s.GetNamesOfAllNonemptyCells().Count());
			//Set A1's formula to depend on B1 and C1
			List<string> firstCellsToRedo =
				(List<string>) s.SetContentsOfCell("A1", "=B1 + C1");
			Assert.AreEqual(1, firstCellsToRedo.Count());
			//Get the list of cells that need to be redone after changing C1
			List<string> originalCellsToRedo =
				(List<string>) s.SetContentsOfCell("C1", "9.2");
			
			foreach (string item in originalCellsToRedo) {
				Debug.WriteLine(item);
			}

			//Assert that the list contains only C1 and A1
			Assert.IsTrue(originalCellsToRedo.Contains("C1"));
			Assert.IsTrue(originalCellsToRedo.Contains("A1"));
			Assert.AreEqual(2, originalCellsToRedo.Count());

			//Change A1's formula to rely on B1 and D1 instead
			s.SetContentsOfCell("A1", "=B1 + D1");
			//Get the list for C1 again
			List<string> newCellsToRedo =
				(List<string>) s.SetContentsOfCell("C1", "9.2");
			//Make sure the list is now only C1 (A1 disappears)
			Assert.IsTrue(newCellsToRedo.Count() == 1);

			//Get B1's list
			newCellsToRedo =
				(List<string>) s.SetContentsOfCell("B1", "9");
			//Make sure that B1's list includes A1 now
			Assert.IsTrue(newCellsToRedo.Count() == 2);
			Assert.IsTrue(newCellsToRedo.Contains("B1"));
			Assert.IsTrue(newCellsToRedo.Contains("A1"));
		}

		/// <summary>
		/// Attempts to create a cell that is dependent on itself
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SS.CircularException))]
		public void CircularDependencyDirectTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=A1 - 3");
		}

		/// <summary>
		/// Attempts to create an indirect circular dependency cycle
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SS.CircularException))]
		public void CircularDependencyIndirectTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "=B1 - 3");
			s.SetContentsOfCell("B1", "=A1 + 3");
		}

		/// <summary>
		/// Tests that cells set with "" as their value are removed from the
		/// list of cells that are not empty
		/// </summary>
		[TestMethod()]
		public void RemoveEmptyCellTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("A1", "Hey dude!");
			Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Count() == 1);
			s.SetContentsOfCell("A1", "");
			Assert.IsTrue(!s.GetNamesOfAllNonemptyCells().Any());
		}

		/// <summary>
		/// Attempts to reference an invalid cell name
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(InvalidNameException))]
		public void InvalidCellNameTest() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("&22", "Hello world!");
		}

		/// <summary>
		/// Attempts to reference an invalid cell name
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(InvalidNameException))]
		public void InvalidCellNameTest2() {
			SS.Spreadsheet s = new();
			s.SetContentsOfCell("b2$", "Hello world!");
		}

	}
}