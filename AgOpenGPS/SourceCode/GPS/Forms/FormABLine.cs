﻿//Please, if you use this, share the improvements

using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace AgOpenGPS
{
    public partial class FormABLine : Form
    {
        //access to the main GPS form and all its variables
        private readonly FormGPS mf = null;

        private double upDnHeading = 0;

        //the abline stored file
        private string filename = "";

        public FormABLine(Form callingForm)
        {
            //get copy of the calling main form
            mf = callingForm as FormGPS;

            InitializeComponent();
        }

        private void FormABLine_Load(object sender, EventArgs e)
        {
            //different start based on AB line already set or not
            if (mf.ABLine.isABLineSet)
            {
                //AB line is on screen and set
                btnAPoint.Enabled = false;
                btnBPoint.Enabled = true;
                upDnHeading = Math.Round(glm.toDegrees(mf.ABLine.abHeading), 6);
                nudTramRepeats.Value = mf.ABLine.tramPassEvery;
                nudBasedOnPass.Value = mf.ABLine.passBasedOn;
                tboxHeading.Text = upDnHeading.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                //no AB line
                btnAPoint.Enabled = true;
                btnBPoint.Enabled = false;
                //btnABLineOk.Enabled = false;
                upDnHeading = Math.Round(glm.toDegrees(mf.fixHeading), 6);
                nudTramRepeats.Value = 0;
                nudBasedOnPass.Value = 0;
                mf.ABLine.tramPassEvery = 0;
                mf.ABLine.passBasedOn = 0;
            }

            //make sure at least a blank AB Line file exists
            string dirABLines = mf.ablinesDirectory;
            string directoryName = Path.GetDirectoryName(dirABLines).ToString(CultureInfo.InvariantCulture);
            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }
            filename = directoryName + "\\ABLines.txt";
            if (!File.Exists(filename))
            {
                using (StreamWriter writer = new StreamWriter(filename))
                {
                    writer.WriteLine("ABLine N S,0,0,0");
                    writer.WriteLine("ABLine E W,90,0,0");
                }
            }

            //get the file of previous AB Lines
            if ((directoryName.Length > 0) && (!Directory.Exists(directoryName)))
            { Directory.CreateDirectory(directoryName); }

            filename = directoryName + "\\ABLines.txt";

            if (!File.Exists(filename))
            {
                mf.TimedMessageBox(2000, "File Error", "Missing AB Line File, Critical Error");
            }
            else
            {
                using (StreamReader reader = new StreamReader(filename))
                {
                    try
                    {
                        string line;
                        ListViewItem itm;

                        //read all the lines
                        while (!reader.EndOfStream)
                        {
                            line = reader.ReadLine();
                            string[] words = line.Split(',');
                            //listboxLines.Items.Add(line);
                            itm = new ListViewItem(words);
                            lvLines.Items.Add(itm);

                            //coords.easting = double.Parse(words[0], CultureInfo.InvariantCulture);
                            //coords.northing = double.Parse(words[1], CultureInfo.InvariantCulture);
                            //youFileList.Add(coords);
                        }
                    }
                    catch (Exception er)
                    {
                        var form = new FormTimedMessage(4000, "ABLine File is Corrupt", "Please delete it!!!");
                        form.Show();
                        mf.WriteErrorLog("FieldOpen, Loading ABLine, Corrupt ABLine File" + er);
                    }
                }

                // go to bottom of list - if there is a bottom
                if (lvLines.Items.Count > 0) lvLines.Items[lvLines.Items.Count - 1].EnsureVisible();
            }
        }

        private void btnAPoint_Click(object sender, EventArgs e)
        {
#pragma warning disable CS1690 // Accessing a member on a field of a marshal-by-reference class may cause a runtime exception
            mf.ABLine.refPoint1.easting = mf.pn.fix.easting;
            mf.ABLine.refPoint1.northing = mf.pn.fix.northing;
#pragma warning restore CS1690 // Accessing a member on a field of a marshal-by-reference class may cause a runtime exception
            btnAPoint.Enabled = false;
            btnUpABHeading.Enabled = true;
            btnDnABHeading.Enabled = true;
            btnUpABHeadingBy1.Enabled = true;
            btnDnABHeadingBy1.Enabled = true;
            upDnHeading = Math.Round(glm.toDegrees(mf.fixHeading), 1);
            //tboxHeading.Text = upDnHeading.ToString(CultureInfo.InvariantCulture);
        }

        private void btnBPoint_Click(object sender, EventArgs e)
        {
            mf.ABLine.SetABLineByBPoint();
            btnABLineOk.Enabled = true;
            btnDeleteAB.Enabled = true;
            upDnHeading = Math.Round(glm.toDegrees(mf.fixHeading), 1);
        }

        private void btnUpABHeading_Click(object sender, EventArgs e)
        {
            if ((upDnHeading += 10) > 359.999999) upDnHeading = 0;
            upDnHeading = (int)upDnHeading;
            mf.ABLine.abHeading = glm.toRadians(upDnHeading);
            tboxHeading.Text = Convert.ToString(upDnHeading, CultureInfo.InvariantCulture);
            btnABLineOk.Enabled = true;
        }

        private void btnDownABHeading_Click(object sender, EventArgs e)
        {
            if ((upDnHeading -= 10) < 0) upDnHeading = 350;
            upDnHeading = (int)upDnHeading;
            mf.ABLine.abHeading = glm.toRadians(upDnHeading);
            tboxHeading.Text = Convert.ToString(upDnHeading, CultureInfo.InvariantCulture);
            btnABLineOk.Enabled = true;
        }

        private void btnUpABHeadingBy1_Click(object sender, EventArgs e)
        {
            if ((upDnHeading++) > 358) upDnHeading = 0;
            upDnHeading = (int)upDnHeading;
            mf.ABLine.abHeading = glm.toRadians(upDnHeading);
            tboxHeading.Text = Convert.ToString(upDnHeading, CultureInfo.InvariantCulture);
            btnABLineOk.Enabled = true;
        }

        private void btnDnABHeadingBy1_Click(object sender, EventArgs e)
        {
            upDnHeading--;
            if (upDnHeading < 0) upDnHeading = 359;
            upDnHeading = (int)upDnHeading;
            mf.ABLine.abHeading = glm.toRadians(upDnHeading);
            tboxHeading.Text = Convert.ToString(upDnHeading, CultureInfo.InvariantCulture);
            btnABLineOk.Enabled = true;
        }

        private void btnABLineOk_Click(object sender, EventArgs e)
        {
            //save the ABLine
            mf.FileSaveABLine();

            //update the default
            mf.AB0.fieldName = mf.currentFieldDirectory;
            mf.AB0.heading = glm.toDegrees(mf.ABLine.abHeading);
            mf.AB0.X = mf.ABLine.refPoint1.easting;
            mf.AB0.Y = mf.ABLine.refPoint1.northing;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDeleteAB_Click(object sender, EventArgs e)
        {
            mf.ABLine.DeleteAB();
            btnAPoint.Enabled = true;
            btnBPoint.Enabled = false;
            //btnDeleteAB.Enabled = false;
            //btnABLineOk.Enabled = false;
            nudTramRepeats.Value = 0;
            nudBasedOnPass.Value = 0;
            mf.ABLine.tramPassEvery = 0;
            mf.ABLine.passBasedOn = 0;

            //save the no ABLine;
            mf.FileSaveABLine();

            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            lblFixHeading.Text = Convert.ToString(Math.Round(glm.toDegrees(mf.fixHeading), 1)) + "°";
            lblKeepGoing.Text = "";
            lblABHeading.Text = Convert.ToString(Math.Round(glm.toDegrees(mf.ABLine.abHeading), 3));

            //make sure we go at least 3 or so meters before allowing B reference point
            if (!btnAPoint.Enabled && !btnBPoint.Enabled)
            {
                double pointAToFixDistance =
                Math.Pow(mf.ABLine.refPoint1.easting - mf.pn.fix.easting, 2)
                + Math.Pow(mf.ABLine.refPoint1.northing - mf.pn.fix.northing, 2);

                if (pointAToFixDistance > 100) btnBPoint.Enabled = true;
                else lblKeepGoing.Text = Convert.ToInt16(100 - pointAToFixDistance).ToString();
            }

            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                btnListDelete.Enabled = true;
                btnListUse.Enabled = true;
            }
            else
            {
                btnListDelete.Enabled = false;
                btnListUse.Enabled = false;
            }
        }

        private void nudTramRepeats_ValueChanged(object sender, EventArgs e)
        {
            mf.ABLine.tramPassEvery = (int)nudTramRepeats.Value;
        }

        private void nudBasedOnPass_ValueChanged(object sender, EventArgs e)
        {
            mf.ABLine.passBasedOn = (int)nudBasedOnPass.Value;
        }

        private void btnAddToFile_Click(object sender, EventArgs e)
        {
            using (StreamWriter writer = new StreamWriter(filename, true))
            {
                //make it culture invariant
                string line = mf.currentFieldDirectory + ',' + (Math.Round(glm.toDegrees(mf.ABLine.abHeading), 8)).ToString(CultureInfo.InvariantCulture)
                + ',' + (Math.Round(mf.ABLine.refPoint1.easting, 3)).ToString(CultureInfo.InvariantCulture)
                + ',' + (Math.Round(mf.ABLine.refPoint1.northing, 3)).ToString(CultureInfo.InvariantCulture);

                //write out to file
                writer.WriteLine(line);

                //update the list box
                ListViewItem itm;
                string[] words = line.Split(',');
                itm = new ListViewItem(words);
                lvLines.Items.Add(itm);

                // go to bottom of list - if there is a bottom
                if (lvLines.Items.Count > 0) lvLines.Items[lvLines.Items.Count - 1].EnsureVisible();
            }
        }

        private void tboxHeading_TextChanged(object sender, EventArgs e)
        {
            var textboxSender = (TextBox)sender;
            var cursorPosition = textboxSender.SelectionStart;
            textboxSender.Text = Regex.Replace(textboxSender.Text, "[^0-9.]", "");
            textboxSender.SelectionStart = cursorPosition;
            string line = tboxHeading.Text.Trim();
            if (line.Length == 0) line = "0";
            if (line == ".") line = "0";
            upDnHeading = double.Parse(line, CultureInfo.InvariantCulture);
            mf.ABLine.abHeading = glm.toRadians(Math.Round(upDnHeading, 6));
            mf.ABLine.SetABLineByHeading();
            btnABLineOk.Enabled = true;
        }

        private void btnListDelete_Click(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                lvLines.SelectedItems[0].Remove();
            }
            using (StreamWriter writer = new StreamWriter(filename))
            {
                string words;
                count = lvLines.Items.Count;
                if (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        words = lvLines.Items[i].SubItems[0].Text + "," +
                                lvLines.Items[i].SubItems[1].Text + "," +
                                lvLines.Items[i].SubItems[2].Text + "," +
                                lvLines.Items[i].SubItems[3].Text;
                        //out to file
                        writer.WriteLine(words);
                    }
                }
            }
        }

        private void btnListUse_Click(object sender, EventArgs e)
        {
            int count = lvLines.SelectedItems.Count;
            if (count > 0)
            {
                double temp = double.Parse(lvLines.SelectedItems[0].SubItems[1].Text, CultureInfo.InvariantCulture);
                mf.ABLine.abHeading = glm.toRadians(temp);
                temp = double.Parse(lvLines.SelectedItems[0].SubItems[2].Text, CultureInfo.InvariantCulture);
                mf.ABLine.refPoint1.easting = temp;
                temp = double.Parse(lvLines.SelectedItems[0].SubItems[3].Text, CultureInfo.InvariantCulture);
                mf.ABLine.refPoint1.northing = temp;
                mf.ABLine.SetABLineByHeading();

                //save the ABLine
                mf.FileSaveABLine();

                //update the default
                mf.AB0.fieldName = lvLines.SelectedItems[0].SubItems[0].Text;
                mf.AB0.heading = double.Parse(lvLines.SelectedItems[0].SubItems[1].Text, CultureInfo.InvariantCulture);
                mf.AB0.X = double.Parse(lvLines.SelectedItems[0].SubItems[2].Text, CultureInfo.InvariantCulture);
                mf.AB0.Y = double.Parse(lvLines.SelectedItems[0].SubItems[3].Text, CultureInfo.InvariantCulture);

                //can go back to Mainform without seeing ABLine form.
                DialogResult = DialogResult.Yes;
                Close();
            }

            //no item selected
            else
            {
                return;
            }
        }

        private void btnPlus90_Click(object sender, EventArgs e)
        {
            upDnHeading += 90;
            if (upDnHeading > 359.999999) upDnHeading -= 360;
            mf.ABLine.abHeading = glm.toRadians(upDnHeading);
            mf.ABLine.SetABLineByHeading();
            tboxHeading.Text = Convert.ToString(upDnHeading, CultureInfo.InvariantCulture);
            btnABLineOk.Enabled = true;
        }
    }
}