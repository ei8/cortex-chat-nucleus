using neurUL.Common.Domain.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.New
{
    public class neurULizer
    {
        public async Task<Neuron> neurULizeAsync<TValue>(TValue value, neurULizationOptions options)
        {
            AssertionConcern.AssertArgumentNotNull(value, nameof(value));
            AssertionConcern.AssertArgumentNotNull(options, nameof(options));
            AssertionConcern.AssertArgumentValid(
                o => o.ExternalReferenceRetriever != null, 
                options,
                $"{nameof(neurULizationOptions)}.{nameof(neurULizationOptions.ExternalReferenceRetriever)} cannot be null.",
                nameof(neurULizationOptions.ExternalReferenceRetriever)
            );

            var root = new Neuron();
            // get ExternalReferenceKeyAttribute of root type
            var erka = value.GetType().GetCustomAttributes(typeof(ExternalReferenceKeyAttribute), true).SingleOrDefault() as ExternalReferenceKeyAttribute;
            var key = string.Empty;
            // if attribute exists
            if (erka != null)
                key = erka.Key;
            else
                // assembly qualified name 
                key = key.GetType().FullName;
            // use key to retrieve external reference url from library
            var rootTypeNeuronId = await options.ExternalReferenceRetriever(new string[] { key });

            // if "Instantiates, Message" exists based on retrieved (1) message class erurl (2) instantiates erurl
            // ... deserialize to and cache ensemble
            // else
            // ... create "instantiates, message" etc.			
            // create Root Neuron
            // link root neuron to InstantiatesMessage neuron

            return root;
        }
    }
}
