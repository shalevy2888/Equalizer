using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CyclicArray<T> : IEnumerable<T> {
		private T[] array;
		private int tail;
		private int size;

		public CyclicArray(int size)
		{
			this.size = size;
			array = new T[size];
			tail = 0;
		}
		public int Length {
			get {
				return size;
			}
		}
		public void Shift(int amount) {
			if (amount > size)
				throw new System.ArgumentOutOfRangeException ("amount");
			tail = (tail + amount) % size; 
		}

		public T this [int index] {
			get {
				if (index < 0 || index >= size)
					throw new System.ArgumentOutOfRangeException ("index");

				return array [(tail + index) % size];
			}
			set {
				if (index < 0 || index >= size)
					throw new System.ArgumentOutOfRangeException ("index");

				array [(tail + index) % size] = value;
			}
		}

		public IEnumerator<T> GetEnumerator ()
		{
			if (size == 0)
				yield break;

			for (var i = 0; i < size; ++i)
				yield return this [i];
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
	}