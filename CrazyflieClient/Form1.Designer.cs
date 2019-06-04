namespace CrazyflieClient
{
    partial class Form1
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
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.button_connect = new System.Windows.Forms.Button();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.textBox3 = new System.Windows.Forms.TextBox();
            this.checkbox_keyboard = new System.Windows.Forms.CheckBox();
            this.button_fly = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 21);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "0xE7E7E7E703";
            this.textBox1.Visible = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(131, 10);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Search";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(12, 39);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(194, 20);
            this.comboBox1.TabIndex = 2;
            this.comboBox1.Visible = false;
            // 
            // button_connect
            // 
            this.button_connect.Location = new System.Drawing.Point(12, 65);
            this.button_connect.Name = "button_connect";
            this.button_connect.Size = new System.Drawing.Size(75, 23);
            this.button_connect.TabIndex = 3;
            this.button_connect.Text = "connect";
            this.button_connect.UseVisualStyleBackColor = true;
            this.button_connect.Click += new System.EventHandler(this.button2_Click);
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(12, 128);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(194, 21);
            this.textBox2.TabIndex = 4;
            this.textBox2.Visible = false;
            this.textBox2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox2_KeyDown);
            // 
            // textBox3
            // 
            this.textBox3.Location = new System.Drawing.Point(12, 155);
            this.textBox3.Multiline = true;
            this.textBox3.Name = "textBox3";
            this.textBox3.ReadOnly = true;
            this.textBox3.Size = new System.Drawing.Size(194, 165);
            this.textBox3.TabIndex = 5;
            this.textBox3.WordWrap = false;
            // 
            // checkbox_keyboard
            // 
            this.checkbox_keyboard.AutoSize = true;
            this.checkbox_keyboard.Location = new System.Drawing.Point(221, 41);
            this.checkbox_keyboard.Name = "checkbox_keyboard";
            this.checkbox_keyboard.Size = new System.Drawing.Size(120, 16);
            this.checkbox_keyboard.TabIndex = 6;
            this.checkbox_keyboard.Text = "Keyboard Control";
            this.checkbox_keyboard.UseVisualStyleBackColor = true;
            this.checkbox_keyboard.Visible = false;
            // 
            // button_fly
            // 
            this.button_fly.Enabled = false;
            this.button_fly.Location = new System.Drawing.Point(131, 65);
            this.button_fly.Name = "button_fly";
            this.button_fly.Size = new System.Drawing.Size(75, 23);
            this.button_fly.TabIndex = 7;
            this.button_fly.Text = "fly";
            this.button_fly.UseVisualStyleBackColor = true;
            this.button_fly.Click += new System.EventHandler(this.button_fly_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Text files (*.txt)|*.txt";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.button_fly);
            this.Controls.Add(this.checkbox_keyboard);
            this.Controls.Add(this.textBox3);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.button_connect);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.textBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button button_connect;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.CheckBox checkbox_keyboard;
        private System.Windows.Forms.Button button_fly;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
    }
}

