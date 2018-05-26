namespace ClientManager
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
            this.selectGroupBox = new System.Windows.Forms.GroupBox();
            this.client3RadioButton = new System.Windows.Forms.RadioButton();
            this.client2RadioButton = new System.Windows.Forms.RadioButton();
            this.client1RadioButton = new System.Windows.Forms.RadioButton();
            this.signalGroupBox = new System.Windows.Forms.GroupBox();
            this.requestButton = new System.Windows.Forms.Button();
            this.destinationLabel = new System.Windows.Forms.Label();
            this.destinationComboBox = new System.Windows.Forms.ComboBox();
            this.bandwidthTextBox = new System.Windows.Forms.TextBox();
            this.bandwidthLabel = new System.Windows.Forms.Label();
            this.textTextBox = new System.Windows.Forms.TextBox();
            this.textLabel = new System.Windows.Forms.Label();
            this.messageGroupBox = new System.Windows.Forms.GroupBox();
            this.messageButton = new System.Windows.Forms.Button();
            this.messageDestinationComboBox = new System.Windows.Forms.ComboBox();
            this.receiverLabel = new System.Windows.Forms.Label();
            this.logTextBox = new System.Windows.Forms.TextBox();
            this.Logs = new System.Windows.Forms.GroupBox();
            this.selectGroupBox.SuspendLayout();
            this.signalGroupBox.SuspendLayout();
            this.messageGroupBox.SuspendLayout();
            this.Logs.SuspendLayout();
            this.SuspendLayout();
            // 
            // selectGroupBox
            // 
            this.selectGroupBox.Controls.Add(this.client3RadioButton);
            this.selectGroupBox.Controls.Add(this.client2RadioButton);
            this.selectGroupBox.Controls.Add(this.client1RadioButton);
            this.selectGroupBox.Location = new System.Drawing.Point(9, 10);
            this.selectGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectGroupBox.Name = "selectGroupBox";
            this.selectGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.selectGroupBox.Size = new System.Drawing.Size(52, 81);
            this.selectGroupBox.TabIndex = 0;
            this.selectGroupBox.TabStop = false;
            this.selectGroupBox.Text = "Clients";
            // 
            // client3RadioButton
            // 
            this.client3RadioButton.AutoSize = true;
            this.client3RadioButton.Location = new System.Drawing.Point(4, 59);
            this.client3RadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.client3RadioButton.Name = "client3RadioButton";
            this.client3RadioButton.Size = new System.Drawing.Size(38, 17);
            this.client3RadioButton.TabIndex = 3;
            this.client3RadioButton.TabStop = true;
            this.client3RadioButton.Text = "K3";
            this.client3RadioButton.UseVisualStyleBackColor = true;
            this.client3RadioButton.CheckedChanged += new System.EventHandler(this.client3RadioButton_CheckedChanged);
            // 
            // client2RadioButton
            // 
            this.client2RadioButton.AutoSize = true;
            this.client2RadioButton.Location = new System.Drawing.Point(4, 39);
            this.client2RadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.client2RadioButton.Name = "client2RadioButton";
            this.client2RadioButton.Size = new System.Drawing.Size(38, 17);
            this.client2RadioButton.TabIndex = 2;
            this.client2RadioButton.TabStop = true;
            this.client2RadioButton.Text = "K2";
            this.client2RadioButton.UseVisualStyleBackColor = true;
            this.client2RadioButton.CheckedChanged += new System.EventHandler(this.client2RadioButton_CheckedChanged);
            // 
            // client1RadioButton
            // 
            this.client1RadioButton.AutoSize = true;
            this.client1RadioButton.Location = new System.Drawing.Point(4, 17);
            this.client1RadioButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.client1RadioButton.Name = "client1RadioButton";
            this.client1RadioButton.Size = new System.Drawing.Size(38, 17);
            this.client1RadioButton.TabIndex = 1;
            this.client1RadioButton.TabStop = true;
            this.client1RadioButton.Text = "K1";
            this.client1RadioButton.UseVisualStyleBackColor = true;
            this.client1RadioButton.CheckedChanged += new System.EventHandler(this.client1RadioButton_CheckedChanged);
            // 
            // signalGroupBox
            // 
            this.signalGroupBox.Controls.Add(this.requestButton);
            this.signalGroupBox.Controls.Add(this.destinationLabel);
            this.signalGroupBox.Controls.Add(this.destinationComboBox);
            this.signalGroupBox.Controls.Add(this.bandwidthTextBox);
            this.signalGroupBox.Controls.Add(this.bandwidthLabel);
            this.signalGroupBox.Location = new System.Drawing.Point(65, 11);
            this.signalGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.signalGroupBox.Name = "signalGroupBox";
            this.signalGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.signalGroupBox.Size = new System.Drawing.Size(192, 81);
            this.signalGroupBox.TabIndex = 1;
            this.signalGroupBox.TabStop = false;
            this.signalGroupBox.Text = "Connection";
            // 
            // requestButton
            // 
            this.requestButton.Location = new System.Drawing.Point(4, 58);
            this.requestButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.requestButton.Name = "requestButton";
            this.requestButton.Size = new System.Drawing.Size(184, 19);
            this.requestButton.TabIndex = 3;
            this.requestButton.Text = "SEND REQUEST";
            this.requestButton.UseVisualStyleBackColor = true;
            this.requestButton.Click += new System.EventHandler(this.requestButton_Click);
            // 
            // destinationLabel
            // 
            this.destinationLabel.AutoSize = true;
            this.destinationLabel.Location = new System.Drawing.Point(4, 40);
            this.destinationLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.destinationLabel.Name = "destinationLabel";
            this.destinationLabel.Size = new System.Drawing.Size(60, 13);
            this.destinationLabel.TabIndex = 4;
            this.destinationLabel.Text = "Destination";
            // 
            // destinationComboBox
            // 
            this.destinationComboBox.FormattingEnabled = true;
            this.destinationComboBox.Location = new System.Drawing.Point(98, 36);
            this.destinationComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.destinationComboBox.Name = "destinationComboBox";
            this.destinationComboBox.Size = new System.Drawing.Size(90, 21);
            this.destinationComboBox.TabIndex = 4;
            // 
            // bandwidthTextBox
            // 
            this.bandwidthTextBox.Location = new System.Drawing.Point(98, 15);
            this.bandwidthTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.bandwidthTextBox.Name = "bandwidthTextBox";
            this.bandwidthTextBox.Size = new System.Drawing.Size(90, 20);
            this.bandwidthTextBox.TabIndex = 1;
            // 
            // bandwidthLabel
            // 
            this.bandwidthLabel.AutoSize = true;
            this.bandwidthLabel.Location = new System.Drawing.Point(4, 18);
            this.bandwidthLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.bandwidthLabel.Name = "bandwidthLabel";
            this.bandwidthLabel.Size = new System.Drawing.Size(90, 13);
            this.bandwidthLabel.TabIndex = 0;
            this.bandwidthLabel.Text = "Bandwidth [Gb/s]";
            // 
            // textTextBox
            // 
            this.textTextBox.Location = new System.Drawing.Point(68, 15);
            this.textTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textTextBox.Name = "textTextBox";
            this.textTextBox.Size = new System.Drawing.Size(92, 20);
            this.textTextBox.TabIndex = 3;
            // 
            // textLabel
            // 
            this.textLabel.AutoSize = true;
            this.textLabel.Location = new System.Drawing.Point(4, 18);
            this.textLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.textLabel.Name = "textLabel";
            this.textLabel.Size = new System.Drawing.Size(28, 13);
            this.textLabel.TabIndex = 2;
            this.textLabel.Text = "Text";
            // 
            // messageGroupBox
            // 
            this.messageGroupBox.Controls.Add(this.messageButton);
            this.messageGroupBox.Controls.Add(this.messageDestinationComboBox);
            this.messageGroupBox.Controls.Add(this.receiverLabel);
            this.messageGroupBox.Controls.Add(this.textLabel);
            this.messageGroupBox.Controls.Add(this.textTextBox);
            this.messageGroupBox.Location = new System.Drawing.Point(261, 11);
            this.messageGroupBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.messageGroupBox.Name = "messageGroupBox";
            this.messageGroupBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.messageGroupBox.Size = new System.Drawing.Size(166, 81);
            this.messageGroupBox.TabIndex = 2;
            this.messageGroupBox.TabStop = false;
            this.messageGroupBox.Text = "Message";
            // 
            // messageButton
            // 
            this.messageButton.Location = new System.Drawing.Point(5, 58);
            this.messageButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.messageButton.Name = "messageButton";
            this.messageButton.Size = new System.Drawing.Size(155, 19);
            this.messageButton.TabIndex = 7;
            this.messageButton.Text = "SEND MESSAGE";
            this.messageButton.UseVisualStyleBackColor = true;
            this.messageButton.Click += new System.EventHandler(this.messageButton_Click);
            // 
            // messageDestinationComboBox
            // 
            this.messageDestinationComboBox.FormattingEnabled = true;
            this.messageDestinationComboBox.Location = new System.Drawing.Point(68, 36);
            this.messageDestinationComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.messageDestinationComboBox.Name = "messageDestinationComboBox";
            this.messageDestinationComboBox.Size = new System.Drawing.Size(92, 21);
            this.messageDestinationComboBox.TabIndex = 6;
            // 
            // receiverLabel
            // 
            this.receiverLabel.AutoSize = true;
            this.receiverLabel.Location = new System.Drawing.Point(4, 38);
            this.receiverLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.receiverLabel.Name = "receiverLabel";
            this.receiverLabel.Size = new System.Drawing.Size(60, 13);
            this.receiverLabel.TabIndex = 5;
            this.receiverLabel.Text = "Destination";
            // 
            // logTextBox
            // 
            this.logTextBox.Location = new System.Drawing.Point(4, 17);
            this.logTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.logTextBox.Multiline = true;
            this.logTextBox.Name = "logTextBox";
            this.logTextBox.ReadOnly = true;
            this.logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.logTextBox.Size = new System.Drawing.Size(408, 388);
            this.logTextBox.TabIndex = 3;
            // 
            // Logs
            // 
            this.Logs.Controls.Add(this.logTextBox);
            this.Logs.Location = new System.Drawing.Point(9, 96);
            this.Logs.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Logs.Name = "Logs";
            this.Logs.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Logs.Size = new System.Drawing.Size(418, 410);
            this.Logs.TabIndex = 4;
            this.Logs.TabStop = false;
            this.Logs.Text = "Logs";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 515);
            this.Controls.Add(this.Logs);
            this.Controls.Add(this.messageGroupBox);
            this.Controls.Add(this.signalGroupBox);
            this.Controls.Add(this.selectGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainWindow";
            this.Text = "Client Manager";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_Closing);
            this.selectGroupBox.ResumeLayout(false);
            this.selectGroupBox.PerformLayout();
            this.signalGroupBox.ResumeLayout(false);
            this.signalGroupBox.PerformLayout();
            this.messageGroupBox.ResumeLayout(false);
            this.messageGroupBox.PerformLayout();
            this.Logs.ResumeLayout(false);
            this.Logs.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox selectGroupBox;
        private System.Windows.Forms.RadioButton client3RadioButton;
        private System.Windows.Forms.RadioButton client2RadioButton;
        private System.Windows.Forms.RadioButton client1RadioButton;
        private System.Windows.Forms.GroupBox signalGroupBox;
        private System.Windows.Forms.Label destinationLabel;
        private System.Windows.Forms.TextBox textTextBox;
        private System.Windows.Forms.Label textLabel;
        private System.Windows.Forms.TextBox bandwidthTextBox;
        private System.Windows.Forms.Label bandwidthLabel;
        private System.Windows.Forms.ComboBox destinationComboBox;
        private System.Windows.Forms.GroupBox messageGroupBox;
        private System.Windows.Forms.Button requestButton;
        private System.Windows.Forms.Button messageButton;
        private System.Windows.Forms.ComboBox messageDestinationComboBox;
        private System.Windows.Forms.Label receiverLabel;
        private System.Windows.Forms.TextBox logTextBox;
        private System.Windows.Forms.GroupBox Logs;
    }
}

