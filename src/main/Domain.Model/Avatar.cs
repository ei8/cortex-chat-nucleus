﻿using ei8.Cortex.Coding;
using ei8.Cortex.Coding.Properties;
using System;

namespace ei8.Cortex.Chat.Nucleus.Domain.Model
{
    public class Avatar
    {
        [neurULNeuronProperty]
        public Guid Id { get; set; }

        [neurULNeuronProperty(nameof(Neuron.Tag))]
        public string Name { get; set; }

        [neurULNeuronProperty]
        public string ExternalReferenceUrl { get; set; }

        [neurULNeuronProperty]
        public string Url { get; set; }
    }
}
