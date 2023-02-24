using Microsoft.Maui.Graphics;

namespace GUI {
	public partial class MainPage: ContentPage {

		private readonly char[] TOP_LABELS = "ABCDEFGHIJKLM".ToCharArray();
		private readonly int ROW_COUNT = 10;
		private readonly int GRID_CELL_WIDTH = 90;
		private readonly int GRID_CELL_HEIGHT = 30;
		private readonly int GRID_LEFT_LABEL_WIDTH = 40;

		private readonly Color LEFT_LABEL_COLOR = Color.FromRgb(220, 220, 220);
		private readonly Color TOP_LABEL_COLOR = Color.FromRgb(190, 190, 190);
		private readonly Color MAGIC_SQUARE_COLOR = Color.FromRgb(100, 100, 100);

		public delegate void ActionOnFocused(CellObject cell);

		public MainPage() {
			InitializeComponent();
			CreateGrid();

			Table.HeightRequest = GRID_CELL_HEIGHT * ROW_COUNT;
			TopLabels.BackgroundColor = TOP_LABEL_COLOR;
			LeftLabels.BackgroundColor = LEFT_LABEL_COLOR;
		}

		protected void FileMenuNew(object sender, EventArgs e) {
			//TODO: fill this method out
		}

		protected void FileMenuOpenAsync(object sender, EventArgs e) {
			//TODO: fill this method out
		}

		protected void CreateGrid() {

			//Add the overhead, letter-based labels
			TopLabels.Add(
				new Border {
					StrokeThickness = 0,
					BackgroundColor = MAGIC_SQUARE_COLOR,
					WidthRequest = GRID_LEFT_LABEL_WIDTH,
					Content = new Label {
						Text = "x",
						HorizontalTextAlignment = TextAlignment.Center,
						VerticalTextAlignment = TextAlignment.Center

					}
				}
			); ;
			foreach (char c in TOP_LABELS) {
				TopLabels.Add(
				new Border {
					Stroke = Color.FromRgb(0, 0, 0),
					StrokeThickness = 0,
					HeightRequest = GRID_CELL_HEIGHT,
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

				//Create row label
				LeftLabels.Add(
					new Border {
						StrokeThickness = 0,
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
				HorizontalStackLayout rowLayout = new();
				foreach (char c in TOP_LABELS) {
					rowLayout.Add(new Border {
						Content = new CellObject(c.ToString(), r, HandleNewCellFocus),
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
			//TODO: implement.
			SelectedCellName.Text = cell.Height.ToString();
		}
	}
}