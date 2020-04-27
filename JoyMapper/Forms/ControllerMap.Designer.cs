namespace JoyMapper {
    partial class ControllerMap {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.AxisGroup = new System.Windows.Forms.FlowLayoutPanel();
            this.AxisSetting_Template = new System.Windows.Forms.Panel();
            this.AxisSettingComboBox_Template = new System.Windows.Forms.ComboBox();
            this.AxisSettingName_Template = new System.Windows.Forms.Label();
            this.AxisLabel = new System.Windows.Forms.Label();
            this.CapbsTextBox = new System.Windows.Forms.RichTextBox();
            this.AxisGroup.SuspendLayout();
            this.AxisSetting_Template.SuspendLayout();
            this.SuspendLayout();
            // 
            // AxisGroup
            // 
            this.AxisGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AxisGroup.AutoScroll = true;
            this.AxisGroup.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.AxisGroup.Controls.Add(this.AxisSetting_Template);
            this.AxisGroup.Location = new System.Drawing.Point(12, 29);
            this.AxisGroup.Name = "AxisGroup";
            this.AxisGroup.Size = new System.Drawing.Size(206, 365);
            this.AxisGroup.TabIndex = 0;
            // 
            // AxisSetting_Template
            // 
            this.AxisSetting_Template.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AxisSetting_Template.Controls.Add(this.AxisSettingComboBox_Template);
            this.AxisSetting_Template.Controls.Add(this.AxisSettingName_Template);
            this.AxisSetting_Template.Enabled = false;
            this.AxisSetting_Template.Location = new System.Drawing.Point(3, 3);
            this.AxisSetting_Template.Name = "AxisSetting_Template";
            this.AxisSetting_Template.Size = new System.Drawing.Size(148, 50);
            this.AxisSetting_Template.TabIndex = 0;
            this.AxisSetting_Template.Visible = false;
            // 
            // AxisSettingComboBox_Template
            // 
            this.AxisSettingComboBox_Template.FormattingEnabled = true;
            this.AxisSettingComboBox_Template.Location = new System.Drawing.Point(6, 20);
            this.AxisSettingComboBox_Template.Name = "AxisSettingComboBox_Template";
            this.AxisSettingComboBox_Template.Size = new System.Drawing.Size(139, 24);
            this.AxisSettingComboBox_Template.TabIndex = 2;
            // 
            // AxisSettingName_Template
            // 
            this.AxisSettingName_Template.AutoSize = true;
            this.AxisSettingName_Template.Location = new System.Drawing.Point(3, 0);
            this.AxisSettingName_Template.Name = "AxisSettingName_Template";
            this.AxisSettingName_Template.Size = new System.Drawing.Size(53, 17);
            this.AxisSettingName_Template.TabIndex = 0;
            this.AxisSettingName_Template.Text = "{name}";
            // 
            // AxisLabel
            // 
            this.AxisLabel.AutoSize = true;
            this.AxisLabel.Location = new System.Drawing.Point(12, 9);
            this.AxisLabel.Name = "AxisLabel";
            this.AxisLabel.Size = new System.Drawing.Size(33, 17);
            this.AxisLabel.TabIndex = 1;
            this.AxisLabel.Text = "Axis";
            // 
            // CapbsTextBox
            // 
            this.CapbsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CapbsTextBox.Location = new System.Drawing.Point(11, 400);
            this.CapbsTextBox.Name = "CapbsTextBox";
            this.CapbsTextBox.ReadOnly = true;
            this.CapbsTextBox.Size = new System.Drawing.Size(207, 155);
            this.CapbsTextBox.TabIndex = 2;
            this.CapbsTextBox.Text = "";
            // 
            // ControllerMap
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 567);
            this.Controls.Add(this.CapbsTextBox);
            this.Controls.Add(this.AxisLabel);
            this.Controls.Add(this.AxisGroup);
            this.Name = "ControllerMap";
            this.Text = "ControllerMap";
            this.Load += new System.EventHandler(this.ControllerMap_Load);
            this.AxisGroup.ResumeLayout(false);
            this.AxisSetting_Template.ResumeLayout(false);
            this.AxisSetting_Template.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FlowLayoutPanel AxisGroup;
        private System.Windows.Forms.Label AxisLabel;
        private System.Windows.Forms.Panel AxisSetting_Template;
        private System.Windows.Forms.Label AxisSettingName_Template;
        private System.Windows.Forms.ComboBox AxisSettingComboBox_Template;
        private System.Windows.Forms.RichTextBox CapbsTextBox;
    }
}