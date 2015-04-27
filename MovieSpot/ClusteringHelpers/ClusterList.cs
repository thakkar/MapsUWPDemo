//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System.Collections.Generic;

namespace ClusteringExtension.DataModel
{
    public sealed class ClusterList : IList<Cluster>
    {
        private IList<Cluster> list = new List<Cluster>();

        public int IndexOf(Cluster item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, Cluster item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new System.NotImplementedException();
        }

        public Cluster this[int index]
        {
            get
            {
                return this.list[index];
            }
            set
            {
                this.list[index] = value;
            }
        }

        public void Add(Cluster item)
        {
            this.list.Add(item);
        }

        public void Clear()
        {
            this.list.Clear();
        }

        public bool Contains(Cluster item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(Cluster[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public int Count
        {
            get { return this.list.Count; }
        }

        public bool IsReadOnly
        {
            get { return this.list.IsReadOnly; }
        }

        public bool Remove(Cluster item)
        {
           return this.list.Remove(item);
        }

        public IEnumerator<Cluster> GetEnumerator()
        {
            return this.list.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new System.NotImplementedException();
        }
    }
}
