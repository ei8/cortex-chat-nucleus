using System;
using System.Collections.Generic;
using System.Text;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model.Library
{
    public class ExternalReference
    {
        public string Url { get; set; }
        public Guid NeuronId { get; set; }
        public ExternalReferenceId Id { get; set; }
    }
}
