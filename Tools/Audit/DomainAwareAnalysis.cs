#region (c) 2010-2012 Lokad - CQRS- New BSD License 

// Copyright (c) Lokad 2010-2012, http://www.lokad.com
// This code is released as Open Source under the terms of the New BSD Licence

#endregion

using System;
using System.Drawing;
using Audit.Util;
using SaaS;

namespace Audit
{
    static class DomainAwareAnalysis
    {
        public static Color GetBackgroundColorForContract(ImmutableMessage message)
        {
            var commad = message.Content as ISampleCommand;

            if (commad != null)
            {
                return CommonColors.Green;
            }

            var @event = message.Content as ISampleEvent;

            if (@event != null)
            {
                //if (@event is PollingApiForForecastsTimedOut)
                //{
                //    return CommonColors.Red;
                //}

                return CommonColors.Blue;
            }
            return CommonColors.Gray;
        }

        public static Color GetBackgroundColorForCategory(string category)
        {
            if (String.IsNullOrEmpty(category))
                return Color.LightGray;

            var hashCode = category.GetHashCode();
            var b = BitConverter.GetBytes(hashCode);
            return Color.FromArgb(b[0] ^ b[1] | 128, b[2] ^ b[1] | 128, b[2] ^ b[3] | 128);
        }

        public static string GetCategoryNames(ImmutableMessage messages)
        {
            var item = messages.Content;

            if (item is IFunctionalCommand || item is IFunctionalEvent || item is InstanceStarted)
                return "system";

            var @event = item as IEvent<IIdentity>;

            if (@event != null)
            {
                return @event.Id.ToString();
            }

            var command = item as ICommand<IIdentity>;

            if (command != null)
            {
                return command.Id.ToString();
            }


            throw new InvalidOperationException("Unknown envelope category name.");
        }
    }
}