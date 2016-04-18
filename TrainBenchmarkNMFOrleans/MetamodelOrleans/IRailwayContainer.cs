using NMF.Collections.Generic;
using NMF.Collections.ObjectModel;
using NMF.Expressions;
using NMF.Expressions.Linq;
using NMF.Models;
using NMF.Models.Collections;
using NMF.Serialization;
using NMF.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Orleans.Collections.Observable;

namespace TTC2015.TrainBenchmark.Railway
{
    
    
    /// <summary>
    /// The public interface for RailwayContainer
    /// </summary>
    [XmlNamespaceAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark")]
    [XmlNamespacePrefixAttribute("hu.bme.mit.trainbenchmark")]
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//RailwayContainer/" +
        "")]
    [XmlDefaultImplementationTypeAttribute(typeof(RailwayContainer))]
    [DefaultImplementationTypeAttribute(typeof(RailwayContainer))]
    public interface IRailwayContainer : IModelElement
    {

        /// <summary>
        /// The invalids property
        /// </summary>
        IObservableContainerGrain<IRailwayElement> Invalids
        {
            get;
        }

        /// <summary>
        /// The semaphores property
        /// </summary>
        IObservableContainerGrain<ISemaphore> Semaphores
        {
            get;
        }

        /// <summary>
        /// The routes property
        /// </summary>
        IObservableContainerGrain<IRoute> Routes
        {
            get;
        }
    }
}

