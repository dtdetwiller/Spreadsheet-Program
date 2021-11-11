// Authors: Daniel Detwiller & Warren Kidman
using SS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SpreadsheetUtilities;


namespace SpreadsheetGUI
{
    /// <summary>
    /// Spreadsheet Form class.
    /// </summary>
    public partial class SpreadsheetForm : Form
    {
        /// <summary>
        /// Represents the spreadsheet in the spreadsheetPanel.
        /// </summary>
        private AbstractSpreadsheet sheet;

        /// <summary>
        /// Holds the file name of this spreadsheet.
        /// </summary>
        private string savedFile;

        /// <summary>
        /// True if nightmode is on, false if otherwise.
        /// </summary>
        private bool nightMode;

        /// <summary>
        /// Creates a new spreadsheet.
        /// </summary>
        public SpreadsheetForm()
        {
            InitializeComponent();
            
            spreadsheetPanel.SelectionChanged += DisplaySelection;

            // selected box when form is run.
            spreadsheetPanel.SetSelection(0, 0);

            // initialized spreadsheet
            sheet = new Spreadsheet(s => isValid(s), s => s.ToUpper(), "ps6");
            cellValueTextBox.Enabled = false;
            cellNameBox.Enabled = false;
            AcceptButton = enterButton;
            cellNameBox.Text = "A1";
            savedFile = "";
            nightMode = false;
            cellContentBox.Select();
            savedMessageLabel.Hide();
        }

        /// <summary>
        /// Creates a spreadsheet from a specified file.
        /// </summary>
        /// <param name="file"></param>
        public SpreadsheetForm(string file)
        {
            InitializeComponent();

            // uses method 'displaySelection' to display current time.
            // will use different method to update based on PS6 specifications.
            spreadsheetPanel.SelectionChanged += DisplaySelection;

            // selected A1 cell when form is run.
            spreadsheetPanel.SetSelection(0, 0);

            // initialized spreadsheet
            sheet = new Spreadsheet(file, s => isValid(s), s => s.ToUpper(), "ps6");
            cellValueTextBox.Enabled = false;
            cellNameBox.Enabled = false;
            AcceptButton = enterButton;
            cellNameBox.Text = "A1";
            savedFile = file;
            nightMode = false;
            savedMessageLabel.Hide();
            cellContentBox.Select();
            UpdateCells();
        }

        /// <summary>
        /// Helper method to that displays the selection, cell name, sell vlaue, and content box.
        /// </summary>
        /// <param name="ss"></param>
        private void DisplaySelection(SpreadsheetPanel ss)
        {
            string value;

            ss.GetSelection(out int col, out int row);
            ss.GetValue(col, row, out value);
            

            cellNameBox.Text = ToCellName(col, row);
            cellValueTextBox.Text = value;

            if (sheet.GetCellContents(cellNameBox.Text) is Formula)
                cellContentBox.Text = "=" + sheet.GetCellContents(cellNameBox.Text).ToString();
            else
                cellContentBox.Text = sheet.GetCellContents(cellNameBox.Text).ToString();

            cellContentBox.Focus();  
        }

        /// <summary>
        /// Handles operating the selection box with arrow keys.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentBox_KeyDown(object sender, KeyEventArgs e)
        {
            ChangeSelectedCell(e);
        }

        /// <summary>
        /// Enter button click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EnterButton_Click(object sender, EventArgs e)
        {
            AddContentToCell();
        }

        /// <summary>
        /// Copies the selected cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(cellContentBox.Text);
        }

        /// <summary>
        /// Save menu item click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If this spreadsheet has been saved before, save to same file.
            if (savedFile != "")
                SaveFile();
            // Else prompt save as dialog.
            else
                SaveAsFile();
        }

        /// <summary>
        /// Opens a new empty spreadsheet window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpreadsheetGUIApplicationContext.getAppContext().RunForm(new SpreadsheetForm());
        }

        /// <summary>
        /// Help menu item click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HelpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisplayHelpDialog();
        }

        /// <summary>
        /// Open menu item click event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }
        
        /// <summary>
        /// Event for when the form is closing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SpreadsheetGUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            CheckIfUnsaved(e);
        }
        
        /// <summary>
        /// Handles when the on button is pressed for night mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!nightMode)
                ToNightMode();
        }

        /// <summary>
        /// Handles when the off button is spressed for nightmode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (nightMode)
                FromNightMode();
        }

        /// <summary>
        /// Pastes the cell content that was copies in the selected cell.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            cellContentBox.Text = Clipboard.GetText();
            AddContentToCell();
        }

        /// <summary>
        /// Handles when the SaveAs menu is pressed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveAsFile();
        }

        /// <summary>
        /// Handles when the close menu item is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Helper method that shows a saved indication for 5 seconds.
        /// </summary>
        private void SavedMessage()
        {
            // Makes the label visible.
            savedMessageLabel.Show();
            Timer t = new Timer();
            // Sets timer for 5 seconds.
            t.Interval = 5000;
            // Sets the tick.
            t.Tick += (sender, e) => 
            {
                // Hides after 5 seconds.
                savedMessageLabel.Hide();
                t.Stop();
            };
            // Starts the timer.
            t.Start();
        }

        /// <summary>
        /// Helper method to tell if variable is in bounds of spreadsheet panel.
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private bool isValid(String str)
        {
            str.Normalize();

            // check if first char is a letter, and remaining char(s) are digits.
            if (!char.IsLetter(str[0]) || !char.IsDigit(str[1]) || !char.IsDigit(str[str.Length-1]))
            { 
                return false;
            }

            // Get the cell row number and check if it's in the boundaries.
            String digit = str.Substring(1, str.Length-1);
            
            if (int.TryParse(digit, out int digitToInt))
            {
                if (digitToInt > 0 && digitToInt < 100)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Changes row and column to cell name
        /// Example: col 0, row 0 = A1
        /// </summary>
        /// <param name="col"></param>
        /// <param name="row"></param>
        /// <returns></returns>
        private string ToCellName(int col, int row)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return alphabet[col] + (row + 1).ToString();
        }

        /// <summary>
        /// Gets the row from cell name.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private int GetRow(string cell)
        {
            return int.Parse(cell.Substring(1, cell.Length - 1)) - 1;
        }

        /// <summary>
        /// Gets the column from cell name.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private int GetCol(string cell)
        {
            string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return alphabet.IndexOf(cell.Substring(0, 1));
        }

        /// <summary>
        /// Helper method that takes care of adding the contents in the content box to the selected cell.
        /// </summary>
        private void AddContentToCell()
        {
            int row, col;
            // Get the selected cell.
            spreadsheetPanel.GetSelection(out col, out row);
            // Set the cell name.
            cellNameBox.Text = ToCellName(col, row);

            try
            {
                // Try to set the contents of the cell.
                sheet.SetContentsOfCell(cellNameBox.Text, cellContentBox.Text);
            }
            // If invalid formula, pop up dialog with message.
            catch (FormulaFormatException)
            {
                string title = "Invalid Cell Name";
                string message = "Valid cells are in the range of A1-Z99." + "\n" + "\nExamples: \n    Valid Cells: A1, G22, K89"
                    + "\n    Invalid Cells: A100, G200, AA1, A01, K00001";

                MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            string value;
            // If value results in a FormulaError, displays FormulaError in the cell.
            if (sheet.GetCellValue(cellNameBox.Text) is FormulaError)
                value = "FormulaError";
            else
                // Gets the cell value.
                value = sheet.GetCellValue(cellNameBox.Text).ToString();
            
            // Sets the cell value text box to the value of the cell.
            cellValueTextBox.Text = value;
            // Sets the value in the cell.
            spreadsheetPanel.SetValue(col, row, cellValueTextBox.Text);

            UpdateCells();
        }

        /// <summary>
        /// Helper method to update the cells.
        /// </summary>
        private void UpdateCells()
        {
            List<string> cells = sheet.GetNamesOfAllNonemptyCells().ToList();
            string value;

            foreach (string name in cells)
            {
                if (sheet.GetCellValue(name) is FormulaError)
                    value = "FormulaError";
                else
                    value = sheet.GetCellValue(name).ToString();

                spreadsheetPanel.SetValue(GetCol(name), GetRow(name), value);
            }

        }

        /// <summary>
        /// Helper method that saves the data to this spreadsheets file.
        /// </summary>
        private void SaveFile()
        {
            sheet.Save(savedFile);
            // Displays the saved message for 5 seconds. (left of cell content label)
            SavedMessage();
        }

        /// <summary>
        /// Helper method to open up the open file dialog.
        /// </summary>
        private void OpenFile()
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Spreadsheet files (*.sprd)|*.sprd|All Files (*.*)|*.*";

            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                SpreadsheetGUIApplicationContext.getAppContext().RunForm(new SpreadsheetForm(openDialog.FileName));
            }
        }

        /// <summary>
        /// Helper method that opens a dialog if data has not been saved in the spreadsheet.
        /// </summary>
        private void CheckIfUnsaved(FormClosingEventArgs e)
        {
            if (sheet.Changed)
            {
                DialogResult dialogResult = MessageBox.Show("Would you like to save your changes to this file?", "Unsaved Data", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (dialogResult.Equals(DialogResult.Yes))
                {
                    Clipboard.Clear();
                    // If this spreadsheet has been saved before, save to same file.
                    if (savedFile != "")
                        SaveFile();
                    // Else prompt save as dialog.
                    else
                        SaveAsFile();
                }
                else if (dialogResult.Equals(DialogResult.Cancel))
                {
                    e.Cancel = true;
                }
            }
            Clipboard.Clear();
        }

        /// <summary>
        /// Helper method that handles changing selected cell with arrow keys.
        /// </summary>
        /// <param name="e"></param>
        private void ChangeSelectedCell(KeyEventArgs e)
        {
            int row, col;

            // Right arrow key is pressed
            if (e.KeyCode.Equals(Keys.Right))
            {
                spreadsheetPanel.GetSelection(out col, out row);
                if (col < 25)
                {
                    spreadsheetPanel.SetSelection(col + 1, row);
                    DisplaySelection(spreadsheetPanel);
                }
            }
            // Left arrow key is pressed.
            else if (e.KeyCode.Equals(Keys.Left))
            {
                spreadsheetPanel.GetSelection(out col, out row);
                if (col > 0)
                {
                    spreadsheetPanel.SetSelection(col - 1, row);
                    DisplaySelection(spreadsheetPanel);
                }
            }
            // Down arrow key is pressed.
            else if (e.KeyCode.Equals(Keys.Down))
            {
                spreadsheetPanel.GetSelection(out col, out row);
                if (row < 98)
                {
                    spreadsheetPanel.SetSelection(col, row + 1);
                    DisplaySelection(spreadsheetPanel);
                }
            }
            // Up arrow key is pressed.
            else if (e.KeyCode.Equals(Keys.Up))
            {
                spreadsheetPanel.GetSelection(out col, out row);
                if (row > 0)
                {
                    spreadsheetPanel.SetSelection(col, row - 1);
                    DisplaySelection(spreadsheetPanel);
                }
            }
        }

        /// <summary>
        /// Helper method that displays the help dialog.
        /// </summary>
        private void DisplayHelpDialog()
        {
            string message = "Entering Content Into Cells:" + "\n    - Select a cell with your mouse pointer, or arrow keys." + "\n    - Enter cell content into the \"Cell Content\" box." + 
                "\n    - Press the \"ENTER\" button or press the enter key to put cell" + "\n      contents into the cell." + 
                "\n    - To enter a formula, append '=' at the beginning of contents." + "\n" + "\nAdditional Features:" + 
                "\n    - Edit Menu:" + "\n        - Night Mode (on/off)" + "\n        - Copy (Select the cell you want to copy, then press copy)" +
                "\n        - Paste (Select cell where you want to paste, then press paste)" +
                "\n    - When you save your data, a saved confirmation is diplayed for 5\n      seconds. (Left of \"Cell Content\")";

            string title = "Help Information";

            MessageBox.Show(message, title, MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        /// <summary>
        /// Prompts the user with a save as dialog.
        /// </summary>
        private void SaveAsFile()
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.FileName = "Untitled";
            saveDialog.Filter = "Spreadsheet files (*.sprd)|*.sprd|All Files (*.*)|*.*";
            // Prompts the user if naming will overwrite another file.
            saveDialog.OverwritePrompt = true;

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                // Sets this spreadsheet filename to the file it was saved to.
                savedFile = saveDialog.FileName;
                sheet.Save(saveDialog.FileName);
                // Displays the saved message for 5 seconds. (left of cell content label)
                SavedMessage();
            }
        }

        /// <summary>
        /// Helper method that sets the spreadsheet to default color.
        /// </summary>
        private void FromNightMode()
        {
            nightMode = false;
            spreadsheetPanel.FromNightMode();
            spreadsheetPanel.BackColor = Color.WhiteSmoke;
            menuStrip.BackColor = Color.Silver;
            fileMenu.ForeColor = Color.Black;
            editMenu.ForeColor = Color.Black;
            helpMenu.ForeColor = Color.Black;
            newToolStripMenuItem.BackColor = Color.WhiteSmoke;
            openToolStripMenuItem.BackColor = Color.WhiteSmoke;
            saveToolStripMenuItem.BackColor = Color.WhiteSmoke;
            saveAsToolStripMenuItem.BackColor = Color.WhiteSmoke;
            closeToolStripMenuItem.BackColor = Color.WhiteSmoke;
            copyToolStripMenuItem.BackColor = Color.WhiteSmoke;
            pasteToolStripMenuItem.BackColor = Color.WhiteSmoke;
            nightModeToolStripMenuItem.BackColor = Color.WhiteSmoke;
            onToolStripMenuItem.BackColor = Color.WhiteSmoke;
            offToolStripMenuItem.BackColor = Color.WhiteSmoke;
            newToolStripMenuItem.ForeColor = Color.Black;
            openToolStripMenuItem.ForeColor = Color.Black;
            saveToolStripMenuItem.ForeColor = Color.Black;
            saveAsToolStripMenuItem.ForeColor = Color.Black;
            closeToolStripMenuItem.ForeColor = Color.Black;
            copyToolStripMenuItem.ForeColor = Color.Black;
            pasteToolStripMenuItem.ForeColor = Color.Black;
            nightModeToolStripMenuItem.ForeColor = Color.Black;
            onToolStripMenuItem.ForeColor = Color.Black;
            offToolStripMenuItem.ForeColor = Color.Black;
            fileMenu.BackColor = Color.Silver;
            editMenu.BackColor = Color.Silver;
            helpMenu.BackColor = Color.Silver;
            panel1.BackColor = Color.White;
            cellNameLabel.BackColor = Color.Transparent;
            cellNameLabel.ForeColor = Color.Black;
            cellNameBox.ForeColor = Color.Black;
            cellNameBox.BackColor = Color.White;
            cellValueLabel.BackColor = Color.Transparent;
            cellValueLabel.ForeColor = Color.Black;
            cellValueTextBox.ForeColor = Color.Black;
            cellValueTextBox.BackColor = Color.White;
            cellContentLabel.BackColor = Color.Transparent;
            cellContentLabel.ForeColor = Color.Black;
            cellContentBox.BackColor = Color.White;
            cellContentBox.ForeColor = Color.Black;
            enterButton.BackColor = Color.White;
            enterButton.ForeColor = Color.Black;
            savedMessageLabel.ForeColor = Color.Green;

            DisplaySelection(spreadsheetPanel);
            UpdateCells();
        }

        /// <summary>
        /// Helper method that sets the spreadsheet to night mode. (Darker colors)
        /// </summary>
        private void ToNightMode()
        {
            nightMode = true;
            spreadsheetPanel.ToNightMode();
            spreadsheetPanel.BackColor = Color.FromArgb(64, 64, 64);
            menuStrip.BackColor = Color.FromArgb(64, 64, 64);
            fileMenu.ForeColor = Color.FromArgb(160, 160, 160);
            editMenu.ForeColor = Color.FromArgb(160, 160, 160);
            helpMenu.ForeColor = Color.FromArgb(160, 160, 160);
            newToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            openToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            saveToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            saveAsToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            closeToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            copyToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            pasteToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            nightModeToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            onToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            offToolStripMenuItem.BackColor = Color.FromArgb(64, 64, 64);
            newToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            openToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            saveToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            saveAsToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            closeToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            copyToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            pasteToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            nightModeToolStripMenuItem.ForeColor = Color.FromArgb(160, 160, 160);
            onToolStripMenuItem.ForeColor = Color.Black;
            offToolStripMenuItem.ForeColor = Color.Black;
            fileMenu.BackColor = Color.FromArgb(64, 64, 64);
            editMenu.BackColor = Color.FromArgb(64, 64, 64);
            helpMenu.BackColor = Color.FromArgb(64, 64, 64);
            panel1.BackColor = Color.FromArgb(80, 80, 80);
            cellNameLabel.BackColor = Color.Transparent;
            cellNameLabel.ForeColor = Color.FromArgb(160, 160, 160);
            cellNameBox.ForeColor = Color.FromArgb(160, 160, 160);
            cellNameBox.BackColor = Color.FromArgb(80, 80, 80);
            cellValueLabel.BackColor = Color.Transparent;
            cellValueLabel.ForeColor = Color.FromArgb(160, 160, 160);
            cellValueTextBox.ForeColor = Color.FromArgb(160, 160, 160);
            cellValueTextBox.BackColor = Color.FromArgb(80, 80, 80);
            cellContentLabel.BackColor = Color.Transparent;
            cellContentLabel.ForeColor = Color.FromArgb(160, 160, 160);
            cellContentBox.BackColor = Color.FromArgb(80, 80, 80);
            cellContentBox.ForeColor = Color.FromArgb(160, 160, 160);
            enterButton.BackColor = Color.FromArgb(80, 80, 80);
            enterButton.ForeColor = Color.FromArgb(160, 160, 160);
            savedMessageLabel.ForeColor = Color.Lime;

            DisplaySelection(spreadsheetPanel);
            UpdateCells();
        }

    }
}
