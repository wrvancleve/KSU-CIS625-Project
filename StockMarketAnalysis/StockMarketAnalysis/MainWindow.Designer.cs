namespace StockMarketAnalysis
{
    partial class MainWindow
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
            this.uxButtonCriteriaSet = new System.Windows.Forms.Button();
            this.uxTextBoxCriteriaSet = new System.Windows.Forms.TextBox();
            this.uxTextBoxData = new System.Windows.Forms.TextBox();
            this.uxButtonData = new System.Windows.Forms.Button();
            this.uxButtonGo = new System.Windows.Forms.Button();
            this.uxTextBoxOutput = new System.Windows.Forms.TextBox();
            this.uxButtonOutput = new System.Windows.Forms.Button();
            this.uxButtonProcessType = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // uxButtonCriteriaSet
            // 
            this.uxButtonCriteriaSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonCriteriaSet.Location = new System.Drawing.Point(12, 72);
            this.uxButtonCriteriaSet.Name = "uxButtonCriteriaSet";
            this.uxButtonCriteriaSet.Size = new System.Drawing.Size(165, 50);
            this.uxButtonCriteriaSet.TabIndex = 0;
            this.uxButtonCriteriaSet.Text = "Select Criteria Set(s)";
            this.uxButtonCriteriaSet.UseVisualStyleBackColor = true;
            this.uxButtonCriteriaSet.Click += new System.EventHandler(this.uxButtonHandler);
            // 
            // uxTextBoxCriteriaSet
            // 
            this.uxTextBoxCriteriaSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxTextBoxCriteriaSet.Location = new System.Drawing.Point(192, 83);
            this.uxTextBoxCriteriaSet.Name = "uxTextBoxCriteriaSet";
            this.uxTextBoxCriteriaSet.Size = new System.Drawing.Size(375, 26);
            this.uxTextBoxCriteriaSet.TabIndex = 1;
            // 
            // uxTextBoxData
            // 
            this.uxTextBoxData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxTextBoxData.Location = new System.Drawing.Point(192, 145);
            this.uxTextBoxData.Name = "uxTextBoxData";
            this.uxTextBoxData.Size = new System.Drawing.Size(375, 26);
            this.uxTextBoxData.TabIndex = 3;
            // 
            // uxButtonData
            // 
            this.uxButtonData.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonData.Location = new System.Drawing.Point(12, 134);
            this.uxButtonData.Name = "uxButtonData";
            this.uxButtonData.Size = new System.Drawing.Size(165, 50);
            this.uxButtonData.TabIndex = 2;
            this.uxButtonData.Text = "Select Data File(s)";
            this.uxButtonData.UseVisualStyleBackColor = true;
            this.uxButtonData.Click += new System.EventHandler(this.uxButtonHandler);
            // 
            // uxButtonGo
            // 
            this.uxButtonGo.Enabled = false;
            this.uxButtonGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonGo.Location = new System.Drawing.Point(217, 274);
            this.uxButtonGo.Name = "uxButtonGo";
            this.uxButtonGo.Size = new System.Drawing.Size(150, 50);
            this.uxButtonGo.TabIndex = 4;
            this.uxButtonGo.Text = "Go";
            this.uxButtonGo.UseVisualStyleBackColor = true;
            this.uxButtonGo.Click += new System.EventHandler(this.uxButtonHandler);
            // 
            // uxTextBoxOutput
            // 
            this.uxTextBoxOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxTextBoxOutput.Location = new System.Drawing.Point(192, 207);
            this.uxTextBoxOutput.Name = "uxTextBoxOutput";
            this.uxTextBoxOutput.Size = new System.Drawing.Size(375, 26);
            this.uxTextBoxOutput.TabIndex = 6;
            // 
            // uxButtonOutput
            // 
            this.uxButtonOutput.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonOutput.Location = new System.Drawing.Point(12, 196);
            this.uxButtonOutput.Name = "uxButtonOutput";
            this.uxButtonOutput.Size = new System.Drawing.Size(165, 50);
            this.uxButtonOutput.TabIndex = 5;
            this.uxButtonOutput.Text = "Select Output Folder";
            this.uxButtonOutput.UseVisualStyleBackColor = true;
            this.uxButtonOutput.Click += new System.EventHandler(this.uxButtonHandler);
            // 
            // uxButtonProcessType
            // 
            this.uxButtonProcessType.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonProcessType.Location = new System.Drawing.Point(217, 12);
            this.uxButtonProcessType.Name = "uxButtonProcessType";
            this.uxButtonProcessType.Size = new System.Drawing.Size(150, 40);
            this.uxButtonProcessType.TabIndex = 7;
            this.uxButtonProcessType.Text = "Single";
            this.uxButtonProcessType.UseVisualStyleBackColor = true;
            this.uxButtonProcessType.Click += new System.EventHandler(this.uxButtonHandler);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(584, 336);
            this.Controls.Add(this.uxButtonProcessType);
            this.Controls.Add(this.uxTextBoxOutput);
            this.Controls.Add(this.uxButtonOutput);
            this.Controls.Add(this.uxButtonGo);
            this.Controls.Add(this.uxTextBoxData);
            this.Controls.Add(this.uxButtonData);
            this.Controls.Add(this.uxTextBoxCriteriaSet);
            this.Controls.Add(this.uxButtonCriteriaSet);
            this.Name = "MainWindow";
            this.Text = "Stock Market Analysis";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button uxButtonCriteriaSet;
        private System.Windows.Forms.TextBox uxTextBoxCriteriaSet;
        private System.Windows.Forms.TextBox uxTextBoxData;
        private System.Windows.Forms.Button uxButtonData;
        private System.Windows.Forms.Button uxButtonGo;
        private System.Windows.Forms.TextBox uxTextBoxOutput;
        private System.Windows.Forms.Button uxButtonOutput;
        private System.Windows.Forms.Button uxButtonProcessType;
    }
}

