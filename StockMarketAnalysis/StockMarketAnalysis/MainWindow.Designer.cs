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
            this.SuspendLayout();
            // 
            // uxButtonCriteriaSet
            // 
            this.uxButtonCriteriaSet.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonCriteriaSet.Location = new System.Drawing.Point(12, 33);
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
            this.uxTextBoxCriteriaSet.Location = new System.Drawing.Point(192, 44);
            this.uxTextBoxCriteriaSet.Name = "uxTextBoxCriteriaSet";
            this.uxTextBoxCriteriaSet.Size = new System.Drawing.Size(375, 26);
            this.uxTextBoxCriteriaSet.TabIndex = 1;
            // 
            // uxTextBoxData
            // 
            this.uxTextBoxData.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxTextBoxData.Location = new System.Drawing.Point(192, 140);
            this.uxTextBoxData.Name = "uxTextBoxData";
            this.uxTextBoxData.Size = new System.Drawing.Size(375, 26);
            this.uxTextBoxData.TabIndex = 3;
            // 
            // uxButtonData
            // 
            this.uxButtonData.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonData.Location = new System.Drawing.Point(12, 129);
            this.uxButtonData.Name = "uxButtonData";
            this.uxButtonData.Size = new System.Drawing.Size(165, 50);
            this.uxButtonData.TabIndex = 2;
            this.uxButtonData.Text = "Select Data File(s)";
            this.uxButtonData.UseVisualStyleBackColor = true;
            this.uxButtonData.Click += new System.EventHandler(this.uxButtonHandler);
            // 
            // uxButtonGo
            // 
            this.uxButtonGo.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.uxButtonGo.Location = new System.Drawing.Point(217, 211);
            this.uxButtonGo.Name = "uxButtonGo";
            this.uxButtonGo.Size = new System.Drawing.Size(150, 50);
            this.uxButtonGo.TabIndex = 4;
            this.uxButtonGo.Text = "Go";
            this.uxButtonGo.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.ClientSize = new System.Drawing.Size(584, 286);
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
    }
}

