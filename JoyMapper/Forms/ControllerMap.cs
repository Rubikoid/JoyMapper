using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JoyMapper {
    public class ExtendedComboBox : ComboBox {
        public JoystickCapabilities axis;
    }

    public partial class ControllerMap : Form {
        private IController controller;
        public ControllerMap(IController controller) {
            this.InitializeComponent();
            this.controller = controller;
        }

        private void ControllerMap_Load(object sender, EventArgs e) {
            this.controller.Connect();
            this.controller.Disconnect();
            this.CapbsTextBox.Text = $"Buttons:{this.controller.ButtonCount}\n" +
                                     string.Join("\n", this.controller.Capabilities.Select(x => x.ToString()));
            foreach (JoystickCapabilities cap in this.controller.Capabilities) {
                this.createNewAxis(cap);
            }
        }

        private void createNewAxis(JoystickCapabilities cap) {
            Panel AxisSetting = new Panel();
            Label AxisSettingName = new Label();
            ExtendedComboBox AxisSettingComboBox = new ExtendedComboBox();

            // 
            // AxisSetting
            // 
            AxisSetting.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
            AxisSetting.Controls.Add(AxisSettingComboBox);
            AxisSetting.Controls.Add(AxisSettingName);
            AxisSetting.Location = new Point(3, 3);
            AxisSetting.Name = $"AxisSetting_{cap.ToString()}";
            AxisSetting.Size = new Size(130, 50);
            AxisSetting.TabIndex = 0;
            // 
            // AxisSettingComboBox
            // 
            AxisSettingComboBox.FormattingEnabled = true;
            AxisSettingComboBox.Location = new Point(0, 20);
            AxisSettingComboBox.Name = $"AxisSettingComboBox_{cap.ToString()}";
            AxisSettingComboBox.Size = new Size(125, 24);
            AxisSettingComboBox.TabIndex = 2;
            AxisSettingComboBox.SelectedIndexChanged += this.AxisSettingComboBox_SelectedIndexChanged;
            AxisSettingComboBox.axis = cap;
            // 
            // AxisSettingName
            // 
            AxisSettingName.AutoSize = true;
            AxisSettingName.Location = new Point(0, 0);
            AxisSettingName.Name = $"AxisSettingName_{cap.ToString()}";
            AxisSettingName.Size = new Size(64, 16);
            AxisSettingName.TabIndex = 0;
            AxisSettingName.Text = $"{cap.ToString()}";

            AxisSettingComboBox.Items.AddRange(ControllerCache.vc.Capabilities.Select(x => x.ToString()).ToArray());
            AxisMap aMap = this.controller.Mappings
                .OfType<AxisMap>()
                .Where(x => x.inAxis == cap)
                .DefaultIfEmpty(null)
                .FirstOrDefault();
            if (aMap != null)
                AxisSettingComboBox.SelectedItem = aMap.outAxis.ToString();
            this.AxisGroup.Controls.Add(AxisSetting);
        }

        private void AxisSettingComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            ExtendedComboBox AxisSettingComboBox = sender as ExtendedComboBox;
            JoystickCapabilities nextCap = (JoystickCapabilities) Enum.Parse(typeof(JoystickCapabilities), AxisSettingComboBox.SelectedItem as string);
            AxisMap aMap = this.controller.Mappings
                .OfType<AxisMap>()
                .Where(x => x.inAxis == AxisSettingComboBox.axis)
                .DefaultIfEmpty(null)
                .FirstOrDefault();
            if (aMap != null) {
                aMap.SetOut(nextCap);
            } else {
                this.controller.Mappings.Add(new AxisMap(AxisSettingComboBox.axis, nextCap));
            }
        }
    }
}
