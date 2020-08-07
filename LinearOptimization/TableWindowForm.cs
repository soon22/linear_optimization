using System;
using System.Configuration;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Data;
using System.ComponentModel;

namespace LinearOptimization
{
    public partial class TableWindowForm : Form
    {
        private SimplexAlgo DisplayOutput;
        private Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
        private DataTable outputDual;
        private DataTable standardDual;
        private int solveOrDual; //1 for normal solving, 2 for dual
        private int[] stepsToEachSol;
        private int cmbOldCount;
        private DataTable allSolutions;
        private string firstSolLeave;
        private string firstSolEnter;
        private int xPos;
        private int yPos;
        private int width;
        private int height;

        private static event EventHandler PropertyChanged;

        private static bool kill;
        public static bool Kill
        {
            get
            {
                return kill;
            }
            set
            {
                if (kill != value)
                {
                    kill = value;
                    if (PropertyChanged != null)
                    { // If someone subscribed to the event
                        PropertyChanged(typeof(TableWindowForm), EventArgs.Empty); // Raise the event
                    }
                }
            }
        }

        public TableWindowForm()
        {
            InitializeComponent();
        }

        static public void InitializeTableForm(ComboBox.ObjectCollection cmbCollection, int index, int solveOrDual, int[] stepsToEachSol, int cmbOldCount, DataTable allSolutions, string firstSolEnter, string firstSolLeave, int width, int height, int xPos, int yPos)
        {
            //the new form created will be handled by a new background thread
            Thread thread = new Thread(new ThreadStart(() => StartForm(cmbCollection, index, solveOrDual, stepsToEachSol, cmbOldCount, allSolutions, firstSolEnter, firstSolLeave, width, height, xPos, yPos)));
            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
        }

        private static void StartForm(ComboBox.ObjectCollection cmbCollection, int index, int solveOrDual, int[] stepsToEachSol, int cmbOldCount, DataTable allSolutions, string firstSolEnter, string firstSolLeave, int width, int height ,int xPos, int yPos)
        {
            //create a new form
            TableWindowForm form = new TableWindowForm();
            form.solveOrDual = solveOrDual;
            if (solveOrDual == 1)
            {
                form.DisplayOutput = Main.DisplayOutput;
                form.stepsToEachSol = stepsToEachSol;
                form.allSolutions = allSolutions.Copy();
                form.cmbOldCount = cmbOldCount;
                form.firstSolEnter = firstSolEnter;
                form.firstSolLeave = firstSolLeave;
            }
            else
            {
                form.outputDual = Main.Outputtable;
                form.standardDual = Main.Standard;
            }
            form.cmbTableSelect.DataSource = cmbCollection;
            form.cmbTableSelect.SelectedIndex = index;
            if (width > 0 && xPos >= 0 && yPos >= 0 && height > 0)
            {
                form.width = width;
                form.xPos = xPos;
                form.yPos = yPos;
                form.height = height;
            }
            TableWindowForm.PropertyChanged += form.listner; //so that all forms can be killed at once
            Application.Run(form);
        }

        //to kill all forms at once
        private void listner(object sender, EventArgs e)
        {
            if(Kill)
            {
                this.InvokeIfRequired(() =>
                {
                    this.Close();
                });
            }
        }

        private void cmbTableSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            //generally the same as cmbTableSelect_SelectedIndexChanged in Main.cs
            dgvOutputTable.DataSource = null;
            dgvOutputTable.Rows.Clear();

            if (solveOrDual == 1)
            {
                if (cmbTableSelect.SelectedIndex == 0)
                {
                    dgvOutputTable.DataSource = DisplayOutput.standardLP;
                    dgvOutputTable.Columns["DELETE"].HeaderText = "";
                }
                else if (cmbTableSelect.SelectedIndex < cmbOldCount - 1)
                {
                    dgvOutputTable.DataSource = DisplayOutput.outputTableau[cmbTableSelect.SelectedIndex - 1];
                    HighlightOutputTable();
                }
                else if (cmbTableSelect.SelectedIndex < cmbOldCount)
                {
                    if (DisplayOutput.state == 1 || DisplayOutput.state == 3)
                    {
                        dgvOutputTable.DataSource = DisplayOutput.Solution;
                    }
                    else
                    {
                        dgvOutputTable.DataSource = DisplayOutput.outputTableau[DisplayOutput.outputTableau.Count - 1];
                    }
                }
                else if (DisplayOutput.state == 3 && cmbTableSelect.SelectedIndex >= cmbOldCount)
                {
                    if (cmbTableSelect.SelectedIndex != cmbTableSelect.Items.Count - 1)
                    {
                        int cmbRange;
                        int previousSolutionIndex = cmbOldCount - 1;
                        for (cmbRange = 0; cmbRange < stepsToEachSol.Length - 1; cmbRange++)
                        {
                            if (cmbTableSelect.SelectedIndex <= previousSolutionIndex + stepsToEachSol[cmbRange])
                            {
                                break;
                            }
                            else
                            {
                                previousSolutionIndex += stepsToEachSol[cmbRange];
                            }
                        }

                        if (cmbTableSelect.SelectedIndex < previousSolutionIndex + stepsToEachSol[cmbRange])
                        {
                            dgvOutputTable.DataSource = DisplayOutput.mulList[cmbRange][cmbTableSelect.SelectedIndex - previousSolutionIndex - 1];
                            if (dgvOutputTable.Columns["enter"] != null)
                            {
                                HighlightOutputTable();
                            }
                        }
                        else if (cmbTableSelect.SelectedIndex == previousSolutionIndex + stepsToEachSol[cmbRange])
                        {
                            dgvOutputTable.DataSource = DisplayOutput.mulSolution[cmbRange];
                        }
                    }
                    else
                    {
                        if (allSolutions != null)
                        {
                            dgvOutputTable.DataSource = allSolutions;
                            dgvOutputTable.Columns["No."].Width = 30;
                        }
                    }
                }
            }
            else if (solveOrDual == 2)
            {
                if (cmbTableSelect.SelectedIndex == 0)
                {
                    if (standardDual != null)
                    {
                        dgvOutputTable.DataSource = standardDual;
                    }
                }
                else if (cmbTableSelect.SelectedIndex == 1)
                {
                    if (outputDual != null)
                    {
                        dgvOutputTable.DataSource = outputDual;
                    }
                }
            }
            foreach(DataGridViewColumn col in dgvOutputTable.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                col.MinimumWidth = 45;
            }
            dgvOutputTable.ClearSelection();
            dgvOutputTable.Refresh();
        }

        private void HighlightOutputTable()
        {
            //generally the same as HighlightOutputTable() in Main.cs
            if (dgvOutputTable.DataSource != null && solveOrDual == 1) //if dgv isn't empty and is normal solving; since dual doesn't need highlight
            {
                //for all without new multiple solutions (old handling)
                if ((cmbTableSelect.SelectedIndex < cmbOldCount - 2 && cmbTableSelect.SelectedIndex > 0) || (cmbTableSelect.SelectedIndex < cmbOldCount - 1 && cmbTableSelect.SelectedIndex > 0 && (DisplayOutput.state == 2 || DisplayOutput.state == 5)))
                {
                    int pivotColumn = -1;
                    int pivotRow = -1;
                    foreach (DataGridViewColumn col in dgvOutputTable.Columns)
                    {
                        if (col.Name == DisplayOutput.enterVariable[cmbTableSelect.SelectedIndex - 1])
                        {
                            col.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                            pivotColumn = col.Index;
                            break;
                        }
                    }
                    foreach (DataGridViewRow row in dgvOutputTable.Rows)
                    {
                        if (row.Cells["Basic"].Value.ToString() == DisplayOutput.leaveVariable[cmbTableSelect.SelectedIndex - 1])
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                            pivotRow = row.Index;
                            break;
                        }
                    }
                    if (pivotRow != -1 && pivotColumn != -1)
                    {
                        dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
                    }
                }
                else if (DisplayOutput.state == 3 && cmbTableSelect.SelectedIndex == cmbOldCount - 2) //topup
                {
                    int pivotColumn = -1;
                    int pivotRow = -1;
                    foreach (DataGridViewColumn col in dgvOutputTable.Columns)
                    {
                        if (col.Name == firstSolEnter)
                        {
                            col.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                            pivotColumn = col.Index;
                            break;
                        }
                    }
                    foreach (DataGridViewRow row in dgvOutputTable.Rows)
                    {
                        if (row.Cells["Basic"].Value.ToString() == firstSolLeave)
                        {
                            row.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                            pivotRow = row.Index;
                            break;
                        }
                    }
                    if (pivotRow != -1 && pivotColumn != -1)
                    {
                        dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
                    }
                }

                //to handle new table for multiple (top-up handling)
                if (DisplayOutput.state == 3 && cmbTableSelect.SelectedIndex >= cmbOldCount)
                {
                    if (dgvOutputTable.Columns["enter"] != null)
                    {
                        dgvOutputTable.Columns["enter"].Visible = false;
                    }
                    if (dgvOutputTable.Columns["leave"] != null)
                    {
                        dgvOutputTable.Columns["leave"].Visible = false;
                        int pivotColumn = -1;
                        int pivotRow = -1;
                        foreach (DataGridViewColumn col in dgvOutputTable.Columns)
                        {
                            if (col.Name == dgvOutputTable.Rows[0].Cells["enter"].Value.ToString())
                            {
                                col.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                                pivotColumn = col.Index;
                                break;
                            }
                        }
                        foreach (DataGridViewRow row in dgvOutputTable.Rows)
                        {
                            if (row.Cells["Basic"].Value.ToString() == dgvOutputTable.Rows[0].Cells["leave"].Value.ToString())
                            {
                                row.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                                pivotRow = row.Index;
                                break;
                            }
                        }
                        if (pivotRow != -1 && pivotColumn != -1)
                        {
                            dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
                        }
                    }
                }
            }
        }

        private void TableWindowForm_Shown(object sender, EventArgs e)
        {
            //initialize the form appearance
            HighlightOutputTable();
            this.dgvOutputTable.ClearSelection();    
            if (width > 0 && xPos >= 0 && yPos >= 0 && height > 0)
            {
                this.Width = width;
                this.Height = height;
                this.Location = new Point(xPos, yPos);
            }
        }

        private void btnCloseTableAll_Click(object sender, EventArgs e)
        {
            Kill = true; //by setting Kill to true will kill all instances of TableWindowForm
        }

    }
}
