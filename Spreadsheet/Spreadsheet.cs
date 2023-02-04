/// <summary>
/// 
/// This file contains the implementation of the AbstractSpreadsheet class.
/// 
/// The class tracks dependencies, used cells, and is used to manipulate the
/// data of the spreadsheet.
/// 
/// Author: Matthew Williams    February 2023
/// 
/// </summary>

using SpreadsheetUtilities;
using System.Text.RegularExpressions;

namespace SS {

	/// <summary>
	/// Holds and manipulates the data for a Spreadsheet.
	/// Functions include:
	///		Tracking data
	///		Tracking dependencies
	///		Preventing dependency cycles
	///		Returning useful info about spreadsheet
	/// </summary>
	public class Spreadsheet: AbstractSpreadsheet {

		//The dependency graph for this sheet
		private DependencyGraph dg;
		//A dictionary of all cells that are not empty
		Dictionary<string, Cell> cells;

		public Spreadsheet() {
			dg = new();
			cells = new();
		}

		public override object GetCellContents(string name) {
			VerifyCellName(name);
			if (cells.ContainsKey(name)) return cells[name].getContents();
			else return "";
		}

		public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
			return new List<string> (cells.Keys);
		}

		public override ISet<string> SetCellContents(string name, double number) {
			VerifyCellName(name);
			StoreCellContents(name, number);
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		public override ISet<string> SetCellContents(string name, string text) {
			VerifyCellName(name);
			if (text is null) throw new ArgumentNullException("Argument 'text' cannot be null!");
			StoreCellContents(name, text);
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		public override ISet<string> SetCellContents(string name, Formula formula) {
			VerifyCellName(name);
			if (formula is null) throw new ArgumentNullException("Argument 'formula' cannot be null!");
			StoreCellContents(name, formula);
			foreach (string dependee in formula.GetVariables()) {
				dg.AddDependency(dependee, name);
			}
			//Return all the cells that depend on this one - directly or indirectly
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		protected override IEnumerable<string> GetDirectDependents(string name) {
			VerifyCellName(name);
			return dg.GetDependents(name);
		}

		protected void StoreCellContents(string name, object contents) {
			VerifyCellName(name);
			if (cells.ContainsKey(name)) {
				if (contents is Formula) {
					foreach (string previousDependee in dg.GetDependees(name)) {
						dg.RemoveDependency(previousDependee, name);
					}
				} else if (contents is string && contents.Equals("")) {
					cells.Remove(name);
					return;
				}

				cells[name].setContents(contents);
			} else {
				cells.Add(name, new Cell(contents));
			}
		}

		protected void VerifyCellName(string name) {
			if (name is null) throw new InvalidNameException();
			if (!Regex.Match(name.Substring(0, 1), "[a-zA-Z]|_").Success) throw new InvalidNameException();
			if (Regex.Match(name, "[^a-zA-Z0-9_]").Success) throw new InvalidNameException();
		}
	}

	internal class Cell {
		protected object content;

		public Cell(object _content) {
			content = _content;
		}

		public object getContents() {
			return content;
		}

		public void setContents(object contents) {
			content = contents;
		}
	}
}
