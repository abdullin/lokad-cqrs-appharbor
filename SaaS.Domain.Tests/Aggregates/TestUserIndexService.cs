#region (c) 2010-2012 Lokad - CQRS Sample for Windows Azure - New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sample.Aggregates
{
    public sealed class TestUserIndexService : IUserIndexService
    {
        readonly HashSet<string> _records = new HashSet<string>();

        public TestUserIndexService includes_email(string email)
        {
            _records.Add("email:" + email);
            return this;
        }

        public bool IsLoginRegistered(string email)
        {
            return _records.Contains("email:" + email);
        }

        public bool IsIdentityRegistered(string identity)
        {
            return _records.Contains("id:" + identity);
        }

        public TestUserIndexService includes_identity(string identity)
        {
            _records.Add("id:" + identity);
            return this;
        }

        public void Clear()
        {
            _records.Clear();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach (var record in _records)
            {
                builder.AppendFormat("index includes {0}", record);
            }
            return builder.ToString();
        }
    }

    public sealed class TestSendEmail : IMailSender
    {
        bool _used;

        public void EnqueueText(Email[] to, string subject, string body, Email replyTo = null)
        {
            _used = true;
            Context.Explain("Send email to {0} '{1}' with body:\r\n{2}", string.Join(";", to.Select(s => s.ToString())),
                subject, body);
        }

        public void EnqueueHtml(Email[] to, string subject, string body, Email replyTo = null)
        {
            _used = true;
            Context.Explain("Send email to {0} '{1}' with body:\r\n{2}", string.Join(";", to.Select(s => s.ToString())),
                subject, body);
        }

        public override string ToString()
        {
            return _used ? "" : "Test mail sender";
        }
    }
}