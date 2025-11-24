using System;
using System.Drawing;
using System.Windows.Forms;

namespace BlockManager.Core
{
    partial class BlockLibraryViewer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                pictureBox?.Image?.Dispose();
                treeView?.ImageList?.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.treeView = new TreeView();
            this.pictureBox = new PictureBox();
            this.statusLabel = new Label();
            
            SuspendLayout();

            // 
            // Form
            // 
            this.Text = "块库浏览器";
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MinimumSize = new Size(600, 400);

            // 
            // treeView
            // 
            this.treeView.Location = new Point(12, 12);
            this.treeView.Size = new Size(300, 520);
            this.treeView.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.treeView.HideSelection = false;
            this.treeView.ShowLines = true;
            this.treeView.ShowPlusMinus = true;
            this.treeView.ShowRootLines = true;
            this.treeView.NodeMouseDoubleClick += TreeView_NodeMouseDoubleClick;
            // 
            // pictureBox
            // 
            this.pictureBox.Location = new Point(330, 12);
            this.pictureBox.Size = new Size(442, 520);
            this.pictureBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.pictureBox.BorderStyle = BorderStyle.FixedSingle;
            this.pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            this.pictureBox.BackColor = Color.White;

            // 
            // statusLabel
            // 
            this.statusLabel.Location = new Point(12, 545);
            this.statusLabel.Size = new Size(760, 23);
            this.statusLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.statusLabel.Text = "就绪";
            this.statusLabel.AutoEllipsis = true;

            // 
            // BlockLibraryViewer
            // 
            this.Controls.Add(this.treeView);
            this.Controls.Add(this.pictureBox);
            this.Controls.Add(this.statusLabel);

            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.ResumeLayout(false);
        }

  

        #endregion

        private TreeView treeView;
        private PictureBox pictureBox;
        private Label statusLabel;
    }
}
