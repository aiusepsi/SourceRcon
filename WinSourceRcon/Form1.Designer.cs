namespace WinSourceRcon
{
    partial class Form1
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
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.IPBox = new System.Windows.Forms.TextBox();
            this.SendInputButton = new System.Windows.Forms.Button();
            this.InputBox = new System.Windows.Forms.TextBox();
            this.OutputBox = new System.Windows.Forms.RichTextBox();
            this.PortBox = new System.Windows.Forms.TextBox();
            this.PasswordBox = new System.Windows.Forms.TextBox();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.PasswordBox);
            this.splitContainer1.Panel1.Controls.Add(this.PortBox);
            this.splitContainer1.Panel1.Controls.Add(this.ConnectButton);
            this.splitContainer1.Panel1.Controls.Add(this.IPBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SendInputButton);
            this.splitContainer1.Panel2.Controls.Add(this.InputBox);
            this.splitContainer1.Panel2.Controls.Add(this.OutputBox);
            this.splitContainer1.Size = new System.Drawing.Size(948, 477);
            this.splitContainer1.SplitterDistance = 229;
            this.splitContainer1.TabIndex = 0;
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.Location = new System.Drawing.Point(20, 395);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(191, 68);
            this.ConnectButton.TabIndex = 3;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // IPBox
            // 
            this.IPBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.IPBox.Location = new System.Drawing.Point(15, 29);
            this.IPBox.Name = "IPBox";
            this.IPBox.Size = new System.Drawing.Size(200, 20);
            this.IPBox.TabIndex = 0;
            this.IPBox.Text = "127.0.0.1";
            // 
            // SendInputButton
            // 
            this.SendInputButton.Enabled = false;
            this.SendInputButton.Location = new System.Drawing.Point(601, 442);
            this.SendInputButton.Name = "SendInputButton";
            this.SendInputButton.Size = new System.Drawing.Size(102, 23);
            this.SendInputButton.TabIndex = 1;
            this.SendInputButton.Text = "Send";
            this.SendInputButton.UseVisualStyleBackColor = true;
            this.SendInputButton.Click += new System.EventHandler(this.SendInputButton_Click);
            // 
            // InputBox
            // 
            this.InputBox.Enabled = false;
            this.InputBox.Location = new System.Drawing.Point(13, 444);
            this.InputBox.Name = "InputBox";
            this.InputBox.Size = new System.Drawing.Size(582, 20);
            this.InputBox.TabIndex = 0;
            this.InputBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.InputBox_KeyDown);
            // 
            // OutputBox
            // 
            this.OutputBox.Enabled = false;
            this.OutputBox.Location = new System.Drawing.Point(13, 12);
            this.OutputBox.Name = "OutputBox";
            this.OutputBox.ReadOnly = true;
            this.OutputBox.Size = new System.Drawing.Size(690, 412);
            this.OutputBox.TabIndex = 2;
            this.OutputBox.TabStop = false;
            this.OutputBox.Text = "";
            // 
            // PortBox
            // 
            this.PortBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PortBox.Location = new System.Drawing.Point(17, 61);
            this.PortBox.Name = "PortBox";
            this.PortBox.Size = new System.Drawing.Size(197, 20);
            this.PortBox.TabIndex = 1;
            this.PortBox.Text = "27015";
            // 
            // PasswordBox
            // 
            this.PasswordBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PasswordBox.Location = new System.Drawing.Point(16, 116);
            this.PasswordBox.Name = "PasswordBox";
            this.PasswordBox.Size = new System.Drawing.Size(198, 20);
            this.PasswordBox.TabIndex = 2;
            this.PasswordBox.UseSystemPasswordChar = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(948, 477);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox InputBox;
        private System.Windows.Forms.RichTextBox OutputBox;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox IPBox;
        private System.Windows.Forms.Button SendInputButton;
        private System.Windows.Forms.TextBox PortBox;
        private System.Windows.Forms.TextBox PasswordBox;
    }
}

