namespace WindowsFormsApplication1
{
    partial class DisplaySetting
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
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button_save = new System.Windows.Forms.Button();
            this.button_exit = new System.Windows.Forms.Button();
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.textBox_PORT = new System.Windows.Forms.TextBox();
            this.label_IP = new System.Windows.Forms.Label();
            this.label_PORT = new System.Windows.Forms.Label();
            this.label_title = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_save
            // 
            this.button_save.Location = new System.Drawing.Point(15, 201);
            this.button_save.Name = "button_save";
            this.button_save.Size = new System.Drawing.Size(120, 40);
            this.button_save.TabIndex = 0;
            this.button_save.Text = "저장";
            this.button_save.UseVisualStyleBackColor = true;
            this.button_save.Click += new System.EventHandler(this.button_save_Click);
            // 
            // button_exit
            // 
            this.button_exit.Location = new System.Drawing.Point(152, 201);
            this.button_exit.Name = "button_exit";
            this.button_exit.Size = new System.Drawing.Size(120, 40);
            this.button_exit.TabIndex = 1;
            this.button_exit.Text = "취소";
            this.button_exit.UseVisualStyleBackColor = true;
            this.button_exit.Click += new System.EventHandler(this.button_exit_Click);
            // 
            // textBox_IP
            // 
            this.textBox_IP.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox_IP.Location = new System.Drawing.Point(71, 31);
            this.textBox_IP.MaxLength = 16;
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.Size = new System.Drawing.Size(179, 21);
            this.textBox_IP.TabIndex = 1;
            // 
            // textBox_PORT
            // 
            this.textBox_PORT.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.textBox_PORT.Location = new System.Drawing.Point(71, 59);
            this.textBox_PORT.MaxLength = 10;
            this.textBox_PORT.Name = "textBox_PORT";
            this.textBox_PORT.Size = new System.Drawing.Size(179, 21);
            this.textBox_PORT.TabIndex = 2;
            // 
            // label_IP
            // 
            this.label_IP.AutoSize = true;
            this.label_IP.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_IP.Location = new System.Drawing.Point(12, 37);
            this.label_IP.Name = "label_IP";
            this.label_IP.Size = new System.Drawing.Size(30, 15);
            this.label_IP.TabIndex = 4;
            this.label_IP.Text = "IP :";
            this.label_IP.Click += new System.EventHandler(this.label_IP_Click);
            // 
            // label_PORT
            // 
            this.label_PORT.AutoSize = true;
            this.label_PORT.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_PORT.Location = new System.Drawing.Point(12, 65);
            this.label_PORT.Name = "label_PORT";
            this.label_PORT.Size = new System.Drawing.Size(57, 15);
            this.label_PORT.TabIndex = 5;
            this.label_PORT.Text = "PORT :";
            // 
            // label_title
            // 
            this.label_title.AutoSize = true;
            this.label_title.Font = new System.Drawing.Font("굴림", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label_title.Location = new System.Drawing.Point(9, 9);
            this.label_title.Name = "label_title";
            this.label_title.Size = new System.Drawing.Size(162, 15);
            this.label_title.TabIndex = 6;
            this.label_title.Text = "WebSocket Setting Info";
            // 
            // DisplaySetting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.label_title);
            this.Controls.Add(this.label_PORT);
            this.Controls.Add(this.label_IP);
            this.Controls.Add(this.textBox_PORT);
            this.Controls.Add(this.textBox_IP);
            this.Controls.Add(this.button_exit);
            this.Controls.Add(this.button_save);
            this.Name = "DisplaySetting";
            this.Text = "DisplaySetting";
            this.Load += new System.EventHandler(this.DisplaySetting_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_save;
        private System.Windows.Forms.Button button_exit;
        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.TextBox textBox_PORT;
        private System.Windows.Forms.Label label_IP;
        private System.Windows.Forms.Label label_PORT;
        private System.Windows.Forms.Label label_title;
    }
}