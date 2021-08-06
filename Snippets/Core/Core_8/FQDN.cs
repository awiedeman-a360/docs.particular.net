namespace Core8
{
    using System;
    using System.Net;
    using NServiceBus;
    using NServiceBus.Support;

    public class FQDNTest
    {
        void FQDN()
        {
            EndpointConfiguration endpointConfigration = null;
            #region MachineNameActionOverride

            endpointConfigration.UniquelyIdentifyRunningInstance().UsingHostName(Dns.GetHostEntry(Environment.MachineName).HostName);

            #endregion
        }
    }
}