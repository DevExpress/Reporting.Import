#region DEMO_REMOVE

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.Design;

namespace DevExpress.XtraReports.Import {
    public class AccessReportSelectionForm : XtraForm {
        private System.Windows.Forms.ListView lvReports;
        private System.Windows.Forms.Label lblInfo;

        private DevExpress.XtraEditors.BaseButton btnOk;
        private DevExpress.XtraEditors.BaseButton btnCancel;
        private System.Windows.Forms.ImageList imageList;
        private System.ComponentModel.IContainer components;

        public string SelectedReport {
            get {
                if(lvReports.SelectedItems.Count <= 0)
                    return String.Empty;
                return lvReports.SelectedItems[0].Text;
            }
        }

        public void SetReportsList(string[] reports) {
            int count = reports.Length;
            for(int i = 0; i < count; i++)
                lvReports.Items.Add(new ListViewItem(reports[i], 0));

            if(count > 0)
                lvReports.TopItem.Selected = true;
        }

        public AccessReportSelectionForm() {
            InitializeComponent();

            DevExpress.Utils.ResourceImageHelper.FillImageListFromResources(imageList, typeof(DevExpress.XtraReports.Design.ResFinder).Namespace + ".Import.AccessReport.bmp", System.Reflection.Assembly.GetExecutingAssembly());
        }

        protected override void Dispose(bool disposing) {
            if(disposing) {
                if(components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.lvReports = new System.Windows.Forms.ListView();
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnOk = new DevExpress.XtraEditors.BaseButton();
            this.btnCancel = new DevExpress.XtraEditors.BaseButton();
            this.SuspendLayout();
            //
            // lvReports
            //
            this.lvReports.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lvReports.FullRowSelect = true;
            this.lvReports.HideSelection = false;
            this.lvReports.Location = new System.Drawing.Point(8, 34);
            this.lvReports.MultiSelect = false;
            this.lvReports.Name = "lvReports";
            this.lvReports.Size = new System.Drawing.Size(400, 187);
            this.lvReports.SmallImageList = this.imageList;
            this.lvReports.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvReports.TabIndex = 0;
            this.lvReports.UseCompatibleStateImageBehavior = false;
            this.lvReports.View = System.Windows.Forms.View.List;
            this.lvReports.DoubleClick += new System.EventHandler(this.lvReports_DoubleClick);
            //
            // imageList
            //
            this.imageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList.TransparentColor = System.Drawing.Color.Magenta;
            //
            // lblInfo
            //
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Location = new System.Drawing.Point(8, 9);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(400, 24);
            this.lblInfo.TabIndex = 3;
            this.lblInfo.Text = "Please select the report to convert from the following list:";
            //
            // btnOk
            //
            this.btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOk.Location = new System.Drawing.Point(248, 230);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 25);
            this.btnOk.TabIndex = 1;
            this.btnOk.Text = "OK";
            //
            // btnCancel
            //
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(336, 230);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 25);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            //
            // AccessReportSelectionForm
            //
            this.AcceptButton = this.btnOk;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(416, 266);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOk);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lvReports);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccessReportSelectionForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Report Conversion";
            this.TopMost = true;
            this.ResumeLayout(false);

        }


        private void lvReports_DoubleClick(object sender, System.EventArgs e) {
            Point pt = lvReports.PointToClient(Control.MousePosition);
            ListViewItem item = lvReports.GetItemAt(pt.X, pt.Y);
            if(item != null) {
                DialogResult = DialogResult.OK;
                Close();
            }
        }
    }
}

#endregion