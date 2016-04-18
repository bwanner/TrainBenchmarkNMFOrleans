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
    /// The public interface for Segment
    /// </summary>
    [XmlNamespaceAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark")]
    [XmlNamespacePrefixAttribute("hu.bme.mit.trainbenchmark")]
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//Segment/")]
    [XmlDefaultImplementationTypeAttribute(typeof(Segment))]
    [DefaultImplementationTypeAttribute(typeof(Segment))]
    public interface ISegment : IModelElement, ITrackElement
    {
        
        /// <summary>
        /// The length property
        /// </summary>
        int Length
        {
            get;
            set;
        }
        
        /// <summary>
        /// Gets fired when the Length property changed its value
        /// </summary>
        event EventHandler LengthChanged;
    }
}

