namespace ProcNetClient
{
    partial class FormProcNetClient
    {
        /// <summary>
        /// Требуется переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Обязательный метод для поддержки конструктора - не изменяйте
        /// содержимое данного метода при помощи редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormProcNetClient));
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.idOpenBrowser = new System.Windows.Forms.ToolStripMenuItem();
            this.idShowWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.idAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.idClose = new System.Windows.Forms.ToolStripMenuItem();
            this.linkLabelOpenSite = new System.Windows.Forms.LinkLabel();
            this.checkBoxAutoLoad = new System.Windows.Forms.CheckBox();
            this.labelCountNewMessages = new System.Windows.Forms.Label();
            this.checkBoxDefaultBrowser = new System.Windows.Forms.CheckBox();
            this.buttonBrowse = new System.Windows.Forms.Button();
            this.comboBoxPathBrowser = new System.Windows.Forms.ComboBox();
            this.labelRestart = new System.Windows.Forms.Label();
            this.labelCountInQuerySend = new System.Windows.Forms.Label();
            this.checkBoxShowNewUsers = new System.Windows.Forms.CheckBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ProcNetClient";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.BalloonTipClicked += new System.EventHandler(this.notifyIcon1_BalloonTipClicked);
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.idOpenBrowser,
            this.idShowWindow,
            this.idAbout,
            this.toolStripMenuItem1,
            this.idClose});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(168, 98);
            // 
            // idOpenBrowser
            // 
            this.idOpenBrowser.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold);
            this.idOpenBrowser.Name = "idOpenBrowser";
            this.idOpenBrowser.Size = new System.Drawing.Size(167, 22);
            this.idOpenBrowser.Text = "Открыть сайт";
            this.idOpenBrowser.Click += new System.EventHandler(this.idOpenBrowser_Click);
            // 
            // idShowWindow
            // 
            this.idShowWindow.Font = new System.Drawing.Font("Tahoma", 8.25F);
            this.idShowWindow.Name = "idShowWindow";
            this.idShowWindow.Size = new System.Drawing.Size(167, 22);
            this.idShowWindow.Text = "Показать окно";
            this.idShowWindow.Click += new System.EventHandler(this.idShowWindow_Click);
            // 
            // idAbout
            // 
            this.idAbout.Name = "idAbout";
            this.idAbout.Size = new System.Drawing.Size(167, 22);
            this.idAbout.Text = "О программе";
            this.idAbout.Click += new System.EventHandler(this.idAbout_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(164, 6);
            // 
            // idClose
            // 
            this.idClose.Name = "idClose";
            this.idClose.Size = new System.Drawing.Size(167, 22);
            this.idClose.Text = "Выход";
            this.idClose.Click += new System.EventHandler(this.idClose_Click);
            // 
            // linkLabelOpenSite
            // 
            this.linkLabelOpenSite.AutoSize = true;
            this.linkLabelOpenSite.Enabled = false;
            this.linkLabelOpenSite.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.linkLabelOpenSite.Location = new System.Drawing.Point(107, 9);
            this.linkLabelOpenSite.Name = "linkLabelOpenSite";
            this.linkLabelOpenSite.Size = new System.Drawing.Size(119, 17);
            this.linkLabelOpenSite.TabIndex = 1;
            this.linkLabelOpenSite.TabStop = true;
            this.linkLabelOpenSite.Text = "Перейти на сайт";
            this.linkLabelOpenSite.VisitedLinkColor = System.Drawing.Color.Blue;
            this.linkLabelOpenSite.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabelOpenSite_LinkClicked);
            // 
            // checkBoxAutoLoad
            // 
            this.checkBoxAutoLoad.AutoSize = true;
            this.checkBoxAutoLoad.Enabled = false;
            this.checkBoxAutoLoad.Location = new System.Drawing.Point(61, 42);
            this.checkBoxAutoLoad.Name = "checkBoxAutoLoad";
            this.checkBoxAutoLoad.Size = new System.Drawing.Size(158, 17);
            this.checkBoxAutoLoad.TabIndex = 2;
            this.checkBoxAutoLoad.Text = "Автозагрузка программы";
            this.checkBoxAutoLoad.UseVisualStyleBackColor = true;
            this.checkBoxAutoLoad.CheckedChanged += new System.EventHandler(this.checkBoxAutoLoad_CheckedChanged);
            // 
            // labelCountNewMessages
            // 
            this.labelCountNewMessages.AutoSize = true;
            this.labelCountNewMessages.Location = new System.Drawing.Point(108, 99);
            this.labelCountNewMessages.Name = "labelCountNewMessages";
            this.labelCountNewMessages.Size = new System.Drawing.Size(112, 13);
            this.labelCountNewMessages.TabIndex = 3;
            this.labelCountNewMessages.Text = "Новых сообщений: 0";
            // 
            // checkBoxDefaultBrowser
            // 
            this.checkBoxDefaultBrowser.AutoSize = true;
            this.checkBoxDefaultBrowser.Checked = true;
            this.checkBoxDefaultBrowser.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxDefaultBrowser.Enabled = false;
            this.checkBoxDefaultBrowser.Location = new System.Drawing.Point(12, 149);
            this.checkBoxDefaultBrowser.Name = "checkBoxDefaultBrowser";
            this.checkBoxDefaultBrowser.Size = new System.Drawing.Size(217, 17);
            this.checkBoxDefaultBrowser.TabIndex = 4;
            this.checkBoxDefaultBrowser.Text = "Использовать браузер по-умолчанию";
            this.checkBoxDefaultBrowser.UseVisualStyleBackColor = true;
            this.checkBoxDefaultBrowser.CheckedChanged += new System.EventHandler(this.checkBoxDefaultBrowser_CheckedChanged);
            // 
            // buttonBrowse
            // 
            this.buttonBrowse.Enabled = false;
            this.buttonBrowse.Location = new System.Drawing.Point(245, 201);
            this.buttonBrowse.Name = "buttonBrowse";
            this.buttonBrowse.Size = new System.Drawing.Size(75, 23);
            this.buttonBrowse.TabIndex = 6;
            this.buttonBrowse.Text = "Другой...";
            this.buttonBrowse.UseVisualStyleBackColor = true;
            this.buttonBrowse.Click += new System.EventHandler(this.buttonBrowse_Click);
            // 
            // comboBoxPathBrowser
            // 
            this.comboBoxPathBrowser.Enabled = false;
            this.comboBoxPathBrowser.FormattingEnabled = true;
            this.comboBoxPathBrowser.Location = new System.Drawing.Point(12, 174);
            this.comboBoxPathBrowser.Name = "comboBoxPathBrowser";
            this.comboBoxPathBrowser.Size = new System.Drawing.Size(308, 21);
            this.comboBoxPathBrowser.TabIndex = 7;
            this.comboBoxPathBrowser.TextChanged += new System.EventHandler(this.comboBoxPathBrowser_TextChanged);
            // 
            // labelRestart
            // 
            this.labelRestart.AutoSize = true;
            this.labelRestart.ForeColor = System.Drawing.Color.Crimson;
            this.labelRestart.Location = new System.Drawing.Point(33, 198);
            this.labelRestart.Name = "labelRestart";
            this.labelRestart.Size = new System.Drawing.Size(187, 13);
            this.labelRestart.TabIndex = 8;
            this.labelRestart.Text = "Требуется перезапуск программы!";
            // 
            // labelCountInQuerySend
            // 
            this.labelCountInQuerySend.AutoSize = true;
            this.labelCountInQuerySend.Location = new System.Drawing.Point(67, 123);
            this.labelCountInQuerySend.Name = "labelCountInQuerySend";
            this.labelCountInQuerySend.Size = new System.Drawing.Size(178, 13);
            this.labelCountInQuerySend.TabIndex = 9;
            this.labelCountInQuerySend.Text = "Процессов в очереди отправки: 0";
            // 
            // checkBoxShowNewUsers
            // 
            this.checkBoxShowNewUsers.AutoSize = true;
            this.checkBoxShowNewUsers.Enabled = false;
            this.checkBoxShowNewUsers.Location = new System.Drawing.Point(61, 65);
            this.checkBoxShowNewUsers.Name = "checkBoxShowNewUsers";
            this.checkBoxShowNewUsers.Size = new System.Drawing.Size(211, 17);
            this.checkBoxShowNewUsers.TabIndex = 10;
            this.checkBoxShowNewUsers.Text = "Уведомлять о новых пользователях";
            this.checkBoxShowNewUsers.UseVisualStyleBackColor = true;
            this.checkBoxShowNewUsers.CheckedChanged += new System.EventHandler(this.checkBoxShowNewUsers_CheckedChanged);
            // 
            // FormProcNetClient
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 239);
            this.Controls.Add(this.checkBoxShowNewUsers);
            this.Controls.Add(this.labelCountInQuerySend);
            this.Controls.Add(this.comboBoxPathBrowser);
            this.Controls.Add(this.checkBoxDefaultBrowser);
            this.Controls.Add(this.labelRestart);
            this.Controls.Add(this.buttonBrowse);
            this.Controls.Add(this.linkLabelOpenSite);
            this.Controls.Add(this.checkBoxAutoLoad);
            this.Controls.Add(this.labelCountNewMessages);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormProcNetClient";
            this.Text = "ProcNetClient";
            this.Load += new System.EventHandler(this.FormProcNetClient_Load);
            this.SizeChanged += new System.EventHandler(this.FormProcNetClient_SizeChanged);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormProcNetClient_FormClosed);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem idShowWindow;
        private System.Windows.Forms.ToolStripMenuItem idClose;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem idAbout;
        private System.Windows.Forms.LinkLabel linkLabelOpenSite;
        private System.Windows.Forms.CheckBox checkBoxAutoLoad;
        private System.Windows.Forms.ToolStripMenuItem idOpenBrowser;
        private System.Windows.Forms.Label labelCountNewMessages;
        private System.Windows.Forms.CheckBox checkBoxDefaultBrowser;
        private System.Windows.Forms.Button buttonBrowse;
        private System.Windows.Forms.ComboBox comboBoxPathBrowser;
        private System.Windows.Forms.Label labelRestart;
        private System.Windows.Forms.Label labelCountInQuerySend;
        private System.Windows.Forms.CheckBox checkBoxShowNewUsers;
    }
}

