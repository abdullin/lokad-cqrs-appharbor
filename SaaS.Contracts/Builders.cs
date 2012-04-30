using System;
using System.Collections.Specialized;
using System.Linq;

namespace SaaS
{
    public sealed class RegistrationInfoBuilder
    {
        public readonly string ContactEmail;
        public readonly string CustomerName;

        public RegistrationInfoBuilder(string contactEmail, string customerName)
        {
            ContactEmail = contactEmail;
            CustomerName = customerName;
            
        }

        public RegistrationInfoBuilder WithUserHost(string userHostAddress)
        {
            if (string.IsNullOrEmpty(userHostAddress))
                return this;
            Headers.Add("UserHostAddress", userHostAddress);
            return this;
        }

        public string OptionalUserPassword;
        public string OptionalUserIdentity;
        public string OptionalCompanyUrl;
        public string OptionalCompanyPhone;
        public DateTime Created = Current.UtcNow;
        public string OptionalUserName;
        public NameValueCollection Headers = new NameValueCollection();

        public RegistrationInfo Build()
        {

            var headers = Headers.AllKeys
                .Select(k => new RegistrationHttpHeader(k, Headers[k])).ToArray();
            return new RegistrationInfo(ContactEmail, CustomerName, OptionalUserIdentity, OptionalUserPassword, OptionalCompanyPhone, OptionalCompanyUrl, OptionalUserName,  headers, Created);
        }
        public RegistrationInfo Build(DateTime date)
        {
            Created = date;
            return Build();
        }
    }

    public static class Current
    {
        static Func<DateTime> _getTime = GetUtc;
        static Func<Guid> _getGuid = GetGuid;

        static DateTime GetUtc()
        {
            return new DateTime(DateTime.UtcNow.Ticks, DateTimeKind.Unspecified);
        }
        static Guid GetGuid()
        {
            return Guid.NewGuid();
        }

        public static void DateIs(DateTime time)
        {
            _getTime = () => new DateTime(time.Ticks, DateTimeKind.Unspecified);
        }

        public static readonly DateTime MaxValue = new DateTime(9999, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);


        public static void DateIs(int year, int month = 1, int day = 1)
        {
            DateIs(new DateTime(year, month, day));
        }

        public static void GuidIs(Guid guid)
        {
            _getGuid = () => guid;
        }
        public static void GuidIs(string guid)
        {
            var g = Guid.Parse(guid);
            _getGuid = () => g;
        }

        public static void Reset()
        {
            _getTime = GetUtc;
            _getGuid = GetGuid;

        }

        public static DateTime UtcNow { get { return _getTime(); } }
        public static Guid NewGuid { get { return _getGuid(); } }
    }


}