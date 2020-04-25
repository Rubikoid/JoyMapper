using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using SharpDX.DirectInput;

namespace JoyMapper {
    public partial class MainForm : Form {
        private Thread bg_thread = null;
        private VirtualController vc = null;
        private Dictionary<string, GameController> controllerDictionary = new Dictionary<string, GameController>();
        public MainForm() {
            InitializeComponent();
            this.loadControllers();
        }

        private void DoWork(object data) {
            IEnumerable<GameController> conrts = data as IEnumerable<GameController>;
            try {
                foreach (GameController gc in conrts) gc.Connect();
                if (vc == null)
                    vc = new VirtualController(1);
                vc.Connect();
                while (true) {
                    State ins = new State(vc);
                    foreach (GameController gc in conrts) gc.FillInfo(ref ins);
                    vc.UpdateInfo(ins);
                    Thread.Sleep(20);
                }
            } catch (ThreadAbortException ex) {
                Console.WriteLine("Stopping");
            } catch (Exception ex) {
                Console.WriteLine($"SHIT SHIT {ex}");
            } finally {
                foreach (GameController gc in conrts) gc.Disconnect();
                vc.Disconnect();
                Console.WriteLine("Disconnected");
            }
        }

        private void loadControllers() {
            // this.GameControllers.Items.Clear();
            // controllerDictionary.
            foreach (GameController controller in GameController.GetAll()) {
                if (!controllerDictionary.ContainsKey(controller.Name)) {
                    controllerDictionary.Add(controller.Name, controller);
                    this.GameControllers.Items.Add(controller.Name);
                }
                //if (controllerDictionary[controller.Name].Connected)
                //    this.GameControllers.SetItemChecked(this.GameControllers.Items.Count - 1, true);
            }
            
        }

        private void button1_Click(object sender, EventArgs e) {
            IEnumerable<GameController> controllers = this.controllerDictionary
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
    }
}
