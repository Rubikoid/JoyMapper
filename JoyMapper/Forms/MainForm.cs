using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using SharpDX.DirectInput;
using JoyMapper.Controller;
using JoyMapper.Controller.Internal;
using JoyMapper.FFB;
using System.ComponentModel;
using System.Reflection;

namespace JoyMapper {
    public partial class MainForm : Form {
        public static IntPtr PublicHandle { get; private set; }

        private Thread bg_thread = null;

        class FFBEvent {
            public string Name { get; set; }
            public int Count { get; set; }
        }
        private BindingList<FFBEvent> data = new BindingList<FFBEvent>() { };
        public MainForm() {
            InitializeComponent();
            Console.WriteLine($"Loading joymapper version: {Assembly.GetExecutingAssembly().GetName().Version.ToString()}");
            VirtualFFBPacketHandler.Init();
            this.loadControllers();
            MainForm.PublicHandle = Handle;

            /*dataGridView1.Columns.Add(new DataGridViewColumn() {
                HeaderText = "Name",
                Name = "name",
                Frozen = true,
                CellTemplate = new DataGridViewTextBoxCell(),
            });
            dataGridView1.Columns.Add(new DataGridViewColumn() {
                HeaderText = "Count",
                Name = "count",
                Frozen = true,
                CellTemplate = new DataGridViewTextBoxCell(),
            });*/
            this.dataGridView1.DataSource = data;
            // ControllerCache.vc.FFBDataReceived += this.callb;
        }

        private void DoWork(object data) {
            IEnumerable<IController> conrts = data as IEnumerable<IController>;
            try {
                Console.WriteLine("Connecting to controllers");
                foreach (IController gc in conrts) gc.Connect();
                ControllerCache.vc.Connect();
                ControllerCache.vc2.Connect();
                Console.WriteLine("Connecting to vjoy");
                while (true) {
                    State ins = new State(ControllerCache.vc, ControllerCache.vc.ButtonCount);
                    foreach (IController gc in conrts) gc.FillExternalInfo(ref ins);
                    ControllerCache.vc.UpdateInfo(ins);
                    ControllerCache.vc2.UpdateInfo(ins);
                    Thread.Sleep(20);
                }
            } catch (ThreadAbortException) {
                Console.WriteLine("Stopping");
            } catch (Exception ex) {
                Console.WriteLine($"SHIT SHIT {ex}");
            } finally {
                foreach (IController gc in conrts) gc.Disconnect();
                ControllerCache.vc.Disconnect();
                ControllerCache.vc2.Disconnect();
                Console.WriteLine("Disconnected");
            }
        }

        private void loadControllers() {
            ControllerCache.Update((name, cont) => { this.GameControllers.Items.Add(cont.ToString()); });
            Console.WriteLine("Controllers loaded");
        }

        private void button1_Click(object sender, EventArgs e) {
            IEnumerable<GameController> controllers = ControllerCache.controllerDictionary
                .Where(x => this.GameControllers.CheckedItems.Contains(x.Key))
                .Select(x => x.Value);
            //this.GameControllers.Enabled = false;
            this.StartBtn.Enabled = false;
            this.StopBtn.Enabled = true;
            this.bg_thread = new Thread(this.DoWork);
            this.bg_thread.Start(controllers);
        }

        private void button2_Click(object sender, EventArgs e) {
            this.bg_thread.Abort();
            //this.GameControllers.Enabled = true;
            this.StartBtn.Enabled = true;
            this.StopBtn.Enabled = false;
            // GameController c = this.cboGameController.SelectedValue as GameController;
        }

        private void UpdateBtn_Click(object sender, EventArgs e) {
            this.loadControllers();
        }

        private void CreateMapBtn_Click(object sender, EventArgs e) {
            string contName = this.GameControllers.SelectedItem as string;
            if (contName != null && ControllerCache.controllerDictionary.ContainsKey(contName)) {
                ControllerMap mapForm = new ControllerMap(ControllerCache.controllerDictionary[contName]);
                mapForm.Show();
            }
        }

        public void callb(object sender, VirtualFFBPacket e) {
            string key = e.FFBPType.ToString();
            this.dataGridView1.Invoke((MethodInvoker)(() => {
                FFBEvent ev = this.data.FirstOrDefault(x=>x.Name == key);
                if (ev != null) {
                    ev.Count++;
                    this.data.ResetItem(this.data.IndexOf(ev));
                } else
                    this.data.Add(new FFBEvent() { Name = e.FFBPType.ToString(), Count = 1 });
            }));
            /*if (this.dataGridView1..Items.ContainsKey(key)) {
                this.listView1.Items[key].Text = (int.Parse(this.listView1.Items[key].Text) + 1).ToString();
            }
            else {

            }*/
        }

        private void button1_Click_1(object sender, EventArgs e) {
            //var cont = ControllerCache.controllerDictionary.Where(x => x.Value.Name.ToLower().Contains("vjoy") && !x.Key.Contains("29") && x.Key.Contains("3e")).First().Value;
            var _cont = ControllerCache.controllerDictionary.Where(x => x.Value.ToString() == (string)this.GameControllers.SelectedItem).DefaultIfEmpty(new KeyValuePair<string, GameController>("", null)).FirstOrDefault();
            if (_cont.Value != null) {
                var cont = _cont.Value;
                if (cont.Connected) {
                    try {
                        var effpar = new EffectParameters() {
                            Duration = -1,
                            Flags = EffectFlags.Cartesian | EffectFlags.ObjectIds,
                            Gain = 100,
                            SamplePeriod = 0,
                            StartDelay = 0,
                            TriggerButton = -1,
                            TriggerRepeatInterval = 0,
                            Envelope = null,

                            Parameters = new ConstantForce() {
                                Magnitude = 100,
                            }
                        };
                        effpar.SetAxes(new int[1] { cont.FFBAxes[0] }, new int[1] { -1 });

                        Effect newE = new Effect(cont.joystick, EffectGuid.ConstantForce, effpar);

                        cont.RunExclusive(() => newE.Start());
                        Console.WriteLine($"{newE.Status}");
                    } catch (Exception ex) {
                        if (sender != null) {
                            Console.WriteLine($"WTF CONTROLLER {cont} SAY {ex}");
                            Thread.Sleep(200);
                            this.button1_Click_1(null, null);
                        } else {
                            Console.WriteLine($"WTF CONTROLLER {cont} DIEEE {ex}");
                        }
                    }
                } else {
                    if (sender != null) {
                        Console.WriteLine($"WTF CONTROLLER {cont} DISCONNECTD -> RECONNECT??");
                        cont.Connect();
                        Thread.Sleep(10);
                        cont.Disconnect();
                        Thread.Sleep(10);
                        cont.Connect();
                        Thread.Sleep(10);
                        this.button1_Click_1(null, null);
                    } else
                        Console.WriteLine($"WTF CONTROLLER {cont} DIE??");
                }
            }
        }
    }
}
