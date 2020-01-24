using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace FssInfo {
	class ServiceBehavior : IEndpointBehavior {
		private readonly IClientMessageInspector messageInspector;

		public ServiceBehavior(IClientMessageInspector messageInspector) {
			this.messageInspector = messageInspector;
		}

		public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) { }

		public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) {
			clientRuntime.ClientMessageInspectors.Add(messageInspector);
		}

		public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) { }

		public void Validate(ServiceEndpoint endpoint) { }
	}
}
