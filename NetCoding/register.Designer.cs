namespace NetCoding
{
    partial class Register
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
            this.rUserLbl = new System.Windows.Forms.Label();
            this.rPassTxt = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.RuserTxt = new System.Windows.Forms.TextBox();
            this.passText = new System.Windows.Forms.TextBox();
            this.confirmPassTxt = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.btnLogin = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // rUserLbl
            // 
            this.rUserLbl.AutoSize = true;
            this.rUserLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rUserLbl.Location = new System.Drawing.Point(12, 24);
            this.rUserLbl.Name = "rUserLbl";
            this.rUserLbl.Size = new System.Drawing.Size(97, 24);
            this.rUserLbl.TabIndex = 1;
            this.rUserLbl.Text = "Username";
            // 
            // rPassTxt
            // 
            this.rPassTxt.AutoSize = true;
            this.rPassTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rPassTxt.Location = new System.Drawing.Point(12, 82);
            this.rPassTxt.Name = "rPassTxt";
            this.rPassTxt.Size = new System.Drawing.Size(92, 24);
            this.rPassTxt.TabIndex = 2;
            this.rPassTxt.Text = "Password";
            this.rPassTxt.Click += new System.EventHandler(this.rPassTxt_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 138);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(75, 24);
            this.label1.TabIndex = 3;
            this.label1.Text = "Confirm";
            // 
            // RuserTxt
            // 
            this.RuserTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RuserTxt.Location = new System.Drawing.Point(129, 21);
            this.RuserTxt.Name = "RuserTxt";
            this.RuserTxt.Size = new System.Drawing.Size(231, 29);
            this.RuserTxt.TabIndex = 4;
            // 
            // passText
            // 
            this.passText.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.passText.Location = new System.Drawing.Point(129, 77);
            this.passText.Name = "passText";
            this.passText.Size = new System.Drawing.Size(231, 29);
            this.passText.TabIndex = 5;
            // 
            // confirmPassTxt
            // 
            this.confirmPassTxt.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.confirmPassTxt.Location = new System.Drawing.Point(129, 135);
            this.confirmPassTxt.Name = "confirmPassTxt";
            this.confirmPassTxt.Size = new System.Drawing.Size(231, 29);
            this.confirmPassTxt.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(215, 196);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(145, 38);
            this.button1.TabIndex = 8;
            this.button1.Text = "Cancel";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // btnLogin
            // 
            this.btnLogin.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLogin.Location = new System.Drawing.Point(33, 196);
            this.btnLogin.Name = "btnLogin";
            this.btnLogin.Size = new System.Drawing.Size(145, 38);
            this.btnLogin.TabIndex = 7;
            this.btnLogin.Text = "Register";
            this.btnLogin.UseVisualStyleBackColor = true;
            // 
            // register
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 246);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnLogin);
            this.Controls.Add(this.confirmPassTxt);
            this.Controls.Add(this.passText);
            this.Controls.Add(this.RuserTxt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.rPassTxt);
            this.Controls.Add(this.rUserLbl);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "register";
            this.Text = "Register";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label rUserLbl;
        private System.Windows.Forms.Label rPassTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox RuserTxt;
        private System.Windows.Forms.TextBox passText;
        private System.Windows.Forms.TextBox confirmPassTxt;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button btnLogin;
    }
}