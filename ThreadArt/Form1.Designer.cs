namespace ThreadArt
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.panelControls = new System.Windows.Forms.Panel();
			this.label8 = new System.Windows.Forms.Label();
			this.btnSavePattern = new System.Windows.Forms.Button();
			this.btnSavePreview = new System.Windows.Forms.Button();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.tbThreadDensity = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.tbThreadWidth = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.tbBorder = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.tbMaxLines = new System.Windows.Forms.TextBox();
			this.label5 = new System.Windows.Forms.Label();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.tbImagePath = new System.Windows.Forms.TextBox();
			this.btnOpenImage = new System.Windows.Forms.Button();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.cbNailCount = new System.Windows.Forms.ComboBox();
			this.tbNailDiameter = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.tbDiameter = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.btnSimulate = new System.Windows.Forms.Button();
			this.panelCanvas = new System.Windows.Forms.Panel();
			this.panelControls.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panelControls
			// 
			this.panelControls.Controls.Add(this.label8);
			this.panelControls.Controls.Add(this.btnSavePattern);
			this.panelControls.Controls.Add(this.btnSavePreview);
			this.panelControls.Controls.Add(this.groupBox3);
			this.panelControls.Controls.Add(this.groupBox2);
			this.panelControls.Controls.Add(this.groupBox1);
			this.panelControls.Controls.Add(this.btnSimulate);
			this.panelControls.Dock = System.Windows.Forms.DockStyle.Left;
			this.panelControls.Location = new System.Drawing.Point(0, 0);
			this.panelControls.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelControls.Name = "panelControls";
			this.panelControls.Size = new System.Drawing.Size(334, 671);
			this.panelControls.TabIndex = 0;
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(117, 14);
			this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(89, 20);
			this.label8.TabIndex = 17;
			this.label8.Text = "Version 1.0";
			// 
			// btnSavePattern
			// 
			this.btnSavePattern.Enabled = false;
			this.btnSavePattern.Location = new System.Drawing.Point(90, 625);
			this.btnSavePattern.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSavePattern.Name = "btnSavePattern";
			this.btnSavePattern.Size = new System.Drawing.Size(150, 35);
			this.btnSavePattern.TabIndex = 11;
			this.btnSavePattern.Text = "Save Pattern";
			this.btnSavePattern.UseVisualStyleBackColor = true;
			this.btnSavePattern.Click += new System.EventHandler(this.btnSavePattern_Click);
			// 
			// btnSavePreview
			// 
			this.btnSavePreview.Enabled = false;
			this.btnSavePreview.Location = new System.Drawing.Point(90, 578);
			this.btnSavePreview.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSavePreview.Name = "btnSavePreview";
			this.btnSavePreview.Size = new System.Drawing.Size(150, 35);
			this.btnSavePreview.TabIndex = 10;
			this.btnSavePreview.Text = "Save Preview";
			this.btnSavePreview.UseVisualStyleBackColor = true;
			this.btnSavePreview.Click += new System.EventHandler(this.btnSavePreview_Click);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.tbThreadDensity);
			this.groupBox3.Controls.Add(this.label7);
			this.groupBox3.Controls.Add(this.tbThreadWidth);
			this.groupBox3.Controls.Add(this.label4);
			this.groupBox3.Controls.Add(this.tbBorder);
			this.groupBox3.Controls.Add(this.label3);
			this.groupBox3.Controls.Add(this.tbMaxLines);
			this.groupBox3.Controls.Add(this.label5);
			this.groupBox3.Location = new System.Drawing.Point(18, 325);
			this.groupBox3.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox3.Size = new System.Drawing.Size(300, 200);
			this.groupBox3.TabIndex = 16;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Artwork";
			// 
			// tbThreadDensity
			// 
			this.tbThreadDensity.Location = new System.Drawing.Point(129, 152);
			this.tbThreadDensity.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbThreadDensity.Name = "tbThreadDensity";
			this.tbThreadDensity.Size = new System.Drawing.Size(160, 26);
			this.tbThreadDensity.TabIndex = 8;
			this.tbThreadDensity.Text = "4";
			this.tbThreadDensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbThreadDensity.Validating += new System.ComponentModel.CancelEventHandler(this.tbThreadDensity_Validating);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(57, 157);
			this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(62, 20);
			this.label7.TabIndex = 14;
			this.label7.Text = "Density";
			// 
			// tbThreadWidth
			// 
			this.tbThreadWidth.Location = new System.Drawing.Point(129, 111);
			this.tbThreadWidth.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbThreadWidth.Name = "tbThreadWidth";
			this.tbThreadWidth.Size = new System.Drawing.Size(160, 26);
			this.tbThreadWidth.TabIndex = 7;
			this.tbThreadWidth.Text = "0.1";
			this.tbThreadWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbThreadWidth.Validating += new System.ComponentModel.CancelEventHandler(this.tbThreadWidth_Validating);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(12, 115);
			this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(104, 20);
			this.label4.TabIndex = 12;
			this.label4.Text = "Thread Width";
			// 
			// tbBorder
			// 
			this.tbBorder.Location = new System.Drawing.Point(129, 29);
			this.tbBorder.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbBorder.Name = "tbBorder";
			this.tbBorder.Size = new System.Drawing.Size(160, 26);
			this.tbBorder.TabIndex = 5;
			this.tbBorder.Text = "0";
			this.tbBorder.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbBorder.Validating += new System.ComponentModel.CancelEventHandler(this.tbBorder_Validating);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(63, 34);
			this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 20);
			this.label3.TabIndex = 5;
			this.label3.Text = "Border";
			// 
			// tbMaxLines
			// 
			this.tbMaxLines.Location = new System.Drawing.Point(129, 69);
			this.tbMaxLines.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbMaxLines.Name = "tbMaxLines";
			this.tbMaxLines.Size = new System.Drawing.Size(160, 26);
			this.tbMaxLines.TabIndex = 6;
			this.tbMaxLines.Text = "5000";
			this.tbMaxLines.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbMaxLines.Validating += new System.ComponentModel.CancelEventHandler(this.tbMaxLines_Validating);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(38, 74);
			this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(80, 20);
			this.label5.TabIndex = 10;
			this.label5.Text = "Max Lines";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.tbImagePath);
			this.groupBox2.Controls.Add(this.btnOpenImage);
			this.groupBox2.Location = new System.Drawing.Point(18, 38);
			this.groupBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox2.Size = new System.Drawing.Size(300, 117);
			this.groupBox2.TabIndex = 15;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Image";
			// 
			// tbImagePath
			// 
			this.tbImagePath.Location = new System.Drawing.Point(9, 29);
			this.tbImagePath.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbImagePath.Name = "tbImagePath";
			this.tbImagePath.Size = new System.Drawing.Size(280, 26);
			this.tbImagePath.TabIndex = 0;
			// 
			// btnOpenImage
			// 
			this.btnOpenImage.Location = new System.Drawing.Point(72, 72);
			this.btnOpenImage.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnOpenImage.Name = "btnOpenImage";
			this.btnOpenImage.Size = new System.Drawing.Size(150, 35);
			this.btnOpenImage.TabIndex = 1;
			this.btnOpenImage.Text = "Open";
			this.btnOpenImage.UseVisualStyleBackColor = true;
			this.btnOpenImage.Click += new System.EventHandler(this.btnOpenImage_Click);
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.cbNailCount);
			this.groupBox1.Controls.Add(this.tbNailDiameter);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.label1);
			this.groupBox1.Controls.Add(this.tbDiameter);
			this.groupBox1.Controls.Add(this.label2);
			this.groupBox1.Location = new System.Drawing.Point(18, 165);
			this.groupBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Padding = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.groupBox1.Size = new System.Drawing.Size(300, 151);
			this.groupBox1.TabIndex = 14;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Nails";
			// 
			// cbNailCount
			// 
			this.cbNailCount.FormattingEnabled = true;
			this.cbNailCount.Location = new System.Drawing.Point(129, 25);
			this.cbNailCount.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.cbNailCount.Name = "cbNailCount";
			this.cbNailCount.Size = new System.Drawing.Size(160, 28);
			this.cbNailCount.TabIndex = 2;
			// 
			// tbNailDiameter
			// 
			this.tbNailDiameter.Location = new System.Drawing.Point(129, 66);
			this.tbNailDiameter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbNailDiameter.Name = "tbNailDiameter";
			this.tbNailDiameter.Size = new System.Drawing.Size(160, 26);
			this.tbNailDiameter.TabIndex = 3;
			this.tbNailDiameter.Text = "1.7";
			this.tbNailDiameter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbNailDiameter.Validating += new System.ComponentModel.CancelEventHandler(this.tbNailDiameter_Validating);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(15, 71);
			this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(104, 20);
			this.label6.TabIndex = 12;
			this.label6.Text = "Nail Diameter";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(68, 29);
			this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(52, 20);
			this.label1.TabIndex = 1;
			this.label1.Text = "Count";
			// 
			// tbDiameter
			// 
			this.tbDiameter.Location = new System.Drawing.Point(129, 106);
			this.tbDiameter.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.tbDiameter.Name = "tbDiameter";
			this.tbDiameter.Size = new System.Drawing.Size(160, 26);
			this.tbDiameter.TabIndex = 4;
			this.tbDiameter.Text = "600";
			this.tbDiameter.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.tbDiameter.Validating += new System.ComponentModel.CancelEventHandler(this.tbDiameter_Validating);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(9, 111);
			this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(111, 20);
			this.label2.TabIndex = 3;
			this.label2.Text = "Ring Diameter";
			// 
			// btnSimulate
			// 
			this.btnSimulate.Enabled = false;
			this.btnSimulate.Location = new System.Drawing.Point(90, 534);
			this.btnSimulate.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.btnSimulate.Name = "btnSimulate";
			this.btnSimulate.Size = new System.Drawing.Size(150, 35);
			this.btnSimulate.TabIndex = 9;
			this.btnSimulate.Text = "Process Image";
			this.btnSimulate.UseVisualStyleBackColor = true;
			this.btnSimulate.Click += new System.EventHandler(this.btnSimulate_Click);
			// 
			// panelCanvas
			// 
			this.panelCanvas.BackColor = System.Drawing.Color.Gray;
			this.panelCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelCanvas.Location = new System.Drawing.Point(334, 0);
			this.panelCanvas.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.panelCanvas.Name = "panelCanvas";
			this.panelCanvas.Size = new System.Drawing.Size(654, 671);
			this.panelCanvas.TabIndex = 1;
			this.panelCanvas.Paint += new System.Windows.Forms.PaintEventHandler(this.panelCanvas_Paint);
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(988, 671);
			this.Controls.Add(this.panelCanvas);
			this.Controls.Add(this.panelControls);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
			this.Name = "Form1";
			this.Text = "GISH Thread Art";
			this.panelControls.ResumeLayout(false);
			this.panelControls.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panelControls;
		private System.Windows.Forms.Panel panelCanvas;
		private System.Windows.Forms.Button btnSimulate;
		private System.Windows.Forms.ComboBox cbNailCount;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbBorder;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox tbDiameter;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbImagePath;
		private System.Windows.Forms.Button btnOpenImage;
		private System.Windows.Forms.TextBox tbMaxLines;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TextBox tbNailDiameter;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Button btnSavePattern;
		private System.Windows.Forms.Button btnSavePreview;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.TextBox tbThreadDensity;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbThreadWidth;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label8;
	}
}

