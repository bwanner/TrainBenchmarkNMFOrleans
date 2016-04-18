//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.34209
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------

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
    [ModelRepresentationClassAttribute("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//RailwayContainer/" +
        "")]
    public class RailwayContainer : ModelElement, IRailwayContainer, ITrainBenchmarkModelElement<TTC2015.TrainBenchmark.Orleans.Railway.IRailwayContainer>
    {
        
        /// <summary>
        /// The backing field for the Invalids property
        /// </summary>
        private ObservableCompositionList<IRailwayElement> _invalids;
        
        /// <summary>
        /// The backing field for the Semaphores property
        /// </summary>
        private ObservableCompositionList<ISemaphore> _semaphores;
        
        /// <summary>
        /// The backing field for the Routes property
        /// </summary>
        private ObservableCompositionList<IRoute> _routes;
        
        public RailwayContainer()
        {
            this._invalids = new ObservableCompositionList<IRailwayElement>(this);
            this._semaphores = new ObservableCompositionList<ISemaphore>(this);
            this._routes = new ObservableCompositionList<IRoute>(this);
        }
        
        /// <summary>
        /// The invalids property
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)]
        [XmlElementNameAttribute("invalids")]
        [XmlAttributeAttribute(false)]
        [ContainmentAttribute()]
        public virtual IListExpression<IRailwayElement> Invalids
        {
            get
            {
                return this._invalids;
            }
        }
        
        /// <summary>
        /// The semaphores property
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)]
        [XmlElementNameAttribute("semaphores")]
        [XmlAttributeAttribute(false)]
        [ContainmentAttribute()]
        public virtual IListExpression<ISemaphore> Semaphores
        {
            get
            {
                return this._semaphores;
            }
        }
        
        /// <summary>
        /// The routes property
        /// </summary>
        [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content)]
        [XmlElementNameAttribute("routes")]
        [XmlAttributeAttribute(false)]
        [ContainmentAttribute()]
        public virtual IListExpression<IRoute> Routes
        {
            get
            {
                return this._routes;
            }
        }
        
        /// <summary>
        /// Gets the child model elements of this model element
        /// </summary>
        public override IEnumerableExpression<IModelElement> Children
        {
            get
            {
                return base.Children.Concat(new RailwayContainerChildrenCollection(this));
            }
        }
        
        /// <summary>
        /// Gets the referenced model elements of this model element
        /// </summary>
        public override IEnumerableExpression<IModelElement> ReferencedElements
        {
            get
            {
                return base.ReferencedElements.Concat(new RailwayContainerReferencedElementsCollection(this));
            }
        }
        
        /// <summary>
        /// Gets the relative URI fragment for the given child model element
        /// </summary>
        /// <returns>A fragment of the relative URI</returns>
        /// <param name="element">The element that should be looked for</param>
        protected override string GetRelativePathForNonIdentifiedChild(IModelElement element)
        {
            int invalidsIndex = ModelHelper.IndexOfReference(this.Invalids, element);
            if ((invalidsIndex != -1))
            {
                return ModelHelper.CreatePath("invalids", invalidsIndex);
            }
            int semaphoresIndex = ModelHelper.IndexOfReference(this.Semaphores, element);
            if ((semaphoresIndex != -1))
            {
                return ModelHelper.CreatePath("semaphores", semaphoresIndex);
            }
            int routesIndex = ModelHelper.IndexOfReference(this.Routes, element);
            if ((routesIndex != -1))
            {
                return ModelHelper.CreatePath("routes", routesIndex);
            }
            return base.GetRelativePathForNonIdentifiedChild(element);
        }
        
        /// <summary>
        /// Resolves the given URI to a child model element
        /// </summary>
        /// <returns>The model element or null if it could not be found</returns>
        /// <param name="reference">The requested reference name</param>
        /// <param name="index">The index of this reference</param>
        protected override IModelElement GetModelElementForReference(string reference, int index)
        {
            if ((reference == "INVALIDS"))
            {
                if ((index < this.Invalids.Count))
                {
                    return this.Invalids[index];
                }
                else
                {
                    return null;
                }
            }
            if ((reference == "SEMAPHORES"))
            {
                if ((index < this.Semaphores.Count))
                {
                    return this.Semaphores[index];
                }
                else
                {
                    return null;
                }
            }
            if ((reference == "ROUTES"))
            {
                if ((index < this.Routes.Count))
                {
                    return this.Routes[index];
                }
                else
                {
                    return null;
                }
            }
            return base.GetModelElementForReference(reference, index);
        }
        
        /// <summary>
        /// Gets the Class element that describes the structure of the current model element
        /// </summary>
        public override NMF.Models.Meta.IClass GetClass()
        {
            return NMF.Models.Repository.MetaRepository.Instance.ResolveClass("http://www.semanticweb.org/ontologies/2015/ttc/trainbenchmark#//RailwayContainer/" +
                    "");
        }
        
        /// <summary>
        /// The collection class to to represent the children of the RailwayContainer class
        /// </summary>
        public class RailwayContainerChildrenCollection : ReferenceCollection, ICollectionExpression<IModelElement>, ICollection<IModelElement>
        {
            
            private RailwayContainer _parent;
            
            /// <summary>
            /// Creates a new instance
            /// </summary>
            public RailwayContainerChildrenCollection(RailwayContainer parent)
            {
                this._parent = parent;
            }
            
            /// <summary>
            /// Gets the amount of elements contained in this collection
            /// </summary>
            public override int Count
            {
                get
                {
                    int count = 0;
                    count = (count + this._parent.Invalids.Count);
                    count = (count + this._parent.Semaphores.Count);
                    count = (count + this._parent.Routes.Count);
                    return count;
                }
            }
            
            protected override void AttachCore()
            {
                this._parent.Invalids.AsNotifiable().CollectionChanged += this.PropagateCollectionChanges;
                this._parent.Semaphores.AsNotifiable().CollectionChanged += this.PropagateCollectionChanges;
                this._parent.Routes.AsNotifiable().CollectionChanged += this.PropagateCollectionChanges;
            }
            
            protected override void DetachCore()
            {
                this._parent.Invalids.AsNotifiable().CollectionChanged -= this.PropagateCollectionChanges;
                this._parent.Semaphores.AsNotifiable().CollectionChanged -= this.PropagateCollectionChanges;
                this._parent.Routes.AsNotifiable().CollectionChanged -= this.PropagateCollectionChanges;
            }
            
            /// <summary>
            /// Adds the given element to the collection
            /// </summary>
            /// <param name="item">The item to add</param>
            public override void Add(IModelElement item)
            {
                IRailwayElement invalidsCasted = item.As<IRailwayElement>();
                if ((invalidsCasted != null))
                {
                    this._parent.Invalids.Add(invalidsCasted);
                }
                ISemaphore semaphoresCasted = item.As<ISemaphore>();
                if ((semaphoresCasted != null))
                {
                    this._parent.Semaphores.Add(semaphoresCasted);
                }
                IRoute routesCasted = item.As<IRoute>();
                if ((routesCasted != null))
                {
                    this._parent.Routes.Add(routesCasted);
                }
            }
            
            /// <summary>
            /// Clears the collection and resets all references that implement it.
            /// </summary>
            public override void Clear()
            {
                this._parent.Invalids.Clear();
                this._parent.Semaphores.Clear();
                this._parent.Routes.Clear();
            }
            
            /// <summary>
            /// Gets a value indicating whether the given element is contained in the collection
            /// </summary>
            /// <returns>True, if it is contained, otherwise False</returns>
            /// <param name="item">The item that should be looked out for</param>
            public override bool Contains(IModelElement item)
            {
                if (this._parent.Invalids.Contains(item))
                {
                    return true;
                }
                if (this._parent.Semaphores.Contains(item))
                {
                    return true;
                }
                if (this._parent.Routes.Contains(item))
                {
                    return true;
                }
                return false;
            }
            
            /// <summary>
            /// Copies the contents of the collection to the given array starting from the given array index
            /// </summary>
            /// <param name="array">The array in which the elements should be copied</param>
            /// <param name="arrayIndex">The starting index</param>
            public override void CopyTo(IModelElement[] array, int arrayIndex)
            {
                IEnumerator<IModelElement> invalidsEnumerator = this._parent.Invalids.GetEnumerator();
                try
                {
                    for (
                    ; invalidsEnumerator.MoveNext(); 
                    )
                    {
                        array[arrayIndex] = invalidsEnumerator.Current;
                        arrayIndex = (arrayIndex + 1);
                    }
                }
                finally
                {
                    invalidsEnumerator.Dispose();
                }
                IEnumerator<IModelElement> semaphoresEnumerator = this._parent.Semaphores.GetEnumerator();
                try
                {
                    for (
                    ; semaphoresEnumerator.MoveNext(); 
                    )
                    {
                        array[arrayIndex] = semaphoresEnumerator.Current;
                        arrayIndex = (arrayIndex + 1);
                    }
                }
                finally
                {
                    semaphoresEnumerator.Dispose();
                }
                IEnumerator<IModelElement> routesEnumerator = this._parent.Routes.GetEnumerator();
                try
                {
                    for (
                    ; routesEnumerator.MoveNext(); 
                    )
                    {
                        array[arrayIndex] = routesEnumerator.Current;
                        arrayIndex = (arrayIndex + 1);
                    }
                }
                finally
                {
                    routesEnumerator.Dispose();
                }
            }
            
            /// <summary>
            /// Removes the given item from the collection
            /// </summary>
            /// <returns>True, if the item was removed, otherwise False</returns>
            /// <param name="item">The item that should be removed</param>
            public override bool Remove(IModelElement item)
            {
                IRailwayElement railwayElementItem = item.As<IRailwayElement>();
                if (((railwayElementItem != null) 
                            && this._parent.Invalids.Remove(railwayElementItem)))
                {
                    return true;
                }
                ISemaphore semaphoreItem = item.As<ISemaphore>();
                if (((semaphoreItem != null) 
                            && this._parent.Semaphores.Remove(semaphoreItem)))
                {
                    return true;
                }
                IRoute routeItem = item.As<IRoute>();
                if (((routeItem != null) 
                            && this._parent.Routes.Remove(routeItem)))
                {
                    return true;
                }
                return false;
            }
            
            /// <summary>
            /// Gets an enumerator that enumerates the collection
            /// </summary>
            /// <returns>A generic enumerator</returns>
            public override IEnumerator<IModelElement> GetEnumerator()
            {
                return Enumerable.Empty<IModelElement>().Concat(this._parent.Invalids).Concat(this._parent.Semaphores).Concat(this._parent.Routes).GetEnumerator();
            }
        }
        
        /// <summary>
        /// The collection class to to represent the children of the RailwayContainer class
        /// </summary>
        public class RailwayContainerReferencedElementsCollection : ReferenceCollection, ICollectionExpression<IModelElement>, ICollection<IModelElement>
        {
            
            private RailwayContainer _parent;
            
            /// <summary>
            /// Creates a new instance
            /// </summary>
            public RailwayContainerReferencedElementsCollection(RailwayContainer parent)
            {
                this._parent = parent;
            }
            
            /// <summary>
            /// Gets the amount of elements contained in this collection
            /// </summary>
            public override int Count
            {
                get
                {
                    int count = 0;
                    count = (count + this._parent.Invalids.Count);
                    count = (count + this._parent.Semaphores.Count);
                    count = (count + this._parent.Routes.Count);
                    return count;
                }
            }
            
            protected override void AttachCore()
            {
                this._parent.Invalids.AsNotifiable().CollectionChanged += this.PropagateCollectionChanges;
                this._parent.Semaphores.AsNotifiable().CollectionChanged += this.PropagateCollectionChanges;
                this._parent.Routes.AsNotifiable().CollectionChanged += this.PropagateCollectionChanges;
            }
            
            protected override void DetachCore()
            {
                this._parent.Invalids.AsNotifiable().CollectionChanged -= this.PropagateCollectionChanges;
                this._parent.Semaphores.AsNotifiable().CollectionChanged -= this.PropagateCollectionChanges;
                this._parent.Routes.AsNotifiable().CollectionChanged -= this.PropagateCollectionChanges;
            }
            
            /// <summary>
            /// Adds the given element to the collection
            /// </summary>
            /// <param name="item">The item to add</param>
            public override void Add(IModelElement item)
            {
                IRailwayElement invalidsCasted = item.As<IRailwayElement>();
                if ((invalidsCasted != null))
                {
                    this._parent.Invalids.Add(invalidsCasted);
                }
                ISemaphore semaphoresCasted = item.As<ISemaphore>();
                if ((semaphoresCasted != null))
                {
                    this._parent.Semaphores.Add(semaphoresCasted);
                }
                IRoute routesCasted = item.As<IRoute>();
                if ((routesCasted != null))
                {
                    this._parent.Routes.Add(routesCasted);
                }
            }
            
            /// <summary>
            /// Clears the collection and resets all references that implement it.
            /// </summary>
            public override void Clear()
            {
                this._parent.Invalids.Clear();
                this._parent.Semaphores.Clear();
                this._parent.Routes.Clear();
            }
            
            /// <summary>
            /// Gets a value indicating whether the given element is contained in the collection
            /// </summary>
            /// <returns>True, if it is contained, otherwise False</returns>
            /// <param name="item">The item that should be looked out for</param>
            public override bool Contains(IModelElement item)
            {
                if (this._parent.Invalids.Contains(item))
                {
                    return true;
                }
                if (this._parent.Semaphores.Contains(item))
                {
                    return true;
                }
                if (this._parent.Routes.Contains(item))
                {
                    return true;
                }
                return false;
            }
            
            /// <summary>
            /// Copies the contents of the collection to the given array starting from the given array index
            /// </summary>
            /// <param name="array">The array in which the elements should be copied</param>
            /// <param name="arrayIndex">The starting index</param>
            public override void CopyTo(IModelElement[] array, int arrayIndex)
            {
                IEnumerator<IModelElement> invalidsEnumerator = this._parent.Invalids.GetEnumerator();
                try
                {
                    for (
                    ; invalidsEnumerator.MoveNext(); 
                    )
                    {
                        array[arrayIndex] = invalidsEnumerator.Current;
                        arrayIndex = (arrayIndex + 1);
                    }
                }
                finally
                {
                    invalidsEnumerator.Dispose();
                }
                IEnumerator<IModelElement> semaphoresEnumerator = this._parent.Semaphores.GetEnumerator();
                try
                {
                    for (
                    ; semaphoresEnumerator.MoveNext(); 
                    )
                    {
                        array[arrayIndex] = semaphoresEnumerator.Current;
                        arrayIndex = (arrayIndex + 1);
                    }
                }
                finally
                {
                    semaphoresEnumerator.Dispose();
                }
                IEnumerator<IModelElement> routesEnumerator = this._parent.Routes.GetEnumerator();
                try
                {
                    for (
                    ; routesEnumerator.MoveNext(); 
                    )
                    {
                        array[arrayIndex] = routesEnumerator.Current;
                        arrayIndex = (arrayIndex + 1);
                    }
                }
                finally
                {
                    routesEnumerator.Dispose();
                }
            }
            
            /// <summary>
            /// Removes the given item from the collection
            /// </summary>
            /// <returns>True, if the item was removed, otherwise False</returns>
            /// <param name="item">The item that should be removed</param>
            public override bool Remove(IModelElement item)
            {
                IRailwayElement railwayElementItem = item.As<IRailwayElement>();
                if (((railwayElementItem != null) 
                            && this._parent.Invalids.Remove(railwayElementItem)))
                {
                    return true;
                }
                ISemaphore semaphoreItem = item.As<ISemaphore>();
                if (((semaphoreItem != null) 
                            && this._parent.Semaphores.Remove(semaphoreItem)))
                {
                    return true;
                }
                IRoute routeItem = item.As<IRoute>();
                if (((routeItem != null) 
                            && this._parent.Routes.Remove(routeItem)))
                {
                    return true;
                }
                return false;
            }
            
            /// <summary>
            /// Gets an enumerator that enumerates the collection
            /// </summary>
            /// <returns>A generic enumerator</returns>
            public override IEnumerator<IModelElement> GetEnumerator()
            {
                return Enumerable.Empty<IModelElement>().Concat(this._parent.Invalids).Concat(this._parent.Semaphores).Concat(this._parent.Routes).GetEnumerator();
            }
        }

        public Orleans.Railway.IRailwayContainer ToSerializableModelElement()
        {
            var container = new Orleans.Railway.RailwayContainer();
            if (_serialized != null)
                return _serialized;

            _serialized = container;
            container.Invalids.AddRange(this.Invalids.Select(o => o.ToSerializableModelElement()));
            container.Routes.AddRange(this.Routes.Select(o => (Orleans.Railway.IRoute) o.ToSerializableModelElement()));
            container.Semaphores.AddRange(this.Semaphores.Select(o => (Orleans.Railway.ISemaphore) o.ToSerializableModelElement()));

            return container;
        }

        public Orleans.Railway.IRailwayContainer _serialized = null;
    }
}

