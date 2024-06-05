using ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.EnsembleServices;
using Microsoft.Extensions.DependencyInjection;
using neurUL.Common.Domain.Model;
using System.Linq;
using System.Threading.Tasks;

namespace ei8.Cortex.Chat.Nucleus.Port.Adapter.IO.Persistence.Remote.e8.Cortex.Ensembles.neurULization
{
    public class neurULizer
    {
        public async Task<Ensemble> neurULizeAsync<TValue>(TValue value, neurULizationOptions options)
        {
            AssertionConcern.AssertArgumentNotNull(value, nameof(value));
            AssertionConcern.AssertArgumentNotNull(options, nameof(options));

            // required services
            var instantiates = options.ServiceProvider.GetRequiredService<IInstantiates>();
            var neuronRepository = options.ServiceProvider.GetRequiredService<INeuronRepository>();
            
            var result = new Ensemble();

            var grandmother = Neuron.CreateTransient(null, null, null);
            result.AddReplace(grandmother);

            // get ExternalReferenceKeyAttribute of root type
            var erka = value.GetType().GetCustomAttributes(typeof(ExternalReferenceKeyAttribute), true).SingleOrDefault() as ExternalReferenceKeyAttribute;
            var key = string.Empty;
            // if attribute exists
            if (erka != null)
                key = erka.Key;
            else
                // assembly qualified name 
                key = value.GetType().ToExternalReferenceKeyString();
            // use key to retrieve external reference url from library
            var erDict = await neuronRepository.GetExternalReferencesAsync(options.UserId, new string[] { key });
            var rootTypeNeuron = erDict[key];

            // instantiates
            var instantiatesType = await instantiates.ObtainAsync(
                result,
                new InstantiatesParameterSet(rootTypeNeuron),
                neuronRepository,
                options.UserId
                );

            // create Root Neuron
            // link root neuron to InstantiatesMessage neuron

            return result;
        }
    }
}
