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

namespace SpreadsheetTests {

	/// <summary>
	/// Tests the Spreadsheet class, which is an implementation of
	/// the AbstractSpreadsheet abstract class.
	/// </summary>
	[TestClass]
	public class SpreadsheetTests {

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void TestConstructor() {
			SS.Spreadsheet s = new();
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void SetCellContentsTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", "Hello, world!");
			s.SetCellContents("A2", 3.20);
			s.SetCellContents("A3", new Formula("(3+4)*7"));
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void GetCellContentsTest() {
			SS.Spreadsheet s = new();
			//Checking for each contents type
			s.SetCellContents("A1", "Hello, world!");
			s.SetCellContents("A2", 3.20);
			s.SetCellContents("A3", new Formula("(3+4)*7"));

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
			s.SetCellContents("A1", "Hello, world!");
			s.SetCellContents("A2", 3.20);
			s.SetCellContents("A3", new Formula("(3+4)*7"));
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
			s.SetCellContents("A1", "Hello, world!");

			//Reset the contents
			s.SetCellContents("A1", "Goodbye");
			Assert.AreEqual("Goodbye", (string) s.GetCellContents("A1"));

			s.SetCellContents("A1", 3.3);
			Assert.AreEqual(3.3, (double) s.GetCellContents("A1"));

			s.SetCellContents("A1", new Formula("3+3"));
			Assert.AreEqual(new Formula("3+3"), s.GetCellContents("A1"));
		}

		/// <summary>
		/// See title.
		/// </summary>
		[TestMethod()]
		public void BasicDependencyTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", new Formula("B1 + C1"));
			HashSet<string> cellsToRecalculate =
				(HashSet<string>)s.SetCellContents("C1", 9.2);

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

			//Set A1's formula to depend on B1 and C1
			s.SetCellContents("A1", new Formula("B1 + C1"));
			//Get the list of cells that need to be redone after changing C1
			HashSet<string> originalCellsToRedo =
				(HashSet<string>) s.SetCellContents("C1", 9.2);
			//Assert that the list contains only C1 and A1
			Assert.IsTrue(originalCellsToRedo.Count() == 2);
			Assert.IsTrue(originalCellsToRedo.Contains("C1"));
			Assert.IsTrue(originalCellsToRedo.Contains("A1"));

			//Change A1's formula to rely on B1 and D1 instead
			s.SetCellContents("A1", new Formula("B1 + D1"));
			//Get the list for C1 again
			HashSet<string> newCellsToRedo =
				(HashSet<string>) s.SetCellContents("C1", 9.2);
			//Make sure the list is now only C1 (A1 disappears)
			Assert.IsTrue(newCellsToRedo.Count() == 1);

			//Get B1's list
			newCellsToRedo =
				(HashSet<string>) s.SetCellContents("B1", 9.0);
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
			s.SetCellContents("A1", new Formula("A1 - 3"));
		}

		/// <summary>
		/// Attempts to create an indirect circular dependency cycle
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(SS.CircularException))]
		public void CircularDependencyIndirectTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", new Formula("B1 - 3"));
			s.SetCellContents("B1", new Formula("A1 + 3"));
		}

		/// <summary>
		/// Tests that cells set with "" as their value are removed from the
		/// list of cells that are not empty
		/// </summary>
		[TestMethod()]
		public void RemoveEmptyCellTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", "Hey dude!");
			Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Count() == 1);
			s.SetCellContents("A1", "");
			Assert.IsTrue(s.GetNamesOfAllNonemptyCells().Count() == 0);
		}

		/// <summary>
		/// Attempts to reference an invalid cell name
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(InvalidNameException))]
		public void InvalidCellNameTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("&22", "Hello world!");
		}

		/// <summary>
		/// Attempts to reference an invalid cell name
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(InvalidNameException))]
		public void InvalidCellNameTest2() {
			SS.Spreadsheet s = new();
			s.SetCellContents("b2$", "Hello world!");
		}

		/// <summary>
		/// Attempts to reference a null cell name
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(InvalidNameException))]
		public void InvalidCellNameTest3() {
			SS.Spreadsheet s = new();
			s.SetCellContents(null, "Hello world!");
		}

		/// <summary>
		/// Attempts to set a cell's contents to null
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetNullCellContentsTest() {
			SS.Spreadsheet s = new();
			string val = null;
			s.SetCellContents("A3", val);
		}

		/// <summary>
		/// Attempts to set a cell's formula to null
		/// </summary>
		[TestMethod()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SetNullCellFormulaTest() {
			SS.Spreadsheet s = new();
			Formula val = null;
			s.SetCellContents("A3", val);
		}
	}
}