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

namespace TTC2015.TrainBenchmark.Orleans.Railway
{
    
    
    public class Segment : TrackElement, ISegment
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
                }
            }
        }
        
    }
}

