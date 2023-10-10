// TODO: using ei8.Cortex.Chat.Common;
//using System;
//using System.Collections.Generic;
//using System.Collections.ObjectModel;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ei8.Cortex.Chat.Nucleus.Application
//{
//    internal static class Extensions
//    {
//        internal static NotificationLog ToInternalType(this EventSourcing.Common.NotificationLog value)
//        {
//            return new NotificationLog()
//            {
//                FirstNotificationLogId = value.FirstNotificationLogId,
//                HasFirstNotificationLog = value.HasFirstNotificationLog,
//                HasNextNotificationLog = value.HasNextNotificationLog,
//                HasPreviousNotificationLog = value.HasPreviousNotificationLog,
//                IsArchived = value.IsArchived,
//                NextNotificationLogId = value.NextNotificationLogId,
//                NotificationList = new ReadOnlyCollection<Common.Notification>(value.NotificationList.Select(n => n.ToInternalType()).ToList()),
//                NotificationLogId = value.NotificationLogId,
//                PreviousNotificationLogId = value.PreviousNotificationLogId,
//                TotalCount = value.TotalCount,
//                TotalNotification = value.TotalNotification
//            };
//        }

//        internal static Common.Notification ToInternalType(this EventSourcing.Common.Notification value)
//        {
//            return new Common.Notification()
//            {
//                AuthorId = value.AuthorId,
//                Data = value.Data,
//                Id = value.Id,
//                SequenceId = value.SequenceId,
//                Timestamp = value.Timestamp,
//                TypeName = value.TypeName,
//                Version = value.Version
//            };
//        }

//        // TODO: transfer to common
//        public static async Task<IEnumerable<T1>> SelectManyAsync<T, T1>(this IEnumerable<T> enumeration, Func<T, Task<IEnumerable<T1>>> func)
//        {
//            return (await Task.WhenAll(enumeration.Select(func))).SelectMany(s => s);
//        }
//    }
//}
