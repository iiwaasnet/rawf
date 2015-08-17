using System;
using System.Collections.Generic;

namespace rawf.Rendezvous.Consensus
{
    public interface ISynodConfigurationProvider
    {
        Uri LocalNode { get; }
        IEnumerable<Uri> Synod { get; }
    }
}