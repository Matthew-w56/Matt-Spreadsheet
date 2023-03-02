/// Author:		Matthew Williams
/// Partner:	None
/// Date:		24-Feb-2023
/// Finished:	02-Mar-2023
/// Course:		CS 3500 - Software Practice - University of Utah
/// Github ID:	matthew - w56
/// Repo:		https://github.com/uofu-cs3500-spring23/spreadsheet-Matthew-w56
/// Solution:	Spreadsheet
/// Project:	GUI Main Controller
/// Copyright:	CS 3500 and Matthew Williams - This work may not be copied for use in Academic Work
/// 
/// <summary>
/// 
/// This file contains the implementation of the GUI Maui class.
/// 
/// The class allows the user to interact with the Spreadsheet object, and use it's methods and
/// properties.
/// 
/// </summary>

using Microsoft.Maui.Graphics;
using SpreadsheetUtilities;
using SS;
using System.Diagnostics;
using System.Xml.Linq;

namespace GUI {

	/// <summary>
	/// This is the main class that controls the GUI.  It contains a number of methods that
	/// fire when events happen.  It is recommended to read all the methods to get a
	/// holistic view of how it works.
	/// </summary>
	public partial class MainPage: ContentPage {

		//Constants that control the grid
		private readonly char[] TOP_LABELS = "ABCDEFGHIJKLM".ToCharArray();
		private readonly int ROW_COUNT = 10;
		private readonly int GRID_CELL_WIDTH = 90;
		private readonly int GRID_CELL_HEIGHT = 30;
		private readonly int GRID_LEFT_LABEL_WIDTH = 40;
		private readonly int GRID_TOP_LABEL_HEIGHT = 22;

		//Constants that control colors
		private readonly Color LEFT_LABEL_COLOR = Color.FromRgb(225, 225, 227);
		private readonly Color TOP_LABEL_COLOR = Color.FromRgb(200, 200, 202);
		private readonly Color HIGHLIGHT_COLOR = Color.FromRgb(255, 255, 150);
		private readonly Color CELL_BG_COLOR = Color.FromRgb(255, 255, 255);

		//Other program constants
		private readonly static string CURRENT_VERSION = "six";
		protected static readonly int DECIMAL_COUNT = 2;

		//Delegates for cell event actions
		public delegate void ActionOnFocused(CellObject cell);
		public delegate void ActionOnNewContentSet(CellObject cell);
		public delegate void ActionOnChanged(CellObject cell);

		//Cell and file variables for the program
		private CellObject currentFocusedCell = null;
		private bool isSheetFromFile = false;
		private string currentFilePath = "";
		private string currentFileName = "";

		//Default Spreadsheet is a new one
		private AbstractSpreadsheet ss = new Spreadsheet((s) => true, (s) => s.ToUpper(), CURRENT_VERSION);
		//This holds cells so that I can refer to them again later
		private Dictionary<int, Dictionary<string, CellObject>> CellStructure = new();

		/// <summary>
		/// Runs when the program is started.  This method creates the grid for the spreadsheet, and sets some
		/// basic attributes programatically
		/// </summary>
		public MainPage() {
			InitializeComponent();
			CreateGrid();

			Table.HeightRequest = GRID_CELL_HEIGHT * ROW_COUNT;
			TopLabels.WidthRequest = GRID_CELL_WIDTH * TOP_LABELS.Length + GRID_LEFT_LABEL_WIDTH;
			TopLabels.BackgroundColor = TOP_LABEL_COLOR;
			LeftLabels.BackgroundColor = LEFT_LABEL_COLOR;

			SelectedCellContent.TextChanged += SyncCellToInfoBar;
			SelectedCellContent.Completed += HandleInfoBarContentCompleted;

			HandleNewCellFocus(CellStructure[1]["A"]);
		}

		/// <summary>
		/// Handles creating a new spreadsheet and clearing out the content from the old one.
		/// Asks the user for confirmation if there are unsaved changes to the current sheet.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected async void FileMenuNew(object sender, EventArgs e) {

			if (ss.Changed) {
				//Check with the user.  If they say cancel, stop the process.
				if (
					!await DisplayAlert(
						"Unsaved Work",
						"The current Spreadsheet has changes that have not been saved yet.  Continue without saving?",
						"Continue", "Cancel")) {
					return;
				}
			}

			ss = new Spreadsheet((s) => true, (s) => s.ToUpper(), CURRENT_VERSION);

			//Clear the current sheet
			for (int i = 1; i <= CellStructure.Count; i++) {
				foreach (var col in CellStructure[i]) {
					col.Value.Text = "";
				}
			}

			HandleNewCellFocus(CellStructure[1]["A"]);
			isSheetFromFile = false;
			currentFilePath = "";
			SpreadsheetFileName.Text = "New File";
			SyncInfoBarToCellTotal(CellStructure[1]["A"]);
		}

		/// <summary>
		/// Handles saving the current Spreadsheet.  If the current sheet is not
		/// from a file directly, it asks the user for a filename.  If it is though,
		/// it just saves to that directory automatically.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected async void FileMenuSave(object sender, EventArgs e) {

			//Store current file info for later
			string filePath = currentFilePath;
			string fileName = currentFileName;

			//If there is no file yet, plug in info from the user (default save location is the desktop)
			if (!isSheetFromFile) {
				fileName = await DisplayPromptAsync("File Name", "What would you like to call the file (without the file type ending)", "Save") + ".sprd";
				string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
				filePath = desktopPath + "\\" + fileName;
			}

			try {
				ss.Save(filePath);

				//Update the status variables for the filename up top
				isSheetFromFile = true;
				currentFilePath = filePath;
				currentFileName = fileName;
				SpreadsheetFileName.Text = fileName;
			} catch (SpreadsheetReadWriteException _e) {
				await DisplayAlert("A Problem Occurred!", _e.Message, "OK");
			}
			
		}

		/// <summary>
		/// Handles opening an existing Spreadsheet from a file.  Utilizes a file picker
		/// and warns against any unsaved data being lost.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected async void FileMenuOpenAsync(object sender, EventArgs e) {

			if (ss.Changed) {
				//Check with the user.  If they say cancel, stop the process.
				if (
					!await DisplayAlert(
						"Unsaved Work",
						"The current Spreadsheet has changes that have not been saved yet.  Continue without saving?",
						"Continue", "Cancel")) {
					return;
				}
			}


			FileResult? file = await FilePicker.Default.PickAsync();

			if (file is not null) {
				try {

					AbstractSpreadsheet testSS = new Spreadsheet(file.FullPath, (s) => true, (s) => s.ToUpper(), CURRENT_VERSION);

					//If creating the test Sheet didn't fail due to file problems, we're in business to make the switch

					//Clear grid data
					foreach (string name in ss.GetNamesOfAllNonemptyCells()) {
						GetRowAndColOf(name, out string col, out int row);
						CellStructure[row][col].Text = "";
					}
					currentFocusedCell.Text = "";

					//Change current SS to the test one
					ss = testSS;
					HandleUnfocus(currentFocusedCell);
					currentFocusedCell = null;

					//Update all the cells that are new (get their value)
					foreach (string name in ss.GetNamesOfAllNonemptyCells()) {
						GetRowAndColOf(name, out string col, out int row);
						PingCellValue(name, CellStructure[row][col]);
					}

					//Update sheet file info
					isSheetFromFile = true;
					currentFileName = file.FileName;
					currentFilePath = file.FullPath;
					SpreadsheetFileName.Text = currentFileName;
					SyncInfoBarToCellTotal(CellStructure[1]["A"]);
				}
				catch (SpreadsheetReadWriteException _e) {
					await DisplayAlert("File Error!", _e.Message, "OK");
				}
			}
			
		}

		/// <summary>
		/// Displays the Help Menu.  This consists of a list of options to start, and also
		/// the breakout menu for the Functions section.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected async void FileMenuShowHelpBox(object sender, EventArgs e) {

			//This is the Help menu, accessible through the File menu
			string action = await DisplayActionSheet("Help Menu", "Cancel", null,
				"Editing the Spreadsheet",
				"Using the Functions (Added!)",
				"Loading Existing Sheets", 
				"Saving My Sheet", 
				"Value Not Showing on top?"
			);
			switch (action) {
				case "Editing the Spreadsheet":
					await DisplayAlert("Editing the Spreadsheet", 
						"You can click any cell and start typing either directly into it, or in the box that holds " +
						"the content up top.", 
						"OK");
					break;
				case "Using the Functions (Added!)":
					string f_action = await DisplayActionSheet("Using the Functions!", "Cancel", null,
						"General Function Use",
						"Average",
						"Count",
						"CountNum",
						"Median",
						"Sum"
					);
					
					switch (f_action) {
						case "General Function Use":
							await DisplayAlert("Using the Average Function",
								"Functions can be used much like formulas.  To use one, type an equals sign, then " +
								"the name in FULL CAPS, followed by parenthesis containing two variables separated " +
								"by a : character.\nHere's an example:\n" +
								"=FUNCTION(A1:B2)",
								"OK");
							break;
						case "Average":
							await DisplayAlert("Using the Average Function",
								"This function returns the average numerical value for the given range.  Blanks are " +
								"not counted as zero, but are ignored.",
								"OK");
							break;
						case "Count":
							await DisplayAlert("Using the Count Function",
								"This function returns the number of non-empty cells within a given range.",
								"OK");
							break;
						case "CountNum":
							await DisplayAlert("Using the CountNum Function",
								"This function returns the number of cells that contain a numerical value within the given range.",
								"OK");
							break;
						case "Median":
							await DisplayAlert("Using the Median Function",
								"This function returns the median numerical value for the given range.  Blanks are " +
								"not counted as zero, but are ignored.",
								"OK");
							break;
						case "Sum":
							await DisplayAlert("Using the Sum Function",
								"This function returns the result of adding all numerical values in a range together.s",
								"OK");
							break;
					}
					break;
				case "Loading Existing Sheets":
					await DisplayAlert("Loading Existing Sheets", 
						"Go to the file menu and click Load.  You will then find your previously saved Spreadsheet.  " +
						"The default save location is your Desktop.",
						"OK");
					break;
				case "Saving My Sheet":
					await DisplayAlert("Saving My Sheet",
						"To save your spreadsheet, go to the file menu and click Save.  If you haven't saved that spreadsheet " +
						"before, it will ask you what you want to name it.  If you already were editing an existing sheet, it will " +
						"save it to the right place for you automatically.",
						"OK");
					break;
				case "Value Not Showing on top?":
					await DisplayAlert("Value Not Showing on top?",
						"Due to a bug in Maui, the text boxes up top do not always display all the text inside of them, and cut off " +
						"the value and content early.  No known fix exists.",
						"OK");
					break;
				default:
					break;
			}
		}

		/// <summary>
		/// Constructs the objects for the Grid, and it's labels.
		/// In the process, it also populates the CellStructure data
		/// structure with references to the cells.
		/// </summary>
		protected void CreateGrid() {

			//Add the overhead, letter-based labels
			TopLabels.Add(
				new Border {
					StrokeThickness = 0,
					BackgroundColor = TOP_LABEL_COLOR,
					WidthRequest = GRID_LEFT_LABEL_WIDTH,
					Content = new Label {
						Text = " ",
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center

					}
				}
			);
			foreach (char c in TOP_LABELS) {
				TopLabels.Add(
				new Border {
					Stroke = Color.FromRgb(100, 100, 100),
					StrokeThickness = 0.5,
					HeightRequest = GRID_TOP_LABEL_HEIGHT,
					WidthRequest = GRID_CELL_WIDTH,
					HorizontalOptions = LayoutOptions.Center,
					Content = new Label {
						Text = $"{c}",
						BackgroundColor = TOP_LABEL_COLOR,
						HorizontalTextAlignment = TextAlignment.Center
					}
				});
			}

			//Create the rest of the grid
			for (int r = 1; r < ROW_COUNT + 1; r++) {

				//Create the empty row for the internal structure
				CellStructure.Add(r, new Dictionary<string, CellObject>());

				//Create row label
				LeftLabels.Add(
					new Border {
						StrokeThickness = 0.5,
						Stroke = Color.FromRgb(180, 180, 180),
						HeightRequest = GRID_CELL_HEIGHT,
						WidthRequest = GRID_LEFT_LABEL_WIDTH,
						HorizontalOptions = LayoutOptions.Center,
						Content = new Label {
							Text = $"{r}",
							TextColor = Color.FromRgb(0, 0, 0),
							BackgroundColor = LEFT_LABEL_COLOR,
							HorizontalTextAlignment = TextAlignment.Center
						}
					}
				);

				//Create cell grid row
				HorizontalStackLayout rowLayout = new() {
					Spacing = 0
				};
				foreach (char c in TOP_LABELS) {
					CellObject cell = new CellObject(c.ToString(), r, HandleNewCellFocus, HandleNewCellContentSet, SyncInfoBarToCellContent);

					CellStructure[r].Add(c.ToString(), cell);

					rowLayout.Add(new Border {
						Content = cell,
						HeightRequest = GRID_CELL_HEIGHT,
						WidthRequest = GRID_CELL_WIDTH,
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						Padding = 0
					});
				}

				Grid.Add(rowLayout);
			}

		}

		/// <summary>
		/// This event fires when a cell is clicked on, or otherwise focused on.
		/// 
		/// It unfocuses the old focused cell, highlights this one, and updates the top bar.
		/// It also changes the cell's text from the value to the content
		/// </summary>
		/// <param name="cell">The cell to focus on</param>
		protected void HandleNewCellFocus(CellObject cell) {
			HandleUnfocus(currentFocusedCell);
			currentFocusedCell = cell;
			cell.BackgroundColor = HIGHLIGHT_COLOR;
			PingTopInfoBar(cell);
			//Change cell text to contents, rather than value
			cell.Text = GetCorrectedContents(cell.GetName());
		}

		/// <summary>
		/// This isn't connected to events that fire, but is called by HandleNewCellFocus.
		/// It is called on the old focused cell.
		/// 
		/// This method changes the background color back to it's original, changes it's
		/// text back to the value, and stores any unsaved content changes
		/// </summary>
		/// <param name="cell">The cell to unfocus from</param>
		protected void HandleUnfocus(CellObject cell) {
			if (cell is not null) {
				cell.BackgroundColor = CELL_BG_COLOR;
				string name = cell.GetName();
				//Don't try to store contents (and therefore set Changed to true) if cell is blank
				if (ss.GetCellContents(name).Equals("") && cell.Text.Equals("")) return;

				StoreCellContents(cell);
				PingCellValue(name, cell);
			}
		}

		/// <summary>
		/// This fires whenever the enter key is pressed inside of a cell object.
		/// 
		/// It stores it's contents in the Spreadsheet model, and moves the focus down a cell
		/// </summary>
		/// <param name="cell">The cell that just got changed</param>
		protected void HandleNewCellContentSet(CellObject cell) {
			StoreCellContents(cell);
			PingTopInfoBar(cell);
			MoveFocusDownwards();
		}

		/// <summary>
		/// This is called by HandleNewCellContentSet.
		/// 
		/// It changes the focus from the current focused cell to the one below it.
		/// If the current cell is at the bottom of the spreadsheet, no move is made.
		/// </summary>
		/// <param name="cell">The cell being moved from</param>
		protected void MoveFocusDownwards() {
			int row = currentFocusedCell.GetRow();
			string col = currentFocusedCell.GetCol();
			if (row == ROW_COUNT) { PingTopInfoBar(currentFocusedCell); return; }
			CellObject newCell = CellStructure[row + 1][col];
			newCell.Focus();
		}

		/// <summary>
		/// Updates the top info bar to reflect the name, value, and content of the given cell.
		/// 
		/// This is done simply by calling SyncInfoBarToCellTotal(cell);
		/// </summary>
		/// <param name="cell">The cell to display data from</param>
		protected void PingTopInfoBar(CellObject cell) {
			SyncInfoBarToCellTotal(cell);
		}

		/// <summary>
		/// Gives a cell a kick by updating it's value and displaying that to the
		/// correlated cell object.
		/// </summary>
		/// <param name="cell">The cell to ping</param>
		protected void PingCellValue(string name, CellObject cell) {
			object val = ss.GetCellValue(name);
			if (val is FormulaError) cell.Text = "Error!";
			else if (val is double) cell.Text = Math.Round((double) val, DECIMAL_COUNT).ToString();
			else cell.Text = val.ToString();
		}

		/// <summary>
		/// Returns the contents of the cell.  This was made into a method because
		/// it also adds a '=' to the beginning of formulas, and changes the content
		/// to a string altogether.
		/// </summary>
		/// <param name="cellName">The name of the cell to get contents of</param>
		/// <returns>Contents of the cell, ready to be set into a cell object</returns>
		protected string GetCorrectedContents(string cellName) {
			object contents = ss.GetCellContents(cellName);
			if (contents is Formula) return "=" + contents.ToString();
			else return contents.ToString();
		}

		/// <summary>
		/// Updates the spreadsheet model with the data given from the cell object.
		/// 
		/// Also handles Circular Exceptions and other exceptions by giving a pop-up message
		/// to the user explaining problems.  When this happens, no change is made to the
		/// spreadsheet.
		/// </summary>
		/// <param name="cell">The cell being changed</param>
		protected async void StoreCellContents(CellObject cell) {
			string name = cell.GetName();
			try {
				IList<string> updatesToDo = ss.SetContentsOfCell(name, cell.Text);
				UpdateCellVein(updatesToDo);
			}
			catch (CircularException) {
				await DisplayAlert("Formula Exception!", "This formula depends on itself, either directly or indirectly!  Please enter a different formula.", "OK");
			}
			catch (Exception e) {
				await DisplayAlert("Formula Exception!", e.Message, "OK");
			}
		}

		/// <summary>
		/// Takes a list of cells that depend, directly or indirectly, on
		/// a given cell and pings each of them, in dependency order.
		/// </summary>
		/// <param name="cells">List of dependent cells</param>
		protected void UpdateCellVein(IList<string> cells) {
			foreach (string cellName in cells) {
				GetRowAndColOf(cellName, out string col, out int row);
				PingCellValue(cellName, CellStructure[row][col]);
			}
		}

		/// <summary>
		/// Takes in a string and outputs the beginning letter, and the rest
		/// of the string parsed as an integer.
		/// 
		/// A1 => A, 1
		/// B22 => B, 22
		/// 
		/// Note: This assumes that only the first character is a letter
		/// </summary>
		/// <param name="name">The input string</param>
		/// <param name="col">The outputted Letter</param>
		/// <param name="row">The outputted Numbers</param>
		protected void GetRowAndColOf(string name, out string col, out int row) {
			col = name[0].ToString();
			row = int.Parse(name[1..]);
		}

		/// <summary>
		/// This is called when the user changes the text in the content
		/// section of the top info bar.  It applies those same changes
		/// to the corresponding cell.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected void SyncCellToInfoBar(object sender, EventArgs e) {
			if (currentFocusedCell is null) return;
			currentFocusedCell.Text = SelectedCellContent.Text;
		}

		/// <summary>
		/// Changes only the top info bar's content section to match
		/// the given cell's content.  This is called when the user
		/// edits a cell from the cell directly.
		/// </summary>
		/// <param name="cell">The cell to set the info bar's data to</param>
		protected void SyncInfoBarToCellContent(CellObject cell) {
			SelectedCellContent.Text = cell.Text;
		}

		/// <summary>
		/// Updates all 3 data points in the top info bar to match
		/// the given cell (name, value, content).
		/// </summary>
		/// <param name="cell">The cell to update to.</param>
		protected void SyncInfoBarToCellTotal(CellObject cell) {
			string name = cell.GetName();
			SelectedCellContent.Text = GetCorrectedContents(name);
			SelectedCellValue.Text = ss.GetCellValue(name).ToString();
			SelectedCellName.Text = name;
		}

		/// <summary>
		/// This is called when the enter key is entered while the
		/// user is editing content from the info bar itself.  This
		/// calls the HandleNewCellContentSet for the current focused
		/// cell.
		/// </summary>
		/// <param name="sender">unused</param>
		/// <param name="e">unused</param>
		protected void HandleInfoBarContentCompleted(object sender, EventArgs e) {
			HandleNewCellContentSet(currentFocusedCell);
		}
	}
}