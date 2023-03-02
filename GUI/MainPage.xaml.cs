using Microsoft.Maui.Graphics;
using SpreadsheetUtilities;
using SS;
using System.Diagnostics;
using System.Xml.Linq;

//TODO: Change content viewer on top bar into entry
//TODO: Display filename of loaded file.  Save to that file by default
//TODO: Implement a default decimal limit for displaying
//TODO: Option: implement a default file path to save to (Desktop would be best)
//TODO: Maintain the spreadsheet file name label

namespace GUI {
	public partial class MainPage: ContentPage {

		private readonly char[] TOP_LABELS = "ABCDEFGHIJKLM".ToCharArray();
		private readonly int ROW_COUNT = 10;
		private readonly int GRID_CELL_WIDTH = 90;
		private readonly int GRID_CELL_HEIGHT = 30;
		private readonly int GRID_LEFT_LABEL_WIDTH = 40;
		private readonly int GRID_TOP_LABEL_HEIGHT = 22;

		private readonly Color LEFT_LABEL_COLOR = Color.FromRgb(225, 225, 227);
		private readonly Color TOP_LABEL_COLOR = Color.FromRgb(200, 200, 202);
		private readonly Color HIGHLIGHT_COLOR = Color.FromRgb(255, 255, 150);
		private readonly Color CELL_BG_COLOR = Color.FromRgb(255, 255, 255);

		private readonly static string CURRENT_VERSION = "six";

		public delegate void ActionOnFocused(CellObject cell);
		public delegate void ActionOnNewContentSet(CellObject cell);
		public delegate void ActionOnChanged(CellObject cell);

		private CellObject currentFocusedCell = null;
		private bool isSheetFromFile = false;
		private string currentFilePath = "";
		private string currentFileName = "";

		//Default Spreadsheet is a new one
		private AbstractSpreadsheet ss = new Spreadsheet((s) => true, (s) => s.ToUpper(), CURRENT_VERSION);
		private Dictionary<int, Dictionary<string, CellObject>> CellStructure = new();

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

		protected async void FileMenuNew(object sender, EventArgs e) {
			bool shouldContinue = true;

			if (ss.Changed) {
				shouldContinue =
					await DisplayAlert(
						"Unsaved Work",
						"The current Spreadsheet has changes that have not been saved yet.  Continue without saving?",
						"Continue", "Cancel");
			}

			if (!shouldContinue) return;

			ss = new Spreadsheet((s) => true, (s) => s.ToUpper(), CURRENT_VERSION);

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

		protected async void FileMenuSave(object sender, EventArgs e) {

			string filePath = currentFilePath;
			string fileName = currentFileName;

			if (!isSheetFromFile) {
				fileName = await DisplayPromptAsync("File Name", "What would you like to call the file (without the file type ending)", "Save") + ".sprd";
				string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
				filePath = desktopPath + "\\" + fileName;
			}

			try {
				ss.Save(filePath);

				isSheetFromFile = true;
				currentFilePath = filePath;
				currentFileName = fileName;
				SpreadsheetFileName.Text = fileName;
			} catch (SpreadsheetReadWriteException _e) {
				await DisplayAlert("A Problem Occurred!", _e.Message, "OK");
			}
			
		}

		protected async void FileMenuOpenAsync(object sender, EventArgs e) {
			bool shouldContinue = true;

			if (ss.Changed) {
				shouldContinue =
					await DisplayAlert(
						"Unsaved Work",
						"The current Spreadsheet has changes that have not been saved yet.  Continue without saving?",
						"Continue", "Cancel");
			}

			if (!shouldContinue) return;


			FileResult? file = await FilePicker.Default.PickAsync();

			if (file is not null) {
				try {

					AbstractSpreadsheet testSS = new Spreadsheet(file.FullPath, (s) => true, (s) => s.ToUpper(), CURRENT_VERSION);

					//If we haven't failed yet, we're in business
					foreach (string name in ss.GetNamesOfAllNonemptyCells()) {
						GetRowAndColOf(name, out string col, out int row);
						CellStructure[row][col].Text = "";
					}
					currentFocusedCell.Text = "";

					ss = testSS;
					HandleUnfocus(currentFocusedCell);
					currentFocusedCell = null;

					foreach (string name in ss.GetNamesOfAllNonemptyCells()) {
						GetRowAndColOf(name, out string col, out int row);
						PingCellValue(name, CellStructure[row][col]);
					}

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

		protected async void FileMenuShowHelpBox(object sender, EventArgs e) {
			string action = await DisplayActionSheet("Help Menu", "Cancel", null,
				"Editing the Spreadsheet", 
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

		protected void HandleNewCellFocus(CellObject cell) {

			HandleUnfocus(currentFocusedCell);
			currentFocusedCell = cell;
			cell.BackgroundColor = HIGHLIGHT_COLOR;
			PingTopInfoBar(cell);
			//Change cell text to contents, rather than value
			cell.Text = GetCorrectedContents(cell.GetName());
		}

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

		protected void HandleNewCellContentSet(CellObject cell) {
			StoreCellContents(cell);
			PingTopInfoBar(cell);
			//TODO: MoveDownwards( cell );
		}

		protected void PingTopInfoBar(CellObject cell) {
			string name = cell.GetName();
			SyncInfoBarToCellTotal(cell);
		}

		protected void PingCellValue(string name, CellObject cell) {
			object val = ss.GetCellValue(name);
			if (val is FormulaError) cell.Text = "Error!";
			else cell.Text = val.ToString();
		}

		protected string GetCorrectedContents(string cellName) {
			object contents = ss.GetCellContents(cellName);
			if (contents is Formula) return "=" + contents.ToString();
			else return contents.ToString();
		}

		protected async void StoreCellContents(CellObject cell) {
			string name = cell.GetName();
			try {
				IList<string> updatesToDo = ss.SetContentsOfCell(name, cell.Text);
				UpdateCellVein(updatesToDo);
			}
			catch (FormulaFormatException f_e) {
				await DisplayAlert("Formula Exception!", f_e.Message, "OK");
			}
		}

		protected void UpdateCellVein(IList<string> cells) {
			foreach (string cellName in cells) {
				GetRowAndColOf(cellName, out string col, out int row);
				CellObject cell = CellStructure[row][col];
				cell.Text = ss.GetCellValue(cellName).ToString();
			}
		}

		protected void GetRowAndColOf(string name, out string col, out int row) {
			col = name[0].ToString();
			row = int.Parse(name[1..]);
		}

		protected void SyncCellToInfoBar(object sender, EventArgs e) {
			if (currentFocusedCell is null) return;
			currentFocusedCell.Text = SelectedCellContent.Text;
		}

		protected void SyncInfoBarToCellContent(CellObject cell) {
			SelectedCellContent.Text = cell.Text;
		}

		protected void SyncInfoBarToCellTotal(CellObject cell) {
			string name = cell.GetName();
			SelectedCellContent.Text = GetCorrectedContents(name);
			SelectedCellValue.Text = ss.GetCellValue(name).ToString();
			SelectedCellName.Text = name;
		}

		protected void HandleInfoBarContentCompleted(object sender, EventArgs e) {
			HandleNewCellContentSet(currentFocusedCell);
		}
	}
}