// TODO: using CQRSlite.Commands;
//using neurUL.Common.Domain.Model;
//using System;

//namespace ei8.Cortex.Chat.Nucleus.Application.Messages.Commands
//{
//    public class ChangeNeuronTag : ICommand
//    {
//        public ChangeNeuronTag(Guid id, string newTag, string userId, int expectedVersion)
//        {
//            AssertionConcern.AssertArgumentValid(
//                g => g != Guid.Empty,
//                id,
//                Messages.Exception.InvalidId,
//                nameof(id)
//                );
//            AssertionConcern.AssertArgumentNotNull(newTag, nameof(newTag));
//            AssertionConcern.AssertArgumentNotEmpty(
//                userId,
//                Messages.Exception.InvalidUserId,
//                nameof(userId)
//                );
//            AssertionConcern.AssertArgumentValid(
//                i => i >= 1,
//                expectedVersion,
//                Messages.Exception.InvalidExpectedVersion,
//                nameof(expectedVersion)
//                );

//            this.Id = id;            
//            this.NewTag = newTag;
//            this.UserId = userId;
//            this.ExpectedVersion = expectedVersion;
//        }

//        public Guid Id { get; private set; }

//        public string NewTag { get; private set; }

//        public string UserId { get; private set; }

//        public int ExpectedVersion { get; private set; }
//    }
//}
