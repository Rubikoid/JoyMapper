using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using SharpDX.DirectInput;

namespace JoyMapper {
    public partial class MainForm : Form {
        private Thread bg_thread = null;
        public MainForm() {
            InitializeComponent();
            this.loadControllers();
        }

        private void DoWork(object data) {
            IEnumerable<IController> conrts = data as IEnumerable<IController>;
            try {
                foreach (IController gc in conrts) gc.Connect();
                ControllerCache.vc.Connect();
                while (true) {
                    State ins = new State(ControllerCache.vc, ControllerCache.vc.ButtonCount);
                    foreach (IController gc in conrts) gc.FillExternalInfo(ref ins);
                    ControllerCache.vc.UpdateInfo(ins);
                    Thread.Sleep(20);
                }
            } catch (ThreadAbortException) {
                Console.WriteLine("Stopping");
            } catch (Exception ex) {
                Console.WriteLine($"SHIT SHIT {ex}");
            } finally {
                foreach (IController gc in conrts) gc.Disconnect();
                ControllerCache.vc.Disconnect();
                Console.WriteLine("Disconnected");
            }
        }

        private void loadControllers() {
            ControllerCache.Update((name, cont) => { this.GameControllers.Items.Add(name); });
        }

        private void button1_Click(object sender, EventArgs e) {
            IEnumerable<GameController> controllers = ControllerCache.controllerDictionary
                .Where(x => this.GameControllers.CheckedItems.Contains(x.Key))
                .Select(x => x.Value);
            this.GameControllers.Enabled = false;
            this.bg_thread = new Thread(this.DoWork);
            this.bg_thread.Start(controllers);
        }

        private void button2_Click(object sender, EventArgs e) {
            this.bg_thread.Abort();
            this.GameControllers.Enabled = true;
            // GameController c = this.cboGameController.SelectedValue as GameController;
        }

        private void UpdateBtn_Click(object sender, EventArgs e) {
            this.loadControllers();
        }

        private void CreateMapBtn_Click(object sender, EventArgs e) {
            string contName = this.GameControllers.SelectedItem as string;
            if(contName != null && ControllerCache.controllerDictionary.ContainsKey(contName)) {
                ControllerMap mapForm = new ControllerMap(ControllerCache.controllerDictionary[contName]);
                mapForm.Show();
            }
        }
    }
}
