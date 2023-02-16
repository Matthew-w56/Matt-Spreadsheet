/// Author:		Matthew Williams
/// Partner:	None
/// Date:		03 Feb 2023
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

		[TestMethod()]
		public void TestSpreadsheetSave() {
			Spreadsheet s = new Spreadsheet();
			s.SetContentsOfCell("A1", "9.0");
			s.SetContentsOfCell("A2", "Hey man");
			s.Save("myfile.xml");

			Assert.AreEqual("default", s.GetSavedVersion("myfile.xml"));
		}

		[TestMethod()]
		public void TestSpreadsheetLoad() {
			Spreadsheet s = new Spreadsheet();
			s.SetContentsOfCell("A1", "4.0");
			s.SetContentsOfCell("A2", "Hey man");
			s.Save("myfile.xml");
			Spreadsheet s2 = new Spreadsheet("myfile.xml", (s)=>true, (s)=>s, "default");
			Assert.AreEqual(s2.GetCellContents("A1").ToString(), "4");
		}

	}
}