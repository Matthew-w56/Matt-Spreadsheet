/// <summary>
/// 
/// This file contains the implementation of the AbstractSpreadsheet class.
/// 
/// The class tracks dependencies and used cells, and is used to manipulate the
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

		/// <inheritdoc />
		public Spreadsheet() {
			dg = new();
			cells = new();
		}

		/// <inheritdoc />
		public override object GetCellContents(string name) {
			VerifyCellName(name);
			if (cells.ContainsKey(name)) return cells[name].getContents();
			else return "";
		}

		/// <inheritdoc />
		public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
			return new List<string> (cells.Keys);
		}

		/// <inheritdoc />
		public override ISet<string> SetCellContents(string name, double number) {
			VerifyCellName(name);
			StoreCellContents(name, number);
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		/// <inheritdoc />
		public override ISet<string> SetCellContents(string name, string text) {
			VerifyCellName(name);
			if (text is null) throw new ArgumentNullException("Argument 'text' cannot be null!");
			StoreCellContents(name, text);
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		/// <inheritdoc />
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

		/// <inheritdoc />
		protected override IEnumerable<string> GetDirectDependents(string name) {
			VerifyCellName(name);
			return dg.GetDependents(name);
		}

		/// <summary>
		/// Takes care of the repeated tasks associated with setting a cell's contents.
		/// 
		/// If the list of cells with contents already has this cell, then
		/// it replaces the content, and makes sure that dependencies are
		/// updated as well to keep the dependency graph up to date.
		/// 
		/// If the cell is set with "" as the contents, the cell is removed from the
		/// list of cells with non-empty contents
		/// 
		/// If the cell doesn't already exist, it creates it.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="contents"></param>
		protected void StoreCellContents(string name, object contents) {
			if (cells.ContainsKey(name)) {
				if (contents is Formula) {
					//Remove any dependencies related to the previous Formula
					//(The SetCellContents method adds the new ones after this method runs)
					foreach (string previousDependee in dg.GetDependees(name)) {
						dg.RemoveDependency(previousDependee, name);
					}
				} else if (contents is string && contents.Equals("")) {
					//Remove a cell from the list of non-empty cells if it's new content is just "".
					cells.Remove(name);
					return;
				}
				//Actually change the cell object's contents
				cells[name].setContents(contents);
			} else {
				//Don't add the cell if it's new value will just be "".
				if (contents is string && contents.Equals("")) return;
				//Otherwise, add a new cell for the desired contents
				cells.Add(name, new Cell(contents));
			}
		}

		protected void VerifyCellName(string name) {
			//Name isn't null
			if (name is null) throw new InvalidNameException();
			//Name starts with a letter or underscore
			if (!Regex.Match(name.Substring(0, 1), "[a-zA-Z]|_").Success) throw new InvalidNameException();
			//Name consists only of letters, numbers, and underscores
			if (Regex.Match(name, "[^a-zA-Z0-9_]").Success) throw new InvalidNameException();
		}
	}

	/// <summary>
	/// Represents a single cell in the spreadsheet, and currently only
	/// serves as a container for the content that it holds.
	/// 
	/// At this point in the program, it doesn't make sense to introduce the
	/// overhead of individual classes for each cell, but we are informed that
	/// it will become efficient and helpful later on.
	/// </summary>
	internal class Cell {

		protected object content;

		/// <summary>
		/// Creates a cell object with some content in it
		/// The content is a string, double, or Formula object
		/// </summary>
		/// <param name="_content">Thing that is inside the cell</param>
		public Cell(object _content) {
			content = _content;
		}

		/// <summary>
		/// Returns the contents of the cell
		/// </summary>
		/// <returns>Cell's contents</returns>
		public object getContents() {
			return content;
		}

		/// <summary>
		/// Sets the contents of the cell to the given input
		/// </summary>
		/// <param name="contents">New content of the cell</param>
		public void setContents(object contents) {
			content = contents;
		}
	}
}
