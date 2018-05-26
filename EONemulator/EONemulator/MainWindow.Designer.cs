namespace EONemulator
{
    partial class MainWindow
    {
        /// <summary>
        /// Wymagana zmienna projektanta.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Wyczyść wszystkie używane zasoby.
        /// </summary>
        /// <param name="disposing">prawda, jeżeli zarządzane zasoby powinny zostać zlikwidowane; Fałsz w przeciwnym wypadku.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Kod generowany przez Projektanta formularzy systemu Windows

        /// <summary>
        /// Metoda wymagana do obsługi projektanta — nie należy modyfikować
        /// jej zawartości w edytorze kodu.
        /// </summary>
        private void InitializeComponent()
        {
            this.domainListBox = new System.Windows.Forms.ListBox();
            this.domainGroupBox = new System.Windows.Forms.GroupBox();
            this.subnetworkGroupBox = new System.Windows.Forms.GroupBox();
            this.subnetworkListBox = new System.Windows.Forms.ListBox();
            this.nodeGroupBox = new System.Windows.Forms.GroupBox();
            this.nodeListBox = new System.Windows.Forms.ListBox();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.logGroupBox = new System.Windows.Forms.GroupBox();
            this.crackButton = new System.Windows.Forms.Button();
            this.crackerGroupBox = new System.Windows.Forms.GroupBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.domainGroupBox.SuspendLayout();
            this.subnetworkGroupBox.SuspendLayout();
            this.nodeGroupBox.SuspendLayout();
            this.logGroupBox.SuspendLayout();
            this.crackerGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // domainListBox
            // 
            this.domainListBox.FormattingEnabled = true;
            this.domainListBox.Location = new System.Drawing.Point(6, 19);
            this.domainListBox.Name = "domainListBox";
            this.domainListBox.Size = new System.Drawing.Size(188, 108);
            this.domainListBox.TabIndex = 0;
            this.domainListBox.SelectedIndexChanged += new System.EventHandler(this.domainListBox_SelectedIndexChanged);
            this.domainListBox.DoubleClick += new System.EventHandler(this.domainListBox_DoubleClick);
            // 
            // domainGroupBox
            // 
            this.domainGroupBox.Controls.Add(this.domainListBox);
            this.domainGroupBox.Location = new System.Drawing.Point(12, 12);
            this.domainGroupBox.Name = "domainGroupBox";
            this.domainGroupBox.Size = new System.Drawing.Size(200, 132);
            this.domainGroupBox.TabIndex = 1;
            this.domainGroupBox.TabStop = false;
            this.domainGroupBox.Text = "Domains";
            // 
            // subnetworkGroupBox
            // 
            this.subnetworkGroupBox.Controls.Add(this.subnetworkListBox);
            this.subnetworkGroupBox.Location = new System.Drawing.Point(218, 12);
            this.subnetworkGroupBox.Name = "subnetworkGroupBox";
            this.subnetworkGroupBox.Size = new System.Drawing.Size(200, 132);
            this.subnetworkGroupBox.TabIndex = 2;
            this.subnetworkGroupBox.TabStop = false;
            this.subnetworkGroupBox.Text = "Subnetworks";
            // 
            // subnetworkListBox
            // 
            this.subnetworkListBox.FormattingEnabled = true;
            this.subnetworkListBox.Location = new System.Drawing.Point(6, 19);
            this.subnetworkListBox.Name = "subnetworkListBox";
            this.subnetworkListBox.Size = new System.Drawing.Size(188, 108);
            this.subnetworkListBox.TabIndex = 3;
            this.subnetworkListBox.SelectedIndexChanged += new System.EventHandler(this.subnetworkListBox_SelectedIndexChanged);
            this.subnetworkListBox.DoubleClick += new System.EventHandler(this.subnetworkListBox_DoubleClick);
            // 
            // nodeGroupBox
            // 
            this.nodeGroupBox.Controls.Add(this.nodeListBox);
            this.nodeGroupBox.Location = new System.Drawing.Point(424, 12);
            this.nodeGroupBox.Name = "nodeGroupBox";
            this.nodeGroupBox.Size = new System.Drawing.Size(200, 132);
            this.nodeGroupBox.TabIndex = 3;
            this.nodeGroupBox.TabStop = false;
            this.nodeGroupBox.Text = "Nodes";
            // 
            // nodeListBox
            // 
            this.nodeListBox.FormattingEnabled = true;
            this.nodeListBox.Location = new System.Drawing.Point(6, 18);
            this.nodeListBox.Name = "nodeListBox";
            this.nodeListBox.Size = new System.Drawing.Size(188, 108);
            this.nodeListBox.TabIndex = 3;
            this.nodeListBox.DoubleClick += new System.EventHandler(this.nodeListBox_DoubleClick);
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(6, 19);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTextBox.Size = new System.Drawing.Size(806, 376);
            this.logTextBox.TabIndex = 5;
            // 
            // logGroupBox
            // 
            this.logGroupBox.Controls.Add(this.logTextBox);
            this.logGroupBox.Location = new System.Drawing.Point(12, 151);
            this.logGroupBox.Name = "logGroupBox";
            this.logGroupBox.Size = new System.Drawing.Size(818, 401);
            this.logGroupBox.TabIndex = 6;
            this.logGroupBox.TabStop = false;
            this.logGroupBox.Text = "Logs";
            // 
            // crackButton
            // 
            this.crackButton.Location = new System.Drawing.Point(6, 102);
            this.crackButton.Name = "crackButton";
            this.crackButton.Size = new System.Drawing.Size(181, 23);
            this.crackButton.TabIndex = 7;
            this.crackButton.Text = "Crack the link";
            this.crackButton.UseVisualStyleBackColor = true;
            this.crackButton.Click += new System.EventHandler(this.crackButton_Click);
            // 
            // crackerGroupBox
            // 
            this.crackerGroupBox.Controls.Add(this.label1);
            this.crackerGroupBox.Controls.Add(this.textBox1);
            this.crackerGroupBox.Controls.Add(this.crackButton);
            this.crackerGroupBox.Location = new System.Drawing.Point(631, 13);
            this.crackerGroupBox.Name = "crackerGroupBox";
            this.crackerGroupBox.Size = new System.Drawing.Size(193, 131);
            this.crackerGroupBox.TabIndex = 8;
            this.crackerGroupBox.TabStop = false;
            this.crackerGroupBox.Text = "Cracker";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(6, 51);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(180, 20);
            this.textBox1.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Link id:";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(871, 564);
            this.Controls.Add(this.crackerGroupBox);
            this.Controls.Add(this.logGroupBox);
            this.Controls.Add(this.nodeGroupBox);
            this.Controls.Add(this.subnetworkGroupBox);
            this.Controls.Add(this.domainGroupBox);
            this.Name = "MainWindow";
            this.Text = "EON emulator";
            this.domainGroupBox.ResumeLayout(false);
            this.subnetworkGroupBox.ResumeLayout(false);
            this.nodeGroupBox.ResumeLayout(false);
            this.logGroupBox.ResumeLayout(false);
            this.logGroupBox.PerformLayout();
            this.crackerGroupBox.ResumeLayout(false);
            this.crackerGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox domainListBox;
        private System.Windows.Forms.GroupBox domainGroupBox;
        private System.Windows.Forms.GroupBox subnetworkGroupBox;
        private System.Windows.Forms.ListBox subnetworkListBox;
        private System.Windows.Forms.GroupBox nodeGroupBox;
        private System.Windows.Forms.ListBox nodeListBox;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.GroupBox logGroupBox;
        private System.Windows.Forms.Button crackButton;
        private System.Windows.Forms.GroupBox crackerGroupBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox1;
    }
}

