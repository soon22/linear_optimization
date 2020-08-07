using System;
using System.Collections;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace LinearOptimization
{
    public partial class Main : Form
    {
        private static int numConstraint = 0;
        private static int numVariable = 0;

        public ArrayList constraintBox = new ArrayList(); //list that stores all controls of textboxes in objective tlp
        public ArrayList objectiveBox = new ArrayList(); //list that stores all controls of texetboxes in subjectTo tlp
        private string previousVariableInput = string.Empty;
        private string previousConfigurationInput = string.Empty;
        public double[][] userInputArray;

        private Color enteringColor;
        private Color leavingColor;
        private Color pivotColor;

        public static SimplexAlgo DisplayOutput;
        private Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

        public int[] radSelected; //selection of variable form; 1 = urs 2 = positive 3 = negative
        private int solveOrDual = 0; // 1= solve 2=dual

        public static DataTable Outputtable;
        public static DataTable Standard;

        private int cmbOldCount; //backward compatibilty for non-multiple code
        private int[] stepsToEachSol;

        private string firstSolEnter = string.Empty;
        private string firstSolLeave = string.Empty;
        private DataTable allSolutions = new DataTable();

        private int newWindowWidth = 0;
        private int newWindowHeight = 0;

        #region YL
        public Main()
        {
            InitializeComponent();
            try
            {
                //read the color settings from appConfig and apply them
                btnColorEntering.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                btnColorLeaving.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                btnColorPivot.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
            }
            catch (Exception ex)
            {   //if the color settings aren't found, reset the settings
                MessageBox.Show("Error in color settings. Auto revert to default values.");
                config.AppSettings.Settings.Remove("enteringColor");
                config.AppSettings.Settings.Remove("leavingColor");
                config.AppSettings.Settings.Remove("pivotColor");
                config.AppSettings.Settings.Add("enteringColor", Color.Beige.ToArgb().ToString());
                config.AppSettings.Settings.Add("leavingColor", Color.AliceBlue.ToArgb().ToString());
                config.AppSettings.Settings.Add("pivotColor", Color.Cornsilk.ToArgb().ToString());
                config.Save();
                btnColorEntering.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                btnColorLeaving.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                btnColorPivot.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            //input checking
            int numVariableTemp = 0;
            int numConstraintTemp = 0;

            bool isError = false;
            if (string.IsNullOrWhiteSpace(txtVariable.Text))
            {
                errorProviderMain.SetError(txtVariable, "Value cannot be empty.");
                isError = true;
            }
            else if (int.TryParse(txtVariable.Text, out numVariableTemp) && numVariableTemp > 0)
            {
                errorProviderMain.SetError(txtVariable, "");
            }
            else
            {
                errorProviderMain.SetError(txtVariable, "Value must be larger than 0");
                isError = true;
            }

            if (string.IsNullOrWhiteSpace(txtConstraint.Text))
            {
                errorProviderMain.SetError(txtConstraint, "Value cannot be empty.");
                isError = true;
            }
            else if (int.TryParse(txtConstraint.Text, out numConstraintTemp) && numConstraintTemp > 0)
            {
                errorProviderMain.SetError(txtConstraint, "");
            }
            else
            {
                errorProviderMain.SetError(txtConstraint, "Value must be larger than 0");
                isError = true;
            }

            if (isError) return;

            //make a new form
            VariableForm.InitializeVariableForm(numVariableTemp);

            //if the user presses OK , else cancel the generate
            if (VariableForm.OK_Pressed)
            {
                dgvOutputTable.DataSource = null;
                cmbTableSelect.Items.Clear();
                lblStatus_Output.Text = "";
                lblEnteringVar.Text = "";
                lblLeavingVar.Text = "";
                numVariable = numVariableTemp;
                numConstraint = numConstraintTemp;
                VariableForm.OK_Pressed = false;
                VariableForm.StoreGeneratedForm();
                solveOrDual = 0;
                btnSolve.InvokeIfRequired(() =>
                {
                    btnSolve.Enabled = true;
                });
                btnClear.InvokeIfRequired(() =>
                {
                    btnClear.Enabled = true;
                });
                btnBoundary.InvokeIfRequired(() =>
                {
                    btnBoundary.Enabled = true;
                });
                btnConvert.InvokeIfRequired(() =>
                {
                    btnConvert.Enabled = true;
                });

                cmbMinMax.SelectedIndex = 0;
                constraintBox.Clear();
                objectiveBox.Clear();
                tlpObjective.Controls.Clear();
                tlpConstraint.Controls.Clear();
                dgvAlternateInput.Rows.Clear();
                //suspend layout to prevent unneccessary rendering when being updated
                SplashForm.ShowSplashScreen();
                tlpConstraint.SuspendLayout();
                tlpObjective.SuspendLayout();
                AddVariableColumn();
                if (numVariable * numConstraint <= 400) //if too many controls, switch a method to display the input fields. pretty machine dependants.
                {
                    pnlConstraint.Visible = true;
                    dgvAlternateInput.Visible = false;
                    AddConstraintColumn();
                }
                else
                {
                    //alternate method to generate input field; winform couldn't handle too many controls
                    pnlConstraint.Visible = false;
                    dgvAlternateInput.Visible = true;
                    generateInputDGV();

                }
                SplashForm.CloseForm();
                tlpConstraint.ResumeLayout();
                tlpObjective.ResumeLayout();
                Dock_grpObjective();
            }
        }

        private void Dock_grpObjective() //to adjust the groupbox size
        {
            if (pnlObjective.HorizontalScroll.Visible)
            {
                grpObjective.Dock = DockStyle.Fill;
            }
            else
            {
                grpObjective.Dock = DockStyle.None;
                grpObjective.Height = 50;
            }
        }

        private void generateInputDGV() //alternate method to generate the input fields as DGV
        {
            dgvAlternateInput.ColumnCount = numVariable + 2;
            dgvAlternateInput.RowCount = numConstraint;
            dgvAlternateInput.Columns[0].Name = "Constraint";
            dgvAlternateInput.Columns[0].ReadOnly = true;
            dgvAlternateInput.Columns[0].SortMode = DataGridViewColumnSortMode.NotSortable;
            for (int x = 1; x < dgvAlternateInput.ColumnCount; x++)
            {
                if (x != dgvAlternateInput.ColumnCount - 1)
                {
                    dgvAlternateInput.Columns[x].Name = "X" + x.ToString();
                }
                else
                {
                    dgvAlternateInput.Columns[x].Name = "<=";
                }
                dgvAlternateInput.Columns[x].SortMode = DataGridViewColumnSortMode.NotSortable;
                dgvAlternateInput.Columns[x].Width = 50;
            }
            for (int x = 0; x < dgvAlternateInput.RowCount; x++)
            {
                dgvAlternateInput["Constraint", x].Value = "Constraint " + (x + 1).ToString();
            }
        }

        private void AddConstraintColumn() //add textboxes and labels to subjectTo tlp
        {
            tlpConstraint.ColumnCount = 1 + 2 * numVariable + 2;
            tlpConstraint.RowCount = 1 + numConstraint;
            for (int x = 0; x < numConstraint; x++)
            {
                Label lbl1 = new Label();
                lbl1.Text = "Constraint " + (x + 1).ToString();
                lbl1.Dock = DockStyle.Fill;
                lbl1.Width = 115;
                lbl1.TextAlign = ContentAlignment.MiddleCenter;
                tlpConstraint.Controls.Add(lbl1, 0, x);

                for (int y = 0; y < numVariable; y++)
                {
                    TextBox txt = new TextBox();
                    txt.Dock = DockStyle.Fill;
                    txt.Width = 30;
                    tlpConstraint.Controls.Add(txt, 2 * y + 1, x);
                    txt.KeyPress += txtVariable_KeyPress;
                    constraintBox.Add(txt);

                    var temp = (y + 1).ToString();
                    var tempString = String.Empty;
                    foreach (char c in temp)
                    {
                        char cOut = (char)(c - '0' + 0x2080);
                        tempString += cOut;
                    }
                    Label lbl2 = new Label();
                    lbl2.Width = 50;
                    if (y != numVariable - 1)
                    {
                        lbl2.Text = "x" + tempString + " +";
                    }
                    else
                    {
                        lbl2.Text = "x" + tempString + " <=";
                        lbl2.Width = 55;
                    }
                    lbl2.Font = new Font("Calibri", 12, FontStyle.Regular);
                    lbl2.Dock = DockStyle.Fill;
                    lbl2.TextAlign = ContentAlignment.MiddleCenter;
                    tlpConstraint.Controls.Add(lbl2, 2 * y + 2, x);
                }
                TextBox txt1 = new TextBox();
                txt1.Dock = DockStyle.Fill;
                txt1.Width = 30;
                constraintBox.Add(txt1);
                txt1.KeyPress += txtVariable_KeyPress;
                tlpConstraint.Controls.Add(txt1, tlpConstraint.ColumnCount - 1, x);
            }
        }

        private void AddVariableColumn() // add textboxes and labels to objective tlp
        {
            tlpObjective.ColumnCount = 2 + 2 * numVariable;
            tlpObjective.Controls.Add(cmbMinMax, 0, 0);
            tlpObjective.Controls.Add(lblZ, 1, 0);
            cmbMinMax.Visible = true;
            lblZ.Visible = true;
            for (int x = 0; x < numVariable; x++)
            {
                TextBox txt = new TextBox();
                txt.Dock = DockStyle.Fill;
                txt.Width = 30;
                tlpObjective.Controls.Add(txt, 2 * x + 2, 0);
                txt.KeyPress += txtVariable_KeyPress;
                objectiveBox.Add(txt);

                Label lbl1 = new Label();
                var temp = (x + 1).ToString();
                var tempString = String.Empty;
                foreach (char c in temp)
                {
                    char cOut = (char)(c - '0' + 0x2080);
                    tempString += cOut;
                }
                if (x != numVariable - 1)
                {
                    lbl1.Text = "x" + tempString + " +";
                }
                else
                {
                    lbl1.Text = "x" + tempString;
                }
                lbl1.Font = new Font("Calibri", 12, FontStyle.Regular);
                lbl1.Dock = DockStyle.Fill;
                lbl1.Width = 50;
                lbl1.TextAlign = ContentAlignment.TopCenter;
                tlpObjective.Controls.Add(lbl1, 2 * x + 3, 0);
            }
        }

        private void btnSolve_Click(object sender, EventArgs e)
        {
            VariableForm.checkRadioBtn();
            radSelected = VariableForm.radSelected;
            if (convertBoxesToArray()) //if the inputs are all valid
            {
                solveOrDual = 1;
                DisplayOutput = new SimplexAlgo(userInputArray, numVariable, numConstraint, radSelected, cmbMinMax.SelectedIndex);
                cmbTableSelect.Items.Clear();
                cmbTableSelect.Items.Add("Standard LP Form");
                //matching the outputs to combobox entries
                if (DisplayOutput.state == 1 || DisplayOutput.state == 2 || DisplayOutput.state == 3)
                {
                    int x;
                    for (x = 0; x < DisplayOutput.outputTableau.Count; x++)
                    {
                        cmbTableSelect.Items.Add(String.Concat("Iteration ", x));               
                    }

                    if (DisplayOutput.state == 1)
                    {
                        lblStatus_Output.Text = "Optimal Solution has been obtained.";
                        lblStatus_Output.ForeColor = Color.Green;
                        cmbTableSelect.Items.Add("Solution");
                    }
                    else if (DisplayOutput.state == 3)
                    {
                        lblStatus_Output.Text = "This problem has multiple solutions.";
                        lblStatus_Output.ForeColor = Color.Blue;
                        cmbTableSelect.Items.Add("Solution");
                        cmbOldCount = cmbTableSelect.Items.Count;
                        int solCounter = 2;

                        //if extra solutions could be obtained
                        if (DisplayOutput.mulList != null && DisplayOutput.mulList.Count > 0 && DisplayOutput.mulSolution != null && DisplayOutput.mulSolution.Count > 0)
                        {
                            firstSolEnter = DisplayOutput.mulList[0][0].Rows[0]["enter"].ToString();
                            firstSolLeave = DisplayOutput.mulList[0][0].Rows[0]["leave"].ToString();

                            //reformatting the output tables from the class; mainly remove duplicate tables
                            for(int z = 0; z < DisplayOutput.mulList.Count; z++)
                            {
                                List<DataTable> temp = new List<DataTable>();

                                if(z + 1 != DisplayOutput.mulList.Count)
                                {
                                    DisplayOutput.mulList[z][DisplayOutput.mulList[z].Count - 1].Rows[0]["enter"] = DisplayOutput.mulList[z + 1][0].Rows[0]["enter"].ToString();
                                    DisplayOutput.mulList[z][DisplayOutput.mulList[z].Count - 1].Rows[0]["leave"] = DisplayOutput.mulList[z + 1][0].Rows[0]["leave"].ToString();
                                    DisplayOutput.mulList[z + 1][0].Rows[0]["enter"] = null;
                                    DisplayOutput.mulList[z + 1][0].Rows[0]["leave"] = null;
                                }
                                for(int y = 1; y < DisplayOutput.mulList[z].Count; y++)
                                {
                                    temp.Add(DisplayOutput.mulList[z][y]);
                                }
                                DisplayOutput.mulList[z] = temp;
                            }
                            stepsToEachSol = new int[DisplayOutput.mulSolution.Count];
                            int stepsCounter = 0;
                            foreach (var subList in DisplayOutput.mulList)
                            {
                                stepsToEachSol[stepsCounter] = subList.Count + 1;                                
                                foreach (DataTable dtStep in subList)
                                {
                                    cmbTableSelect.Items.Add(String.Concat("Iteration ", x));
                                    x++;
                                }
                                cmbTableSelect.Items.Add(String.Concat("Solution ", solCounter));
                                stepsCounter++;
                                solCounter++;
                            }
                            //create a new table consisting of all solutions table
                            allSolutions = DisplayOutput.Solution.Copy();
                            DataColumn dtCol = new DataColumn("No.", System.Type.GetType("System.Int32"));
                            dtCol.AutoIncrement = true;
                            dtCol.AutoIncrementSeed = 1;
                            dtCol.AutoIncrementStep = 1;
                            allSolutions.Columns.Add(dtCol);
                            allSolutions.Rows[0]["No."] = 1;

                            foreach (DataTable dtSol in DisplayOutput.mulSolution)
                            {
                                string[] temp = new string[allSolutions.Columns.Count];
                                for (int y = 0; y < allSolutions.Columns.Count - 1; y++)
                                {
                                    temp[y] = dtSol.Rows[0][y].ToString();
                                }
                                allSolutions.Rows.Add(temp);
                            }
                            
                            allSolutions.Columns["No."].SetOrdinal(0);
                            cmbTableSelect.Items.Add("All Solutions");
                        }
                    }
                    else
                    {
                        lblStatus_Output.Text = "This problem is unbounded. No solution is produced.";
                        lblStatus_Output.ForeColor = Color.Red;
                    }
                }
                else if (DisplayOutput.state == 4)
                {
                    lblStatus_Output.Text = "This problem has Negative value on its RHS. No solution is produced.";
                    lblStatus_Output.ForeColor = Color.Red;
                }
                else if (DisplayOutput.state == 5)
                {
                    for (int x = 0; x < DisplayOutput.outputTableau.Count; x++)
                    {
                        cmbTableSelect.Items.Add(String.Concat("Iteration ", x));
                    }
                    lblStatus_Output.Text = "Cycling is encountered. The current tableau is same as iteration "+ DisplayOutput.cycledTableauNo.ToString() + ".";                   
                    lblStatus_Output.ForeColor = Color.Red;
                }
                //cmbOldCount is used so that the old codes won't be affected by the newly added mulitple solutions
                if(DisplayOutput.state != 3)
                {
                    cmbOldCount = cmbTableSelect.Items.Count;
                }
                //run the fraction converting process on another thread
                if (chkFraction.Checked)
                {
                    Task convertTask = Task.Run(() => { convertRealToFractionDT(); });
                }
                tbLeft.SelectedIndex = 1;
                //estimate the window size that the dgv needs (for add window function)
                cmbTableSelect.SelectedIndex = 0;
                newWindowWidth = dgvOutputTable.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 50;
                newWindowHeight = dgvOutputTable.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + dgvOutputTable.ColumnHeadersHeight + 105;
                cmbTableSelect.SelectedIndex = cmbTableSelect.Items.Count - 1;
            }
        }

        private void convertRealToFractionDT()
        {
            //convert all tables cell values to fraction
            if (solveOrDual == 1)
            {
                if (allSolutions != null && allSolutions.Rows.Count > 0)
                {
                    DataTable temp = dataTableConvertRealToFraction(allSolutions);
                    allSolutions = temp.Copy();
                }

                if (DisplayOutput.Solution != null && DisplayOutput.Solution.Rows.Count > 0)
                {
                    DataTable temp = dataTableConvertRealToFraction(DisplayOutput.Solution);
                    DisplayOutput.Solution = temp.Copy();
                }

                if (DisplayOutput.standardLP != null && DisplayOutput.standardLP.Rows.Count > 0)
                {
                    DataTable temp = dataTableConvertRealToFraction(DisplayOutput.standardLP);
                    DisplayOutput.standardLP = temp.Copy();
                }

                for (int x = 0; x < DisplayOutput.outputTableau.Count; x++)
                {
                    if (DisplayOutput.outputTableau[x] != null && DisplayOutput.outputTableau[x].Rows.Count > 0)
                    {
                        DataTable temp = dataTableConvertRealToFraction(DisplayOutput.outputTableau[x]);
                        DisplayOutput.outputTableau[x] = temp.Copy();
                    }
                }

                foreach (List<DataTable> dtList in DisplayOutput.mulList)
                {
                    for (int x = 0; x < dtList.Count; x++)
                    {
                        if (dtList[x] != null && dtList[x].Rows.Count > 0)
                        {
                            DataTable temp = dataTableConvertRealToFraction(dtList[x]);
                            dtList[x] = temp.Copy();
                        }
                    }
                }

                if (DisplayOutput.mulSolution != null)
                {
                    for (int x = 0; x < DisplayOutput.mulSolution.Count; x++)
                    {
                        if (DisplayOutput.mulSolution[x] != null && DisplayOutput.mulSolution[x].Rows.Count > 0)
                        {
                            DataTable temp = dataTableConvertRealToFraction(DisplayOutput.mulSolution[x]);
                            DisplayOutput.mulSolution[x] = temp.Copy();
                        }
                    }
                }
            }
            else if(solveOrDual == 2)
            {
                if (Outputtable != null && Outputtable.Rows.Count > 0)
                {
                    DataTable temp = dataTableConvertRealToFraction(Outputtable);
                    Outputtable = temp.Copy();
                }

                if (Standard != null && Standard.Rows.Count > 0)
                {
                    DataTable temp = dataTableConvertRealToFraction(Standard);
                    Standard = temp.Copy();
                }
            }

        }
        //feed in any datatable will get a new datatable with all its double converted to fraction
        private DataTable dataTableConvertRealToFraction(DataTable dt)
        {
            //get the list of columnNames from dt
            string[] columnNames = (from dc in dt.Columns.Cast<DataColumn>()
                                    select dc.ColumnName).ToArray();
            DataTable temp = new DataTable();
            temp.TableName = dt.TableName;

            //create exact columnname columns in DataTable temp, but in string type so that it can store fraction
            foreach (string name in columnNames)
            {
                temp.Columns.Add(name, System.Type.GetType("System.String"));
            }

            //port everything from dt to temp; if column datatype is double then convert to fraction, else just copy
            foreach (DataRow dr in dt.Rows)
            {
                string[] rowData = new string[columnNames.Length];
                int counter = 0;
                foreach (DataColumn dc in dt.Columns)
                {
                    if (dc.DataType == System.Type.GetType("System.Double") && dr[dc] != null && !String.IsNullOrWhiteSpace(dr[dc].ToString()))
                    {
                        Fraction tempOut = RealToFraction((double)dr[dc], 0.001);
                        if (tempOut.D != 1)
                        {
                            rowData[counter] = tempOut.N.ToString() + "/" + tempOut.D.ToString();
                        }
                        else
                        {
                            rowData[counter] = ((double)dr[dc]).ToString();
                        }
                    }
                    else
                    {
                        rowData[counter] = dr[dc].ToString();
                    }
                    counter++;
                }
                temp.Rows.Add(rowData);
            }

            return temp;
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            tbLeft.SelectedIndex = 0;
        }

        private void txtVariable_KeyPress(object sender, KeyPressEventArgs e) //verifying keypress event for objective and SubjectTo textboxes
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != '-') && (e.KeyChar != '/'))
            {
                e.Handled = true;
            }

            // only allow one decimal point
            if ((e.KeyChar == '.') && ((sender as TextBox).Text.IndexOf('.') > -1))
            {
                e.Handled = true;
            }
            //same for '-'
            if ((e.KeyChar == '-') && ((sender as TextBox).Text.IndexOf('-') > -1))
            {
                e.Handled = true;
            }
            //same for '/'
            if ((e.KeyChar == '/') && ((sender as TextBox).Text.IndexOf('/') > -1))
            {
                e.Handled = true;
            }
            previousVariableInput = (sender as TextBox).Text;
        }

        private void txtConfiguration_KeyPress(object sender, KeyPressEventArgs e) //input verifying for numVariables and numConstarint
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void txtConfiguration_TextChanged(object sender, EventArgs e) //input verifying for numVariables and numConstarint
        {
            int temp = new int();
            if (!int.TryParse((sender as TextBox).Text, out temp))
            {
                (sender as TextBox).Text = previousConfigurationInput;
            }
        }

        private bool convertBoxesToArray() //convert ArrayList Boxes in txt controls to jagged array, as well as input checking
        {
            double[][] output = new double[numConstraint + 1][];

            int xCount = 0;
            int yCount = 0;
            bool isZero = true;
            foreach (TextBox txt in objectiveBox)
            {
                if (output[xCount] == null)
                {
                    output[xCount] = new double[numVariable];
                }

                try
                {
                    if (txt.Text.Contains("/")) //perform fraction to decimal conversion
                    {
                        string[] num = txt.Text.Split('/');
                        output[xCount][yCount] = double.Parse(num[0]) / double.Parse(num[1]);
                        if (double.IsInfinity(output[xCount][yCount])) //reject divideByZero
                        {
                            throw new DivideByZeroException();
                        }
                    }
                    else
                    {
                        output[xCount][yCount] = double.Parse(txt.Text);
                    }
                    isZero = false;
                    errorProviderMain.SetError(txt, "");
                }
                catch (Exception ex)
                {
                    if (string.IsNullOrEmpty(txt.Text)) //assume empty input as 0
                    {
                        output[xCount][yCount] = 0;
                        errorProviderMain.SetError(txt, "");
                    }
                    else
                    {
                        errorProviderMain.SetError(txt, "Invalid Input");
                        MessageBox.Show("Invalid Input in Objective");
                        return false;
                    }
                }
                yCount++;
            }

            if(isZero) //reject Z=0
            {
                foreach (TextBox txt in objectiveBox)
                {
                    errorProviderMain.SetError(txt, "Z cannot be equal to 0");
                }
                MessageBox.Show("Z cannot be equal to 0 for Objective");
                return false;
            }

            if (numVariable * numConstraint <= 400) //display method 1
            {
                xCount = 1;
                yCount = 0;
                foreach (TextBox txt in constraintBox)
                {
                    if (output[xCount] == null)
                    {
                        output[xCount] = new double[numVariable + 1];
                    }

                    try
                    {
                        if (txt.Text.Contains("/")) //perform fraction to decimal conversion
                        {
                            string[] num = txt.Text.Split('/');
                            output[xCount][yCount] = double.Parse(num[0]) / double.Parse(num[1]);
                            if(double.IsInfinity(output[xCount][yCount]))
                            {
                                throw new DivideByZeroException();
                            }
                        }
                        else
                        {
                            output[xCount][yCount] = double.Parse(txt.Text);
                        }
                        errorProviderMain.SetError(txt, "");
                    }
                    catch (Exception ex)
                    {
                        if (string.IsNullOrEmpty(txt.Text)) //assume empty input as 0
                        {
                            output[xCount][yCount] = 0;
                            errorProviderMain.SetError(txt, "");
                        }
                        else
                        {
                            errorProviderMain.SetError(txt, "Invalid Input");
                            MessageBox.Show("Invalid Input in Constraints");
                            return false;
                        }
                    }
                    yCount++;
                    if (yCount == numVariable + 1)
                    {
                        xCount++;
                        yCount = 0;
                    }
                }
            }
            else //display method 2, different way to retrieve the values
            {
                for (int x = 0; x < numConstraint; x++)
                {
                    for (int y = 1; y < dgvAlternateInput.ColumnCount; y++)
                    {
                        dgvAlternateInput[y, x].ErrorText = "";
                        if (output[x + 1] == null) //row 0 has been filled by objective, so offset by 1
                        {
                            output[x + 1] = new double[numVariable + 1];
                        }

                        try
                        {
                            if (dgvAlternateInput[y, x].Value.ToString().Contains("/")) //perform fraction to decimal conversion
                            {
                                string[] num = dgvAlternateInput[y, x].Value.ToString().Split('/');
                                output[x + 1][y - 1] = double.Parse(num[0]) / double.Parse(num[1]);
                                if (double.IsInfinity(output[x + 1][y - 1]))
                                {
                                    throw new DivideByZeroException();
                                }
                            }
                            else
                            {
                                output[x + 1][y - 1] = double.Parse(dgvAlternateInput[y, x].Value.ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            if (dgvAlternateInput[y, x].Value == null) //assume empty input as 0
                            {
                                output[x + 1][y - 1] = 0;
                            }
                            else
                            {
                                dgvAlternateInput[y, x].ErrorText = "Invalid Input";
                                MessageBox.Show("Invalid Input in Constraints");
                                return false;
                            }
                        }
                    }
                }
            }
            userInputArray = output;
            return true;
        }

        private void Main_FormClosed(object sender, FormClosedEventArgs e)
        {
            VariableForm.DiscardVariableForm();
        }

        private void btnBoundary_Click(object sender, EventArgs e)
        {
            VariableForm.ShowVariableForm();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            //clear all errors as well as inputs
            foreach (TextBox txt in objectiveBox)
            {
                errorProviderMain.SetError(txt, "");
                txt.Text = string.Empty;
            }

            foreach (TextBox txt in constraintBox)
            {
                errorProviderMain.SetError(txt, "");
                txt.Text = string.Empty;
            }
        }

        private void tbLeft_DrawItem(object sender, DrawItemEventArgs e)
        {   //*****adapted from MSDN********
            Graphics g = e.Graphics;
            Brush _textBrush;

            // Get the item from the collection.
            TabPage _tabPage = tbLeft.TabPages[e.Index];

            // Get the real bounds for the tab rectangle.
            Rectangle _tabBounds = tbLeft.GetTabRect(e.Index);

            if (e.State == DrawItemState.Selected)
            {

                // Draw a different background color, and don't paint a focus rectangle.
                _textBrush = new SolidBrush(Color.DeepSkyBlue);
                g.FillRectangle(Brushes.LemonChiffon, e.Bounds);
            }
            else
            {
                _textBrush = new System.Drawing.SolidBrush(e.ForeColor);
                g.FillRectangle(Brushes.Transparent, e.Bounds);
            }

            // Use our own font.
            Font _tabFont = new Font("Arial", (float)16.0, FontStyle.Bold, GraphicsUnit.Pixel);

            // Draw string. Center the text.
            StringFormat _stringFlags = new StringFormat();
            _stringFlags.Alignment = StringAlignment.Center;
            _stringFlags.LineAlignment = StringAlignment.Center;
            g.DrawString(_tabPage.Text, _tabFont, _textBrush, _tabBounds, new StringFormat(_stringFlags));
        }


        private void btnColorEntering_Click(object sender, EventArgs e)
        {
            ColorDialog cdEntering = new ColorDialog();
            cdEntering.AllowFullOpen = true;
            cdEntering.Color = btnColorEntering.BackColor;

            // Update the text box color if the user clicks OK 
            if (cdEntering.ShowDialog() == DialogResult.OK)
            {
                enteringColor = cdEntering.Color;
                config.AppSettings.Settings["enteringColor"].Value = enteringColor.ToArgb().ToString();
                config.Save();
                HighlightOutputTable();
                btnColorEntering.BackColor = enteringColor;
            }
        }

        private void btnColorLeaving_Click(object sender, EventArgs e)
        {
            ColorDialog cdLeaving = new ColorDialog();
            cdLeaving.AllowFullOpen = true;
            cdLeaving.Color = btnColorLeaving.BackColor;

            // Update the text box color if the user clicks OK 
            if (cdLeaving.ShowDialog() == DialogResult.OK)
            {
                leavingColor = cdLeaving.Color;
                config.AppSettings.Settings["leavingColor"].Value = leavingColor.ToArgb().ToString();
                config.Save();
                HighlightOutputTable();
                btnColorLeaving.BackColor = leavingColor;
            }
        }

        private void btnColorPivot_Click(object sender, EventArgs e)
        {
            ColorDialog cdPivot = new ColorDialog();
            cdPivot.AllowFullOpen = true;
            cdPivot.Color = btnColorPivot.BackColor;

            // Update the text box color if the user clicks OK 
            if (cdPivot.ShowDialog() == DialogResult.OK)
            {
                pivotColor = cdPivot.Color;
                config.AppSettings.Settings["pivotColor"].Value = pivotColor.ToArgb().ToString();
                config.Save();
                HighlightOutputTable();
                btnColorPivot.BackColor = pivotColor;
            }
        }

        private void HighlightOutputTable()
        {
            if (dgvOutputTable.DataSource != null && solveOrDual == 1) //if dgv isn't empty and is normal solving; since dual doesn't need highlight
            {
                //for all without new multiple solutions (old handling) // due to lack of enter and leave columns
                if ((cmbTableSelect.SelectedIndex < cmbOldCount - 2 && cmbTableSelect.SelectedIndex > 0) || (cmbTableSelect.SelectedIndex < cmbOldCount - 1 && cmbTableSelect.SelectedIndex > 0 && (DisplayOutput.state == 2 || DisplayOutput.state == 5)))
                {
                    int pivotColumn = -1;
                    int pivotRow = -1;
                    foreach (DataGridViewColumn col in dgvOutputTable.Columns)
                    {
                        if (col.Name == DisplayOutput.enterVariable[cmbTableSelect.SelectedIndex - 1])
                        {
                            lblEnteringVar.Text = DisplayOutput.enterVariable[cmbTableSelect.SelectedIndex - 1];
                            if (chkEnteringHL.Checked)
                            {
                                col.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                            }
                            else
                            {
                                col.DefaultCellStyle.BackColor = Color.White;
                            }
                            pivotColumn = col.Index;
                            break;
                        }
                    }
                    foreach (DataGridViewRow row in dgvOutputTable.Rows)
                    {
                        if (row.Cells["Basic"].Value.ToString() == DisplayOutput.leaveVariable[cmbTableSelect.SelectedIndex - 1])
                        {
                            lblLeavingVar.Text = DisplayOutput.leaveVariable[cmbTableSelect.SelectedIndex - 1];
                            if (chkLeavingHL.Checked)
                            {
                                row.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                            }
                            else
                            {
                                row.DefaultCellStyle.BackColor = Color.White;
                            }
                            pivotRow = row.Index;
                            break;
                        }
                    }
                    if (pivotRow != -1 && pivotColumn != -1)
                    {
                        if (chkPivotHL.Checked)
                        {
                            dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
                        }
                        else
                        {
                            dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.White;
                        }
                    }
                }
                else if (DisplayOutput.state == 3 && cmbTableSelect.SelectedIndex == cmbOldCount - 2) //topup //to handle new reformatted output
                {
                    int pivotColumn = -1;
                    int pivotRow = -1;
                    foreach (DataGridViewColumn col in dgvOutputTable.Columns)
                    {
                        if (col.Name == firstSolEnter)
                        {
                            lblEnteringVar.Text = firstSolEnter;
                            if (chkEnteringHL.Checked)
                            {
                                col.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                            }
                            else
                            {
                                col.DefaultCellStyle.BackColor = Color.White;
                            }
                            pivotColumn = col.Index;
                            break;
                        }
                    }
                    foreach (DataGridViewRow row in dgvOutputTable.Rows)
                    {
                        if (row.Cells["Basic"].Value.ToString() == firstSolLeave)
                        {
                            lblLeavingVar.Text = firstSolLeave;
                            if (chkLeavingHL.Checked)
                            {
                                row.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                            }
                            else
                            {
                                row.DefaultCellStyle.BackColor = Color.White;
                            }
                            pivotRow = row.Index;
                            break;
                        }
                    }
                    if (pivotRow != -1 && pivotColumn != -1)
                    {
                        if (chkPivotHL.Checked)
                        {
                            dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
                        }
                        else
                        {
                            dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.White;
                        }
                    }
                }

                //to handle new table for multiple (top-up handling)
                if (DisplayOutput.state == 3 && cmbTableSelect.SelectedIndex >= cmbOldCount)
                {
                    if (dgvOutputTable.Columns["enter"] != null)
                    {
                        lblEnteringVar.Text = dgvOutputTable.Rows[0].Cells["enter"].Value.ToString();
                        dgvOutputTable.Columns["enter"].Visible = false;
                    }
                    if (dgvOutputTable.Columns["leave"] != null)
                    {
                        lblLeavingVar.Text = dgvOutputTable.Rows[0].Cells["leave"].Value.ToString();
                        dgvOutputTable.Columns["leave"].Visible = false;
                        int pivotColumn = -1;
                        int pivotRow = -1;
                        foreach (DataGridViewColumn col in dgvOutputTable.Columns)
                        {
                            if (col.Name == lblEnteringVar.Text)
                            {
                                if (chkEnteringHL.Checked)
                                {
                                    col.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["enteringColor"].Value));
                                }
                                else
                                {
                                    col.DefaultCellStyle.BackColor = Color.White;
                                }
                                pivotColumn = col.Index;
                                break;
                            }
                        }
                        foreach (DataGridViewRow row in dgvOutputTable.Rows)
                        {
                            if (row.Cells["Basic"].Value.ToString() == lblLeavingVar.Text)
                            {
                                if (chkLeavingHL.Checked)
                                {
                                    row.DefaultCellStyle.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["leavingColor"].Value));
                                }
                                else
                                {
                                    row.DefaultCellStyle.BackColor = Color.White;
                                }
                                pivotRow = row.Index;
                                break;
                            }
                        }
                        if (pivotRow != -1 && pivotColumn != -1)
                        {
                            if (chkPivotHL.Checked)
                            {
                                dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.FromArgb(Int32.Parse(config.AppSettings.Settings["pivotColor"].Value));
                            }
                            else
                            {
                                dgvOutputTable[pivotColumn, pivotRow].Style.BackColor = Color.White;
                            }
                        }
                    }
                }
            }
        }

        private void dgvOutputTable_DataSourceChanged(object sender, EventArgs e)
        {
            //Show/Hide the labels based on dgv is empty or not
            if (dgvOutputTable.DataSource == null)
            {
                lblEnteringFix.InvokeIfRequired(() =>
                {
                    lblEnteringFix.Visible = false;
                });
                lblLeavingFix.InvokeIfRequired(() =>
                {
                    lblLeavingFix.Visible = false;
                });
            }
            else
            {
                lblEnteringFix.InvokeIfRequired(() =>
                {
                    lblEnteringFix.Visible = true;
                });
                lblLeavingFix.InvokeIfRequired(() =>
                {
                    lblLeavingFix.Visible = true;
                });
            }
        }

        private void chkEnteringHL_CheckedChanged(object sender, EventArgs e)
        {
            dgvOutputTable.Refresh();
            HighlightOutputTable();
        }

        private void chkLeavingHL_CheckedChanged(object sender, EventArgs e)
        {
            dgvOutputTable.Refresh();
            HighlightOutputTable();
        }

        private void chkPivotHL_CheckedChanged(object sender, EventArgs e)
        {
            dgvOutputTable.Refresh();
            HighlightOutputTable();
        }

        private void btnAddWindow_Click(object sender, EventArgs e)
        {
            //Kill is a variable to determine whether the form should kill itsef or not
            TableWindowForm.Kill = false;
            TableWindowForm.InitializeTableForm(cmbTableSelect.Items, cmbTableSelect.SelectedIndex, solveOrDual, stepsToEachSol, cmbOldCount, allSolutions, firstSolEnter, firstSolLeave, -1, -1, -1, -1);
        }

        private void btnListAll_Click(object sender, EventArgs e)
        {
            TableWindowForm.Kill = false;
            int width = newWindowWidth;
            int height = newWindowHeight;
            Rectangle resolution = Screen.PrimaryScreen.Bounds;
            int numOfWindowsHorizontal = ((resolution.Width - 10) / width); //calculate how many windows can be fitted into the user screen
            int numOfWindowsVertical = ((resolution.Height - 10) / height);
            int numOfTables = cmbTableSelect.Items.Count;
            int counter = 0;
            int xPos = 0;
            int yPos = 0;

            for (int x = 0; x < numOfWindowsHorizontal; x++)
            {
                for(int y = 0; y < numOfWindowsVertical; y++)
                {
                    xPos = 10+x * width; //10 as margin
                    yPos = 10+y * height;
                    TableWindowForm.InitializeTableForm(cmbTableSelect.Items, counter, solveOrDual, stepsToEachSol, cmbOldCount, allSolutions, firstSolEnter, firstSolLeave, width, height, xPos, yPos);
                    counter++;
                    if(counter == numOfTables)
                    {
                        return;
                    }
                }
            }
            if(counter != numOfTables)
            {
                MessageBox.Show((numOfTables - counter).ToString() + " Table(s) cannot be displayed due to limitation in screen resolution.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, (MessageBoxOptions)0x40000);
                this.SendToBack();
            }
        }

        private void tbLeft_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tbLeft.SelectedIndex == 1)
            {
                if (cmbTableSelect.Items.Count < 1)
                {
                    btnAddWindow.InvokeIfRequired(() =>
                    {
                        btnAddWindow.Enabled = false;
                    });

                    btnListAll.InvokeIfRequired(() =>
                    {
                        btnListAll.Enabled = false;
                    });
                }
                else
                {
                    btnAddWindow.InvokeIfRequired(() =>
                    {
                        btnAddWindow.Enabled = true;
                    });

                    btnListAll.InvokeIfRequired(() =>
                    {
                        btnListAll.Enabled = true;
                    });
                }
            }
        }

        #endregion

        #region PCW
        private void btnConvert_Click(object sender, EventArgs e)
        {
            ArrayList inobjective = new ArrayList();
            ArrayList inconstraint = new ArrayList();
            int NOVariable = numVariable;
            int NOConstraint = numConstraint;
            int inMinOrMax = cmbMinMax.SelectedIndex;
            inobjective = objectiveBox;
            inconstraint = constraintBox;
            if (convertBoxesToArray())
            {
                VariableForm.checkRadioBtn();
                radSelected = VariableForm.radSelected;
                solveOrDual = 2;
                double[][] Inputtable;
                Inputtable = userInputArray;
                int x = 0;
                int[] Varcondition = radSelected;
                string[][] dual = new string[2][];
                int varCounter = 0;
                for (int i = 0; i < NOVariable; i++)
                {
                    if (Varcondition[i] == 1)
                    {
                        varCounter = varCounter + 2;
                    }
                    else if ((Varcondition[i] == 2) || (Varcondition[i] == 3))
                    {
                        varCounter = varCounter + 1;
                    }
                }
                double[][] HalfTable = new double[NOConstraint + 1][];
                for (int i = 0; i <= NOConstraint; i++)
                {
                    HalfTable[i] = new double[varCounter + NOConstraint + 1];
                }
                int Convert = 0;
                for (int i = 0; i < (NOVariable); i++)
                {
                    if (Varcondition[i] == 1)
                    {
                        HalfTable[0][Convert] = -1 * Inputtable[0][i];
                        HalfTable[0][Convert + 1] = Inputtable[0][i];
                        Convert = Convert + 2;
                    }
                    if (Varcondition[i] == 2)
                    {
                        HalfTable[0][Convert] = -1 * Inputtable[0][i];
                        Convert = Convert + 1;
                    }
                    if (Varcondition[i] == 3)
                    {
                        HalfTable[0][Convert] = Inputtable[0][i];
                        Convert = Convert + 1;
                    }
                }
                for (int i = varCounter; i <= (varCounter + NOConstraint); i++)
                {
                    HalfTable[0][i] = 0;
                }
                Convert = 0;
                for (int h = 1; h <= NOConstraint; h++)
                {
                    for (int i = 0; i < NOVariable; i++)
                    {
                        if (Varcondition[i] == 1)
                        {
                            HalfTable[h][Convert] = Inputtable[h][i];
                            HalfTable[h][Convert + 1] = -1 * Inputtable[h][i];
                            Convert = Convert + 2;
                        }
                        if (Varcondition[i] == 2)
                        {
                            HalfTable[h][Convert] = Inputtable[h][i];
                            Convert = Convert + 1;
                        }
                        if (Varcondition[i] == 3)
                        {
                            HalfTable[h][Convert] = -1 * Inputtable[h][i];
                            Convert = Convert + 1;
                        }
                    }
                    for (int i = varCounter; i <= (varCounter + NOConstraint); i++)
                    {
                        if (i == (varCounter + NOConstraint))
                        {
                            HalfTable[h][i] = Inputtable[h][NOVariable];
                        }
                        else
                        {
                            HalfTable[h][i] = 0;
                        }
                    }
                    Convert = 0;
                }
                int SlackCount = varCounter;
                for (int i = 1; i <= NOConstraint; i++)
                {
                    HalfTable[i][SlackCount] = 1;
                    SlackCount++;
                }
                dual[0] = new string[varCounter + NOConstraint];
                dual[1] = new string[NOConstraint];
                int arrayCount = 0;
                int xCount = 0;
                int sCount = 0;

                for (int i = 0; i < NOVariable; i++)
                {
                    if (Varcondition[i] == 1)
                    {
                        xCount++;
                        dual[0][arrayCount] = "x" + (xCount) + "⁺";
                        dual[0][arrayCount + 1] = "x" + (xCount) + "⁻";
                        arrayCount = arrayCount + 2;
                    }
                    else if (Varcondition[i] == 2)
                    {
                        xCount++;
                        dual[0][arrayCount] = "x" + (xCount);
                        arrayCount++;
                    }
                    else if (Varcondition[i] == 3)
                    {
                        xCount++;
                        dual[0][arrayCount] = "x" + (xCount) + "⁻";
                        arrayCount++;
                    }
                }

                for (int i = 0; i < NOConstraint; i++)
                {
                    sCount = i + 1;
                    dual[0][arrayCount] = "s" + (sCount);
                    arrayCount++;
                }

                sCount = 0;
                for (int i = 0; i < NOConstraint; i++)
                {
                    sCount = i + 1;
                    dual[1][i] = "s" + (sCount);
                }

                Standard = new DataTable("Standard LP");
                Standard.Columns.Add("Standard Form", typeof(string));
                for (int i = 0; i < (varCounter + NOConstraint); i++)
                {
                    Standard.Columns.Add(dual[0][i], typeof(double));
                }
                Standard.Columns.Add("=", typeof(string));
                Standard.Columns.Add("RHS", typeof(double));

                for (int i = 0; i <= NOConstraint; i++)
                {
                    DataRow ROW = Standard.NewRow();
                    for (int j = 0; j <= (varCounter + NOConstraint + 2); j++)
                    {
                        if ((i == 0) && (j == 0))
                        {
                            if (inMinOrMax == 0)
                            {
                                ROW[j] = "Minimize :";
                            }
                            else if (inMinOrMax == 1)
                            {
                                ROW[j] = "Maximize :";
                            }
                        }
                        if ((i == 1) && (j == 0))
                        {
                            ROW[j] = "Subjective to :";
                        }
                        if ((i == 0) && (j > 0) && (j != (varCounter + NOConstraint + 1)) && (j != (varCounter + NOConstraint + 2)))
                        {
                            ROW[j] = -1 * (Math.Round(HalfTable[i][j - 1], 2));
                        }
                        if ((i != 0) && (j != 0) && (j != (varCounter + NOConstraint + 1)) && (j != (varCounter + NOConstraint + 2)))
                        {
                            double check = (double)HalfTable[i][varCounter + NOConstraint];
                            if (check >= 0)
                            {
                                ROW[j] = Math.Round(HalfTable[i][j - 1], 2);
                            }
                            else
                            {
                                ROW[j] = -1 * Math.Round(HalfTable[i][j - 1], 2);
                            }
                            //ROW[j] = Math.Round(HalfTable[i][j - 1], 2);
                        }
                        if ((i != 0) && (j == (varCounter + NOConstraint + 2)))
                        {
                            double check = (double)HalfTable[i][varCounter + NOConstraint];
                            if (check > 0)
                            {
                                ROW[j] = check;
                            }
                            else
                            {
                                ROW[j] = -1 * check;
                            }
                        }
                    }
                    Standard.Rows.Add(ROW);
                }

                string[][] Second = new string[2][];
                int Ycount = 0;
                int NOArray = 0;
                Second[0] = new string[varCounter + NOConstraint];
                Second[1] = new string[NOConstraint];

                for (int i = 0; i < NOConstraint; i++)
                {
                    Ycount = i + 1;
                    Second[0][NOArray] = "y" + (Ycount);
                    NOArray++;
                }

                Outputtable = new DataTable("Dual");
                Outputtable.Columns.Add("Dual Form", typeof(string));
                for (int i = 0; i < (/*varCounter + */NOConstraint); i++)
                {
                    Outputtable.Columns.Add(Second[0][i], typeof(double));
                }
                Outputtable.Columns.Add(">=/<=", typeof(string));
                Outputtable.Columns.Add("RHS", typeof(double));

                for (int i = 0; i <= varCounter + NOConstraint; i++)
                {
                    DataRow ROW = Outputtable.NewRow();
                    for (int j = 0; j <= (NOConstraint + 2); j++)
                    {
                        if ((i == 0) && (j == 0))
                        {
                            if (inMinOrMax == 0)
                            {
                                ROW[j] = "Maximize :";
                            }
                            else if (inMinOrMax == 1)
                            {
                                ROW[j] = "Minimize :";
                            }
                        }
                        if ((i == 1) && (j == 0))
                        {
                            ROW[j] = "Subjective to :";
                        }
                        if ((i == 0) && (j > 0) && (j != (NOConstraint + 1)) && (j != (NOConstraint + 2)))
                        {
                            ROW[j] = Standard.Rows[j].ItemArray[varCounter + NOConstraint + 2];
                        }
                        if ((i != 0) && (j != 0) && (j != (NOConstraint + 1)) && (j != (NOConstraint + 2)))
                        {
                            double Data = (double)Standard.Rows[j].ItemArray[i];
                            ROW[j] = Data;
                        }
                        if ((i != 0) && (j == (NOConstraint + 2)))
                        {
                            ROW[j] = Standard.Rows[0].ItemArray[i];
                        }
                        if ((i != 0) && (j == (NOConstraint + 1)))
                        {
                            if (inMinOrMax == 0)
                            {
                                ROW[j] = "<=";
                            }
                            else if (inMinOrMax == 1)
                            {
                                ROW[j] = ">=";
                            }
                        }
                    }
                    Outputtable.Rows.Add(ROW);
                }
                //YL: added to link to GUI
                if (chkFraction.Checked)
                {
                    Task convertTask = Task.Run(() => { convertRealToFractionDT(); });
                }
                cmbTableSelect.Items.Clear();
                cmbTableSelect.Items.Add("Stardard LP Form");
                cmbTableSelect.Items.Add("Dual Form");
                tbLeft.SelectedIndex = 1;
                cmbTableSelect.SelectedIndex = 0;
                newWindowWidth = dgvOutputTable.Columns.GetColumnsWidth(DataGridViewElementStates.Visible) + 50;
                newWindowHeight = dgvOutputTable.Rows.GetRowsHeight(DataGridViewElementStates.Visible) + dgvOutputTable.ColumnHeadersHeight + 105;
                cmbTableSelect.SelectedIndex = 1;
            }
        }
        #endregion

        #region tey
        private void cmbTableSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            dgvOutputTable.DataSource = null;
            dgvOutputTable.Rows.Clear();
            lblVariableAll.Text = "";
            lblEnteringVar.Text = "";
            lblLeavingVar.Text = "";

            if (solveOrDual == 1)
            {
                if (cmbTableSelect.SelectedIndex == 0)
                {
                    dgvOutputTable.DataSource = DisplayOutput.standardLP;
                    lblVariableAll.Text = "All variables are >= 0";
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
                else if (DisplayOutput.state == 3 && cmbTableSelect.SelectedIndex >= cmbOldCount) //YL:added for extra solutinos when multiple
                {
                    //if it's not the last table
                    if (cmbTableSelect.SelectedIndex != cmbTableSelect.Items.Count - 1)
                    {
                        int cmbRange; //to keep track the current user selected table is within which solution
                        int previousSolutionIndex = cmbOldCount - 1;
                        for (cmbRange = 0; cmbRange < stepsToEachSol.Length - 1; cmbRange++) //estimate the selected table position
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
                        //based on the esitmated range, if it's not the solution table in the range
                        if (cmbTableSelect.SelectedIndex < previousSolutionIndex + stepsToEachSol[cmbRange])
                        {
                            dgvOutputTable.DataSource = DisplayOutput.mulList[cmbRange][cmbTableSelect.SelectedIndex - previousSolutionIndex - 1];
                            if (dgvOutputTable.Columns["enter"] != null)
                            {
                                HighlightOutputTable();
                            }
                        }
                        else if (cmbTableSelect.SelectedIndex == previousSolutionIndex + stepsToEachSol[cmbRange]) //if solution table
                        {
                            dgvOutputTable.DataSource = DisplayOutput.mulSolution[cmbRange];
                        }
                    }
                    else //in multiple solutions cases, last table is table allSolutions
                    {
                        if (allSolutions != null)
                        {
                            dgvOutputTable.DataSource = allSolutions;
                            dgvOutputTable.Columns["No."].Width = 30;
                        }
                    }
                }
            }
            else if(solveOrDual == 2) //YL: added for dual
            {
                lblStatus_Output.Text = "The problem has been successfully converted into its Dual.";
                lblStatus_Output.ForeColor = Color.Green;
                lblEnteringVar.Text = "N/A";
                lblLeavingVar.Text = "N/A";
                if (cmbTableSelect.SelectedIndex == 0) //just two tables
                {
                    if(Standard != null)
                    {
                        dgvOutputTable.DataSource = Standard;
                        lblVariableAll.Text = "All variables are >= 0";
                    }
                }
                else if(cmbTableSelect.SelectedIndex == 1)
                {
                    if (Outputtable != null)
                    {
                        dgvOutputTable.DataSource = Outputtable;
                        lblVariableAll.Text = "All variables are Urs.";
                    }
                }
            }
            dgvOutputTable.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.AllCells);
            foreach (DataGridViewColumn col in dgvOutputTable.Columns) //sorting is bad for simplex where row position is important
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgvOutputTable.ClearSelection();

            if (string.IsNullOrEmpty(lblLeavingVar.Text) || string.IsNullOrEmpty(lblEnteringVar.Text))
            {
                lblEnteringVar.Text = "N/A";
                lblLeavingVar.Text = "N/A";
            }
        }
        #endregion

        public struct Fraction
        {
            public Fraction(int n, int d)
            {
                N = n;
                D = d;
            }

            public int N { get; private set; }
            public int D { get; private set; }
        }
        //Fraction converter Credit to Kay Zed from stackoverflow
        public static Fraction RealToFraction(double value, double error)
        {
            if (error <= 0.0 || error >= 1.0)
            {
                throw new ArgumentOutOfRangeException("error", "Must be between 0 and 1 (exclusive).");
            }

            int sign = Math.Sign(value);

            if (sign == -1)
            {
                value = Math.Abs(value);
            }

            if (sign != 0)
            {
                // error is the maximum relative error; convert to absolute
                error *= value;
            }

            int n = (int)Math.Floor(value);
            value -= n;

            if (value < error)
            {
                return new Fraction(sign * n, 1);
            }

            if (1 - error < value)
            {
                return new Fraction(sign * (n + 1), 1);
            }

            // The lower fraction is 0/1
            int lower_n = 0;
            int lower_d = 1;

            // The upper fraction is 1/1
            int upper_n = 1;
            int upper_d = 1;

            while (true)
            {
                // The middle fraction is (lower_n + upper_n) / (lower_d + upper_d)
                int middle_n = lower_n + upper_n;
                int middle_d = lower_d + upper_d;

                if (middle_d * (value + error) < middle_n)
                {
                    // real + error < middle : middle is our new upper
                    upper_n = middle_n;
                    upper_d = middle_d;
                }
                else if (middle_n < (value - error) * middle_d)
                {
                    // middle < real - error : middle is our new lower
                    lower_n = middle_n;
                    lower_d = middle_d;
                }
                else
                {
                    // Middle is our best fraction
                    return new Fraction((n * middle_d + middle_n) * sign, middle_d);
                }
            }
        }
    }

    static class ExtensionMethod
    {
        public static void InvokeIfRequired(this Control control, MethodInvoker action)
        {
            if (control.InvokeRequired)
            {
                control.Invoke(action);
            }
            else
            {
                action();
            }
        }
    }
}
