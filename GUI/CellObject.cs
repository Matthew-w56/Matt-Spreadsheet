
using static GUI.MainPage;

namespace GUI {
	public class CellObject : Entry {

		private string col;
		private int row;

		ActionOnFocused actionWhenFocus;
		ActionOnNewContentSet actionWhenSet;
		ActionOnChanged actionOnChanged;
		
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

			//TODO: To handle moving around on enter key, save a passed in method here to call once changed.
			//Outside world handles changing focus from this to another cell object
		}

		protected void OnContentSet(object sender, EventArgs e) { actionWhenSet(this); }
		protected void OnContentChanged(object sender, EventArgs e) { actionOnChanged(this); }
		protected void OnFocusEvent(object sender, EventArgs e) { actionWhenFocus(this); }

		public string GetCol() {
			return col;
		}

		public int GetRow() {
			return row;
		}

		public string GetName() {
			return col + row.ToString();
		}
	}
}
