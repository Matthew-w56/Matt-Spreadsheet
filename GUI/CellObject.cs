/// Author:		Matthew Williams
/// Partner:	None
/// Date:		24-Feb-2023
/// Finished:	02-Mar-2023
/// Course:		CS 3500 - Software Practice - University of Utah
/// Github ID:	matthew - w56
/// Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
/// Solution:	Spreadsheet
/// Project:	Cell Object
/// Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Work
/// 
/// <summary>
/// 
/// This class acts as a wrapper for an Entry in the Maui program.  It has additional methods and properties
/// that make it easier to work with in the Maui GUI class.
/// 
/// </summary>

using static GUI.MainPage;

namespace GUI {

	/// <summary>
	/// This class wraps around the Entry class, and adds functionality that helps
	/// in the GUI program such as it's row and column, and the actions it should
	/// take when various events are fired.
	/// </summary>
	public class CellObject : Entry {

		private string col;
		private int row;

		ActionOnFocused actionWhenFocus;
		ActionOnNewContentSet actionWhenSet;
		ActionOnChanged actionOnChanged;
		
		/// <summary>
		/// Constructs a Cell Object.
		/// </summary>
		/// <param name="_column">The column of this cell</param>
		/// <param name="_row">The row of this cell</param>
		/// <param name="focusAction">The method to be called when this cell is focused on</param>
		/// <param name="setAction">The method to be called when the enter key is pressed inside of this cell</param>
		/// <param name="changeAction">The method to be called when this cell's text is changed at all</param>
		public CellObject(string _column, int _row, ActionOnFocused focusAction, ActionOnNewContentSet setAction, ActionOnChanged changeAction) {
			col = _column;
			row = _row;

			//Assign all event-based methods
			this.Completed += OnContentSet;
			this.TextChanged += OnContentChanged;
			actionWhenFocus = focusAction;
			this.Focused += OnFocusEvent;
			actionWhenSet = setAction;
			actionOnChanged = changeAction;
		}

		/// <summary>
		/// Redirects the call to the given method, passing in this
		/// object as a parameter.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected void OnContentSet(object sender, EventArgs e) { actionWhenSet(this); }

		/// <summary>
		/// Redirects the call to the given method, passing in this
		/// object as a parameter.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected void OnContentChanged(object sender, EventArgs e) { actionOnChanged(this); }

		/// <summary>
		/// Redirects the call to the given method, passing in this
		/// object as a parameter.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected void OnFocusEvent(object sender, EventArgs e) { actionWhenFocus(this); }

		/// <summary>
		/// Returns the column of this cell.
		/// </summary>
		/// <returns>The column letter for this cell</returns>
		public string GetCol() {
			return col;
		}

		/// <summary>
		/// Returns the row of this cell.
		/// </summary>
		/// <returns>The row number for this cell</returns>
		public int GetRow() {
			return row;
		}

		/// <summary>
		/// Returns the name of this cell, which is equal to the
		/// column followed by the row.
		/// </summary>
		/// <returns>The name for this cell</returns>
		public string GetName() {
			return col + row.ToString();
		}
	}
}
