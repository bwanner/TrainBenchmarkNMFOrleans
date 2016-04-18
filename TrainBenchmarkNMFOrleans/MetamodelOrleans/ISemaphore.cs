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

namespace TTC2015.TrainBenchmark.Railway
{
    
    
    /// <summary>
    /// The public interface for Semaphore
    /// </summary>
    [XmlNamespaceAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark")]
    [XmlNamespacePrefixAttribute("hu.bme.mit.trainbenchmark")]
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//Semaphore/")]
    //[XmlDefaultImplementationTypeAttribute(typeof(Semaphore))]
    //[DefaultImplementationTypeAttribute(typeof(Semaphore))]
    public interface ISemaphore : IModelElement, IRailwayElement
    {
        
        /// <summary>
        /// The signal property
        /// </summary>
        Signal Signal
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets fired when the Signal property changed its value
        /// </summary>
        event EventHandler SignalChanged;
    }
}

