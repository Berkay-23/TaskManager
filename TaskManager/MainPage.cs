using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using System.Management;

namespace TaskManager
{
    public partial class MainPage : MetroFramework.Forms.MetroForm
    {
        Computer computer = new Computer() { CPUEnabled = true, GPUEnabled = true, RAMEnabled = true};
        PowerStatus PowerStatus = SystemInformation.PowerStatus;

        private double cpuValue { get; set; }
        private double ramValue { get; set; }
        private double gpuValue { get; set; }
        private double storageValue { get; set; }
        private int batteryValue { get; set; }

        public MainPage()
        {
            InitializeComponent();
        }
        // --------------------------------------------------------------------
        private void MainPage_Load(object sender, EventArgs e)
        {
            computer.Open();
            GetDrivers();
            comboBoxDrivers.SelectedIndex = 0;

            if (!Intro.isTheOpenMainPage)
            {
                GetDetails();
                batteryValue = (int)(PowerStatus.BatteryLifePercent * 100);
                timer_Loader.Start();
                Intro.isTheOpenMainPage = true;
            }
            else
            {
                timer.Start();
                timer_Loader.Stop();
                timer_unloader.Stop();
                FillDetails();
            }
        }

        private void panelCPU_MouseDown(object sender, MouseEventArgs e)
        {
            FormCPU formCPU = new FormCPU();
            formCPU.Show();
            this.Hide();
        }
        private void panelRAM_MouseDown(object sender, MouseEventArgs e)
        {
            FormRAM formRAM = new FormRAM();
            formRAM.Show();
            this.Hide();
        }

        private void panelGPU_MouseDown(object sender, MouseEventArgs e)
        {
            FormGPU formGPU = new FormGPU();
            formGPU.Show();
            this.Hide();
        }

        private void panelStorage_MouseDown(object sender, MouseEventArgs e)
        {
            FormStorage storage = new FormStorage();
            storage.Show();
            this.Hide();
        }

        // --------------------------------------------------------------------
        private void timer_Tick(object sender, EventArgs e)
        {
            FillDetails();
        }

        private void GetDetails()
        {
            // CPU GPU RAM Info
            foreach (IHardware hardware in computer.Hardware)
            {
                hardware.Update();

                foreach (ISensor sensor in hardware.Sensors)
                {

                    if (sensor.Name.Equals("CPU Total") && sensor.SensorType.Equals(SensorType.Load))
                    {
                        cpuValue = (double)sensor.Value;
                    }

                    else if (sensor.Name.Equals("Memory") && sensor.SensorType.Equals(SensorType.Load))
                    {
                        ramValue = (double)sensor.Value;
                    }

                    else if (sensor.Name.Equals("GPU Core") && sensor.SensorType.Equals(SensorType.Load))
                    {
                        gpuValue = (double)sensor.Value;
                    }
                }
            }
            StorageChecker();
        }
        private void FillDetails()
        {
            GetDetails();

            progressBarCPU.Value = Convert.ToInt32(cpuValue);
            lblCPUStatus.Text = String.Format("%{0:0.00}", cpuValue);

            progressBarRAM.Value = Convert.ToInt32(ramValue);
            lblRAMStatus.Text = String.Format("%{0:0.00}", ramValue);

            progressBarGPU.Value = Convert.ToInt32(gpuValue);
            lblGPUStatus.Text = String.Format("%{0:0.00}", gpuValue);

            progressBarStorage.Value = (int)storageValue;
            lblStorageStatus.Text = String.Format("%{0:0.00}", storageValue);

            BatteryInformationFiller();
        }
        public void StorageChecker()
        {
            string driveName = comboBoxDrivers.SelectedItem.ToString();
            double totalSpace = DriveInfo.GetDrives().Where(x => x.Name == driveName && x.IsReady).FirstOrDefault().TotalSize;
            double freeSpace = DriveInfo.GetDrives().Where(x => x.Name == driveName && x.IsReady).FirstOrDefault().TotalFreeSpace;

            double filledSpace = totalSpace - freeSpace;
            double filledPercent = filledSpace * 100 / totalSpace;

            storageValue = filledPercent;
        }

        private void BatteryInformationFiller()
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            int battery_percentage = (int)(powerStatus.BatteryLifePercent * 100);

            if (battery_percentage < 100)
            {
                progressBarBattery.Value = battery_percentage;
                lblBatteryLevel.Text = $"%{battery_percentage}";
            }
            else if (battery_percentage == 100)
            {
                progressBarBattery.Value = battery_percentage;
                lblBatteryLevel.Text = $"FULL";
            }
            else
            {
                progressBarBattery.Value = 0;
            }

            switch (powerStatus.PowerLineStatus)
            {
                case PowerLineStatus.Online:
                    pctBatteryEnergy.Visible = true;
                    lblBatteryLevel.Visible = false;
                    break;

                case PowerLineStatus.Offline:
                    pctBatteryEnergy.Visible = false;
                    lblBatteryLevel.Visible = true;
                    break;
            }
        }
        private void GetDrivers()
        {
            DriveInfo[] allDrives = DriveInfo.GetDrives();

            foreach (DriveInfo driver in allDrives)
            {
                comboBoxDrivers.Items.Add(driver.Name);
            }
        }

        // Greeting timers ------------------------------------------------
        int counter = 0;
        private void timer_Loader_Tick(object sender, EventArgs e)
        {
            counter += 2;
            progressBarCPU.Value = counter;
            progressBarGPU.Value = counter;
            progressBarRAM.Value = counter;
            progressBarStorage.Value = counter;
            progressBarBattery.Value = counter;

            if (counter == 100)
            {
                timer_Loader.Stop();
                timer_unloader.Start();
            }
        }
        private void timer_unloader_Tick(object sender, EventArgs e)
        {
            counter -= 2;
            progressBarCPU.Value = counter;
            progressBarGPU.Value = counter;
            progressBarRAM.Value = counter;
            progressBarBattery.Value = counter;
            progressBarStorage.Value = counter;

            if (counter == 0)
            {
                timer_unloader.Stop();
                timer_PB_filler.Start();
            }
        }

        private void timer_PB_filler_Tick(object sender, EventArgs e)
        {
            counter += 1;

            if (counter <= cpuValue)
            {
                progressBarCPU.Value = counter;
                lblCPUStatus.Text = String.Format("%{0:0.00}", counter);
            }

            if (counter <= gpuValue)
            {
                progressBarGPU.Value = counter;
                lblGPUStatus.Text = String.Format("%{0:0.00}", counter);
            }

            if (counter <= ramValue)
            {
                progressBarRAM.Value = counter;
                lblRAMStatus.Text = String.Format("%{0:0.00}", counter);
            }

            if (counter <= storageValue)
            {
                progressBarStorage.Value = counter;
                lblStorageStatus.Text = String.Format("%{0:0.00}", counter);
            }

            if (counter <= batteryValue)
            {
                progressBarBattery.Value = counter;
            }

            if (counter == 100)
            {
                FillDetails();
                timer_PB_filler.Stop();
                timer.Start();
            }
        }
        // btnProcess -----------------------------------------------------
        private void btnProcess_Click(object sender, EventArgs e)
        {
            ProcessList processList = new ProcessList();
            processList.Show();
            this.Hide();
        }
        private void btnProcess_MouseHover(object sender, EventArgs e)
        {
            pctrProcessListLine.Visible = true;
        }
        private void btnProcess_MouseLeave(object sender, EventArgs e)
        {
            pctrProcessListLine.Visible = false;
        }
        // pctRefresh ---------------------------------------------------
        private void pctRefresh_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
        private void pctRefresh_MouseHover(object sender, EventArgs e)
        {
            pctrRefresfLine.Visible = true;
        }
        private void pctRefresh_MouseLeave(object sender, EventArgs e)
        {
            pctrRefresfLine.Visible = false;
        }
        // btnExitApplication ---------------------------------------------
        private void btnExitApplication_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}