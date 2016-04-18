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

namespace TTC2015.TrainBenchmark.Orleans.Railway
{
    
    
    /// <summary>
    /// The public interface for RailwayElement
    /// </summary>
    [XmlNamespaceAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark")]
    [XmlNamespacePrefixAttribute("hu.bme.mit.trainbenchmark")]
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//RailwayElement/")]
    //[XmlDefaultImplementationTypeAttribute(typeof(RailwayElement))]
    //[DefaultImplementationTypeAttribute(typeof(RailwayElement))]
    public interface IRailwayElement : IOrleansModelElement
    {

        ObjectIdentifier Guid { get; set; }

        /// <summary>
        /// The id property
        /// </summary>
        Nullable<int> Id
        {
            get;
            set;
        }
    }
}

