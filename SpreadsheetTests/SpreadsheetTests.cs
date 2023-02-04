



using SpreadsheetUtilities;
using System.Diagnostics;

namespace SpreadsheetTests {

	[TestClass]
	public class SpreadsheetTests {

		[TestMethod()]
		public void TestConstructor() {
			SS.Spreadsheet s = new();
		}

		[TestMethod()]
		public void SetCellContentsTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", "Hello, world!");
			s.SetCellContents("A2", 3.20);
			s.SetCellContents("A3", new Formula("(3+4)*7"));
		}

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

		[TestMethod()]
		public void GetEmptyCellContentsTest() {
			SS.Spreadsheet s = new();
			Assert.IsTrue("".Equals((string) s.GetCellContents("A1")));
		}

		[TestMethod()]
		public void GetEmptyCellNamesTest() {
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

		[TestMethod()]
		public void SetExistingCellContentsTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", "Hello, world!");

			s.SetCellContents("A1", "Goodbye");
			Assert.AreEqual("Goodbye", (string) s.GetCellContents("A1"));

			s.SetCellContents("A1", 3.3);
			Assert.AreEqual(3.3, (double) s.GetCellContents("A1"));

			s.SetCellContents("A1", new Formula("3+3"));
			Assert.AreEqual(new Formula("3+3"), (Formula) s.GetCellContents("A1"));
		}

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

		[TestMethod()]
		[ExpectedException(typeof(SS.CircularException))]
		public void CircularDependencyDirectTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", new Formula("A1 - 3"));
		}

		[TestMethod()]
		[ExpectedException(typeof(SS.CircularException))]
		public void CircularDependencyIndirectTest() {
			SS.Spreadsheet s = new();
			s.SetCellContents("A1", new Formula("B1 - 3"));
			s.SetCellContents("B1", new Formula("A1 + 3"));
		}
	}
}