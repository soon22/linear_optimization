namespace LinearOptimization
{
    partial class TableWindowForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tlpTableMain = new System.Windows.Forms.TableLayoutPanel();
            this.dgvOutputTable = new System.Windows.Forms.DataGridView();
            this.cmbTableSelect = new System.Windows.Forms.ComboBox();
            this.tlpButtonTable = new System.Windows.Forms.TableLayoutPanel();
            this.btnCloseTableAll = new System.Windows.Forms.Button();
            this.tlpTableMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputTable)).BeginInit();
            this.tlpButtonTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // tlpTableMain
            // 
            this.tlpTableMain.ColumnCount = 1;
            this.tlpTableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTableMain.Controls.Add(this.dgvOutputTable, 0, 1);
            this.tlpTableMain.Controls.Add(this.cmbTableSelect, 0, 0);
            this.tlpTableMain.Controls.Add(this.tlpButtonTable, 0, 2);
            this.tlpTableMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpTableMain.Location = new System.Drawing.Point(0, 0);
            this.tlpTableMain.Name = "tlpTableMain";
            this.tlpTableMain.RowCount = 3;
            this.tlpTableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpTableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpTableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 25F));
            this.tlpTableMain.Size = new System.Drawing.Size(455, 229);
            this.tlpTableMain.TabIndex = 0;
            // 
            // dgvOutputTable
            // 
            this.dgvOutputTable.AllowUserToAddRows = false;
            this.dgvOutputTable.AllowUserToDeleteRows = false;
            this.dgvOutputTable.AllowUserToResizeColumns = false;
            this.dgvOutputTable.AllowUserToResizeRows = false;
            this.dgvOutputTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvOutputTable.BackgroundColor = System.Drawing.SystemColors.ControlLightLight;
            this.dgvOutputTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvOutputTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvOutputTable.Location = new System.Drawing.Point(0, 25);
            this.dgvOutputTable.Margin = new System.Windows.Forms.Padding(0);
            this.dgvOutputTable.Name = "dgvOutputTable";
            this.dgvOutputTable.ReadOnly = true;
            this.dgvOutputTable.RowHeadersVisible = false;
            this.dgvOutputTable.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            this.dgvOutputTable.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvOutputTable.Size = new System.Drawing.Size(455, 179);
            this.dgvOutputTable.TabIndex = 5;
            // 
            // cmbTableSelect
            // 
            this.cmbTableSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTableSelect.FormattingEnabled = true;
            this.cmbTableSelect.Location = new System.Drawing.Point(3, 3);
            this.cmbTableSelect.Name = "cmbTableSelect";
            this.cmbTableSelect.Size = new System.Drawing.Size(449, 21);
            this.cmbTableSelect.TabIndex = 2;
            this.cmbTableSelect.SelectedIndexChanged += new System.EventHandler(this.cmbTableSelect_SelectedIndexChanged);
            // 
            // tlpButtonTable
            // 
            this.tlpButtonTable.ColumnCount = 2;
            this.tlpButtonTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtonTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tlpButtonTable.Controls.Add(this.btnCloseTableAll, 1, 0);
            this.tlpButtonTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpButtonTable.Location = new System.Drawing.Point(0, 204);
            this.tlpButtonTable.Margin = new System.Windows.Forms.Padding(0);
            this.tlpButtonTable.Name = "tlpButtonTable";
            this.tlpButtonTable.RowCount = 1;
            this.tlpButtonTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tlpButtonTable.Size = new System.Drawing.Size(455, 25);
            this.tlpButtonTable.TabIndex = 6;
            // 
            // btnCloseTableAll
            // 
            this.btnCloseTableAll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnCloseTableAll.Location = new System.Drawing.Point(335, 0);
            this.btnCloseTableAll.Margin = new System.Windows.Forms.Padding(0);
            this.btnCloseTableAll.Name = "btnCloseTableAll";
            this.btnCloseTableAll.Size = new System.Drawing.Size(120, 25);
            this.btnCloseTableAll.TabIndex = 0;
            this.btnCloseTableAll.Text = "Close All Tables";
            this.btnCloseTableAll.UseVisualStyleBackColor = true;
            this.btnCloseTableAll.Click += new System.EventHandler(this.btnCloseTableAll_Click);
            // 
            // TableWindowForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(455, 229);
            this.Controls.Add(this.tlpTableMain);
            this.Name = "TableWindowForm";
            this.Text = "TableWindowForm";
            this.Shown += new System.EventHandler(this.TableWindowForm_Shown);
            this.tlpTableMain.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvOutputTable)).EndInit();
            this.tlpButtonTable.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tlpTableMain;
        private System.Windows.Forms.ComboBox cmbTableSelect;
        private System.Windows.Forms.DataGridView dgvOutputTable;
        private System.Windows.Forms.TableLayoutPanel tlpButtonTable;
        private System.Windows.Forms.Button btnCloseTableAll;
    }
}