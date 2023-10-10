// TODO: using CQRSlite.Commands;
//using neurUL.Common.Domain.Model;
//using neurUL.Cortex.Common;
//using System;

//namespace ei8.Cortex.Chat.Nucleus.Application.Messages.Commands
//{
//    public class CreateTerminal : ICommand
//    {
//        public CreateTerminal(Guid id, Guid presynapticNeuronId, Guid postsynapticNeuronId, NeurotransmitterEffect effect, float strength, string externalReferenceUrl, string userId)
//        {
//            AssertionConcern.AssertArgumentValid(
//                g => g != Guid.Empty,
//                id,
//                Messages.Exception.InvalidId,
//                nameof(id)
//                );
//            AssertionConcern.AssertArgumentValid(
//                g => g != Guid.Empty,
//                presynapticNeuronId,
//                Messages.Exception.InvalidId,
//                nameof(presynapticNeuronId)
//                );
//            AssertionConcern.AssertArgumentValid(
//                g => g != Guid.Empty,
//                postsynapticNeuronId,
//                Messages.Exception.InvalidId,
//                nameof(postsynapticNeuronId)
//                );
//            AssertionConcern.AssertArgumentNotEmpty(
//                userId,
//                Messages.Exception.InvalidUserId,
//                nameof(userId)
//                );

//            this.Id = id;
//            this.PresynapticNeuronId = presynapticNeuronId;
//            this.PostsynapticNeuronId = postsynapticNeuronId;
//            this.Effect = effect;
//            this.Strength = strength;
//            this.ExternalReferenceUrl = externalReferenceUrl;
//            this.UserId = userId;
//        }

//        public Guid Id { get; private set; }

//        public Guid PresynapticNeuronId { get; private set; }

//        public Guid PostsynapticNeuronId { get; private set; }

//        public NeurotransmitterEffect Effect { get; private set; }

//        public float Strength { get; private set; }

//        public string ExternalReferenceUrl { get; private set; }

//        public string UserId { get; private set; }

//        public int ExpectedVersion { get; private set; }
//    }
//}
