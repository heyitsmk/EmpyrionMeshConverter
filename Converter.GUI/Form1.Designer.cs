namespace Converter.GUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cancellationTokenSource?.Dispose();
                components?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
                         groupBoxFiles = new GroupBox();
                         buttonBrowseOutput = new Button();
            textBoxOutputPath = new TextBox();
            labelOutputPath = new Label();
            buttonBrowseMesh = new Button();
            buttonPreviewMesh = new Button();
            textBoxMeshPath = new TextBox();
            labelMeshPath = new Label();
             buttonBrowseTemplate = new Button();
             textBoxTemplatePath = new TextBox();
             labelTemplatePath = new Label();
             groupBoxBlueprintInfo = new GroupBox();
             labelBlueprintTypeValue = new Label();
             labelBlueprintType = new Label();
             labelBlueprintNameValue = new Label();
             labelBlueprintName = new Label();
             labelBlueprintWarning = new Label();
             groupBoxParameters = new GroupBox();
            numericUpDownMaxSize = new NumericUpDown();
            labelMaxSize = new Label();
            checkBoxHollow = new CheckBox();
            numericUpDownHollowRadius = new NumericUpDown();
            labelHollowRadius = new Label();
            numericUpDownDilationRadius = new NumericUpDown();
            labelDilationRadius = new Label();
            numericUpDownErosionRadius = new NumericUpDown();
            labelErosionRadius = new Label();
            numericUpDownResolution = new NumericUpDown();
            labelResolution = new Label();
            comboBoxBlockType = new ComboBox();
            labelBlockType = new Label();
            textBoxDisplayName = new TextBox();
            labelDisplayName = new Label();
            groupBoxProgress = new GroupBox();
            progressBar = new ProgressBar();
            textBoxStatus = new TextBox();
            buttonConvert = new Button();
            buttonCancel = new Button();
            openFileDialogTemplate = new OpenFileDialog();
            openFileDialogMesh = new OpenFileDialog();
                         groupBoxFiles.SuspendLayout();
             groupBoxBlueprintInfo.SuspendLayout();
             groupBoxParameters.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMaxSize).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownHollowRadius).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDilationRadius).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownErosionRadius).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownResolution).BeginInit();
            groupBoxProgress.SuspendLayout();
            SuspendLayout();
            // 
            // groupBoxFiles
            // 
            groupBoxFiles.Controls.Add(buttonBrowseOutput);
            groupBoxFiles.Controls.Add(textBoxOutputPath);
            groupBoxFiles.Controls.Add(labelOutputPath);
            groupBoxFiles.Controls.Add(buttonBrowseMesh);
            groupBoxFiles.Controls.Add(buttonPreviewMesh);
            groupBoxFiles.Controls.Add(textBoxMeshPath);
            groupBoxFiles.Controls.Add(labelMeshPath);
            groupBoxFiles.Controls.Add(buttonBrowseTemplate);
            groupBoxFiles.Controls.Add(textBoxTemplatePath);
            groupBoxFiles.Controls.Add(labelTemplatePath);
            groupBoxFiles.Location = new Point(12, 12);
            groupBoxFiles.Name = "groupBoxFiles";
            groupBoxFiles.Size = new Size(560, 120);
            groupBoxFiles.TabIndex = 0;
            groupBoxFiles.TabStop = false;
            groupBoxFiles.Text = "Files";
            // 
            // buttonBrowseOutput
            // 
            buttonBrowseOutput.Location = new Point(479, 85);
            buttonBrowseOutput.Name = "buttonBrowseOutput";
            buttonBrowseOutput.Size = new Size(75, 23);
            buttonBrowseOutput.TabIndex = 8;
            buttonBrowseOutput.Text = "Browse...";
            buttonBrowseOutput.UseVisualStyleBackColor = true;
            buttonBrowseOutput.Click += buttonBrowseOutput_Click;
            // 
            // textBoxOutputPath
            // 
            textBoxOutputPath.Location = new Point(120, 86);
            textBoxOutputPath.Name = "textBoxOutputPath";
            textBoxOutputPath.Size = new Size(353, 23);
            textBoxOutputPath.TabIndex = 7;
            // 
            // labelOutputPath
            // 
            labelOutputPath.AutoSize = true;
            labelOutputPath.Location = new Point(6, 89);
            labelOutputPath.Name = "labelOutputPath";
            labelOutputPath.Size = new Size(99, 15);
            labelOutputPath.TabIndex = 6;
            labelOutputPath.Text = "Output Directory:";
            // 
            // buttonBrowseMesh
            // 
            buttonBrowseMesh.Location = new Point(479, 56);
            buttonBrowseMesh.Name = "buttonBrowseMesh";
            buttonBrowseMesh.Size = new Size(75, 23);
            buttonBrowseMesh.TabIndex = 5;
            buttonBrowseMesh.Text = "Browse...";
            buttonBrowseMesh.UseVisualStyleBackColor = true;
            buttonBrowseMesh.Click += buttonBrowseMesh_Click;
            // 
            // buttonPreviewMesh
            // 
            buttonPreviewMesh.Location = new Point(398, 56);
            buttonPreviewMesh.Name = "buttonPreviewMesh";
            buttonPreviewMesh.Size = new Size(75, 23);
            buttonPreviewMesh.TabIndex = 6;
            buttonPreviewMesh.Text = "Preview...";
            buttonPreviewMesh.UseVisualStyleBackColor = true;
            buttonPreviewMesh.Click += buttonPreviewMesh_Click;
            // 
            // textBoxMeshPath
            // 
            textBoxMeshPath.Location = new Point(120, 57);
            textBoxMeshPath.Name = "textBoxMeshPath";
            textBoxMeshPath.Size = new Size(272, 23);
            textBoxMeshPath.TabIndex = 4;
            // 
            // labelMeshPath
            // 
            labelMeshPath.AutoSize = true;
            labelMeshPath.Location = new Point(6, 60);
            labelMeshPath.Name = "labelMeshPath";
            labelMeshPath.Size = new Size(60, 15);
            labelMeshPath.TabIndex = 3;
            labelMeshPath.Text = "Mesh File:";
            // 
            // buttonBrowseTemplate
            // 
            buttonBrowseTemplate.Location = new Point(479, 27);
            buttonBrowseTemplate.Name = "buttonBrowseTemplate";
            buttonBrowseTemplate.Size = new Size(75, 23);
            buttonBrowseTemplate.TabIndex = 2;
            buttonBrowseTemplate.Text = "Browse...";
            buttonBrowseTemplate.UseVisualStyleBackColor = true;
            buttonBrowseTemplate.Click += buttonBrowseTemplate_Click;
            // 
            // textBoxTemplatePath
            // 
            textBoxTemplatePath.Location = new Point(120, 28);
            textBoxTemplatePath.Name = "textBoxTemplatePath";
            textBoxTemplatePath.Size = new Size(353, 23);
            textBoxTemplatePath.TabIndex = 1;
            // 
            // labelTemplatePath
            // 
            labelTemplatePath.AutoSize = true;
            labelTemplatePath.Location = new Point(6, 31);
            labelTemplatePath.Name = "labelTemplatePath";
            labelTemplatePath.Size = new Size(110, 15);
            labelTemplatePath.TabIndex = 0;
                         labelTemplatePath.Text = "Template Blueprint:";
             // 
             // groupBoxBlueprintInfo
             // 
             groupBoxBlueprintInfo.Controls.Add(labelBlueprintWarning);
             groupBoxBlueprintInfo.Controls.Add(labelBlueprintTypeValue);
             groupBoxBlueprintInfo.Controls.Add(labelBlueprintType);
             groupBoxBlueprintInfo.Controls.Add(labelBlueprintNameValue);
             groupBoxBlueprintInfo.Controls.Add(labelBlueprintName);
             groupBoxBlueprintInfo.Location = new Point(12, 138);
             groupBoxBlueprintInfo.Name = "groupBoxBlueprintInfo";
             groupBoxBlueprintInfo.Size = new Size(560, 80);
             groupBoxBlueprintInfo.TabIndex = 1;
             groupBoxBlueprintInfo.TabStop = false;
             groupBoxBlueprintInfo.Text = "Blueprint Information";
             // 
             // labelBlueprintTypeValue
             // 
             labelBlueprintTypeValue.AutoSize = true;
             labelBlueprintTypeValue.Location = new Point(120, 25);
             labelBlueprintTypeValue.Name = "labelBlueprintTypeValue";
             labelBlueprintTypeValue.Size = new Size(75, 15);
             labelBlueprintTypeValue.TabIndex = 3;
             labelBlueprintTypeValue.Text = "Not selected";
             // 
             // labelBlueprintType
             // 
             labelBlueprintType.AutoSize = true;
             labelBlueprintType.Location = new Point(6, 25);
             labelBlueprintType.Name = "labelBlueprintType";
             labelBlueprintType.Size = new Size(88, 15);
             labelBlueprintType.TabIndex = 2;
             labelBlueprintType.Text = "Blueprint Type:";
             // 
             // labelBlueprintNameValue
             // 
             labelBlueprintNameValue.AutoSize = true;
             labelBlueprintNameValue.Location = new Point(120, 45);
             labelBlueprintNameValue.Name = "labelBlueprintNameValue";
             labelBlueprintNameValue.Size = new Size(75, 15);
             labelBlueprintNameValue.TabIndex = 1;
             labelBlueprintNameValue.Text = "Not selected";
             // 
             // labelBlueprintName
             // 
             labelBlueprintName.AutoSize = true;
             labelBlueprintName.Location = new Point(6, 45);
             labelBlueprintName.Name = "labelBlueprintName";
             labelBlueprintName.Size = new Size(96, 15);
             labelBlueprintName.TabIndex = 0;
             labelBlueprintName.Text = "Blueprint Name:";
             // 
             // labelBlueprintWarning
             // 
             labelBlueprintWarning.AutoSize = true;
             labelBlueprintWarning.ForeColor = Color.Red;
             labelBlueprintWarning.Location = new Point(290, 25);
             labelBlueprintWarning.Name = "labelBlueprintWarning";
             labelBlueprintWarning.Size = new Size(240, 30);
             labelBlueprintWarning.TabIndex = 4;
             labelBlueprintWarning.Text = "⚠ Warning: Template contains blocks other\r\nthan core. Please use a clean template.";
             labelBlueprintWarning.Visible = false;
             // 
             // groupBoxParameters
            // 
            groupBoxParameters.Controls.Add(numericUpDownMaxSize);
            groupBoxParameters.Controls.Add(labelMaxSize);
            groupBoxParameters.Controls.Add(checkBoxHollow);
            groupBoxParameters.Controls.Add(numericUpDownHollowRadius);
            groupBoxParameters.Controls.Add(labelHollowRadius);
            groupBoxParameters.Controls.Add(numericUpDownDilationRadius);
            groupBoxParameters.Controls.Add(labelDilationRadius);
            groupBoxParameters.Controls.Add(numericUpDownErosionRadius);
            groupBoxParameters.Controls.Add(labelErosionRadius);
            groupBoxParameters.Controls.Add(numericUpDownResolution);
            groupBoxParameters.Controls.Add(labelResolution);
            groupBoxParameters.Controls.Add(comboBoxBlockType);
            groupBoxParameters.Controls.Add(labelBlockType);
            groupBoxParameters.Controls.Add(textBoxDisplayName);
            groupBoxParameters.Controls.Add(labelDisplayName);
                         groupBoxParameters.Location = new Point(12, 224);
             groupBoxParameters.Name = "groupBoxParameters";
             groupBoxParameters.Size = new Size(560, 220);
             groupBoxParameters.TabIndex = 2;
            groupBoxParameters.TabStop = false;
            groupBoxParameters.Text = "Conversion Parameters";
            // 
            // numericUpDownMaxSize
            // 
            numericUpDownMaxSize.Location = new Point(120, 185);
            numericUpDownMaxSize.Maximum = new decimal(new int[] { 2000, 0, 0, 0 });
            numericUpDownMaxSize.Minimum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownMaxSize.Name = "numericUpDownMaxSize";
            numericUpDownMaxSize.Size = new Size(100, 23);
            numericUpDownMaxSize.TabIndex = 14;
                         numericUpDownMaxSize.Value = new decimal(new int[] { 100, 0, 0, 0 });
            // 
            // labelMaxSize
            // 
            labelMaxSize.AutoSize = true;
            labelMaxSize.Location = new Point(6, 187);
            labelMaxSize.Name = "labelMaxSize";
            labelMaxSize.Size = new Size(55, 15);
            labelMaxSize.TabIndex = 13;
            labelMaxSize.Text = "Max Size:";
            // 
            // checkBoxHollow
            // 
            checkBoxHollow.AutoSize = true;
            checkBoxHollow.Checked = true;
            checkBoxHollow.CheckState = CheckState.Checked;
            checkBoxHollow.Location = new Point(6, 127);
            checkBoxHollow.Name = "checkBoxHollow";
            checkBoxHollow.Size = new Size(126, 19);
            checkBoxHollow.TabIndex = 12;
            checkBoxHollow.Text = "Create Hollow Hull";
            checkBoxHollow.UseVisualStyleBackColor = true;
            checkBoxHollow.CheckedChanged += checkBoxHollow_CheckedChanged;
            // 
            // numericUpDownHollowRadius
            // 
            numericUpDownHollowRadius.Location = new Point(120, 156);
            numericUpDownHollowRadius.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownHollowRadius.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numericUpDownHollowRadius.Name = "numericUpDownHollowRadius";
            numericUpDownHollowRadius.Size = new Size(100, 23);
            numericUpDownHollowRadius.TabIndex = 11;
            numericUpDownHollowRadius.Value = new decimal(new int[] { 1, 0, 0, 0 });
            // 
            // labelHollowRadius
            // 
            labelHollowRadius.AutoSize = true;
            labelHollowRadius.Location = new Point(6, 158);
            labelHollowRadius.Name = "labelHollowRadius";
            labelHollowRadius.Size = new Size(86, 15);
            labelHollowRadius.TabIndex = 10;
            labelHollowRadius.Text = "Hollow Radius:";
            // 
            // numericUpDownDilationRadius
            // 
            numericUpDownDilationRadius.Location = new Point(400, 98);
            numericUpDownDilationRadius.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownDilationRadius.Name = "numericUpDownDilationRadius";
            numericUpDownDilationRadius.Size = new Size(100, 23);
            numericUpDownDilationRadius.TabIndex = 9;
            // 
            // labelDilationRadius
            // 
            labelDilationRadius.AutoSize = true;
            labelDilationRadius.Location = new Point(290, 100);
            labelDilationRadius.Name = "labelDilationRadius";
            labelDilationRadius.Size = new Size(89, 15);
            labelDilationRadius.TabIndex = 8;
            labelDilationRadius.Text = "Dilation Radius:";
            // 
            // numericUpDownErosionRadius
            // 
            numericUpDownErosionRadius.Location = new Point(120, 98);
            numericUpDownErosionRadius.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownErosionRadius.Name = "numericUpDownErosionRadius";
            numericUpDownErosionRadius.Size = new Size(100, 23);
            numericUpDownErosionRadius.TabIndex = 7;
            // 
            // labelErosionRadius
            // 
            labelErosionRadius.AutoSize = true;
            labelErosionRadius.Location = new Point(6, 100);
            labelErosionRadius.Name = "labelErosionRadius";
            labelErosionRadius.Size = new Size(87, 15);
            labelErosionRadius.TabIndex = 6;
            labelErosionRadius.Text = "Erosion Radius:";
            // 
            // numericUpDownResolution
            // 
            numericUpDownResolution.DecimalPlaces = 1;
            numericUpDownResolution.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDownResolution.Location = new Point(400, 69);
            numericUpDownResolution.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
            numericUpDownResolution.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
            numericUpDownResolution.Name = "numericUpDownResolution";
            numericUpDownResolution.Size = new Size(100, 23);
            numericUpDownResolution.TabIndex = 5;
            numericUpDownResolution.Value = new decimal(new int[] { 10, 0, 0, 65536 });
            // 
            // labelResolution
            // 
            labelResolution.AutoSize = true;
            labelResolution.Location = new Point(290, 71);
            labelResolution.Name = "labelResolution";
            labelResolution.Size = new Size(66, 15);
            labelResolution.TabIndex = 4;
            labelResolution.Text = "Resolution:";
            // 
            // comboBoxBlockType
            // 
            comboBoxBlockType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxBlockType.FormattingEnabled = true;
            comboBoxBlockType.Location = new Point(120, 69);
            comboBoxBlockType.Name = "comboBoxBlockType";
            comboBoxBlockType.Size = new Size(150, 23);
            comboBoxBlockType.TabIndex = 3;
            // 
            // labelBlockType
            // 
            labelBlockType.AutoSize = true;
            labelBlockType.Location = new Point(6, 72);
            labelBlockType.Name = "labelBlockType";
            labelBlockType.Size = new Size(67, 15);
            labelBlockType.TabIndex = 2;
            labelBlockType.Text = "Block Type:";
            // 
            // textBoxDisplayName
            // 
            textBoxDisplayName.Location = new Point(120, 28);
            textBoxDisplayName.Name = "textBoxDisplayName";
            textBoxDisplayName.Size = new Size(353, 23);
            textBoxDisplayName.TabIndex = 1;
            // 
            // labelDisplayName
            // 
            labelDisplayName.AutoSize = true;
            labelDisplayName.Location = new Point(6, 31);
            labelDisplayName.Name = "labelDisplayName";
            labelDisplayName.Size = new Size(83, 15);
            labelDisplayName.TabIndex = 0;
            labelDisplayName.Text = "Display Name:";
            // 
            // groupBoxProgress
            // 
            groupBoxProgress.Controls.Add(progressBar);
            groupBoxProgress.Controls.Add(textBoxStatus);
                         groupBoxProgress.Location = new Point(12, 450);
             groupBoxProgress.Name = "groupBoxProgress";
             groupBoxProgress.Size = new Size(560, 130);
             groupBoxProgress.TabIndex = 3;
            groupBoxProgress.TabStop = false;
            groupBoxProgress.Text = "Progress";
            // 
                         // progressBar
             // 
             progressBar.Location = new Point(6, 22);
             progressBar.Name = "progressBar";
             progressBar.Size = new Size(548, 15);
             progressBar.Style = ProgressBarStyle.Marquee;
             progressBar.TabIndex = 1;
             progressBar.Visible = false;
            // 
                         // textBoxStatus
             // 
             textBoxStatus.Location = new Point(6, 43);
             textBoxStatus.Multiline = true;
             textBoxStatus.Name = "textBoxStatus";
             textBoxStatus.ReadOnly = true;
             textBoxStatus.ScrollBars = ScrollBars.Vertical;
             textBoxStatus.Size = new Size(548, 77);
             textBoxStatus.TabIndex = 0;
            // 
            // buttonConvert
            // 
                         buttonConvert.Location = new Point(416, 586);
             buttonConvert.Name = "buttonConvert";
             buttonConvert.Size = new Size(75, 32);
             buttonConvert.TabIndex = 4;
            buttonConvert.Text = "Convert";
            buttonConvert.UseVisualStyleBackColor = true;
            buttonConvert.Click += buttonConvert_Click;
            // 
            // buttonCancel
            // 
                         buttonCancel.Location = new Point(497, 586);
             buttonCancel.Name = "buttonCancel";
             buttonCancel.Size = new Size(75, 32);
             buttonCancel.TabIndex = 5;
            buttonCancel.Text = "Cancel";
            buttonCancel.UseVisualStyleBackColor = true;
            buttonCancel.Click += buttonCancel_Click;
            // 
            // openFileDialogTemplate
            // 
            openFileDialogTemplate.Filter = "Empyrion Blueprint Files (*.epb)|*.epb|All Files (*.*)|*.*";
            openFileDialogTemplate.Title = "Select Template Blueprint";
            // 
            // openFileDialogMesh
            // 
            openFileDialogMesh.Filter = "STL Files (*.stl)|*.stl|All Files (*.*)|*.*";
            openFileDialogMesh.Title = "Select Mesh File";
            // 
            // Form1
            // 
                         AutoScaleDimensions = new SizeF(7F, 15F);
             AutoScaleMode = AutoScaleMode.Font;
             ClientSize = new Size(584, 627);
                         Controls.Add(buttonCancel);
             Controls.Add(buttonConvert);
             Controls.Add(groupBoxProgress);
             Controls.Add(groupBoxParameters);
             Controls.Add(groupBoxBlueprintInfo);
             Controls.Add(groupBoxFiles);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Empyrion Mesh Converter";
                         groupBoxFiles.ResumeLayout(false);
             groupBoxFiles.PerformLayout();
             groupBoxBlueprintInfo.ResumeLayout(false);
             groupBoxBlueprintInfo.PerformLayout();
             groupBoxParameters.ResumeLayout(false);
             groupBoxParameters.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numericUpDownMaxSize).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownHollowRadius).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownDilationRadius).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownErosionRadius).EndInit();
            ((System.ComponentModel.ISupportInitialize)numericUpDownResolution).EndInit();
            groupBoxProgress.ResumeLayout(false);
            groupBoxProgress.PerformLayout();
            ResumeLayout(false);

        }

        #endregion

                 private System.Windows.Forms.GroupBox groupBoxFiles;
         private System.Windows.Forms.Button buttonBrowseTemplate;
         private System.Windows.Forms.TextBox textBoxTemplatePath;
         private System.Windows.Forms.Label labelTemplatePath;
                 private System.Windows.Forms.Button buttonBrowseMesh;
        private System.Windows.Forms.Button buttonPreviewMesh;
        private System.Windows.Forms.TextBox textBoxMeshPath;
        private System.Windows.Forms.Label labelMeshPath;
        private System.Windows.Forms.Button buttonBrowseOutput;
         private System.Windows.Forms.TextBox textBoxOutputPath;
         private System.Windows.Forms.Label labelOutputPath;
         private System.Windows.Forms.GroupBox groupBoxBlueprintInfo;
         private System.Windows.Forms.Label labelBlueprintTypeValue;
         private System.Windows.Forms.Label labelBlueprintType;
         private System.Windows.Forms.Label labelBlueprintNameValue;
         private System.Windows.Forms.Label labelBlueprintName;
         private System.Windows.Forms.Label labelBlueprintWarning;
         private System.Windows.Forms.GroupBox groupBoxParameters;
        private System.Windows.Forms.TextBox textBoxDisplayName;
        private System.Windows.Forms.Label labelDisplayName;
        private System.Windows.Forms.ComboBox comboBoxBlockType;
        private System.Windows.Forms.Label labelBlockType;
        private System.Windows.Forms.NumericUpDown numericUpDownResolution;
        private System.Windows.Forms.Label labelResolution;
        private System.Windows.Forms.NumericUpDown numericUpDownErosionRadius;
        private System.Windows.Forms.Label labelErosionRadius;
        private System.Windows.Forms.NumericUpDown numericUpDownDilationRadius;
        private System.Windows.Forms.Label labelDilationRadius;
        private System.Windows.Forms.NumericUpDown numericUpDownHollowRadius;
        private System.Windows.Forms.Label labelHollowRadius;
        private System.Windows.Forms.CheckBox checkBoxHollow;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxSize;
        private System.Windows.Forms.Label labelMaxSize;
        private System.Windows.Forms.GroupBox groupBoxProgress;
        private System.Windows.Forms.TextBox textBoxStatus;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Button buttonCancel;
                 private System.Windows.Forms.OpenFileDialog openFileDialogTemplate;
         private System.Windows.Forms.OpenFileDialog openFileDialogMesh;
    }
}
