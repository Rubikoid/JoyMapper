using JoyMapper.Controller;
using JoyMapper.Controller.Internal;
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
    public partial class ControllerMap : Form {
        public class ExtendedComboBox : ComboBox {
            // public JoystickCapabilities axis;
            public object data;
        }

        public class SettingPanel {
            public Panel Setting = new Panel();
            public Label SettingName = new Label();
            public ExtendedComboBox SettingComboBox = new ExtendedComboBox();

            public SettingPanel(EventHandler evh, string name, Action<ExtendedComboBox> comboFiller) {
                Setting.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right);
                Setting.Controls.Add(SettingComboBox);
                Setting.Controls.Add(SettingName);
                Setting.Location = new Point(3, 3);
                Setting.Name = $"Setting_{name}";
                Setting.Size = new Size(130, 50);
                Setting.TabIndex = 0;

                SettingComboBox.FormattingEnabled = true;
                SettingComboBox.Location = new Point(0, 20);
                SettingComboBox.Name = $"SettingComboBox_{name}";
                SettingComboBox.Size = new Size(125, 24);
                SettingComboBox.TabIndex = 2;
                SettingComboBox.SelectedIndexChanged += evh;

                SettingName.AutoSize = true;
                SettingName.Location = new Point(0, 0);
                SettingName.Name = $"SettingName_{name}";
                SettingName.Size = new Size(64, 16);
                SettingName.TabIndex = 0;
                SettingName.Text = $"{name}";

                comboFiller(SettingComboBox);

            }
        }

        private IController controller;
        private IList<SettingPanel> panels = new List<SettingPanel>();

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
                SettingPanel newPanel = new SettingPanel(
                AxisSettingComboBox_SelectedIndexChanged,
                cap.ToString(),
                (cb) => {
                    cb.data = cap;
                    cb.Items.AddRange(ControllerCache.vc.Capabilities.Select(x => x.ToString()).ToArray());

                    AxisMap aMap = this.controller.Mappings
                    .OfType<AxisMap>()
                    .Where(x => x.inAxis == cap)
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
                    if (aMap != null)
                        cb.SelectedItem = aMap.outAxis.ToString();
                });
                this.panels.Add(newPanel);
                this.AxisGroup.Controls.Add(newPanel.Setting);
            }

            // int x in (this.controller as GameController).FFBAxes
            if ((this.controller as GameController).FFBAxes != null && (this.controller as GameController).FFBAxes.Length > 0) {
                foreach (int vcAxis in ControllerCache.vc.FFBAxes) {
                    // axisN - axis on VC to get FFB
                    SettingPanel newPanel = new SettingPanel(
                FFBSettingComboBox_SelectedIndexChanged,
                $"VCAxis={vcAxis}",
                (cb) => {
                    cb.data = vcAxis;

                    // axis on GC to send FFB
                    cb.Items.AddRange((this.controller as GameController).FFBAxes.Select(f => f.ToString()).ToArray());

                    FFBMap aMap = this.controller.Mappings
                    .OfType<FFBMap>()
                    .Where(x => x.vcAxis == vcAxis)
                    .DefaultIfEmpty(null)
                    .FirstOrDefault();
                    if (aMap != null)
                        cb.SelectedItem = aMap.gcAxis.ToString();
                });
                    this.panels.Add(newPanel);
                    this.FFBGroup.Controls.Add(newPanel.Setting);
                }
            }
        }

        private void FFBSettingComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            ExtendedComboBox SettingComboBox = sender as ExtendedComboBox;
            int vcAxis = (int)SettingComboBox.data;
            int gcAxis = int.Parse(SettingComboBox.SelectedItem as string);
            FFBMap aMap = this.controller.Mappings
                .OfType<FFBMap>()
                .Where(x => x.gcAxis == vcAxis)
                .DefaultIfEmpty(null)
                .FirstOrDefault();
            if (aMap != null) {
                aMap.SetGC(gcAxis);
            } else {
                this.controller.Mappings.Add(new FFBMap(this.controller as GameController, vcAxis, gcAxis));
            }
        }

        private void AxisSettingComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            ExtendedComboBox AxisSettingComboBox = sender as ExtendedComboBox;
            JoystickCapabilities data = (JoystickCapabilities) AxisSettingComboBox.data;
            JoystickCapabilities nextCap = (JoystickCapabilities) Enum.Parse(typeof(JoystickCapabilities), AxisSettingComboBox.SelectedItem as string);
            AxisMap aMap = this.controller.Mappings
                .OfType<AxisMap>()
                .Where(x => x.inAxis == data)
                .DefaultIfEmpty(null)
                .FirstOrDefault();
            if (aMap != null) {
                aMap.SetOut(nextCap);
            } else {
                this.controller.Mappings.Add(new AxisMap(data, nextCap));
            }
        }
    }
}
