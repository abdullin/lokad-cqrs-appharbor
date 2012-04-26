#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Windows.Forms;
using ServiceStack.Text;

namespace Audit.Views
{
    public partial class ModifyMessage : Form
    {
        public ModifyMessage()
        {
            InitializeComponent();
        }

        Type MessageType { get; set; }

        public void BindMessage(Type messageType, object instance)
        {
            MessageType = messageType;
            richTextBox1.Text = JsvFormatter.Format(JsonSerializer.SerializeToString(instance));
        }

        public object GetMessage()
        {
            return JsonSerializer.DeserializeFromString(richTextBox1.Text, MessageType);
        }
    }
}