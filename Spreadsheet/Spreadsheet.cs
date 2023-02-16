/// Author:		Matthew Williams
/// Partner:	None
/// Date:		05-16 Feb 2023
/// Course:		CS 3500 - Software Practice - University of Utah
/// Github ID:	matthew - w56
/// Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
/// Solution:	Spreadsheet
/// Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Work
/// 
/// <summary>
/// 
/// This file contains the implementation of the AbstractSpreadsheet class.
/// 
/// The class tracks dependencies and used cells, and is used to manipulate the
/// data of the spreadsheet.
/// 
/// Author: Matthew Williams    February 2023
/// 
/// 
/// </summary>

using SpreadsheetUtilities;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;

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

		/// <summary>
		/// Tracks whether or not this Spreadsheet has been changed at all since either
		/// being created, loading in from a file, or saving.
		/// </summary>
		public override bool Changed { get; protected set; }

		/// <summary>
		/// Creates a Spreadsheet with the given parameters.
		/// </summary>
		/// <param name="isValid">Method that validates a string to be used as a cell name</param>
		/// <param name="normalize">Method that normalizes text in a formula</param>
		/// <param name="version">String describing the version of this Spreadsheet</param>
		public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version)
			: base(isValid, normalize, version) {
			dg = new();
			cells = new();
			IsValid = isValid;
			Normalize = normalize;
			Version = version;
			Changed = false;
		}

		/// <summary>
		/// Creates a blank spreadsheet that imposes no extra requirements for
		/// cell names and does not normalize formula input.  Version is "default".
		/// </summary>
		public Spreadsheet() 
			: this( (s)=>true, (s)=>s, "default" ) {  }

		/// <summary>
		/// Loads a Spreadsheet from a file with the given parameters.
		/// </summary>
		/// <param name="filepath">The string file-path to the saved spreadsheet</param>
		/// <param name="isValid">Method that validates a string to be used as a cell name</param>
		/// <param name="normalize">Method that normalizes text in a formula</param>
		/// <param name="version">String describing the version of this Spreadsheet</param>
		public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version)
			: this(isValid, normalize, version) { LoadSavedSpreadsheet(filepath); }

		/// <inheritdoc />
		public override object GetCellContents(string name) {
			VerifyCellName(name);
			if (cells.ContainsKey(name)) return cells[name].getContents();
			else return "";
		}

		/// <inheritdoc />
		protected override IList<string> SetCellContents(string name, double number) {
			StoreCellContents(name, number);
			return new List<string>(GetCellsToRecalculate(name));
		}

		/// <inheritdoc />
		protected override IList<string> SetCellContents(string name, string text) {
			StoreCellContents(name, text);
			return new List<string>(GetCellsToRecalculate(name));
		}

		/// <inheritdoc />
		protected override IList<string> SetCellContents(string name, Formula formula) {
			StoreCellContents(name, formula);
			//Return all the cells that depend on this one - directly or indirectly
			return new List<string>(GetCellsToRecalculate(name));
		}

		/// <inheritdoc />
		public override IList<string> SetContentsOfCell(string name, string content) {
			//Verify cell name first so it doesn't have to be done in the separate SetCellContents methods
			VerifyCellName(name);

			//Now we figure out what kind of input it is
			if (Double.TryParse(content, out double passedVal)) {
				return SetCellContents(name, passedVal);

			} else if (content.StartsWith("=")) {
				//This throws any needed FormulaFormatExceptions
				Formula f1 = new Formula(content[1..], Normalize, IsValid);

				string oldContent = "";
				//Make a backup of the current cell's contents so that we can revert in case of a circular dependency
				if (cells.ContainsKey(name)) {
					oldContent = "" + cells[name].getContents();
				}

				//If we got this far, the Formula is valid and A-OK for us
				try {
					return SetCellContents(name, f1);
				} catch (CircularException) {
					SetContentsOfCell(name, oldContent);
					throw new CircularException();
				}
				
			
			} else {
				//We assume that it is a string at this point
				return SetCellContents(name, content);
			}
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
			Changed = true;

			if (!cells.ContainsKey(name)) { cells.Add(name, new Cell()); }

			if (contents is string && contents.Equals("")) {
				if (cells[name].getContents() is Formula) { dg.ReplaceDependees(name, new List<string>()); }
				cells.Remove(name);
			
			} else if (contents is Formula) {
				cells[name].setContents(contents);
				dg.ReplaceDependees(name, ((Formula) contents).GetVariables());

			} else {
				//Contents assumed to be a double or non-empty string
				cells[name].setContents(contents);
			}
		}

		/// <inheritdoc />
		public override object GetCellValue(string name) {
			VerifyCellName(name);
			if (!cells.ContainsKey(name)) return "";
			if (cells[name].getContents() is Formula) {
				
				return ((Formula) cells[name].getContents()).Evaluate(lookupDelegate);
			} else {
				return cells[name].getContents();
			}
		}

		/// <inheritdoc />
		public override IEnumerable<string> GetNamesOfAllNonemptyCells() {
			return new List<string>(cells.Keys);
		}

		/// <inheritdoc />
		public override string GetSavedVersion(string filename) {
			try {
				using (XmlReader reader = XmlReader.Create(filename)) {

					reader.ReadStartElement("Spreadsheet");
					reader.ReadStartElement("version");
					return reader.ReadString();

				}
			}
			catch (FileNotFoundException) { throw new SpreadsheetReadWriteException($"File cannot be found!  {filename}"); }
			catch (Exception) { throw new SpreadsheetReadWriteException($"Cannot read that file!  {filename}"); }
		}

		/// <inheritdoc />
		public override void Save(string filename) {
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.IndentChars = "   ";
			try {
				using (XmlWriter writer = XmlWriter.Create(filename, settings)) {
					writer.WriteStartElement("Spreadsheet");

					writer.WriteStartElement("version");
					writer.WriteString(Version);
					writer.WriteEndElement();

					writer.WriteStartElement("Cells");

					foreach (string cellName in cells.Keys) {
						writer.WriteStartElement("cell");
						writer.WriteAttributeString("name", cellName);
						writer.WriteAttributeString("content", cells[cellName].getContents().ToString());
						writer.WriteEndElement();
					}

					writer.WriteEndElement();
					writer.WriteEndElement();
				}
			} catch (Exception) {  }

			Changed = false;
		}

		/// <summary>
		/// Reads the given file and copies all the data in to this Spreadsheet object.  Called
		/// in the constructor and is used only the once.
		/// </summary>
		/// <param name="filename">String decribing the location fo the saved spreadsheet</param>
		/// <exception cref="SpreadsheetReadWriteException">Thrown when file can't be found, or layout is inconsistent</exception>
		protected void LoadSavedSpreadsheet(string filename) {
			try {
				using (XmlReader reader = XmlReader.Create(filename)) {

					reader.ReadStartElement("Spreadsheet");
					reader.ReadStartElement("version");
					string fileV = reader.ReadString();
					if (!fileV.Equals(Version)) throw new SpreadsheetReadWriteException(
						$"File version ({fileV}) does not match up with given version ({Version})!"
					);
					reader.ReadEndElement();
					reader.ReadStartElement("Cells");

					//This loops through cell nodes until it hits the end of the file
					while (reader.Read()) {
						if (reader.Name.Equals("cell")) {
							string name = "" + reader.GetAttribute("name");
							string content = "" + reader.GetAttribute("content");
							try { VerifyCellName(name); }
							catch (InvalidNameException) { throw new SpreadsheetReadWriteException($"Spreadsheet contains an invalid cell name! ({name})"); }
							SetCellContents(name, content);
						}
					}
				}
			}
			catch (FileNotFoundException) { throw new SpreadsheetReadWriteException($"File cannot be found!  {filename}"); }
			catch (DirectoryNotFoundException) { throw new SpreadsheetReadWriteException($"Directory cannot be found!  {filename}"); }
			catch (Exception) { throw new SpreadsheetReadWriteException($"Cannot read from path {filename}!"); }

			Changed = false;
		}

		/// <summary>
		/// Makes sure the given name passes internal requirements for a cell name, and
		/// then runs it against the given IsValid method, passed in during construction.
		/// </summary>
		/// <param name="name">String to be examined</param>
		/// <exception cref="InvalidNameException">Name is not valid</exception>
		protected void VerifyCellName(string name) {
			//Name follows required syntax
			if (!Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$")) throw new InvalidNameException();
			//Verify with passed in isValid delegate method
			if (!IsValid(name)) throw new InvalidNameException();
		}

		/// <summary>
		/// Helper method that attempts to get a value from a cell, and throws an
		/// Argument Exception if it can't (Cell is empty or contains a string)
		/// </summary>
		/// <param name="name">Cell name</param>
		/// <returns>The value of that cell</returns>
		/// <exception cref="ArgumentException">Cell is empty, or contains a string</exception>
		protected double lookupDelegate(string name) {
			if (Double.TryParse(GetCellValue(name).ToString(), out double outVal)) {
				return outVal;
			}
			throw new ArgumentException();
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
		/// Creates a cell object without content.
		/// To set content, use setContent() method.
		/// </summary>
		public Cell() {
			content = 0;
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
