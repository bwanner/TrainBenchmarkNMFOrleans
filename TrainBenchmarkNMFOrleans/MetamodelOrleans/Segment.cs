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
    
    
    [XmlNamespaceAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark")]
    [XmlNamespacePrefixAttribute("hu.bme.mit.trainbenchmark")]
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//Segment/")]
    public class Segment : TrackElement, ISegment, IModelElement
    {
        
        /// <summary>
        /// The backing field for the Length property
        /// </summary>
        private int _length;
        
        /// <summary>
        /// The length property
        /// </summary>
        [XmlElementNameAttribute("length")]
        [XmlAttributeAttribute(true)]
        public virtual int Length
        {
            get
            {
                return this._length;
            }
            set
            {
                if ((value != this._length))
                {
                    this._length = value;
                    this.OnLengthChanged(EventArgs.Empty);
                    this.OnPropertyChanged("Length");
                }
            }
        }
        
        /// <summary>
        /// Gets fired when the Length property changed its value
        /// </summary>
        public event EventHandler LengthChanged;
        
        /// <summary>
        /// Raises the LengthChanged event
        /// </summary>
        /// <param name="eventArgs">The event data</param>
        protected virtual void OnLengthChanged(EventArgs eventArgs)
        {
            EventHandler handler = this.LengthChanged;
            if ((handler != null))
            {
                handler.Invoke(this, eventArgs);
            }
        }
        
        /// <summary>
        /// Gets the Class element that describes the structure of the current model element
        /// </summary>
        public override NMF.Models.Meta.IClass GetClass()
        {
            return NMF.Models.Repository.MetaRepository.Instance.ResolveClass("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//Segment/");
        }
    }
}

