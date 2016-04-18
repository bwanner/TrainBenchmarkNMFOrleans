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
    /// The public interface for Route
    /// </summary>
    [XmlNamespaceAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark")]
    [XmlNamespacePrefixAttribute("hu.bme.mit.trainbenchmark")]
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//Route/")]
    //[XmlDefaultImplementationTypeAttribute(typeof(Route))]
    //[DefaultImplementationTypeAttribute(typeof(Route))]
    public interface IRoute : IModelElement, IRailwayElement
    {
        
        /// <summary>
        /// The entry property
        /// </summary>
        ISemaphore Entry
        {
            get;
            set;
        }
        
        /// <summary>
        /// The follows property
        /// </summary>
        IListExpression<ISwitchPosition> Follows
        {
            get;
        }
        
        /// <summary>
        /// The exit property
        /// </summary>
        ISemaphore Exit
        {
            get;
            set;
        }
        
        /// <summary>
        /// The definedBy property
        /// </summary>
        IListExpression<ISensor> DefinedBy
        {
            get;
        }
        
        /// <summary>
        /// Gets fired when the Entry property changed its value
        /// </summary>
        event EventHandler<ValueChangedEventArgs> EntryChanged;
        
        /// <summary>
        /// Gets fired when the Exit property changed its value
        /// </summary>
        event EventHandler<ValueChangedEventArgs> ExitChanged;
    }
}

