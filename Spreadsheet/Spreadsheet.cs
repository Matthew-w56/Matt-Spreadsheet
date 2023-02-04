

using SpreadsheetUtilities;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace SS {
	public class Spreadsheet: AbstractSpreadsheet {

		private DependencyGraph dg;
		Dictionary<string, Cell> cells;

		public Spreadsheet() {
			dg = new();
			cells = new();
		}

		public override object GetCellContents(string name) {
			if (cells.ContainsKey(name)) return cells[name].getContents();
			else return "";
		}

		public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
			return new List<string> (cells.Keys);
		}

		public override ISet<string> SetCellContents(string name, double number) {
			StoreCellContents(name, number);
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		public override ISet<string> SetCellContents(string name, string text) {
			StoreCellContents(name, text);
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		public override ISet<string> SetCellContents(string name, Formula formula) {
			StoreCellContents(name, formula);
			foreach (string dependee in formula.GetVariables()) {
				dg.AddDependency(dependee, name);
			}
			//Return all the cells that depend on this one - directly or indirectly
			return new HashSet<string>(GetCellsToRecalculate(name));
		}

		protected override IEnumerable<string> GetDirectDependents(string name) {
			return dg.GetDependents(name);
		}

		protected void StoreCellContents(string name, object contents) {
			if (cells.ContainsKey(name)) {
				if (contents is Formula) {
					foreach (string previousDependee in dg.GetDependees(name)) {
						dg.RemoveDependency(previousDependee, name);
					}
				}

				cells[name].setContents(contents);
			} else {
				cells.Add(name, new Cell(contents));
			}
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
