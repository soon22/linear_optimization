using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;

namespace LinearOptimization
{
    public partial class VariableForm : Form
    {
        private static VariableForm variableForm;
        private int numVariable = -1;
        public static int[] radSelected;
        private ArrayList mainList = new ArrayList();
        public static bool OK_Pressed = false;
        private static VariableForm generatedForm;

        public VariableForm(int numOfVariable)
        {
            InitializeComponent();
            numVariable = numOfVariable;
            if(numVariable <= 0)
            {
                variableForm.Close();
            }
            else
            {
                tlpSub_Variable.RowCount = numVariable + 1;
                for (int x = 0; x < numVariable; x++)
                {
                    ArrayList subList = new ArrayList();
                    Label lbl1 = new Label();
                    lbl1.Text = "Variable " + (x + 1).ToString();
                    lbl1.Dock = DockStyle.Fill;
                    lbl1.Width = 70;
                    lbl1.TextAlign = ContentAlignment.MiddleCenter;
                    tlpSub_Variable.Controls.Add(lbl1, 0, x);

                    TableLayoutPanel tlp1 = new TableLayoutPanel();
                    tlp1.Padding = new System.Windows.Forms.Padding(0,0,0,0);
                    tlp1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
                    tlp1.AutoSize = true;
                    tlp1.ColumnCount = 3;

                    RadioButton rdb1 = new RadioButton();
                    rdb1.Text = "Urs.";
                    rdb1.Width = 65;
                    rdb1.Dock = DockStyle.Fill;
                    rdb1.Margin =new System.Windows.Forms.Padding(0, 0, 0, 0);
                    rdb1.Checked = true;
                    subList.Add(rdb1);
                    tlp1.Controls.Add(rdb1, 0, 0);

                    RadioButton rdb2 = new RadioButton();
                    rdb2.Text = "\u2265"+"0";
                    rdb2.Width = 65;
                    rdb2.Dock = DockStyle.Fill;
                    rdb2.Font = new Font("Calibri", 11, FontStyle.Regular);
                    rdb2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
                    subList.Add(rdb2);
                    tlp1.Controls.Add(rdb2, 1, 0);

                    RadioButton rdb3 = new RadioButton();
                    rdb3.Text = "\u2264" + "0";
                    rdb3.Width = 65;
                    rdb3.Dock = DockStyle.Fill;
                    rdb3.Font = new Font("Calibri", 11, FontStyle.Regular);
                    rdb3.Margin = new System.Windows.Forms.Padding(0, 0, 0, 0);
                    subList.Add(rdb3);
                    tlp1.Controls.Add(rdb3, 2, 0);

                    tlpSub_Variable.Controls.Add(tlp1, 1, x);
                    mainList.Add(subList);
                }
            }
        }

        public static void StoreGeneratedForm()
        {
            generatedForm = variableForm;
        }

        static public void InitializeVariableForm(int numOfVariable)
        {
            variableForm = new VariableForm(numOfVariable);
            OK_Pressed = false;
            variableForm.ShowDialog();
        }

        static public void ShowVariableForm()
        {
            if (generatedForm != null)
            {
                generatedForm.ShowDialog();
            }
        }

        static public void DiscardVariableForm()
        {
            if (variableForm != null)
            {
                variableForm = null;
            }
            if(generatedForm != null)
            {
                generatedForm = null;
            }
        }

        public static void checkRadioBtn()
        {
            //read through all radiobutton instances and store the result in radSelected as array
            if (generatedForm != null)
            {
                radSelected = new int[generatedForm.numVariable];
                int mainCounter = 0;
                foreach (ArrayList subList in generatedForm.mainList)
                {
                    int subCounter = 1;
                    foreach (RadioButton rdb in subList)
                    {
                        if (rdb.Checked)
                        {
                            radSelected[mainCounter] = subCounter;
                        }
                        subCounter++;
                    }
                    mainCounter++;
                }
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            OK_Pressed = true;
            this.Close();
        }

        private void btnUrs_Click(object sender, EventArgs e)
        {
            selectAllRdb(1);
        }

        private void btnPos_Click(object sender, EventArgs e)
        {
            selectAllRdb(2);
        }

        private void btnNeg_Click(object sender, EventArgs e)
        {
            selectAllRdb(3);
        }

        private void selectAllRdb(int input) //1 -Urs 2-Positive 3-Negative
        {
            foreach (ArrayList subList in mainList)
            {
                int subCounter = 1;
                foreach (RadioButton rdb in subList)
                {
                    if(subCounter == input)
                    {
                        rdb.Checked = true;
                        break;
                    }
                    subCounter++;
                }
            }
        }
    }

}
